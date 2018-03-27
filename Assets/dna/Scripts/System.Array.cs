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
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif   

    public unsafe struct tSystemArray
    {
    	// How many elements in array
    	public uint length;
    	// The elements
    	//public byte elements[0];

        public static byte* GetElements(tSystemArray* array)
        {
            return (byte*)array + 4;
        }
    };

    public unsafe class System_Array
    {
        public static byte* GetElements(void* pArray)
        {
            return (byte*)tSystemArray.GetElements((tSystemArray*)pArray);
        }

        public static uint GetLength(void* pArray)
        {
            return ((tSystemArray*)pArray)->length;
        }

        // Must return a boxed version of value-Type.types
        public static tAsyncCall* Internal_GetValue(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemArray *pArray = (tSystemArray*)pThis_;
        	tMD_TypeDef *pArrayType;
        	uint index, elementSize;
        	tMD_TypeDef *pElementType;
        	byte* pElement;

            index = (*((uint*)(pParams + 0)));
        	pArrayType = Heap.GetType(pThis_);
        	pElementType = pArrayType->pArrayElementType;
        	elementSize = pElementType->arrayElementSize;
            pElement = tSystemArray.GetElements(pArray) + elementSize * index;
        	if (pElementType->isValueType != 0) {
        		// If it's a value-type, then box it
        		/*HEAP_PTR*/byte* boxedValue;
        		if (pElementType->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE]) {
        			// Nullable type, so box specially
        			if (*(uint*)pElement != 0) {
        				// Nullable has value
        				boxedValue = Heap.AllocType(pElementType->ppClassTypeArgs[0]);
        				// Don't copy the .hasValue part
        				Mem.memcpy(boxedValue, pElement + 4, elementSize - 4);
        			} else {
        				// Nullable does not have value
        				boxedValue = null;
        			}
        		} else {
        			boxedValue = Heap.AllocType(pElementType);
        			Mem.memcpy(boxedValue, pElement, elementSize);
        		}
        		Sys.INTERNALCALL_RESULT_PTR(pReturnValue, boxedValue);
        	} else {
        		Sys.INTERNALCALL_RESULT_PTR(pReturnValue, *(byte**)pElement);
        	}

        	return null;
        }

        // Value-Type.types will be boxed
        public static tAsyncCall* Internal_SetValue(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemArray* pArray = (tSystemArray*)pThis_;
            tMD_TypeDef* pArrayType, pObjType;
        	uint index, elementSize;
        	/*HEAP_PTR*/byte* obj;
        	tMD_TypeDef* pElementType;
        	byte* pElement;

        	pArrayType = Heap.GetType(pThis_);
            obj = (*((byte**)(pParams + 0)));
        	pObjType = Heap.GetType(obj);
        	pElementType = pArrayType->pArrayElementType;
        	// Check to see if the Type is ok to put in the array
        	if (!(Type.IsAssignableFrom(pElementType, pObjType) != 0 ||
        		(pElementType->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE] &&
        		pElementType->ppClassTypeArgs[0] == pObjType))) {
        		// Can't be done
        		Sys.INTERNALCALL_RESULT_U32(pReturnValue, 0);
        		return null;
        	}

            index = (*((uint*)(pParams + Sys.S_PTR)));

        #if WIN32 && _DEBUG
        	// Do a bounds-check
        	if (index >= pArray->length) {
        		printf("[Array] Internal_SetValue() Bounds-check failed\n");
        		__debugbreak();
        	}
        #endif

        	elementSize = pElementType->arrayElementSize;
            pElement = tSystemArray.GetElements(pArray) + elementSize * index;
        	if (pElementType->isValueType != 0) {
        		if (pElementType->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE]) {
        			// Nullable type, so treat specially
        			if (obj == null) {
        				Mem.memset(pElement, 0, elementSize);
        			} else {
        				*(uint*)pElement = 1;
        				Mem.memcpy(pElement + 4, obj, elementSize - 4);
        			}
        		} else {
        			// Get the value out of the box
        			Mem.memcpy(pElement, obj, elementSize);
        		}
        	} else {
        		// This must be a reference type, so it must be 32-bits wide
        		*(/*HEAP_PTR*/byte**)pElement = obj;
        	}
        	Sys.INTERNALCALL_RESULT_U32(pReturnValue, 1);

        	return null;
        }

        public static tAsyncCall* Clear(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemArray* pArray;
        	uint index, length, elementSize;
        	tMD_TypeDef* pArrayType;
            byte* pElements;

            pArray = (*((tSystemArray**)(pParams + 0)));
            index = (*((uint*)(pParams + Sys.S_PTR)));
            length = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT32)));
        	pArrayType = Heap.GetType((/*HEAP_PTR*/byte*)pArray);
        	elementSize = pArrayType->pArrayElementType->arrayElementSize;
            pElements = tSystemArray.GetElements(pArray);
            Mem.memset(pElements + index * elementSize, 0, (SIZE_T)(length * elementSize));

        	return null;
        }

        public static tAsyncCall* Internal_Copy(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemArray* pSrc, pDst;
        	tMD_TypeDef* pSrcType, pDstType, pSrcElementType;
            byte* pSrcElements, pDstElements;

            pSrc = (*((tSystemArray**)(pParams + 0)));
            pDst = (*((tSystemArray**)(pParams + Sys.S_PTR + Sys.S_INT32)));
        	
        	// Check if we can do a fast-copy with these two arrays
        	pSrcType = Heap.GetType((/*HEAP_PTR*/byte*)pSrc);
        	pDstType = Heap.GetType((/*HEAP_PTR*/byte*)pDst);
        	pSrcElementType = pSrcType->pArrayElementType;
        	if (Type.IsAssignableFrom(pDstType->pArrayElementType, pSrcElementType) != 0) {
        		// Can do fast-copy
        		uint srcIndex, dstIndex, length, elementSize;

                srcIndex = (*((uint*)(pParams + Sys.S_PTR)));
                dstIndex = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT32 + Sys.S_PTR)));
                length = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT32 + Sys.S_PTR + Sys.S_INT32)));

        #if WIN32 && _DEBUG
        		// Do bounds check
        		if (srcIndex + length > pSrc->length || dstIndex + length > pDst->length) {
        			printf("[Array] Internal_Copy() Bounds check failed\n");
        			__debugbreak();
        		}
        #endif

        		elementSize = pSrcElementType->arrayElementSize;

                pSrcElements = tSystemArray.GetElements(pSrc);
                pDstElements = tSystemArray.GetElements(pDst);

                Mem.memcpy(pDstElements + dstIndex * elementSize, pSrcElements + srcIndex * elementSize, (SIZE_T)(length * elementSize));

        		Sys.INTERNALCALL_RESULT_U32(pReturnValue, 1);
        	} else {
        		// Cannot do fast-copy
        		Sys.INTERNALCALL_RESULT_U32(pReturnValue, 0);
        	}

        	return null;
        }

        public static tAsyncCall* Resize(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte** ppArray_, pHeap;
        	tSystemArray* pOldArray, pNewArray;
        	uint newSize, oldSize;
        	tMD_TypeDef *pArrayTypeDef;
            byte* pOldElements, pNewElements;

            ppArray_ = (*((byte***)(pParams + 0)));
            newSize = (*((uint*)(pParams + Sys.S_PTR)));

        	pOldArray = (tSystemArray*)*ppArray_;
        	oldSize = pOldArray->length;

        	if (oldSize == newSize) {
        		// Do nothing if new length equals the current length.
        		return null;
        	}

        	pArrayTypeDef = Heap.GetType(*ppArray_);
            pHeap = (byte**)System_Array.NewVector(pArrayTypeDef, newSize);
        	pNewArray = (tSystemArray*)pHeap;
            *ppArray_ = (byte*)pHeap;
            pOldElements = tSystemArray.GetElements(pOldArray);
            pNewElements = tSystemArray.GetElements(pNewArray);
            Mem.memcpy(pNewElements, pOldElements,
                (SIZE_T)(pArrayTypeDef->pArrayElementType->arrayElementSize * ((newSize<oldSize)?newSize:oldSize)));

        	return null;
        }

        public static tAsyncCall* Reverse(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemArray* pArray;
        	uint index, length, elementSize, i, dec;
        	tMD_TypeDef* pArrayType;
        	byte* pE1, pE2;
            byte* pElements;

            pArray = (*((tSystemArray**)(pParams + 0)));
            index = (*((uint*)(pParams + Sys.S_PTR)));
            length = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT32)));

        	pArrayType = Heap.GetType((/*HEAP_PTR*/byte*)pArray);
        	elementSize = pArrayType->pArrayElementType->arrayElementSize;
        	
            pElements = tSystemArray.GetElements(pArray);
            pE1 = pElements + index * elementSize;
            pE2 = pElements + (index + length - 1) * elementSize;
        	dec = elementSize << 1;

        	while (pE2 > pE1) {
        		for (i=elementSize; i>0; i--) {
        			byte c = *pE1;
        			*pE1++ = *pE2;
        			*pE2++ = c;
        		}
        		pE2 -= dec;
        	}

        	return null;
        }

        public static /*HEAP_PTR*/byte* NewVector(tMD_TypeDef* pArrayTypeDef, uint length) 
        {
        	uint heapSize;
        	tSystemArray *pArray;

            heapSize = (uint)(sizeof(tSystemArray) + length * pArrayTypeDef->pArrayElementType->arrayElementSize);
        	pArray = (tSystemArray*)Heap.Alloc(pArrayTypeDef, heapSize);
        	pArray->length = length;
        	return (/*HEAP_PTR*/byte*)pArray;
        }

        public static /*HEAP_PTR*/byte* NewVectorOfType(tMD_TypeDef* pElementType, uint length)
        {
            uint heapSize;
            tSystemArray* pArray;

            tMD_TypeDef* pArrayType = Type.GetArrayTypeDef(pElementType, null, null);
            heapSize = (uint)(sizeof(tSystemArray) + length * pArrayType->pArrayElementType->arrayElementSize);
            pArray = (tSystemArray*)Heap.Alloc(pArrayType, heapSize);
            pArray->length = length;
            return (/*HEAP_PTR*/byte*)pArray;
        }

        public static void StoreElement(/*HEAP_PTR*/byte* pThis_, uint index, byte* value) {
        	tSystemArray *pArray = (tSystemArray*)pThis_;
        	tMD_TypeDef *pArrayTypeDef;
        	uint elemSize;
            byte* pElements;

        #if WIN32 && _DEBUG
        	// Do a bounds check
        	if (index >= pArray->length) {
        		printf("SystemArray.StoreElement() Bounds check failed. Array length: %d  index: %d\n", pArray->length, index);
        		__debugbreak();
        	}
        #endif

        	pArrayTypeDef = Heap.GetType(pThis_);
        	elemSize = pArrayTypeDef->pArrayElementType->arrayElementSize;
            pElements = tSystemArray.GetElements(pArray);
        	switch (elemSize) {
        	case 1:
                ((byte*)pElements)[index] = *(byte*)value;
        		break;
        	case 2:
                ((ushort*)pElements)[index] = *(ushort*)value;
        		break;
        	case 4:
                ((uint*)pElements)[index] = *(uint*)value;
        		break;
        	default:
                Mem.memcpy(&pElements[index * elemSize], value, (SIZE_T)elemSize);
        		break;
        	}
        }

        public static void LoadElement(/*HEAP_PTR*/byte* pThis_, uint index, byte* value) 
        {
        	tSystemArray *pArray = (tSystemArray*)pThis_;
        	tMD_TypeDef *pArrayTypeDef;
        	uint elemSize;
            byte* pElements;

        	pArrayTypeDef = Heap.GetType(pThis_);
        	elemSize = pArrayTypeDef->pArrayElementType->arrayElementSize;
            pElements = tSystemArray.GetElements(pArray);
        	switch (elemSize) {
        	case 1:
                *(byte*)value =((byte*)pElements)[index];
        		break;
        	case 2:
                *(ushort*)value = ((ushort*)pElements)[index];
        		break;
        	case 4:
                *(uint*)value = ((uint*)pElements)[index];
        		break;
        	default:
                Mem.memcpy(value, &pElements[index * elemSize], (SIZE_T)elemSize);
        		break;
        	}
        }

        public static byte* LoadElementAddress(tJITCallNative* pCallNative, /*HEAP_PTR*/byte* pThis_, uint index) 
        {
        	tSystemArray *pArray = (tSystemArray*)pThis_;
        	tMD_TypeDef *pArrayTypeDef;
            byte* pElements;

        #if WIN32 && _DEBUG
        	if (index >= pArray->length) {
        		printf("SystemArray.LoadElementAddress() Bounds check failed\n");
        		__debugbreak();
        	}
        #endif

        	pArrayTypeDef = Heap.GetType(pThis_);
            pElements = tSystemArray.GetElements(pArray);
            return pElements + pArrayTypeDef->pArrayElementType->arrayElementSize * index;
        }

        public static uint GetNumBytes(tJITCallNative* pCallNative, /*HEAP_PTR*/byte* pThis_, tMD_TypeDef *pElementType) 
        {
            return (uint)((((tSystemArray*)pThis_)->length * pElementType->arrayElementSize) + sizeof(tSystemArray));
        }

        public static tAsyncCall* CreateInstance(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            tRuntimeType* pRuntimeType = (*((tRuntimeType**)(pParams + 0)));
            tMD_TypeDef *pElementType = System_RuntimeType.DeRef((byte*)pRuntimeType);
        	tMD_TypeDef *pArrayType = Type.GetArrayTypeDef(pElementType, null, null);
            uint length = (*((uint*)(pParams + Sys.S_PTR)));
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, System_Array.NewVector(pArrayType, length));
        	return null;
        }

    }
}
