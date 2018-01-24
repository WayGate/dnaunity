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

    public unsafe static class System_Threading_Monitor
    {

        static uint Internal_TryEntry_Check(byte* pThis_, byte* pParams, byte* pReturnValue, tAsyncCall *pAsync) 
        {
        	/*HEAP_PTR*/byte* pObj = ((/*HEAP_PTR*/byte**)pParams)[0];
        	int timeout = ((int*)pParams)[1];
        	uint ret = Heap.SyncTryEnter(pObj);
        	ulong now;
        	if (ret != 0) {
        		// Lock achieved, so return that we've got it, and unblock this thread
        		*(uint*)pReturnValue = 1;
        		return 1;
        	}
        	// Can't get lock - check timeout
        	if (timeout < 0) {
        		// Infinite timeout, continue to block thread
        		return 0;
        	}
        	if (timeout == 0) {
        		// Timeout is 0, so always unblock, and return failure to get lock
        		*(uint*)pReturnValue = 0;
        		return 1;
        	}
        	if (pAsync == null) {
        		// This is the first time, so it can always block thread and wait
        		return 0;
        	}
        	now = Sys.msTime();
        	if ((int)(now - pAsync->startTime) > timeout) {
        		// Lock not got, but timeout has expired, unblock thread and return no lock
        		*(uint*)pReturnValue = 0;
        		return 1;
        	}
        	// Continue waiting, timeout not yet expired
        	return 0;
        }

        public static tAsyncCall* Internal_TryEnter(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	uint ok = Internal_TryEntry_Check(pThis_, pParams, pReturnValue, null);
        	tAsyncCall *pAsync;
        	if (ok != 0) {
        		// Got lock already, so don't block thread
        		return null;
        	}
            pAsync = ((tAsyncCall*)Mem.malloc((SIZE_T)(sizeof(tAsyncCall))));
        	pAsync->sleepTime = -1;
            pAsync->checkFn = new H(Internal_TryEntry_Check);
        	pAsync->state = null;
        	return pAsync;
        }

        public static tAsyncCall* Internal_Exit(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* pObj = ((/*HEAP_PTR*/byte**)pParams)[0];
        	Heap.SyncExit(pObj);
            return Thread.ASYNC_LOCK_EXIT();
        }

    }

}