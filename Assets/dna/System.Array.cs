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

#include "Compat.h"
#include "Sys.h"

#include "System.Array.h"
#include "System.RuntimeType.h"

#include "Types.h"
#include "MetaData.h"
#include "Heap.h"
#include "Type.h"

typedef struct tSystemArray_ tSystemArray;
struct tSystemArray_ {
	// How many elements in array
	U32 length;
	// The elements
	U8 elements[0];
};

// Must return a boxed version of value-types
tAsyncCall* System_Array_Internal_GetValue(PTR pThis_, PTR pParams, PTR pReturnValue) {
	tSystemArray *pArray = (tSystemArray*)pThis_;
	tMD_TypeDef *pArrayType;
	U32 index, elementSize;
	tMD_TypeDef *pElementType;
	PTR pElement;

    index = INTERNALCALL_PARAM(0, U32);
	pArrayType = Heap_GetType(pThis_);
	pElementType = pArrayType->pArrayElementType;
	elementSize = pElementType->arrayElementSize;
	pElement = pArray->elements + elementSize * index;
	if (pElementType->isValueType) {
		// If it's a value-type, then box it
		HEAP_PTR boxedValue;
		if (pElementType->pGenericDefinition == types[TYPE_SYSTEM_NULLABLE]) {
			// Nullable type, so box specially
			if (*(U32*)pElement) {
				// Nullable has value
				boxedValue = Heap_AllocType(pElementType->ppClassTypeArgs[0]);
				// Don't copy the .hasValue part
				memcpy(boxedValue, pElement + 4, elementSize - 4);
			} else {
				// Nullable does not have value
				boxedValue = NULL;
			}
		} else {
			boxedValue = Heap_AllocType(pElementType);
			memcpy(boxedValue, pElement, elementSize);
		}
		INTERNALCALL_RETURN_PTR(boxedValue);
	} else {
		INTERNALCALL_RETURN_PTR(*(PTR*)pElement);
	}

	return NULL;
}

// Value-types will be boxed
tAsyncCall* System_Array_Internal_SetValue(PTR pThis_, PTR pParams, PTR pReturnValue) {
	tSystemArray *pArray = (tSystemArray*)pThis_;
	tMD_TypeDef *pArrayType, *pObjType;
	U32 index, elementSize;
	HEAP_PTR obj;
	tMD_TypeDef *pElementType;
	PTR pElement;

	pArrayType = Heap_GetType(pThis_);
	obj = INTERNALCALL_PARAM(0, HEAP_PTR);
	pObjType = Heap_GetType(obj);
	pElementType = pArrayType->pArrayElementType;
	// Check to see if the Type is ok to put in the array
	if (!(Type_IsAssignableFrom(pElementType, pObjType) ||
		(pElementType->pGenericDefinition == types[TYPE_SYSTEM_NULLABLE] &&
		pElementType->ppClassTypeArgs[0] == pObjType))) {
		// Can't be done
		INTERNALCALL_RETURN_U32(0);
		return NULL;
	}

    index = INTERNALCALL_PARAM(S_PTR, U32);

#if defined(WIN32) && defined(_DEBUG)
	// Do a bounds-check
	if (index >= pArray->length) {
		printf("[Array] Internal_SetValue() Bounds-check failed\n");
		__debugbreak();
	}
#endif

	elementSize = pElementType->arrayElementSize;
	pElement = pArray->elements + elementSize * index;
	if (pElementType->isValueType) {
		if (pElementType->pGenericDefinition == types[TYPE_SYSTEM_NULLABLE]) {
			// Nullable type, so treat specially
			if (obj == NULL) {
				memset(pElement, 0, elementSize);
			} else {
				*(U32*)pElement = 1;
				memcpy(pElement + 4, obj, elementSize - 4);
			}
		} else {
			// Get the value out of the box
			memcpy(pElement, obj, elementSize);
		}
	} else {
		// This must be a reference type, so it must be 32-bits wide
		*(HEAP_PTR*)pElement = obj;
	}
	INTERNALCALL_RETURN_U32(1);

	return NULL;
}

tAsyncCall* System_Array_Clear(PTR pThis_, PTR pParams, PTR pReturnValue) {
	tSystemArray *pArray;
	U32 index, length, elementSize;
	tMD_TypeDef *pArrayType;

	pArray = INTERNALCALL_PARAM(0, tSystemArray*);
	index = INTERNALCALL_PARAM(S_PTR, U32);
	length = INTERNALCALL_PARAM(S_PTR + S_INT, U32);
	pArrayType = Heap_GetType((HEAP_PTR)pArray);
	elementSize = pArrayType->pArrayElementType->arrayElementSize;
	memset(pArray->elements + index * elementSize, 0, length * elementSize);

	return NULL;
}

tAsyncCall* System_Array_Internal_Copy(PTR pThis_, PTR pParams, PTR pReturnValue) {
	tSystemArray *pSrc, *pDst;
	tMD_TypeDef *pSrcType, *pDstType, *pSrcElementType;

	pSrc = INTERNALCALL_PARAM(0, tSystemArray*);
	pDst = INTERNALCALL_PARAM(S_PTR + S_INT, tSystemArray*);
	
	// Check if we can do a fast-copy with these two arrays
	pSrcType = Heap_GetType((HEAP_PTR)pSrc);
	pDstType = Heap_GetType((HEAP_PTR)pDst);
	pSrcElementType = pSrcType->pArrayElementType;
	if (Type_IsAssignableFrom(pDstType->pArrayElementType, pSrcElementType)) {
		// Can do fast-copy
		U32 srcIndex, dstIndex, length, elementSize;

		srcIndex = INTERNALCALL_PARAM(S_PTR, U32);
		dstIndex = INTERNALCALL_PARAM(S_PTR + S_INT + S_PTR, U32);
		length = INTERNALCALL_PARAM(S_PTR + S_INT + S_PTR + S_INT, U32);

#if defined(WIN32) && defined(_DEBUG)
		// Do bounds check
		if (srcIndex + length > pSrc->length || dstIndex + length > pDst->length) {
			printf("[Array] Internal_Copy() Bounds check failed\n");
			__debugbreak();
		}
#endif

		elementSize = pSrcElementType->arrayElementSize;

		memcpy(pDst->elements + dstIndex * elementSize, pSrc->elements + srcIndex * elementSize, length * elementSize);

		INTERNALCALL_RETURN_U32(1);
	} else {
		// Cannot do fast-copy
		INTERNALCALL_RETURN_U32(0);
	}

	return NULL;
}

tAsyncCall* System_Array_Resize(PTR pThis_, PTR pParams, PTR pReturnValue) {
	HEAP_PTR* ppArray_, pHeap;
	tSystemArray *pOldArray, *pNewArray;
	U32 newSize, oldSize;
	tMD_TypeDef *pArrayTypeDef;

    ppArray_ = INTERNALCALL_PARAM(0, HEAP_PTR*);
	newSize = INTERNALCALL_PARAM(S_PTR, U32);

	pOldArray = (tSystemArray*)*ppArray_;
	oldSize = pOldArray->length;

	if (oldSize == newSize) {
		// Do nothing if new length equals the current length.
		return NULL;
	}

	pArrayTypeDef = Heap_GetType(*ppArray_);
	pHeap = SystemArray_NewVector(pArrayTypeDef, newSize);
	pNewArray = (tSystemArray*)pHeap;
	*ppArray_ = pHeap;
	memcpy(pNewArray->elements, pOldArray->elements,
		pArrayTypeDef->pArrayElementType->arrayElementSize * ((newSize<oldSize)?newSize:oldSize));

	return NULL;
}

tAsyncCall* System_Array_Reverse(PTR pThis_, PTR pParams, PTR pReturnValue) {
	tSystemArray *pArray;
	U32 index, length, elementSize, i, dec;
	tMD_TypeDef *pArrayType;
	U8 *pE1, *pE2;

	pArray = INTERNALCALL_PARAM(0, tSystemArray*);
	index = INTERNALCALL_PARAM(S_PTR, U32);
	length = INTERNALCALL_PARAM(S_PTR + S_INT, U32);

	pArrayType = Heap_GetType((HEAP_PTR)pArray);
	elementSize = pArrayType->pArrayElementType->arrayElementSize;
	
	pE1 = pArray->elements + index * elementSize;
	pE2 = pArray->elements + (index + length - 1) * elementSize;
	dec = elementSize << 1;

	while (pE2 > pE1) {
		for (i=elementSize; i>0; i--) {
			U8 c = *pE1;
			*pE1++ = *pE2;
			*pE2++ = c;
		}
		pE2 -= dec;
	}

	return NULL;
}

HEAP_PTR SystemArray_NewVector(tMD_TypeDef *pArrayTypeDef, U32 length) {
	U32 heapSize;
	tSystemArray *pArray;

	heapSize = sizeof(tSystemArray) + length * pArrayTypeDef->pArrayElementType->arrayElementSize;
	pArray = (tSystemArray*)Heap_Alloc(pArrayTypeDef, heapSize);
	pArray->length = length;
	return (HEAP_PTR)pArray;
}

void SystemArray_StoreElement(HEAP_PTR pThis_, U32 index, PTR value) {
	tSystemArray *pArray = (tSystemArray*)pThis_;
	tMD_TypeDef *pArrayTypeDef;
	U32 elemSize;

#if defined(WIN32) && defined(_DEBUG)
	// Do a bounds check
	if (index >= pArray->length) {
		printf("SystemArray_StoreElement() Bounds check failed. Array length: %d  index: %d\n", pArray->length, index);
		__debugbreak();
	}
#endif

	pArrayTypeDef = Heap_GetType(pThis_);
	elemSize = pArrayTypeDef->pArrayElementType->arrayElementSize;
	switch (elemSize) {
	case 1:
		((U8*)(pArray->elements))[index] = *(U8*)value;
		break;
	case 2:
		((U16*)(pArray->elements))[index] = *(U16*)value;
		break;
	case 4:
		((U32*)(pArray->elements))[index] = *(U32*)value;
		break;
	default:
		memcpy(&pArray->elements[index * elemSize], value, elemSize);
		break;
	}
}

void SystemArray_LoadElement(HEAP_PTR pThis_, U32 index, PTR value) {
	tSystemArray *pArray = (tSystemArray*)pThis_;
	tMD_TypeDef *pArrayTypeDef;
	U32 elemSize;

	pArrayTypeDef = Heap_GetType(pThis_);
	elemSize = pArrayTypeDef->pArrayElementType->arrayElementSize;
	switch (elemSize) {
	case 1:
		*(U8*)value =((U8*)(pArray->elements))[index];
		break;
	case 2:
		*(U16*)value = ((U16*)(pArray->elements))[index];
		break;
	case 4:
		*(U32*)value = ((U32*)(pArray->elements))[index];
		break;
	default:
		memcpy(value, &pArray->elements[index * elemSize], elemSize);
		break;
	}
}

PTR SystemArray_LoadElementAddress(HEAP_PTR pThis_, U32 index) {
	tSystemArray *pArray = (tSystemArray*)pThis_;
	tMD_TypeDef *pArrayTypeDef;

#if defined(WIN32) && defined(_DEBUG)
	if (index >= pArray->length) {
		printf("SystemArray_LoadElementAddress() Bounds check failed\n");
		__debugbreak();
	}
#endif

	pArrayTypeDef = Heap_GetType(pThis_);
	return pArray->elements + pArrayTypeDef->pArrayElementType->arrayElementSize * index;
}

U32 SystemArray_GetNumBytes(HEAP_PTR pThis_, tMD_TypeDef *pElementType) {
	return (((tSystemArray*)pThis_)->length * pElementType->arrayElementSize) + sizeof(tSystemArray);
}

tAsyncCall* System_Array_CreateInstance(PTR pThis_, PTR pParams, PTR pReturnValue) {
	tMD_TypeDef *pElementType = RuntimeType_DeRef(INTERNALCALL_PARAM(0, tRuntimeType*));
	tMD_TypeDef *pArrayType = Type_GetArrayTypeDef(pElementType, NULL, NULL);
	U32 length = INTERNALCALL_PARAM(S_PTR, U32);
	INTERNALCALL_RETURN_PTR(SystemArray_NewVector(pArrayType, length));
	return NULL;
}
