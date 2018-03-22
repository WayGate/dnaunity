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

namespace DnaUnity
{

    public unsafe struct tRuntimeType 
    {
        // The pointer to the TypeDef object of this type.
        public tMD_TypeDef *pTypeDef;
    }

    public unsafe static class System_RuntimeType
    {

        public static /*HEAP_PTR*/byte* New(tMD_TypeDef *pTypeDef)
        {
        	tRuntimeType *pRuntimeType;

        	pRuntimeType = (tRuntimeType*)Heap.AllocType(Type.types[Type.TYPE_SYSTEM_RUNTIMETYPE]);
        	Heap.MakeUndeletable((/*HEAP_PTR*/byte*)pRuntimeType);
        	pRuntimeType->pTypeDef = pTypeDef;

        	return (/*HEAP_PTR*/byte*)pRuntimeType;
        }

        public static tAsyncCall* get_Name(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	tSystemString* strResult;

        	strResult = System_String.FromCharPtrASCII(pRuntimeType->pTypeDef->name);
        	*(tSystemString**)pReturnValue = strResult;

        	return null;
        }

        public static tAsyncCall* get_Namespace(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	tSystemString* strResult;

        	strResult = System_String.FromCharPtrASCII(pRuntimeType->pTypeDef->nameSpace);
        	*(tSystemString**)pReturnValue = strResult;

        	return null;
        }

        public static tAsyncCall* GetNestingParentType(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	tMD_TypeDef *pNestingParentType;
        	
        	pNestingParentType = pRuntimeType->pTypeDef->pNestedIn;
        	if (pNestingParentType == null) {
        		*(/*HEAP_PTR*/byte**)pReturnValue = null;
        	} else {
        		*(/*HEAP_PTR*/byte**)pReturnValue = Type.GetTypeObject(pNestingParentType);
        	}

        	return null;
        }

        public static tAsyncCall* get_BaseType(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	tMD_TypeDef *pBaseType = pRuntimeType->pTypeDef->pParent;

        	if (pBaseType == null) {
        		*(/*HEAP_PTR*/byte**)pReturnValue = null;
        	} else {
        		*(/*HEAP_PTR*/byte**)pReturnValue = Type.GetTypeObject(pBaseType);
        	}

        	return null;
        }

        public static tAsyncCall* get_IsEnum(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tMD_TypeDef *pType = ((tRuntimeType*)pThis_)->pTypeDef;

            uint isEnum = (pType->pParent == Type.types[Type.TYPE_SYSTEM_ENUM]) ? (uint)1 : (uint)0;
        	*(uint*)pReturnValue = isEnum;

        	return null;
        }

        public static tAsyncCall* get_IsGenericType(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tMD_TypeDef *pType = ((tRuntimeType*)pThis_)->pTypeDef;

            *(uint*)pReturnValue = (MetaData.TYPE_ISGENERICINSTANCE(pType) || pType->isGenericDefinition != 0) ? (uint)1 : (uint)0;
        	return null;
        }

        public static tAsyncCall* Internal_GetGenericTypeDefinition(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tMD_TypeDef *pType = ((tRuntimeType*)pThis_)->pTypeDef;

        	if (MetaData.TYPE_ISGENERICINSTANCE(pType)) {
        		pType = pType->pGenericDefinition;
        	}

        	*(/*HEAP_PTR*/byte**)pReturnValue = Type.GetTypeObject(pType);

        	return null;
        }

        public static tAsyncCall* GetGenericArguments(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tMD_TypeDef *pType = ((tRuntimeType*)pThis_)->pTypeDef;
        	tMD_TypeDef *pCoreType;
        	uint i, argCount = 0;
        	/*HEAP_PTR*/byte* ret;

        	pCoreType = pType->pGenericDefinition;
        	if (pCoreType != null) {
        		// Find the core instantiation of this type
        		tGenericInstance *pInst = pCoreType->pGenericInstances;
        		while (pInst != null) {
        			if (pInst->pInstanceTypeDef == pType) {
        				// Found it!
        				argCount = pInst->numTypeArgs;
        			}
        			pInst = pInst->pNext;
        		}
        	}

        	ret = System_Array.NewVector(pCallNative, Type.types[Type.TYPE_SYSTEM_ARRAY_TYPE], argCount);
        	// Allocate to return value straight away, so it cannot be GCed
        	*(/*HEAP_PTR*/byte**)pReturnValue = ret;

        	for (i=0; i<argCount; i++) {
        		/*HEAP_PTR*/byte* argType = Type.GetTypeObject(pType->ppClassTypeArgs[i]);
        		System_Array.StoreElement(ret, i, (byte*)&argType);
        	}

        	return null;
        }

        public static tMD_TypeDef* DeRef(byte* type)
        {
        	return ((tRuntimeType*)type)->pTypeDef;
        }

        public static tAsyncCall* GetElementType(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	tMD_TypeDef *pType = ((tRuntimeType*)pThis_)->pTypeDef;
        	tMD_TypeDef *pElementTypeDef = pType->pArrayElementType;

        	if (pElementTypeDef != null) {
        		*(/*HEAP_PTR*/byte**)pReturnValue = Type.GetTypeObject(pElementTypeDef);
        	} else {
        		*(/*HEAP_PTR*/byte**)pReturnValue = null;
        	}

        	return null;
        }

    }  
}
