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

    public unsafe static class System_Runtime_CompilerServices
    {

        public static tAsyncCall* InitializeArray(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* pArray;
        	byte* pRawData;
        	tMD_TypeDef *pArrayTypeDef;
        	byte* pElements;
        	uint arrayLength;

        	pArray = (*((/*HEAP_PTR*/byte**)(pParams + 0)));
        	pRawData = (*((byte**)(pParams + Sys.S_PTR)));
        	pArrayTypeDef = Heap.GetType(pArray);
        	arrayLength = System_Array.GetLength(pArray);
        	pElements = System_Array.GetElements(pArray);
        	Mem.memcpy(pElements, pRawData, pArrayTypeDef->pArrayElementType->arrayElementSize * arrayLength);

        	return null;
        }
    }
}
