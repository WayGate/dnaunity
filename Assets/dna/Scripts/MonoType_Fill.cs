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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

#if UNITY_5 || UNITY_2017 || UNITY_2018
using UnityEngine;
#endif


#if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
using SIZE_T = System.UInt32;
using PTR = System.UInt32;
#else
using SIZE_T = System.UInt64;
using PTR = System.UInt64;
#endif

namespace DnaUnity
{
    public unsafe static partial class MonoType
    {

        public static void Fill_FieldDef(tMD_TypeDef* pParentType, FieldInfo fieldInfo, tMD_FieldDef* pFieldDef,
            uint memOffset, uint* pAlignment, tMD_TypeDef** ppClassTypeArgs)
        {
            tMetaData* pMetaData;
            uint fieldSize;
            uint fieldAlignment;

            if (pFieldDef->isFilled == 1) {
                return;
            }
            pFieldDef->isFilled = 1;

            pFieldDef->pParentType = pParentType;

            pFieldDef->pType = MonoType.GetTypeForMonoType(fieldInfo.FieldType, ppClassTypeArgs, null);
            if (pFieldDef->pType == null) {
                // If the field is a core generic type definition, then we can't do anything more
                return;
            }
            if (pFieldDef->pType->fillState < Type.TYPE_FILL_LAYOUT) {
                MetaData.Fill_TypeDef(pFieldDef->pType, null, null);
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
            pFieldDef->pFieldDef = pFieldDef;

            pFieldDef->monoFieldInfo = new H(fieldInfo);
            pFieldDef->monoGetter = new H(GetFieldTrampoline);
            pFieldDef->monoSetter = new H(SetFieldTrampoline);

            pMetaData = pFieldDef->pMetaData;
        }

        public static void Fill_MethodDef(tMD_TypeDef* pParentType, MethodBase methodBase, tMD_MethodDef* pMethodDef, 
            tMD_TypeDef** ppClassTypeArgs, tMD_TypeDef** ppMethodTypeArgs)
        {
            uint i, totalSize, start;

            if (pMethodDef->isFilled == 1) {
                return;
            }

            pMethodDef->pParentType = pParentType;
            pMethodDef->pMethodDef = pMethodDef;
            pMethodDef->isFilled = 1;
            pMethodDef->isGenericDefinition = (byte)(methodBase.IsGenericMethodDefinition ? 1 : 0);

            if (methodBase.IsGenericMethodDefinition) {
                // Generic definition method, so can't do any more.
                //Sys.log_f("Method<>: %s.%s.%s()\n", pParentType->nameSpace, pParentType->name, pMethodDef->name);
                return;
            }

            ParameterInfo[] paramInfos = methodBase.GetParameters();

            pMethodDef->numberOfParameters = (ushort)(paramInfos.Length + (methodBase.IsStatic ? 0 : 1));
            if (methodBase is MethodInfo) {
                pMethodDef->pReturnType = GetTypeForMonoType(((MethodInfo)methodBase).ReturnType, 
                    ppClassTypeArgs, ppMethodTypeArgs);
            } else {
                pMethodDef->pReturnType = null;
            }
            if (pMethodDef->pReturnType == Type.types[Type.TYPE_SYSTEM_VOID]) {
                pMethodDef->pReturnType = null;
            }
            if (pMethodDef->pReturnType != null && pMethodDef->pReturnType->fillState < Type.TYPE_FILL_ALL) {
                MetaData.Fill_Defer(pMethodDef->pReturnType, null, null);
            }
            pMethodDef->pParams = (tParameter*)Mem.malloc((SIZE_T)(pMethodDef->numberOfParameters * sizeof(tParameter)));
            totalSize = 0;
            start = 0;
            if (!methodBase.IsStatic) {
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
            for (i = start; i < pMethodDef->numberOfParameters; i++) {
                tMD_TypeDef* pStackTypeDef;
                tMD_TypeDef* pByRefTypeDef;
                uint size;

                // NOTE: Byref values are treated as intptr's in DNA
                System.Type paramType = paramInfos[i - start].ParameterType;
                if (paramType.IsByRef) {
                    pStackTypeDef = Type.types[Type.TYPE_SYSTEM_INTPTR];
                    pByRefTypeDef = GetTypeForMonoType(paramType.GetElementType(), 
                        ppClassTypeArgs, ppMethodTypeArgs);
                } else {
                    pStackTypeDef = GetTypeForMonoType(paramType, 
                        ppClassTypeArgs, ppMethodTypeArgs);
                    pByRefTypeDef = null;
                }

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

            if (pMethodDef->monoMethodInfo == null)
                pMethodDef->monoMethodInfo = new H(methodBase);
            if (pMethodDef->monoMethodCall == null)
                pMethodDef->monoMethodCall = new H(CallMethodTrampoline);
        }

        // Get only public methods, or public/protected if type is not sealed
        public static MethodInfo[] GetMethods(System.Type monoType)
        {
            MethodInfo[] methodInfos = monoType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            List<MethodInfo> interfaceMethods = null;

            // Make sure we include non-public methods implementing interfaces
            if (!monoType.IsInterface && !monoType.IsGenericTypeDefinition) {
                System.Type[] interfaceTypes = monoType.GetInterfaces();
                if (interfaceTypes.Length > 0) {
                    for (int i = 0; i < interfaceTypes.Length; i++) {
                        InterfaceMapping interfaceMapping = monoType.GetInterfaceMap(interfaceTypes[i]);
                        MethodInfo[] targetMethods = interfaceMapping.TargetMethods;
                        if (interfaceMethods == null)
                            interfaceMethods = new List<MethodInfo>();
                        interfaceMethods.AddRange(targetMethods);
                    }
                }
            }

            List<MethodInfo> finalInfos = new List<MethodInfo>();
            foreach (MethodInfo methodInfo in methodInfos) {
                if (methodInfo.IsPublic || (!monoType.IsSealed && methodInfo.IsFamily)) {
                    finalInfos.Add(methodInfo);
                } else if (interfaceMethods != null && interfaceMethods.Contains(methodInfo)) { 
                    finalInfos.Add(methodInfo);
                }
            }
            return finalInfos.ToArray();
        }

        // Get only public methods, or public/protected if type is not sealed
        public static ConstructorInfo[] GetConstructors(System.Type monoType)
        {
            ConstructorInfo[] constructorInfos = monoType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            List<ConstructorInfo> finalInfos = new List<ConstructorInfo>();
            foreach (ConstructorInfo constructorInfo in constructorInfos) {
                if (constructorInfo.IsPublic || (!monoType.IsSealed && constructorInfo.IsFamily)) {
                    finalInfos.Add(constructorInfo);
                } else {
                    // Also add no param constructor
                    ParameterInfo[] parms = constructorInfo.GetParameters();
                    if (parms.Length == 0)
                        finalInfos.Add(constructorInfo);
                }
            }
            return finalInfos.ToArray();
        }

        // Get only public methods, or public/protected if type is not sealed
        public static FieldInfo[] GetFields(System.Type monoType)
        {
            FieldInfo[] fieldInfos = monoType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            List<FieldInfo> finalInfos = new List<FieldInfo>();
            foreach (FieldInfo fieldInfo in fieldInfos) {
                if (fieldInfo.IsPublic || (monoType.IsValueType && !fieldInfo.IsStatic))
                    finalInfos.Add(fieldInfo);
            }
            return finalInfos.ToArray();
        }

        public static tMD_MethodDef* FindInterfaceOverriddenMethod(tMD_MethodDef* pInterfaceMethod, MethodInfo[] interfaceMethods, MethodInfo[] targetMethods)
        {
            byte* targetName = stackalloc byte[256];
            MethodInfo interfaceMethodInfo = null;
            for (int i = 0; i < interfaceMethods.Length; i++) {
                if (S.strcmp(pInterfaceMethod->name, interfaceMethods[i].Name) == 0) {
                    interfaceMethodInfo = targetMethods[i];
                    break;
                }
            }
            if (interfaceMethodInfo == null) {
                Sys.Crash("Unable to find mapped method %s", (PTR)(pInterfaceMethod->name));
            }
            S.strncpy(targetName, interfaceMethodInfo.Name, 256);
            tMD_MethodDef* pOverriddenMethod = null;
            tMD_TypeDef* pInterfaceTargetType = GetTypeForMonoType(interfaceMethodInfo.DeclaringType, null, null);
            for (int i = 0; i < pInterfaceTargetType->numMethods; i++) {
                tMD_MethodDef* pMethodDef = pInterfaceTargetType->ppMethods[i];
                MethodInfo methodInfo = H.ToObj(pMethodDef->monoMethodInfo) as MethodInfo;
                if (methodInfo == interfaceMethodInfo) {
                    return pInterfaceTargetType->ppMethods[i];
                } else if (MetaData.CompareNameAndMethodInfo(targetName, interfaceMethodInfo, pInterfaceMethod->pMetaData,
                                pInterfaceMethod->pParentType->ppClassTypeArgs, pInterfaceMethod->ppMethodTypeArgs, 
                                pMethodDef, pMethodDef->pParentType->ppClassTypeArgs, null) != 0) {
                    return pInterfaceTargetType->ppMethods[i];
                }

            }
            return null;
        }

        public static void Fill_TypeDef(tMD_TypeDef* pTypeDef, tMD_TypeDef** ppClassTypeArgs, 
            tMD_TypeDef** ppMethodTypeArgs, uint resolve = Type.TYPE_FILL_ALL)
        {
            uint instanceMemSize, staticMemSize, virtualOfs, isDeferred, i, j;
            int lastPeriod;
            tMetaData* pMetaData;
            tMD_TypeDef* pParent;
            System.Type monoType;
            tMD_FieldDef* pFieldDefs;
            tMD_MethodDef* pMethodDefs;
            FieldInfo[] fieldInfos = null;
            FieldInfo fieldInfo;
            MethodInfo[] methodInfos = null;
            ConstructorInfo[] constructorInfos = null;
            MethodBase methodBase;
            tMD_MethodDef* pMethodDef;

            if (pTypeDef->fillState >= resolve) {
                return;
            }

            if (pTypeDef->monoType == null) {
                MetaData.Fill_TypeDef(pTypeDef, ppClassTypeArgs, ppMethodTypeArgs, resolve);
                return;
            }

            //Sys.printf("FILLING TYPE: %s\n", (PTR)pTypeDef->name);

            if (MetaData.typesToFill == null) {
                MetaData.Fill_StartDefer();
                isDeferred = 1;
            } else {
                isDeferred = 0;
            }

            if (resolve < Type.TYPE_FILL_ALL) {
                MetaData.Fill_Defer(pTypeDef, ppClassTypeArgs, ppMethodTypeArgs);
            }

            MetaData.Fill_GetDeferredTypeArgs(pTypeDef, ref ppClassTypeArgs, ref ppMethodTypeArgs);

            monoType = H.ToObj(pTypeDef->monoType) as System.Type;
            pMetaData = pTypeDef->pMetaData;

            if (pTypeDef->fillState < Type.TYPE_FILL_PARENTS) {
                pTypeDef->fillState = Type.TYPE_FILL_PARENTS;

                // For Methods, we get only public if sealed, or public/protected if not sealed
                methodInfos = GetMethods(monoType);
                // For fields, we only get private fields for value types
                fieldInfos = GetFields(monoType);
                // For constructors, we get only public if sealed, or public/protected if not sealed
                constructorInfos = GetConstructors(monoType);

                pTypeDef->pTypeDef = pTypeDef;

                pTypeDef->pParent = MonoType.GetTypeForMonoType(monoType.BaseType, null, null);
                pParent = pTypeDef->pParent;

                if (pParent != null) {
                    MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_PARENTS);
                }
                pTypeDef->isValueType = (byte)(monoType.IsValueType ? 1 : 0);
                pTypeDef->alignment = 1;

                // Mark all ref types as having a base Mono Handle pointer as the first slot in their instance data.  This allows
                // the Heap system to call FREE on this Handle whenever we garbage collect mono wrapped or derived heap objects.
                pTypeDef->hasMonoBase = (byte)(monoType.IsValueType ? 0 : 1);

                // If not primed, then work out how many methods & fields there are.
                if (pTypeDef->isPrimed == 0) {
                    // Methods
                    pTypeDef->numMethods = (uint)(constructorInfos.Length + methodInfos.Length);
                    // Fields
                    pTypeDef->numFields = (uint)fieldInfos.Length;
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

                // This only needs to be done for non-generic Type.types, or for generic type that are not a definition
                // I.e. Fully instantiated generic Type.types
                if (pTypeDef->isGenericDefinition == 0) {

                    // For fields, we only get private fields for value types
                    if (fieldInfos == null)
                        fieldInfos = GetFields(monoType);

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
                    staticMemSize = 0;
                    if (pTypeDef->numFields > 0) {
                        pTypeDef->ppFields = (tMD_FieldDef**)Mem.mallocForever((SIZE_T)(pTypeDef->numFields * sizeof(tMD_FieldDef*)));
                        pFieldDefs = (tMD_FieldDef*)Mem.mallocForever((SIZE_T)(pTypeDef->numFields * sizeof(tMD_FieldDef)));
                        Mem.memset(pFieldDefs, 0, (SIZE_T)(pTypeDef->numFields * sizeof(tMD_FieldDef)));
                    } else {
                        pFieldDefs = null;
                    }
                    instanceMemSize = 0;
                    for (i = 0; i < fieldInfos.Length; i++) {

                        fieldInfo = fieldInfos[i];
                        tMD_FieldDef* pFieldDef = &pFieldDefs[i];

                        pFieldDef->name = new S(fieldInfo.Name);
                        pFieldDef->flags = (ushort)(
                            (fieldInfo.IsStatic ? MetaData.FIELDATTRIBUTES_STATIC : 0) |
                            (fieldInfo.IsLiteral ? MetaData.FIELDATTRIBUTES_LITERAL : 0)
                            );

                        if (!fieldInfo.IsStatic) {
                            if (fieldInfo.IsLiteral /*|| MetaData.FIELD_HASFIELDRVA(pFieldDef)*/) {
                                // If it's a literal, then analyse the field, but don't include it in any memory allocation
                                // If is has an RVA, then analyse the field, but don't include it in any memory allocation
                                MonoType.Fill_FieldDef(pTypeDef, fieldInfo, pFieldDef, 0, null, ppClassTypeArgs);
                            } else {
                                MonoType.Fill_FieldDef(pTypeDef, fieldInfo, pFieldDef, instanceMemSize, &(pTypeDef->alignment), ppClassTypeArgs);
                                instanceMemSize = pFieldDef->memOffset + pFieldDef->memSize;
                            }
                            pTypeDef->ppFields[i] = pFieldDef;
                        }
                    }
                    if (pTypeDef->instanceMemSize == 0) {
                        if (pTypeDef->isValueType != 0) {
                            // Our dna value types are the same size as they are in mono (hopefully!)
                            pTypeDef->instanceMemSize = (instanceMemSize + (pTypeDef->alignment - 1)) & ~(pTypeDef->alignment - 1);
                        } else {
                            // For mono reference types, the instance size is ALWAYS ptr size because we're wrapping a mono GCHandle pointer
                            pTypeDef->instanceMemSize = sizeof(PTR);
                        }
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
                    for (i = 0; i < fieldInfos.Length; i++) {

                        fieldInfo = fieldInfos[i];
                        tMD_FieldDef* pFieldDef = &pFieldDefs[i];

                        if (fieldInfo.IsStatic) {
                            if (fieldInfo.IsLiteral /*|| MetaData.FIELD_HASFIELDRVA(pFieldDef)*/) {
                                // If it's a literal, then analyse the field, but don't include it in any memory allocation
                                // If is has an RVA, then analyse the field, but don't include it in any memory allocation
                                MonoType.Fill_FieldDef(pTypeDef, fieldInfo, pFieldDef, 0, null, ppClassTypeArgs);
                            } else {
                                MonoType.Fill_FieldDef(pTypeDef, fieldInfo, pFieldDef, staticMemSize, null, ppClassTypeArgs);
                                staticMemSize += pFieldDef->memSize;
                            }
                            pTypeDef->ppFields[i] = pFieldDef;
                        }
                    }
                }

                if (pTypeDef->fillState >= resolve)
                    return;
            }

            if (pTypeDef->fillState < Type.TYPE_FILL_VTABLE) {
                pTypeDef->fillState = Type.TYPE_FILL_VTABLE;

                if (pParent != null) {
                    if (pParent->fillState < Type.TYPE_FILL_VTABLE) {
                        MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_VTABLE);
                    } else if (pParent->fillState < Type.TYPE_FILL_ALL) {
                        MetaData.Fill_Defer(pParent, null, null);
                    }
                }

                // This only needs to be done for non-generic Type.types, or for generic type that are not a definition
                // I.e. Fully instantiated generic Type.types
                if (pTypeDef->isGenericDefinition == 0) {

                    virtualOfs = (pParent != null) ? pParent->numVirtualMethods : 0;

                    // For Methods, we get only public if sealed, or public/protected if not sealed
                    if (methodInfos == null)
                        methodInfos = GetMethods(monoType);
                    // For constructors, we get only public if sealed, or public/protected if not sealed
                    if (constructorInfos == null)
                        constructorInfos = GetConstructors(monoType);

                    // Populate methods
                    pTypeDef->ppMethods = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pTypeDef->numMethods * sizeof(tMD_MethodDef*)));
                    pMethodDefs = (tMD_MethodDef*)Mem.mallocForever((SIZE_T)(pTypeDef->numMethods * sizeof(tMD_MethodDef)));
                    Mem.memset(pMethodDefs, 0, (SIZE_T)(pTypeDef->numMethods * sizeof(tMD_MethodDef)));
                    for (i = 0; i < pTypeDef->numMethods; i++) {
                        methodBase = (i < constructorInfos.Length) ?
                            (MethodBase)constructorInfos[i] : methodInfos[i - constructorInfos.Length];
                        pMethodDef = &pMethodDefs[i];

                        lastPeriod = methodBase.Name.LastIndexOf('.');
                        if (methodBase is ConstructorInfo || lastPeriod == -1) {
                            pMethodDef->name = new S(methodBase.Name);
                        } else {
                            string nameMinusExclInterfaceName = methodBase.Name.Substring(lastPeriod + 1);
                            pMethodDef->name = new S(nameMinusExclInterfaceName);
                        }

                        pMethodDef->monoMethodInfo = new H(methodBase);
                        pMethodDef->pMetaData = pMetaData;
                        pMethodDef->pParentType = pTypeDef;
                        pMethodDef->flags = (ushort)(
                            (methodBase.IsVirtual ? MetaData.METHODATTRIBUTES_VIRTUAL : 0) |
                            (methodBase.IsStatic ? MetaData.METHODATTRIBUTES_STATIC : 0));

                        // NOTE: All mono calls are considered internal calls
                        pMethodDef->implFlags = (ushort)MetaData.METHODIMPLATTRIBUTES_INTERNALCALL;
                        pTypeDef->ppMethods[i] = pMethodDef;

                        // Assign vtable slots
                        if (methodBase.IsVirtual) {
                            if (((MethodInfo)methodBase).GetBaseDefinition().DeclaringType == monoType) {
                                // Allocate a new vTable slot if method is explicitly marked as NewSlot, or
                                // this is of type Object.
                                pMethodDef->vTableOfs = virtualOfs++;
                            } else {
                                tMD_MethodDef* pVirtualOveriddenMethod;
                                pVirtualOveriddenMethod = MetaData.FindVirtualOverriddenMethod(pTypeDef->pParent, pMethodDef);
                                if (pVirtualOveriddenMethod == null) {
                                    if (pTypeDef->pParent->monoType == null) {
                                        // DNA types don't always have all base methods that Unity/Mono has.  In those
                                        // cases, just add the missing method to the VTable as a new virtual method.
                                        pMethodDef->vTableOfs = virtualOfs++;
                                    } else {
                                        Sys.Crash("Unable to find virtual override %s", (PTR)(pMethodDef->name));
                                    }
                                } else {
                                    pMethodDef->vTableOfs = pVirtualOveriddenMethod->vTableOfs;
                                }
                            }
                        } else {
                            // Dummy value - make it obvious it's not valid!
                            pMethodDef->vTableOfs = 0xffffffff;
                        }

                        pTypeDef->ppMethods[i] = pMethodDef;
                    }

                    // Create the virtual method table
                    pTypeDef->numVirtualMethods = virtualOfs;

                    // Resolve all members
                    pTypeDef->pVTable = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pTypeDef->numVirtualMethods * sizeof(tMD_MethodDef*)));
                    // Copy initial vTable from parent
                    if (pTypeDef->pParent != null) {
                        Mem.memcpy(pTypeDef->pVTable, pTypeDef->pParent->pVTable, (SIZE_T)(pTypeDef->pParent->numVirtualMethods * sizeof(tMD_MethodDef*)));
                    }
                    for (i = 0; i < pTypeDef->numMethods; i++) {
                        pMethodDef = pTypeDef->ppMethods[i];
                        methodBase = H.ToObj(pMethodDef->monoMethodInfo) as MethodBase;

                        if (methodBase.IsStatic && methodBase.Name == ".cctor") {
                            // This is a static constructor
                            pTypeDef->pStaticConstructor = pMethodDef;
                        }
                        if (methodBase.IsStatic && pTypeDef->pParent != null &&
                            methodBase.Name == "Finalize") {
                            // This is a Finalizer method, but not for Object.
                            // Delibrately miss out Object's Finalizer because it's empty and will cause every object
                            // of any type to have a Finalizer which will be terrible for performance.
                            pTypeDef->pFinalizer = pMethodDef;
                        }
                        if (methodBase.IsVirtual) {
                            if (pMethodDef->vTableOfs == 0xffffffff) {
                                Sys.Crash("Illegal vtableoffset");
                            }
                            if (pMethodDef->vTableOfs >= pTypeDef->numVirtualMethods) {
                                Sys.Crash("Illegal vtableoffset");
                            }
                            pTypeDef->pVTable[pMethodDef->vTableOfs] = pMethodDef;
                        }
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

                // This only needs to be done for non-generic Type.types, or for generic type that are not a definition
                // I.e. Fully instantiated generic Type.types
                if (pTypeDef->isGenericDefinition == 0) {

                    // Fill all method definitions for this type
                    for (i = 0; i < pTypeDef->numMethods; i++) {
                        pMethodDef = pTypeDef->ppMethods[i];
                        methodBase = H.ToObj(pMethodDef->monoMethodInfo) as MethodBase;
                        MonoType.Fill_MethodDef(pTypeDef, methodBase, pTypeDef->ppMethods[i], ppClassTypeArgs, ppMethodTypeArgs);
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

                // This only needs to be done for non-generic Type.types, or for generic type that are not a definition
                // I.e. Fully instantiated generic Type.types
                if (pTypeDef->isGenericDefinition == 0) {

                    // Map all interface method calls. This only needs to be done for Classes, not Interfaces
                    // And is not done for generic definitions.
                    if (!monoType.IsInterface) {
                        System.Type[] interfaceTypes = monoType.GetInterfaces();
                        pTypeDef->numInterfaces = (uint)interfaceTypes.Length;
                        if (interfaceTypes.Length > 0 && pTypeDef->isGenericDefinition == 0) {
                            if (pTypeDef->pInterfaceMaps == null)
                                pTypeDef->pInterfaceMaps = (tInterfaceMap*)Mem.mallocForever((SIZE_T)(pTypeDef->numInterfaces * sizeof(tInterfaceMap)));
                            for (i = 0; i < interfaceTypes.Length; i++) {
                                // Get the interface that this type implements
                                tMD_TypeDef* pInterface = MonoType.GetTypeForMonoType(interfaceTypes[i], ppClassTypeArgs, ppMethodTypeArgs);
                                Fill_TypeDef(pInterface, ppClassTypeArgs, null, Type.TYPE_FILL_VTABLE);
                                InterfaceMapping interfaceMapping = monoType.GetInterfaceMap(interfaceTypes[i]);
                                MetaData.Fill_TypeDef(pInterface, null, null);
                                tInterfaceMap* pMap = &pTypeDef->pInterfaceMaps[i];
                                pMap->pInterface = pInterface;
                                pMap->pVTableLookup = (uint*)Mem.mallocForever((SIZE_T)(pInterface->numVirtualMethods * sizeof(uint)));
                                pMap->ppMethodVLookup = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pInterface->numVirtualMethods * sizeof(tMD_MethodDef*)));
                                MethodInfo[] interfaceMethods = interfaceMapping.InterfaceMethods;
                                MethodInfo[] targetMethods = interfaceMapping.TargetMethods;
                                // Discover interface mapping for each interface method
                                for (j = 0; j < pInterface->numVirtualMethods; j++) {
                                    tMD_MethodDef* pInterfaceMethod = pInterface->pVTable[j];
                                    tMD_MethodDef* pOverriddenMethod = FindInterfaceOverriddenMethod(pInterfaceMethod, interfaceMethods, targetMethods);
                                    if (pOverriddenMethod == null) {
                                        Sys.Crash("Unable to find override method %s in type %s.%s for interface %s.%s", (PTR)(pInterfaceMethod->name), 
                                            (PTR)pTypeDef->nameSpace, (PTR)pTypeDef->name, 
                                            (PTR)pInterface->nameSpace, (PTR)pInterface->name);
                                    }
                                    pMap->pVTableLookup[j] = pOverriddenMethod->vTableOfs;
                                    pMap->ppMethodVLookup[j] = pOverriddenMethod;
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

                if (pParent != null && pParent->fillState < Type.TYPE_FILL_ALL) {
                    MetaData.Fill_TypeDef(pParent, null, null, Type.TYPE_FILL_ALL);
                }

                if (isDeferred != 0) {
                    MetaData.Fill_ResolveDeferred();
                }

            }

            Sys.log_f(2, "Mono Type:  %s.%s\n", (PTR)pTypeDef->nameSpace, (PTR)pTypeDef->name);
        }
    }
}
