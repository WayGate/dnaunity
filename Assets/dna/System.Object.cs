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

    public unsafe static class System_Object
    {

        public static tAsyncCall* Equals(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (pThis_ == *(byte**)pParams) ? 1U : 0U);

        	return null;
        }

        public static tAsyncCall* Clone(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* obj, clone;

        	obj = (*((/*HEAP_PTR*/byte**)(pParams + 0)));
        	clone = Heap.Clone(obj);
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, clone);

        	return null;
        }

        public static tAsyncCall* GetHashCode(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (uint)((((uint)pThis_) >> 2) * 2654435761UL));

        	return null;
        }

        public static tAsyncCall* GetType(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* typeObject;
        	tMD_TypeDef *pTypeDef;

        	pTypeDef = Heap.GetType((/*HEAP_PTR*/byte*)pThis_);
        	typeObject = Type.GetTypeObject(pTypeDef);
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, typeObject);

        	return null;
        }
    }

}
