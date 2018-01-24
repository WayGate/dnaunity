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

    public unsafe static class SystemThreadingThread
    {

        public static tAsyncCall* ctor(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tThread *pThread = Thread.New();
        	pThread->startDelegate = ((byte**)pParams)[0];
        	*(/*HEAP_PTR*/byte**)pReturnValue = (/*HEAP_PTR*/byte*)pThread;
        	return null;
        }

        public static tAsyncCall* ctorParam(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tThread *pThread = Thread.New();
        	pThread->startDelegate = ((byte**)pParams)[0];
        	*(/*HEAP_PTR*/byte**)pReturnValue = (/*HEAP_PTR*/byte*)pThread;
        	pThread->hasParam = 1;
        	return null;
        }

        public static tAsyncCall* Start(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tThread *pThread = (tThread*)pThis_;
        	tMD_MethodDef *pStartMethod;
        	/*HEAP_PTR*/byte* pStartObj;
            byte** _params = stackalloc byte*[2];
        	uint paramBytes = 0;

        	// This selects the RUNNING state (=0), without changing the IsBackground bit
        	pThread->state &= Thread.THREADSTATE_BACKGROUND;

        	pStartMethod = Delegate.GetMethodAndStore(pThread->startDelegate, &pStartObj, null);

        	if (pStartObj != null) {
        		// If this method is not static, so it has a start object, then make it the first parameter
        		_params[0] = (byte*)pStartObj;
                paramBytes = (uint)sizeof(void*);
        	}
        	if (pThread->hasParam != 0) {
        		// If this method has an object parameter (ParameterizedThreadStart)
        		_params[paramBytes] = (byte*)pThread->param;
                paramBytes += (uint)sizeof(void*);
        	}

        	Thread.SetEntryPoint(pThread, pStartMethod->pMetaData, pStartMethod->tableIndex, (byte*)&_params, paramBytes);

        	return null;
        }

        public static tAsyncCall* Sleep(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            tAsyncCall *pAsync = ((tAsyncCall*)Mem.malloc((SIZE_T)sizeof(tAsyncCall)));

        	pAsync->sleepTime = ((int*)pParams)[0];

        	return pAsync;
        }

        public static tAsyncCall* get_CurrentThread(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tThread *pThread = Thread.GetCurrent();
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pThread);

        	return null;
        }

    }

}
