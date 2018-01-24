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

using System.Runtime.InteropServices;

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif    

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMethodState
    {
        // This method's meta-data
        public tMetaData *pMetaData;
        // The method to execute
        public tMD_MethodDef *pMethod;
        // The JITted code that this method-state is using.
        // When using the combined opcode JITter, this can vary between unoptimized and optimized.
        public tJITted *pJIT;
        // The current offset into the method's JITted code (instruction offset, not byte offset)
        public uint ipOffset;
        // This method's evaluation stack
        public byte* pEvalStack;
        // The evaluation stack current offset
        public uint stackOfs;
        // This method's parameters & local variable storage. Params are first, followed by locals
        public byte* pParamsLocals;
        // Is this methodstate from a NEWOBJ op-code?
        public uint isInternalNewObjCall;
        // If this is a Finalizer, then the 'this' object goes here,
        // so it can be marked in the 'return' statement that it no longer has a Finalizer to run
        public /*HEAP_PTR*/byte* finalizerThis;
        // When in a delegate invoke, store the next delegate to invoke here.
        // This is to allow multi-cast delegates to call all their methods.
        public void *pNextDelegate;
        // And store the parameters to go to this delegate call
        public void *pDelegateParams;
        // When a leave instruction has to run a 'finally' bit of code, store the leave jump address here
        public uint *pOpEndFinally;

        #if DIAG_METHOD_CALLS
        // For tracking execution time.
        public ulong startTime;
        #endif

        // Link to caller methodstate
        public tMethodState *pCaller;

        // In the case of a reflection-initiated invocation (i.e., someMethodBase.Invoke(...)),
        // we need to track the target method's return type so we can interpret the stack when
        // it's done.
        public tMD_TypeDef *pReflectionInvokeReturnType;
    }

    public unsafe static class MethodState
    {

        public static tMethodState* Direct(tThread *pThread, tMD_MethodDef *pMethod, tMethodState *pCaller, uint isInternalNewObjCall) 
        {
        	tMethodState *pThis;

        	if (pMethod->isFilled == 0) {
        		tMD_TypeDef *pTypeDef;

        		pTypeDef = MetaData.GetTypeDefFromMethodDef(pMethod);
        		MetaData.Fill_TypeDef(pTypeDef, null, null);
        	}

            pThis = (tMethodState*)Thread.StackAlloc(pThread, (uint)sizeof(tMethodState));
        	pThis->finalizerThis = null;
        	pThis->pCaller = pCaller;
        	pThis->pMetaData = pMethod->pMetaData;
        	pThis->pMethod = pMethod;
        	if (pMethod->pJITted == null) {
        		// If method has not already been JITted
        		JIT.Prepare(pMethod, 0);
        	}
        	pThis->pJIT = pMethod->pJITted;
        	pThis->ipOffset = 0;
        	pThis->pEvalStack = (byte*)Thread.StackAlloc(pThread, pThis->pMethod->pJITted->maxStack);
        	pThis->stackOfs = 0;
        	pThis->isInternalNewObjCall = isInternalNewObjCall;
        	pThis->pNextDelegate = null;
        	pThis->pDelegateParams = null;

        	pThis->pParamsLocals = (byte*)Thread.StackAlloc(pThread, pMethod->parameterStackSize + pMethod->pJITted->localsStackSize);
        	Mem.memset(pThis->pParamsLocals, 0, pMethod->parameterStackSize + pMethod->pJITted->localsStackSize);

        #if DIAG_METHOD_CALLS
        	// Keep track of the number of times this method is called
        	pMethod->callCount++;
        	pThis->startTime = microTime();
        #endif

        	return pThis;
        }

        public static tMethodState* New(tThread *pThread, tMetaData *pMetaData, /*IDX_TABLE*/uint methodToken, tMethodState *pCaller) 
        {
        	tMD_MethodDef *pMethod;

        	pMethod = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, methodToken, null, null);
        	return Direct(pThread, pMethod, pCaller, 0);
        }

        public static void SetParameters(tMethodState* pMethodState, tMD_MethodDef *pCallMethod, byte* pParams) 
        {
            Mem.memcpy(pMethodState->pParamsLocals, pParams, pCallMethod->parameterStackSize);
        }

        public static void GetReturnValue(tMethodState* pMethodState, byte* pReturnValue) 
        {
            uint stackSize = pMethodState->pMethod->pReturnType->stackSize;
            if (stackSize == 4) {
                *(uint*)pReturnValue = *(uint*)pMethodState->pEvalStack;
            } else if (stackSize == 8) {
                *(ulong*)pReturnValue = *(ulong*)pMethodState->pEvalStack;
            } else {
                Mem.memcpy(pReturnValue, pMethodState->pEvalStack, stackSize);
            }
        }

        public static void Delete(tThread *pThread, ref tMethodState *pMethodState) 
        {
        	tMethodState *pThis = pMethodState;

        #if DIAG_METHOD_CALLS
        	pThis->pMethod->totalTime += microTime() - pThis->startTime;
        #endif

        	// If this MethodState is a Finalizer, then let the heap know this Finalizer has been run
        	if (pThis->finalizerThis != null) {
        		Heap.UnmarkFinalizer(pThis->finalizerThis);
        	}

        	if (pThis->pDelegateParams != null) {
        		Mem.free(pThis->pDelegateParams);
        	}

        	// Note that the way the stack Mem.free funtion works means that only the 1st allocated chunk
        	// needs to be Mem.free'd, as this function just sets the current allocation offset to the address given.
        	Thread.StackFree(pThread, pThis);

        	pMethodState = null;
        }

    }

}
