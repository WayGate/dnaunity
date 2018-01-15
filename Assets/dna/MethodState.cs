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
    #if UNITY_WEBGL || DNA_32BIT
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

        #if GEN_COMBINED_OPCODES

        // Pointer to the least called method
        static tMD_MethodDef *pLeastCalledMethod = null;
        // Amount of memory currently used by combined JITted methods
        static uint combinedJITSize = 0;

        static void AddCall(tMD_MethodDef *pMethod) {
        	pMethod->genCallCount++;
        	// See if this method needs moving in the 'call quantity' linked-list,
        	// or if this method needs adding to the list for the first time
        	if (pMethod->genCallCount == 1) {
        		// Add for the first time
        		pMethod->pNextHighestCalls = pLeastCalledMethod;
        		pMethod->pPrevHighestCalls = null;
        		if (pLeastCalledMethod != null) {
        			pLeastCalledMethod->pPrevHighestCalls = pMethod;
        		}
        		pLeastCalledMethod = pMethod;
        	} else {
        		// See if this method needs moving up the linked-list
        		tMD_MethodDef *pCheckMethod = pMethod;
        		ulong numCalls = pMethod->genCallCount;
        		while (pCheckMethod->pNextHighestCalls != null && numCalls > pCheckMethod->pNextHighestCalls->genCallCount) {
        			pCheckMethod = pCheckMethod->pNextHighestCalls;
        		}
        		if (numCalls > pCheckMethod->genCallCount) {
        			// Swap the two methods in the linked-list
        			tMD_MethodDef *pT1, *pT2;
        			uint adjacent = pCheckMethod->pPrevHighestCalls == pMethod;

        			if (pCheckMethod->pNextHighestCalls != null) {
        				pCheckMethod->pNextHighestCalls->pPrevHighestCalls = pMethod;
        			}
        			pT1 = pMethod->pNextHighestCalls;
        			pMethod->pNextHighestCalls = pCheckMethod->pNextHighestCalls;

        			if (pMethod->pPrevHighestCalls != null) {
        				pMethod->pPrevHighestCalls->pNextHighestCalls = pCheckMethod;
        			} else {
        				pLeastCalledMethod = pCheckMethod;
        			}
        			pT2 = pCheckMethod->pPrevHighestCalls;
        			pCheckMethod->pPrevHighestCalls = pMethod->pPrevHighestCalls;

        			if (!adjacent) {
        				pT2->pNextHighestCalls = pMethod;
        				pMethod->pPrevHighestCalls = pT2;
        				pT1->pPrevHighestCalls = pCheckMethod;
        				pCheckMethod->pNextHighestCalls = pT1;
        			} else {
        				pMethod->pPrevHighestCalls = pCheckMethod;
        				pCheckMethod->pNextHighestCalls = pMethod;
        			}
        		}
        	}	
        }

        static void DeleteCombinedJIT(tMD_MethodDef *pMethod) {
        	tCombinedOpcodesMem *pCOM;
        	tJITted *pJIT = pMethod->pJITtedCombined;
        	Mem.free(pJIT->pExceptionHeaders);
        	Mem.free(pJIT->pOps);
        	pCOM = pJIT->pCombinedOpcodesMem;
        	while (pCOM != null) {
        		tCombinedOpcodesMem *pT = pCOM;
        		Mem.free(pCOM->pMem);
        		pCOM = pCOM->pNext;
        		Mem.free(pT);
        	}
        }

        static void RemoveCombinedJIT(tMD_MethodDef *pMethod) {
        	if (pMethod->callStackCount == 0) {
        		DeleteCombinedJIT(pMethod);
        	} else {
        		// Mark this JIT for removal. Don't quite know how to do this!
        		Sys.log_f(0, "!!! CANNOT REMOVE COMBINED JIT !!!\n");
        	}
        	combinedJITSize -= pMethod->pJITtedCombined->opsMemSize;
        	pMethod->pJITtedCombined = null;
        	Sys.log_f(1, "Removing Combined JIT: %s\n", Sys_GetMethodDesc(pMethod));
        }

        static void AddCombinedJIT(tMD_MethodDef *pMethod) {
        	JIT_Prepare(pMethod, 1);
        	combinedJITSize += pMethod->pJITtedCombined->opsMemSize;
        	Sys.log_f(1, "Creating Combined JIT: %s\n", Sys_GetMethodDesc(pMethod));
        }

        #endif

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

        #if GEN_COMBINED_OPCODES
        	AddCall(pMethod);

        	/*if (combinedJITSize < GEN_COMBINED_OPCODES_MAX_MEMORY) {
        		if (pMethod->genCallCount > GEN_COMBINED_OPCODES_CALL_TRIGGER) {
        			if (pMethod->pJITtedCombined == null) {
        				JIT_Prepare(pMethod, 1);
        				combinedJITSize += pMethod->pJITtedCombined->opsMemSize;
        			}
        		}
        	}*/
        	if (pMethod->pJITtedCombined == null && pMethod->genCallCount >= GEN_COMBINED_OPCODES_CALL_TRIGGER &&
        		(pMethod->pNextHighestCalls == null || pMethod->pPrevHighestCalls == null ||
        		pMethod->pPrevHighestCalls->pJITtedCombined != null ||
        		(combinedJITSize < GEN_COMBINED_OPCODES_MAX_MEMORY && pMethod->pNextHighestCalls->pJITtedCombined != null))) {
        		// Do a combined JIT, if there's enough room after removing combined JIT from previous
        		if (combinedJITSize > GEN_COMBINED_OPCODES_MAX_MEMORY) {
        			// Remove the least-called function's combined JIT
        			tMD_MethodDef *pToRemove = pMethod;
        			while (pToRemove->pPrevHighestCalls != null && pToRemove->pPrevHighestCalls->pJITtedCombined != null) {
        				pToRemove = pToRemove->pPrevHighestCalls;
        			}
        			if (pToRemove != pMethod) {
        				RemoveCombinedJIT(pToRemove);
        			}
        		}
        		if (combinedJITSize < GEN_COMBINED_OPCODES_MAX_MEMORY) {
        			// If there's enough room, then create new combined JIT
        			AddCombinedJIT(pMethod);
        		}
        	}

        	// See if there is a combined opcode JIT ready to use
        	if (pMethod->pJITtedCombined != null) {
        		pThis->pJIT = pMethod->pJITtedCombined;
        		pMethod->callStackCount++;
        	}
        #endif

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

        public static void Delete(tThread *pThread, tMethodState **ppMethodState) 
        {
        	tMethodState *pThis = *ppMethodState;


        #if GEN_COMBINED_OPCODES
        	if (pThis->pJIT != pThis->pMethod->pJITted) {
        		// Only decrease call-stack count if it's been using the combined JIT
        		pThis->pMethod->callStackCount--;
        	}
        	if (pThis->pCaller != null) {
        		// Add a call to the method being returned to.
        		// This is neccesary to give a more correct 'usage heuristic' to long-running
        		// methods that call lots of other methods.
        		AddCall(pThis->pCaller->pMethod);
        	}
        #endif

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

        	*ppMethodState = null;
        }

    }

}
