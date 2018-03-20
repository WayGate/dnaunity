// Copyright (c) 2012 DotNetAnywhere
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Runtime.InteropServices;

#if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
using SIZE_T = System.UInt32;
using PTR = System.UInt32;
#else
using SIZE_T = System.UInt64;
using PTR = System.UInt64;
#endif 

namespace DnaUnity
{

    public unsafe static partial class MetaData
    {
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        const uint STACK_ALIGNMENT = 4;
        #else
        const uint STACK_ALIGNMENT = 8;
#endif

        public struct FillState
        {
            public tMD_TypeDef* pTypeDef;
            public tMD_TypeDef** ppClassTypeArgs;
            public tMD_TypeDef** ppMethodTypeArgs;
        }

        public static Dictionary<PTR, FillState> typesToFill = null;

        public static void Fill_StartDefer()
        {
            typesToFill = new Dictionary<PTR, FillState>();
        }

        public static void Fill_Defer(tMD_TypeDef* pTypeDef, tMD_TypeDef** ppClassTypeArgs, tMD_TypeDef** ppMethodTypeArgs)
        {
            if (typesToFill != null && pTypeDef->fillState < Type.TYPE_FILL_ALL && !typesToFill.ContainsKey((PTR)pTypeDef)) {
                typesToFill.Add((PTR)pTypeDef, new FillState {
                    pTypeDef = pTypeDef,
                    ppClassTypeArgs = ppClassTypeArgs,
                    ppMethodTypeArgs = ppMethodTypeArgs
                });
            }
        }

        public static void Fill_GetDeferredTypeArgs(tMD_TypeDef* pTypeDef, ref tMD_TypeDef** ppClassTypeArgs, ref tMD_TypeDef** ppMethodTypeArgs)
        {
            FillState state = new FillState();
            if (typesToFill != null) {
                if (typesToFill.TryGetValue((PTR)pTypeDef, out state)) {
                    ppClassTypeArgs = state.ppClassTypeArgs;
                    ppMethodTypeArgs = state.ppMethodTypeArgs;
                }
            }
        }

        public static void Fill_ResolveDeferred()
        {
            if (typesToFill != null) {
                // Filling types will cause other types to be added to fill dictionary.  We loop until we have no more
                // types to fill.
                while (typesToFill.Count > 0) {
                    List<FillState> fillTypes = new List<FillState>(typesToFill.Values);
                    foreach (var state in fillTypes) { 
                        if (state.pTypeDef->fillState < Type.TYPE_FILL_ALL) {
                            Fill_TypeDef(state.pTypeDef, state.ppClassTypeArgs, state.ppMethodTypeArgs, Type.TYPE_FILL_ALL);
                        }
                        typesToFill.Remove((PTR)state.pTypeDef);
                    }
                }
                typesToFill = null;
            }
        }

        public static void Fill_FieldDef(tMD_TypeDef *pParentType, tMD_FieldDef *pFieldDef, uint memOffset, 
            uint* pAlignment, tMD_TypeDef **ppClassTypeArgs) 
        {
        	uint sigLength;
        	byte* sig;
        	tMetaData *pMetaData;
            uint fieldSize, fieldAlignment;

            if (pFieldDef->isFilled == 1) {
                return;
            }

            // Note: parent type can be null for module level fields (frequently seen with auto-gen RVA init fields)
        	pFieldDef->pParentType = pParentType;
            pFieldDef->pFieldDef = pFieldDef;
            pFieldDef->isFilled = 1;

            sig = MetaData.GetBlob(pFieldDef->signature, &sigLength);

        	MetaData.DecodeSigEntry(&sig); // First entry always 0x06
        	pFieldDef->pType = Type.GetTypeFromSig(pFieldDef->pMetaData, &sig, ppClassTypeArgs, null, null);
        	if (pFieldDef->pType == null) {
        		// If the field is a core generic type definition, then we can't do anything more
        		return;
        	}
            if (pFieldDef->pType->fillState < Type.TYPE_FILL_LAYOUT) {
                MetaData.Fill_TypeDef(pFieldDef->pType, null, null, Type.TYPE_FILL_LAYOUT);
            } else if (pFieldDef->pType->fillState < Type.TYPE_FILL_ALL) {
                MetaData.Fill_Defer(pFieldDef->pType, null, null);
            }
            if (pFieldDef->pType->isValueType != 0) {
                fieldSize = pFieldDef->pType->instanceMemSize;
                fieldAlignment = (pFieldDef->pType->isValueType == 0 || pFieldDef->pType->alignment == 0) ? sizeof(PTR) : pFieldDef->pType->alignment;
            } else {
                fieldSize = fieldAlignment = sizeof(PTR);
            }
            if (pAlignment != null && *pAlignment < fieldAlignment)
                *pAlignment = fieldAlignment;
            pFieldDef->memOffset = (memOffset + fieldAlignment - 1) & ~(fieldAlignment - 1);
            pFieldDef->memSize = fieldSize;

            pMetaData = pFieldDef->pMetaData;
        	if (MetaData.FIELD_HASFIELDRVA(pFieldDef)) {
        		uint i, top;

        		// Field has RVA, so load it from FieldRVA
        		top = pMetaData->tables.numRows[MetaDataTable.MD_TABLE_FIELDRVA];
        		for (i=1; i<=top; i++) {
        			tMD_FieldRVA *pFieldRVA;

        			pFieldRVA = (tMD_FieldRVA*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_FIELDRVA, i));
        			if (pFieldRVA->field == pFieldDef->tableIndex) {
        				pFieldDef->pMemory = (byte*)pFieldRVA->rva;
        				break;
        			}
        		}
        	} else if (MetaData.FIELD_ISLITERAL(pFieldDef)) {
        		// Field is literal, so make pMemory point to the value signature
        		uint i, top;

        		top = pMetaData->tables.numRows[MetaDataTable.MD_TABLE_CONSTANT];
        		for (i=1; i<=top; i++) {
        			tMD_Constant *pConst;
        			pConst = (tMD_Constant*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_CONSTANT, i));
        			if (pConst->parent == pFieldDef->tableIndex) {
        				// Found the field
        				pFieldDef->pMemory = (byte*)pConst;
        				break;
        			}
        		}
        	}
        }

        public static void Fill_MethodDef(tMD_TypeDef *pParentType, tMD_MethodDef *pMethodDef, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	/*SIG*/byte* sig;
        	uint i, entry, totalSize, start;

            if (pMethodDef->isFilled == 1) {
                return;
            }

            // Note: parent type can be null for module level methods (not typical in C# assemblies)
        	pMethodDef->pParentType = pParentType;
        	pMethodDef->pMethodDef = pMethodDef;
        	pMethodDef->isFilled = 1;

        	if (pMethodDef->isGenericDefinition != 0) {
        		// Generic definition method, so can't do any more.
        		//Sys.log_f("Method<>: %s.%s.%s()\n", pParentType->nameSpace, pParentType->name, pMethodDef->name);
        		return;
        	}

        	sig = MetaData.GetBlob(pMethodDef->signature, null);
        	entry = MetaData.DecodeSigEntry(&sig);
            if ((entry & SIG_METHODDEF_GENERIC) != 0) {
        		// Has generic parameters. Read how many, but don't care about the answer
        		MetaData.DecodeSigEntry(&sig);
        	}
            pMethodDef->numberOfParameters = (ushort)(MetaData.DecodeSigEntry(&sig) + (MetaData.METHOD_ISSTATIC(pMethodDef)?0:1));
        	pMethodDef->pReturnType = Type.GetTypeFromSig(pMethodDef->pMetaData, &sig, ppClassTypeArgs, ppMethodTypeArgs, null);
            if (pMethodDef->pReturnType != null && pMethodDef->pReturnType->fillState < Type.TYPE_FILL_ALL) {
                Fill_Defer(pMethodDef->pReturnType, ppClassTypeArgs, ppMethodTypeArgs);
            }
            pMethodDef->pParams = (tParameter*)Mem.malloc((SIZE_T)(pMethodDef->numberOfParameters * sizeof(tParameter)));
        	totalSize = 0;
            start = 0;
        	if (!MetaData.METHOD_ISSTATIC(pMethodDef)) {
        		// Fill in parameter info for the 'this' pointer
        		pMethodDef->pParams->offset = 0;
        		if (pParentType->isValueType != 0) {
        			// If this is a value-type then the 'this' pointer is actually an IntPtr to the value-type's location
                    pMethodDef->pParams->size = sizeof(PTR);
        			pMethodDef->pParams->pStackTypeDef = Type.types[Type.TYPE_SYSTEM_INTPTR];
        		} else {
                    pMethodDef->pParams->size = sizeof(PTR);
        			pMethodDef->pParams->pStackTypeDef = pParentType;
        		}
                totalSize = sizeof(PTR);
                start = 1;
        	}
        	for (i=start; i<pMethodDef->numberOfParameters; i++) {
        		tMD_TypeDef *pStackTypeDef;
                tMD_TypeDef* pByRefTypeDef;
        		uint size;

                pByRefTypeDef = null;
        		pStackTypeDef = Type.GetTypeFromSig(pMethodDef->pMetaData, &sig, ppClassTypeArgs, ppMethodTypeArgs, &pByRefTypeDef);
                if (pStackTypeDef != null) {
                    if (pStackTypeDef->fillState < Type.TYPE_FILL_LAYOUT) {
                        MetaData.Fill_TypeDef(pStackTypeDef, null, null, Type.TYPE_FILL_LAYOUT);
                    } else if (pStackTypeDef->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pStackTypeDef, null, null);
                    }
                    size = pStackTypeDef->stackSize;
                } else {
                    size = 0;
                }
                if (pByRefTypeDef != null) {
                    if (pByRefTypeDef->fillState < Type.TYPE_FILL_LAYOUT) {
                        MetaData.Fill_TypeDef(pByRefTypeDef, null, null, Type.TYPE_FILL_LAYOUT);
                    } else if (pByRefTypeDef->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pByRefTypeDef, null, null);
                    }
                }
                pMethodDef->pParams[i].pStackTypeDef = pStackTypeDef;
                pMethodDef->pParams[i].pByRefTypeDef = pByRefTypeDef;
                pMethodDef->pParams[i].offset = totalSize;
        		pMethodDef->pParams[i].size = size;
        		totalSize += size;
        	}
        	pMethodDef->parameterStackSize = totalSize;
        }

        // Find the method that has been overridden by pMethodDef.
        // This is to get the correct vTable offset for the method.
        // This must search the MethodImpl table to see if the default inheritence rules are being overridden.
        // Return null if this method does not override anything.
        public static tMD_MethodDef* FindVirtualOverriddenMethod(tMD_TypeDef *pTypeDef, tMD_MethodDef *pMethodDef) 
        {
        	uint i;

        	do {
        		// Search MethodImpl table
        		for (i=pTypeDef->pMetaData->tables.numRows[MetaDataTable.MD_TABLE_METHODIMPL]; i>0; i--) {
        			tMD_MethodImpl *pMethodImpl;

        			pMethodImpl = (tMD_MethodImpl*)MetaData.GetTableRow(pTypeDef->pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODIMPL, i));
        			if (pMethodImpl->class_ == pTypeDef->tableIndex) {
        				tMD_MethodDef *pMethodDeclDef;

        				pMethodDeclDef = MetaData.GetMethodDefFromDefRefOrSpec(pTypeDef->pMetaData, pMethodImpl->methodDeclaration, 
                            pTypeDef->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        				if (pMethodDeclDef->tableIndex == pMethodDef->tableIndex) {
        					/*IDX_TABLE*/uint methodToken;
        					tMD_MethodDef *pMethod;

        					methodToken = pMethodImpl->methodBody;
        					pMethod = (tMD_MethodDef*)MetaData.GetTableRow(pTypeDef->pMetaData, methodToken);
        					return pMethod;
        				}
        			}
        		}

        		// Use normal inheritence rules
        		// It must be a virtual method that's being overridden.
        		for (i=pTypeDef->numMethods - 1; i != 0xffffffff; i--) {
                    tMD_MethodDef* pVirtMethodDef = pTypeDef->ppMethods[i];
                    if (MetaData.METHOD_ISVIRTUAL(pVirtMethodDef)) {
                        if (pMethodDef->signature != null) {
                            if (MetaData.CompareNameAndSig(pMethodDef->name, pMethodDef->signature, pMethodDef->pMetaData,
                                pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs, 
                                pVirtMethodDef, pTypeDef->ppClassTypeArgs, null) != 0) {
                                return pVirtMethodDef;
                            }
                        } else if (pMethodDef->monoMethodInfo != null) {
                            System.Reflection.MethodBase methodInfo = H.ToObj(pMethodDef->monoMethodInfo) as System.Reflection.MethodBase;
                            if (MetaData.CompareNameAndMethodInfo(pMethodDef->name, methodInfo, pMethodDef->pMetaData,
                                pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs, pVirtMethodDef, pTypeDef->ppClassTypeArgs, null) != 0) {
                                return pVirtMethodDef;
                            }
                        }
                    }
                }
        		pTypeDef = pTypeDef->pParent;
        	} while (pTypeDef != null);

        	return null;
        }

        public static void Fill_TypeDef(tMD_TypeDef *pTypeDef, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs, 
            uint resolve = Type.TYPE_FILL_ALL) 
        {
        	/*IDX_TABLE*/uint firstIdx, lastIdx, token;
        	uint instanceMemSize, staticMemSize, virtualOfs, isDeferred, i, j;
        	tMetaData *pMetaData = pTypeDef->pMetaData;
        	tMD_TypeDef *pParent;

            if (pTypeDef->fillState >= resolve) {
                return;
            }

            if (pTypeDef->monoType != null) {
                MonoType.Fill_TypeDef(pTypeDef, ppClassTypeArgs, ppMethodTypeArgs, resolve);
                return;
            }

//            Sys.printf("FILLING TYPE: %s\n", (PTR)pTypeDef->name);
//            string name = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)pTypeDef->name);


            if (typesToFill == null) {
                Fill_StartDefer();
                isDeferred = 1;
            } else {
                isDeferred = 0;
            }

            if (resolve < Type.TYPE_FILL_ALL) {
                MetaData.Fill_Defer(pTypeDef, ppClassTypeArgs, ppMethodTypeArgs);
            }

            MetaData.Fill_GetDeferredTypeArgs(pTypeDef, ref ppClassTypeArgs, ref ppMethodTypeArgs);

            // Fill parent info
            if (pTypeDef->fillState < Type.TYPE_FILL_PARENTS) {
                pTypeDef->fillState = Type.TYPE_FILL_PARENTS;

                pTypeDef->pTypeDef = pTypeDef;
                if (pTypeDef->alignment == 0) {
                    pTypeDef->alignment = 1;
                }

                if (pTypeDef->pParent == null) {
                    pTypeDef->pParent = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, pTypeDef->extends, ppClassTypeArgs, ppMethodTypeArgs);
                }
                pParent = pTypeDef->pParent;

                if (pParent != null) {
                    if (pParent->fillState < Type.TYPE_FILL_PARENTS) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_PARENTS);
                    } else if (pParent->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pParent, null, null);
                    }
                    if (pParent->hasMonoBase != 0) {
                        pTypeDef->hasMonoBase = 1;
                    }
                }

                // If this type is an interface, then return 0
                if (pTypeDef->stackSize != 0) {
                    pTypeDef->isValueType = (byte)(pTypeDef->stackType != EvalStack.EVALSTACK_O ? 1 : 0);
                } else if (MetaData.TYPE_ISINTERFACE(pTypeDef)) {
                    pTypeDef->isValueType = 0;
                } else if (pTypeDef->nameSpace[0] == 'S' && S.strcmp(pTypeDef->nameSpace, new S(ref scSystem, "System")) == 0) {
                    if ((pTypeDef->name[0] == 'V' && S.strcmp(pTypeDef->name, new S(ref scValueType, "ValueType")) == 0) ||
                        (pTypeDef->name[0] == 'E' && S.strcmp(pTypeDef->name, new S(ref scEnum, "Enum")) == 0)) {
                        pTypeDef->isValueType = 1;
                    } else if (pTypeDef->name[0] == 'O' && S.strcmp(pTypeDef->name, new S(ref scObject, "Object")) == 0) {
                        pTypeDef->isValueType = 0;
                    } else if (pParent != null) {
                        pTypeDef->isValueType = pParent->isValueType;
                    }
                } else if (pParent != null) {
                    pTypeDef->isValueType = pParent->isValueType;
                }

                // If not primed, then work out how many methods & fields there are.
                if (pTypeDef->isPrimed == 0) {
                    // Methods
                    lastIdx = (pTypeDef->isLast != 0) ?
                        MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODDEF, pTypeDef->pMetaData->tables.numRows[MetaDataTable.MD_TABLE_METHODDEF]) :
                        (pTypeDef[1].methodList - 1);
                    pTypeDef->numMethods = lastIdx - pTypeDef->methodList + 1;
                    // Fields
                    lastIdx = (pTypeDef->isLast != 0) ?
                        MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_FIELDDEF, pTypeDef->pMetaData->tables.numRows[MetaDataTable.MD_TABLE_FIELDDEF]) :
                        (pTypeDef[1].fieldList - 1);
                    pTypeDef->numFields = lastIdx - pTypeDef->fieldList + 1;
                }

                // If this is a nested type, then find the namespace of it
                if (pTypeDef->pNestedIn != null) {
                    tMD_TypeDef* pRootTypeDef = pTypeDef->pNestedIn;
                    while (pRootTypeDef->pNestedIn != null) {
                        pRootTypeDef = pRootTypeDef->pNestedIn;
                    }
                    pTypeDef->nameSpace = pRootTypeDef->nameSpace;
                }

                // If this is an enum type, then pretend its stack type is its underlying type
                if (pTypeDef->pParent == Type.types[Type.TYPE_SYSTEM_ENUM]) {
                    pTypeDef->stackType = EvalStack.EVALSTACK_INT32;
                    pTypeDef->stackSize = sizeof(PTR);
                    pTypeDef->instanceMemSize = 4;
                    pTypeDef->arrayElementSize = 4;
                }

                if (pTypeDef->fillState >= resolve)
                    return;

            } else {

                pParent = pTypeDef->pParent;

            }

            if (pTypeDef->fillState < Type.TYPE_FILL_LAYOUT) {
                pTypeDef->fillState = Type.TYPE_FILL_LAYOUT;

                if (pParent != null) {
                    if (pParent->fillState < Type.TYPE_FILL_LAYOUT) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_LAYOUT);
                    } else if (pParent->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pParent, null, null);
                    }
                }

                if (pTypeDef->isGenericDefinition == 0) {

                    // Resolve fields, members, interfaces.
                    // Only needs to be done if it's not a generic definition type

                    // It it's not a value-type and the stack-size is not preset, then set it up now.
                    // It needs to be done here as non-static fields in non-value type can point to the containing type
                    if (pTypeDef->stackSize == 0 && pTypeDef->isValueType == 0) {
                        pTypeDef->stackType = EvalStack.EVALSTACK_O;
                        pTypeDef->stackSize = sizeof(PTR);
                        pTypeDef->alignment = sizeof(PTR);
                    }

                    // Resolve all fields - instance ONLY at this point,
                    // because static fields in value-Type.types can be of the containing type, and the size is not yet known.
                    firstIdx = pTypeDef->fieldList;
                    lastIdx = firstIdx + pTypeDef->numFields - 1;
                    staticMemSize = 0;
                    if (pTypeDef->numFields > 0) {
                        pTypeDef->ppFields = (tMD_FieldDef**)Mem.mallocForever((SIZE_T)(pTypeDef->numFields * sizeof(tMD_FieldDef*)));
                    }
                    instanceMemSize = (pTypeDef->pParent == null) ? 0 : pTypeDef->pParent->instanceMemSize;
                    for (token = firstIdx, i = 0; token <= lastIdx; token++, i++) {
                        tMD_FieldDef* pFieldDef;

                        pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);
                        if (!MetaData.FIELD_ISSTATIC(pFieldDef)) {
                            // Only handle non-static fields at the moment
                            if (pTypeDef->pGenericDefinition != null) {
                                // If this is a generic instantiation type, then all field defs need to be copied,
                                // as there will be lots of different instantiations.
                                tMD_FieldDef* pFieldCopy = ((tMD_FieldDef*)Mem.mallocForever((SIZE_T)sizeof(tMD_FieldDef)));
                                Mem.memcpy(pFieldCopy, pFieldDef, (SIZE_T)sizeof(tMD_FieldDef));
                                pFieldDef = pFieldCopy;
                            }
                            if (MetaData.FIELD_ISLITERAL(pFieldDef) || MetaData.FIELD_HASFIELDRVA(pFieldDef)) {
                                // If it's a literal, then analyse the field, but don't include it in any memory allocation
                                // If is has an RVA, then analyse the field, but don't include it in any memory allocation
                                MetaData.Fill_FieldDef(pTypeDef, pFieldDef, 0, null, ppClassTypeArgs);
                            } else {
                                MetaData.Fill_FieldDef(pTypeDef, pFieldDef, instanceMemSize, &(pTypeDef->alignment), ppClassTypeArgs);
                                instanceMemSize = pFieldDef->memOffset + pFieldDef->memSize;
                            }
                            pTypeDef->ppFields[i] = pFieldDef;
                        }
                    }
                    if (pTypeDef->instanceMemSize == 0) {
                        pTypeDef->instanceMemSize = (instanceMemSize + (pTypeDef->alignment - 1)) & ~(pTypeDef->alignment - 1);
                    }

                    // Sort out stack type and size.
                    // Note that this may already be set, as some basic type have this preset;
                    // or if it's not a value-type it'll already be set
                    if (pTypeDef->stackSize == 0) {
                        // if it gets here then it must be a value type
                        pTypeDef->stackType = EvalStack.EVALSTACK_VALUETYPE;
                        pTypeDef->stackSize = pTypeDef->instanceMemSize;
                    }

                    // Sort out array element size. Note that some basic type will have this preset.
                    if (pTypeDef->arrayElementSize == 0) {
                        pTypeDef->arrayElementSize = pTypeDef->stackSize;
                    }

                    // Make sure stack size is even multiple of stack alignment
                    pTypeDef->stackSize = (pTypeDef->stackSize + (STACK_ALIGNMENT - 1)) & ~(STACK_ALIGNMENT - 1);

                    // Handle static fields
                    for (token = firstIdx, i = 0; token <= lastIdx; token++, i++) {
                        tMD_FieldDef* pFieldDef;

                        pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);
                        if (MetaData.FIELD_ISSTATIC(pFieldDef)) {
                            // Only handle static fields here
                            if (pTypeDef->pGenericDefinition != null) {
                                // If this is a generic instantiation type, then all field defs need to be copied,
                                // as there will be lots of different instantiations.
                                tMD_FieldDef* pFieldCopy = ((tMD_FieldDef*)Mem.mallocForever((SIZE_T)sizeof(tMD_FieldDef)));
                                Mem.memcpy(pFieldCopy, pFieldDef, (SIZE_T)sizeof(tMD_FieldDef));
                                pFieldDef = pFieldCopy;
                            }
                            if (MetaData.FIELD_ISLITERAL(pFieldDef) || MetaData.FIELD_HASFIELDRVA(pFieldDef)) {
                                // If it's a literal, then analyse the field, but don't include it in any memory allocation
                                // If is has an RVA, then analyse the field, but don't include it in any memory allocation
                                MetaData.Fill_FieldDef(pTypeDef, pFieldDef, 0, null, ppClassTypeArgs);
                            }
                            else {
                                MetaData.Fill_FieldDef(pTypeDef, pFieldDef, staticMemSize, null, ppClassTypeArgs);
                                staticMemSize += pFieldDef->memSize;
                            }
                            pTypeDef->ppFields[i] = pFieldDef;
                        }
                    }

                    if (staticMemSize > 0) {
                        pTypeDef->pStaticFields = (byte*)Mem.mallocForever((SIZE_T)staticMemSize);
                        Mem.memset(pTypeDef->pStaticFields, 0, staticMemSize);
                        // Set the field addresses (->pMemory) of all static fields
                        for (i = 0; i < pTypeDef->numFields; i++) {
                            tMD_FieldDef* pFieldDef;

                            pFieldDef = pTypeDef->ppFields[i];
                            if (MetaData.FIELD_ISSTATIC(pFieldDef) && pFieldDef->pMemory == null) {
                                // Only set it if it isn't already set. It will be already set if this field has an RVA
                                pFieldDef->pMemory = pTypeDef->pStaticFields + pFieldDef->memOffset;
                            }
                        }
                        pTypeDef->staticFieldSize = staticMemSize;
                    }
                }

                if (pTypeDef->fillState >= resolve)
                    return;
            }

            // This only needs to be done for non-generic Type.types, or for generic type that are not a definition
            // I.e. Fully instantiated generic Type.types
            if (pTypeDef->fillState < Type.TYPE_FILL_VTABLE) {
                pTypeDef->fillState = Type.TYPE_FILL_VTABLE;

                if (pParent != null) {
                    if (pParent->fillState < Type.TYPE_FILL_VTABLE) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_VTABLE);
                    } else if (pParent->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pParent, null, null);
                    }
                }

                if (pTypeDef->isGenericDefinition == 0) {

                    virtualOfs = (pParent != null) ? pParent->numVirtualMethods : 0;

                    // Must create the virtual method table BEFORE any other type resolution is done
                    // Note that this must not do ANY filling of type or methods.
                    // This is to ensure that the parent object(s) in any type inheritance hierachy are allocated
                    // their virtual method offset before derived Type.types.
                    firstIdx = pTypeDef->methodList;
                    lastIdx = firstIdx + pTypeDef->numMethods - 1;
                    for (token = firstIdx; token <= lastIdx; token++) {
                        tMD_MethodDef* pMethodDef;

                        pMethodDef = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);

                        //Sys.printf("Method: %s\n", (PTR)pMethodDef->name);

                        // This is needed, so array resolution can work correctly and FindVirtualOverriddenMethod() can work.
                        pMethodDef->pParentType = pTypeDef;

                        if (MetaData.METHOD_ISVIRTUAL(pMethodDef)) {
                            if (MetaData.METHOD_ISNEWSLOT(pMethodDef) || pTypeDef->pParent == null) {
                                // Allocate a new vTable slot if method is explicitly marked as NewSlot, or
                                // this is of type Object.
                                pMethodDef->vTableOfs = virtualOfs++;
                            } else {
                                tMD_MethodDef* pVirtualOveriddenMethod;
                                pVirtualOveriddenMethod = FindVirtualOverriddenMethod(pTypeDef->pParent, pMethodDef);
                                if (pVirtualOveriddenMethod == null) {
                                    Sys.Crash("Unable to find virtual override method for %s %s", (PTR)pTypeDef->name, (PTR)pMethodDef->name);
                                }
                                pMethodDef->vTableOfs = pVirtualOveriddenMethod->vTableOfs;
                            }
                        } else {
                            // Dummy value - make it obvious it's not valid!
                            pMethodDef->vTableOfs = 0xffffffff;
                        }

                    }

                    // Create the virtual method table
                    pTypeDef->numVirtualMethods = virtualOfs;

                    // Resolve all members
                    firstIdx = pTypeDef->methodList;
                    lastIdx = firstIdx + pTypeDef->numMethods - 1;
                    pTypeDef->ppMethods = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pTypeDef->numMethods * sizeof(tMD_MethodDef*)));
                    pTypeDef->pVTable = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pTypeDef->numVirtualMethods * sizeof(tMD_MethodDef*)));
                    // Copy initial vTable from parent
                    if (pTypeDef->pParent != null) {
                        if (pTypeDef->pParent->fillState != Type.TYPE_FILL_MEMBERS) {
                            Fill_TypeDef(pTypeDef->pParent, null, null, Type.TYPE_FILL_MEMBERS);
                        }
                        Mem.memcpy(pTypeDef->pVTable, pTypeDef->pParent->pVTable, (SIZE_T)(pTypeDef->pParent->numVirtualMethods * sizeof(tMD_MethodDef*)));
                    }
                    for (token = firstIdx, i = 0; token <= lastIdx; token++, i++) {
                        tMD_MethodDef* pMethodDef;

                        pMethodDef = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);
                        if (pTypeDef->pGenericDefinition != null) {
                            // If this is a generic instantiation type, then all method defs need to be copied,
                            // as there will be lots of different instantiations.
                            tMD_MethodDef* pMethodCopy = ((tMD_MethodDef*)Mem.mallocForever((SIZE_T)sizeof(tMD_MethodDef)));
                            Mem.memcpy(pMethodCopy, pMethodDef, (SIZE_T)sizeof(tMD_MethodDef));
                            pMethodDef = pMethodCopy;
                        }
                        if (MetaData.METHOD_ISSTATIC(pMethodDef) && S.strcmp(pMethodDef->name, ".cctor") == 0) {
                            // This is a static constructor
                            pTypeDef->pStaticConstructor = pMethodDef;
                        }
                        if (!MetaData.METHOD_ISSTATIC(pMethodDef) && pTypeDef->pParent != null &&
                            S.strcmp(pMethodDef->name, "Finalize") == 0) {
                            // This is a Finalizer method, but not for Object.
                            // Delibrately miss out Object's Finalizer because it's empty and will cause every object
                            // of any type to have a Finalizer which will be terrible for performance.
                            pTypeDef->pFinalizer = pMethodDef;
                        }
                        if (MetaData.METHOD_ISVIRTUAL(pMethodDef)) {
                            // This is a virtual method, so enter it in the vTable
                            pTypeDef->pVTable[pMethodDef->vTableOfs] = pMethodDef;
                        }
                        pTypeDef->ppMethods[i] = pMethodDef;
                    }
                    // Find inherited Finalizer, if this type doesn't have an explicit Finalizer, and if there is one
                    if (pTypeDef->pFinalizer == null) {
                        tMD_TypeDef* pInheritedType = pTypeDef->pParent;
                        while (pInheritedType != null) {
                            if (pInheritedType->pFinalizer != null) {
                                pTypeDef->pFinalizer = pInheritedType->pFinalizer;
                                break;
                            }
                            pInheritedType = pInheritedType->pParent;
                        }
                    }

                }

                if (pTypeDef->fillState >= resolve)
                    return;
            }

            if (pTypeDef->fillState < Type.TYPE_FILL_MEMBERS) {
                pTypeDef->fillState = Type.TYPE_FILL_MEMBERS;

                if (pParent != null) {
                    if (pParent->fillState < Type.TYPE_FILL_MEMBERS) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_MEMBERS);
                    } else if (pParent->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pParent, null, null);
                    }
                }

                if (pTypeDef->isGenericDefinition == 0) {

                    // Fill all method definitions for this type
                    for (i = 0; i < pTypeDef->numMethods; i++) {
                        MetaData.Fill_MethodDef(pTypeDef, pTypeDef->ppMethods[i], ppClassTypeArgs, ppMethodTypeArgs);
                    }
                }

                if (pTypeDef->fillState >= resolve)
                    return;
            }

            if (pTypeDef->fillState < Type.TYPE_FILL_INTERFACES) {
                pTypeDef->fillState = Type.TYPE_FILL_INTERFACES;

                if (pParent != null) {
                    if (pParent->fillState < Type.TYPE_FILL_INTERFACES) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_INTERFACES);
                    } else if (pParent->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pParent, null, null);
                    }
                }

                if (pTypeDef->isGenericDefinition == 0 && !MetaData.TYPE_ISINTERFACE(pTypeDef)) {

                    if (pParent != null && pParent->fillState < Type.TYPE_FILL_INTERFACES) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_INTERFACES);
                    }

                    // Map all interface method calls. This only needs to be done for Classes, not Interfaces
                    // And is not done for generic definitions.
                    firstIdx = 0;
                    if (pTypeDef->pParent != null) {
                        j = pTypeDef->numInterfaces = pTypeDef->pParent->numInterfaces;
                    } else {
                        j = 0;
                    }

                    lastIdx = firstIdx;
                    for (i = 1; i <= pMetaData->tables.numRows[MetaDataTable.MD_TABLE_INTERFACEIMPL]; i++) {
                        tMD_InterfaceImpl* pInterfaceImpl;
                        pInterfaceImpl = (tMD_InterfaceImpl*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_INTERFACEIMPL, i));
                        if (pInterfaceImpl->class_ == pTypeDef->tableIndex) {
                            // count how many interfaces are implemented
                            pTypeDef->numInterfaces++;
                            if (firstIdx == 0) {
                                firstIdx = MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_INTERFACEIMPL, i);
                            }
                            lastIdx = MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_INTERFACEIMPL, i);
                        }
                    }

                    if (pTypeDef->numInterfaces > 0) {
                        uint mapNum;

                        pTypeDef->pInterfaceMaps = (tInterfaceMap*)Mem.mallocForever((SIZE_T)(pTypeDef->numInterfaces * sizeof(tInterfaceMap)));
                        // Copy interface maps from parent type
                        if (j > 0) {
                            Mem.memcpy(pTypeDef->pInterfaceMaps, pTypeDef->pParent->pInterfaceMaps, (SIZE_T)(j * sizeof(tInterfaceMap)));
                        }
                        mapNum = j;
                        if (firstIdx > 0) {
                            for (token = firstIdx; token <= lastIdx; token++, mapNum++) {
                                tMD_InterfaceImpl* pInterfaceImpl;

                                pInterfaceImpl = (tMD_InterfaceImpl*)MetaData.GetTableRow(pMetaData, token);
                                if (pInterfaceImpl->class_ == pTypeDef->tableIndex) {
                                    tMD_TypeDef* pInterface;
                                    tInterfaceMap* pMap;

                                    // Get the interface that this type implements
                                    pInterface = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, pInterfaceImpl->interface_, ppClassTypeArgs, ppMethodTypeArgs);
                                    MetaData.Fill_TypeDef(pInterface, null, null, Type.TYPE_FILL_INTERFACES);
                                    pMap = &pTypeDef->pInterfaceMaps[mapNum];
                                    pMap->pInterface = pInterface;
                                    pMap->pVTableLookup = (uint*)Mem.mallocForever((SIZE_T)(pInterface->numVirtualMethods * sizeof(uint)));
                                    pMap->ppMethodVLookup = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pInterface->numVirtualMethods * sizeof(tMD_MethodDef*)));
                                    // Discover interface mapping for each interface method
                                    for (i = 0; i < pInterface->numVirtualMethods; i++) {
                                        tMD_MethodDef* pInterfaceMethod;
                                        tMD_MethodDef* pOverriddenMethod;
                                        pInterfaceMethod = pInterface->pVTable[i];
                                        pOverriddenMethod = FindVirtualOverriddenMethod(pTypeDef, pInterfaceMethod);
                                        pMap->pVTableLookup[i] = pOverriddenMethod->vTableOfs;
                                        pMap->ppMethodVLookup[i] = pOverriddenMethod;
                                    }
                                } else {
                                    Sys.Crash("Problem with interface class");
                                }
                            }
                        }
                    }
                }

                if (pTypeDef->fillState >= resolve)
                    return;
            }

            if (pTypeDef->fillState < Type.TYPE_FILL_ALL) {
                pTypeDef->fillState = Type.TYPE_FILL_ALL;

                if (pTypeDef->isGenericDefinition == 0 && pTypeDef->stackSize == 0) {
                    j = 0;
                }

                if (pParent != null && pParent->fillState < Type.TYPE_FILL_ALL) {
                    MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_ALL);
                }

                if (isDeferred != 0) {
                    Fill_ResolveDeferred();
                }
            }

            Sys.log_f(2, "Type:  %s.%s\n", (PTR)pTypeDef->nameSpace, (PTR)pTypeDef->name);
        }
    }
}

