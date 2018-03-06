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

    public unsafe static class System_ValueType
    {

        // Get all the fields in the value-Type.types in the parameters.
        // If the 2nd parameter is null, then don't include it!
        // The type of the objects will always be identical.
        public static tAsyncCall* GetFields(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* o1,o2, ret;
        	tMD_TypeDef *pType;
        	//tMetaData *pMetaData;
        	uint i, retOfs, numInstanceFields;

        	o1 = ((/*HEAP_PTR*/byte**)pParams)[0];
        	o2 = ((/*HEAP_PTR*/byte**)pParams)[1];
        	pType = Heap.GetType(o1);
        	//pMetaData = pType->pMetaData;

        	numInstanceFields = 0;
        	for (i=0; i<pType->numFields; i++) {
        		if (!MetaData.FIELD_ISSTATIC(pType->ppFields[i])) {
        			numInstanceFields++;
        		}
        	}

        	ret = System_Array.NewVector(pCallNative, Type.types[Type.TYPE_SYSTEM_ARRAY_OBJECT], numInstanceFields << ((o2 == null)?0:1));

        	retOfs = 0;
        	for (i=0; i<pType->numFields; i++) {
        		tMD_FieldDef *pField;

        		pField = pType->ppFields[i];
        		if (!MetaData.FIELD_ISSTATIC(pField)) {
        			if (pField->pType->isValueType != 0) {
        				/*HEAP_PTR*/byte* boxed;

        				boxed = Heap.Box(pField->pType, o1 + pField->memOffset);
        				System_Array.StoreElement(ret, retOfs++, (byte*)&boxed);
        				if (o2 != null) {
        					boxed = Heap.Box(pField->pType, o2 + pField->memOffset);
        					System_Array.StoreElement(ret, retOfs++, (byte*)&boxed);
        				}
        			} else {
        				System_Array.StoreElement(ret, retOfs++, o1 + pField->memOffset);
        				if (o2 != null) {
        					System_Array.StoreElement(ret, retOfs++, o2 + pField->memOffset);
        				}
        			}
        		}
        	}

        	*(/*HEAP_PTR*/byte**)pReturnValue = ret;

        	return null;
        }

    }

}
