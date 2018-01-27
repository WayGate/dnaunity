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

using System.Runtime.InteropServices;

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif   

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tGenericInstance 
    {
        // The tMD_TypeDef for this instance of this generic type
        public tMD_TypeDef *pInstanceTypeDef;

        // The next instantiation of this generic type
        public tGenericInstance *pNext;

        // The number of type arguments for this instance
        public uint numTypeArgs;
        // The type arguments for this instantiation
        public tMD_TypeDef *pTypeArgs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tGenericMethodInstance
    {
        // This instance method.
        public tMD_MethodDef *pInstanceMethodDef;

        // The next instantiation of this generic method
        public tGenericMethodInstance *pNext;

        // The number of type arguments for this instance
        public uint numTypeArgs;
        // The method type arguments for this instance
        public tMD_TypeDef *pTypeArgs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe static class Generics
    {
        static byte* scBracket, scComma, scNsDotName, scGenericName, scQuestion, scEndBracket;

        public static void Init()
        {
            scBracket = scComma = scNsDotName = scGenericName = scQuestion = scEndBracket = null;
        }

        public static void GetHeapRoots(tHeapRoots *pHeapRoots, tMD_TypeDef *pTypeDef) 
        {
        	tGenericInstance *pInst = pTypeDef->pGenericInstances;
        	while (pInst != null) {
                tMD_TypeDef *pInstTypeDef = pInst->pInstanceTypeDef;
        		if (pInstTypeDef->staticFieldSize > 0) {
        			Heap.SetRoots(pHeapRoots, pInstTypeDef->pStaticFields, pInstTypeDef->staticFieldSize);
        		}
        		pInst = pInst->pNext;
        	}
        }

        public static tMD_TypeDef* GetGenericTypeFromSig(tMetaData *pMetaData, /*SIG*/byte* *pSig, 
            tMD_TypeDef **ppCallingClassTypeArgs, tMD_TypeDef **ppCallingMethodTypeArgs) 
        {
            tMD_TypeDef *pCoreType;
            tMD_TypeDef *pRet;
        	uint numTypeArgs, i;
        	tMD_TypeDef **ppTypeArgs;

        	pCoreType = Type.GetTypeFromSig(pMetaData, pSig, ppCallingClassTypeArgs, ppCallingMethodTypeArgs);
        	MetaData.Fill_TypeDef(pCoreType, ppCallingClassTypeArgs, ppCallingMethodTypeArgs); //null, null);

        	numTypeArgs = MetaData.DecodeSigEntry(pSig);
            ppTypeArgs = (tMD_TypeDef**)Mem.malloc((SIZE_T)(numTypeArgs * sizeof(tMD_TypeDef*)));
        	for (i=0; i<numTypeArgs; i++) {
        		ppTypeArgs[i] = Type.GetTypeFromSig(pMetaData, pSig, ppCallingClassTypeArgs, ppCallingMethodTypeArgs);
        		if (ppTypeArgs[i] != null) {
        			MetaData.Fill_TypeDef(ppTypeArgs[i], null, null);
        		}
        	}

        	pRet = GetGenericTypeFromCoreType(pCoreType, numTypeArgs, ppTypeArgs);
        	Mem.free(ppTypeArgs);
        	return pRet;
        }

        // TODO: This is not the most efficient way of doing this, as it has to search through all the
        // entries in the GenericParams table for all lookups. This can be improved.
        static tMD_GenericParam* FindGenericParam(tMD_TypeDef *pCoreType, uint typeArgIndex) 
        {
        	tMD_GenericParam *pGenericParam;
        	uint i;

        	pGenericParam = (tMD_GenericParam*)MetaData.GetTableRow(pCoreType->pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_GENERICPARAM, 1));

        	for (i=0; i<pCoreType->pMetaData->tables.numRows[MetaDataTable.MD_TABLE_GENERICPARAM]; i++, pGenericParam++) {
        		if (pGenericParam->owner == pCoreType->tableIndex && pGenericParam->number == typeArgIndex) {
        			return pGenericParam;
        		}
        	}
        	return null;
        }

        public static tMD_TypeDef* GetGenericTypeFromCoreType(tMD_TypeDef *pCoreType, uint numTypeArgs, tMD_TypeDef **ppTypeArgs) 
        {
        	tGenericInstance *pInst;
        	tMD_TypeDef *pTypeDef;
        	uint i;
            byte* name = stackalloc byte[2048];
        	tMetaData *pMetaData;

        	pMetaData = pCoreType->pMetaData;
        	MetaData.Fill_TypeDef(pCoreType, null, null);

        	// See if we have already built an instantiation of this type with the given type args.
        	pInst = pCoreType->pGenericInstances;
        	while (pInst != null) {
        		if (pInst->numTypeArgs == numTypeArgs &&
                    Mem.memcmp(pInst->pTypeArgs, ppTypeArgs, (SIZE_T)(numTypeArgs * sizeof(tMD_TypeDef*))) == 0) {
        			return pInst->pInstanceTypeDef;
        		}
        		pInst = pInst->pNext;
        	}

        	// This has not already been instantiated, so instantiate it now.
            pInst = (tGenericInstance*)Mem.mallocForever((SIZE_T)(sizeof(tGenericInstance) + numTypeArgs * sizeof(tMD_TypeDef*)));
        	// Insert this into the chain of instantiations.
        	pInst->pNext = pCoreType->pGenericInstances;
        	pCoreType->pGenericInstances = pInst;
        	// Copy the type args into the instantiation.
        	pInst->numTypeArgs = numTypeArgs;
            Mem.memcpy(pInst->pTypeArgs, ppTypeArgs, (SIZE_T)(numTypeArgs * sizeof(tMD_TypeDef*)));

        	// Create the new instantiated type
            pInst->pInstanceTypeDef = pTypeDef = ((tMD_TypeDef*)Mem.mallocForever((SIZE_T)sizeof(tMD_TypeDef)));
            Mem.memset(pTypeDef, 0, (SIZE_T)sizeof(tMD_TypeDef));
        	// Make the name of the instantiation.
        	S.strcpy(name, pCoreType->name);
            S.strcat(name, new S(ref scBracket, "["));
        	for (i=0; i<numTypeArgs; i++) {
        		if (i > 0) {
                    S.strcat(name, new S(ref scComma, ","));
        		}
        		if (ppTypeArgs[i] != null) {
                    S.sprintf(S.strchr(name, 0), "%s.%s", (PTR)ppTypeArgs[i]->nameSpace, (PTR)ppTypeArgs[i]->name);
        		} else {
        			tMD_GenericParam *pGenericParam = FindGenericParam(pCoreType, i);
        			if (pGenericParam != null) {
                        S.sprintf(S.strchr(name, 0), "%s", (PTR)pGenericParam->name);
        			} else {
                        S.sprintf(S.strchr(name, 0), "???");
        			}
        		}
        	}
            S.strcat(name, new S(ref scEndBracket, "]"));
        	// Fill in the basic bits of the new type def.
        	pTypeDef->pTypeDef = pTypeDef;
        	pTypeDef->pMetaData = pMetaData;
        	pTypeDef->flags = pCoreType->flags;
        	pTypeDef->pGenericDefinition = pCoreType;
        	for (i=0; i<numTypeArgs; i++) {
        		if (ppTypeArgs[i] == null) {
        			pTypeDef->isGenericDefinition = 1;
        			break;
        		}
        	}
        	pTypeDef->nameSpace = pCoreType->nameSpace;
            pTypeDef->name = (/*STRING*/byte*)Mem.mallocForever((SIZE_T)((uint)S.strlen(name)+1));
        	S.strcpy(pTypeDef->name, name);
            pTypeDef->ppClassTypeArgs = (tMD_TypeDef**)pInst->pTypeArgs;
        	pTypeDef->extends = pCoreType->extends;
        	pTypeDef->tableIndex = pCoreType->tableIndex;
        	pTypeDef->fieldList = pCoreType->fieldList;
        	pTypeDef->methodList = pCoreType->methodList;
        	pTypeDef->numFields = pCoreType->numFields;
        	pTypeDef->numMethods = pCoreType->numMethods;
        	pTypeDef->numVirtualMethods = pCoreType->numVirtualMethods;
        	pTypeDef->pNestedIn = pCoreType->pNestedIn;
        	pTypeDef->isPrimed = 1;

        	MetaData.Fill_TypeDef(pTypeDef, ppTypeArgs, null);

        	return pTypeDef;
        }

        public static tMD_MethodDef* GetMethodDefFromSpec(tMD_MethodSpec *pMethodSpec, 
            tMD_TypeDef **ppCallingClassTypeArgs, tMD_TypeDef **ppCallingMethodTypeArgs) 
        {

            tMD_MethodDef *pCoreMethod;
            tMD_MethodDef *pMethod;
        	/*SIG*/byte* sig;
        	uint argCount, i;
        	tMD_TypeDef **ppTypeArgs;

        	pCoreMethod = MetaData.GetMethodDefFromDefRefOrSpec(pMethodSpec->pMetaData, pMethodSpec->method, null, null);//ppCallingClassTypeArgs, ppCallingMethodTypeArgs);

        	//ppClassTypeArgs = pCoreMethod->pParentType->ppClassTypeArgs;
        	sig = MetaData.GetBlob(pMethodSpec->instantiation, null);
        	MetaData.DecodeSigEntry(&sig); // always 0x0a
        	argCount = MetaData.DecodeSigEntry(&sig);
            ppTypeArgs = (tMD_TypeDef**)Mem.malloc((SIZE_T)(argCount * sizeof(tMD_TypeDef*)));

        	for (i=0; i<argCount; i++) {
        		tMD_TypeDef *pArgType;

        		pArgType = Type.GetTypeFromSig(pMethodSpec->pMetaData, &sig, ppCallingClassTypeArgs, ppCallingMethodTypeArgs);
        		ppTypeArgs[i] = pArgType;
        	}

        	pMethod = Generics.GetMethodDefFromCoreMethod(pCoreMethod, pCoreMethod->pParentType, argCount, ppTypeArgs);
        	Mem.free(ppTypeArgs);

        	return pMethod;
        }

        public static tMD_MethodDef* GetMethodDefFromCoreMethod(tMD_MethodDef *pCoreMethod, 
            tMD_TypeDef *pParentType, uint numTypeArgs, tMD_TypeDef **ppTypeArgs) 
        {

        	tGenericMethodInstance *pInst;
        	tMD_MethodDef *pMethod;

        	// See if we already have an instance with the given type args
        	pInst = pCoreMethod->pGenericMethodInstances;
        	while (pInst != null) {
        		if (pInst->numTypeArgs == numTypeArgs &&
                    Mem.memcmp(pInst->pTypeArgs, ppTypeArgs, (SIZE_T)(numTypeArgs * sizeof(tMD_TypeDef*))) == 0) {
        			return pInst->pInstanceMethodDef;
        		}
        		pInst = pInst->pNext;
        	}

        	// We don't have an instance so create one now.
            pInst = (tGenericMethodInstance*)Mem.mallocForever((SIZE_T)(sizeof(tGenericMethodInstance) + numTypeArgs * sizeof(tMD_TypeDef*)));
        	pInst->pNext = pCoreMethod->pGenericMethodInstances;
        	pCoreMethod->pGenericMethodInstances = pInst;
        	pInst->numTypeArgs = numTypeArgs;
            Mem.memcpy(pInst->pTypeArgs, ppTypeArgs, (SIZE_T)(numTypeArgs * sizeof(tMD_TypeDef*)));

            pInst->pInstanceMethodDef = pMethod = ((tMD_MethodDef*)Mem.mallocForever((SIZE_T)sizeof(tMD_MethodDef)));
            Mem.memset(pMethod, 0, (SIZE_T)sizeof(tMD_MethodDef));
        	pMethod->pMethodDef = pMethod;
        	pMethod->pMetaData = pCoreMethod->pMetaData;
        	pMethod->pCIL = pCoreMethod->pCIL;
        	pMethod->implFlags = pCoreMethod->implFlags;
        	pMethod->flags = pCoreMethod->flags;
        	pMethod->name = pCoreMethod->name;
        	pMethod->signature = pCoreMethod->signature;
        	pMethod->vTableOfs = pCoreMethod->vTableOfs;
            pMethod->ppMethodTypeArgs = (tMD_TypeDef**)pInst->pTypeArgs; // IS THIS RIGHT??

            MetaData.Fill_MethodDef(pParentType, pMethod, pParentType->ppClassTypeArgs, (tMD_TypeDef**)pInst->pTypeArgs /* IS THIS RIGHT?? */);

        	return pMethod;
        }

    }

}
