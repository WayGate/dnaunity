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
    #if UNITY_WEBGL || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif 

    public unsafe static class System_Threading_Interlocked
    {

        public static tAsyncCall* CompareExchange_Int32(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	uint *pLoc = (*((uint**)(pParams + 0)));
        	uint value = (*((uint*)(pParams + 4)));
        	uint comparand = (*((uint*)(pParams + 8)));

        	*(uint*)pReturnValue = *pLoc;
        	if (*pLoc == comparand) {
        		*pLoc = value;
        	}

        	return null;
        }

        public static tAsyncCall* Increment_Int32(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	int *pLoc = (*((int**)(pParams + 0)));

        	(*pLoc)++;
        	*(int*)pReturnValue = *pLoc;

        	return null;
        }

        public static tAsyncCall* Decrement_Int32(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	int *pLoc = (*((int**)(pParams + 0)));

        	(*pLoc)--;
        	*(int*)pReturnValue = *pLoc;

        	return null;
        }

        public static tAsyncCall* Add_Int32(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	uint *pLoc = (*((uint**)(pParams + 0)));
        	uint value = (*((uint*)(pParams + 4)));

        	*pLoc += value;
        	*(uint*)pReturnValue = *pLoc;

        	return null;
        }

        public static tAsyncCall* Exchange_Int32(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	uint *pLoc = (*((uint**)(pParams + 0)));
        	uint value = (*((uint*)(pParams + 4)));

        	*(uint*)pReturnValue = *pLoc;
        	*pLoc = value;

        	return null;
        }

    }

}