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

    public unsafe static class System_Console
    {
        public static tAsyncCall* Write(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	byte* /*HEAP_PTR*/ _string;
            char* wStr;
        	uint strLen;

        	_string = *(byte**)pParams;
        	if (_string != null) {
                wStr = System_String.GetString(_string, &strLen);
                string netStr = System.Runtime.InteropServices.Marshal.PtrToStringUni((System.IntPtr)wStr, (int)strLen);
                #if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL || UNITY_STANDALONE
                UnityEngine.Debug.Log(netStr);
                #else
                System.Console.Write(netStr);
                #endif
        	}

        	return null;
        }

        static uint Internal_ReadKey_Check(byte* pThis_, byte* pParams, byte* pReturnValue, tAsyncCall *pAsync) 
        {
            throw new System.NotImplementedException();
        }

//        static uint nextKeybC = 0xFFFFFFFF;

        public static tAsyncCall* Internal_ReadKey(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            tAsyncCall *pAsync = (tAsyncCall*)Mem.malloc((SIZE_T)sizeof(tAsyncCall));

        	pAsync->sleepTime = -1;
            pAsync->checkFn = new H(Internal_ReadKey_Check);
        	pAsync->state = null;

        	return pAsync;
        }

        public static tAsyncCall* Internal_KeyAvailable(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	uint c, isKey;

        	isKey = Internal_ReadKey_Check(null, null, (byte*)&c, null);
        	if (isKey != 0) {
//        		nextKeybC = c;
        		*(uint*)pReturnValue = 1;
        	} else {
        		*(uint*)pReturnValue = 0;
        	}

        	return null;
        }

    }
}