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

    public unsafe static class System_Enum
    {

        public static tAsyncCall* Internal_GetValue(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	*(uint*)pReturnValue = *(uint*)pThis_;

        	return null;
        }

        public static tAsyncCall* Internal_GetInfo(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tMD_TypeDef *pEnumType = System_RuntimeType.DeRef((byte*)((tMD_TypeDef**)pParams)[0]);
        	uint i, retIndex;
        	/*HEAP_PTR*/byte* names, values;

        	// An enum type always has just one non-literal field, with all other fields being the values.
        	names = System_Array.NewVector(pCallNative, Type.types[Type.TYPE_SYSTEM_ARRAY_STRING], pEnumType->numFields - 1);
        	values = System_Array.NewVector(pCallNative, Type.types[Type.TYPE_SYSTEM_ARRAY_INT32], pEnumType->numFields - 1);
        	
        	for (i=0, retIndex=0; i<pEnumType->numFields; i++) {
        		tMD_FieldDef *pField = pEnumType->ppFields[i];
        		tSystemString* name;
        		int value;

        		if (!MetaData.FIELD_ISLITERAL(pField)) {
        			continue;
        		}

        		name = System_String.FromCharPtrASCII(pField->name);
        		System_Array.StoreElement(names, retIndex, (byte*)&name);
        		MetaData.GetConstant(pField->pMetaData, pField->tableIndex, (byte*)&value);
        		System_Array.StoreElement(values, retIndex, (byte*)&value);
        		retIndex++;
        	}

        	*(((/*HEAP_PTR*/byte***)pParams)[1]) = names;
        	*(((/*HEAP_PTR*/byte***)pParams)[2]) = values;

        	return null;
        }

    }

}