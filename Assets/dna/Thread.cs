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
    public unsafe struct tThreadStack
    {
        public const int THREADSTACK_CHUNK_SIZE = 10000;

        // This chunk of stack memory
        public fixed byte memory[THREADSTACK_CHUNK_SIZE];
        // Current offset into this memory chunk
        public uint ofs;
        // Pointer to the next chunk.
        public tThreadStack *pNext;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tThread
    {
        // Stuff that's synced with Thread.cs

        // The delegate that this thread starts by executing
        public byte* startDelegate;
        // The parameter to pass to the starting method (this is ignored if no parameter is needed).
        public /*HEAP_PTR*/byte* param;
        // The threadID of this thread
        public uint threadID;
        // The current state of the thread (running/paused/etc...)
        public uint state;
        // The current culture of the thread. Never accessed in C
        public void *pCurrentCulture;

        // Stuff that is independant of Thread.cs
        // Note that the size of this can be anything we like, as the size of the Thread .NET type is ignored.

        // This thread's currently executing method
        public tMethodState *pCurrentMethodState;
        // Thread exit value
        public int threadExitValue;
        // The current exception object of this thread (for use by RETHROW)
        public /*HEAP_PTR*/byte* pCurrentExceptionObject;
        // When unwinding the stack after a throw, this keeps track of which finally clauses have already been executed
        public uint nextFinallyUnwindStack;
        // And the method state that we're aiming for..
        public tMethodState *pCatchMethodState;
        // And the exception catch handler we're aiming for...
        public tExceptionHeader *pCatchExceptionHandler;
        // If this thread is waiting on async data, then the details are stored here
        public tAsyncCall *pAsync;
        // Does this thread start with a parameter?
        public uint hasParam;
        // Pointer to the first chunk of thread-stack memory
        public tThreadStack *pThreadStack;

        // The next thread in the system (needed for garbage collection and theading)
        public tThread *pNextThread;
    }

    public unsafe static class Thread
    {

        // The thread has finished
        public const int THREAD_STATUS_EXIT = 1;
        // The thread is still running, but has completed its timeslot
        public const int THREAD_STATUS_RUNNING = 2;
        // The thread is waiting on some async data (sleep or IO)
        public const int THREAD_STATUS_ASYNC = 3;
        // The thread has just exited a lock, so allow other threads to acquire it if they are waiting
        public const int THREAD_STATUS_LOCK_EXIT = 4;

        public static tAsyncCall* ASYNC_LOCK_EXIT()
        {
            return ((tAsyncCall*)(void*)0x00000001);
        }

        // These are the same as the C# definitions in corelib,
        // and can be ORed together.
        public const int THREADSTATE_RUNNING           = 0x0000;
        public const int THREADSTATE_BACKGROUND        = 0x0004;
        public const int THREADSTATE_UNSTARTED         = 0x0008;
        public const int THREADSTATE_STOPPED           = 0x0010;
        public const int THREADSTATE_SUSPENDED         = 0x0040;

        static uint maxInstrPerThread = 100;
        static tThread *pAllThreads = null;
        static tThread *pCurrentThread = null;
        static tThread *pCallThread = null;
        static tThread *pLastCallThread = null;

        static void Reset(tThread *pThis)
        {
            pThis->pCurrentMethodState = null;
            pThis->threadExitValue = 0;
            pThis->nextFinallyUnwindStack = 0;
            pThis->pAsync = null;
            pThis->hasParam = 0;
            
            pThis->startDelegate = null;
            pThis->param = null;
            pThis->state = THREADSTATE_UNSTARTED;
            
            // Allocate the first chunk of thread-local stack
            if (pThis->pThreadStack == null) {
                pThis->pThreadStack = ((tThreadStack*)Mem.malloc((SIZE_T)sizeof(tThreadStack)));
                pThis->pThreadStack->ofs = 0;
                pThis->pThreadStack->pNext = null;
            }
        }

        static uint threadID = 0;

        public static tThread* New() 
        {
        	tThread *pThis;

        	// Create thread and initial method state. This is allocated on the managed heap, and
        	// mark as undeletable. When the thread exits, it was marked as deletable.
        	pThis = (tThread*)Heap.AllocType(Type.types[Type.TYPE_SYSTEM_THREADING_THREAD]);
        	Heap.MakeUndeletable((/*HEAP_PTR*/byte*)pThis);
            Mem.memset(pThis, 0, (SIZE_T)sizeof(tThread));
        	pThis->threadID = ++threadID;

            Reset(pThis);
            
        	// Add to list of all thread
        	pThis->pNextThread = pAllThreads;
        	pAllThreads = pThis;

        	return pThis;
        }

        public static void* StackAlloc(tThread *pThread, uint size) 
        {
        	tThreadStack *pStack = pThread->pThreadStack;
        	void *pAddr = pStack->memory + pStack->ofs;
        #if _DEBUG
        	*(uint*)pAddr = 0xabababab;
        	((uint*)pAddr)++;
        	pStack->ofs += 4;
        #endif
        	pStack->ofs += size;
        	if (pStack->ofs > tThreadStack.THREADSTACK_CHUNK_SIZE) {
        		Sys.Crash("Thread-local stack is too large");
        	}
        #if _DEBUG
        	Mem.memset(pAddr, 0xcd, size);
        	*(uint*)(((byte*)pAddr) + size) = 0xfbfbfbfb;
        	pStack->ofs += 4;
        #endif
        	return pAddr;
        }

        public static void StackFree(tThread *pThread, void *pAddr) 
        {
        	tThreadStack *pStack = pThread->pThreadStack;
        #if _DEBUG
        	((uint*)pAddr)--;
        	Mem.memset(pAddr, 0xfe, pStack->ofs - (uint)(((byte*)pAddr) - pStack->memory));
        #endif
        	pStack->ofs = (uint)(((byte*)pAddr) - pStack->memory);
        }

        public static void SetEntryPoint(tThread *pThis, tMetaData *pMetaData, /*IDX_TABLE*/uint entryPointToken, byte* _params, uint paramBytes) 
        {
        	// Set up the initial MethodState
        	pThis->pCurrentMethodState = MethodState.New(pThis, pMetaData, entryPointToken, null);
        	// Insert initial parameters (if any)
        	if (paramBytes > 0) {
        		Mem.memcpy(pThis->pCurrentMethodState->pParamsLocals, _params, paramBytes);
        	}
        }

        static void Delete(tThread *pThis) 
        {
        	tThreadStack *pStack = pThis->pThreadStack;
        	while (pStack != null) {
        		tThreadStack *pNextStack = pStack->pNext;
        		Mem.free(pStack);
        		pStack = pNextStack;
        	}
        	Heap.MakeDeletable((/*HEAP_PTR*/byte*)pThis);
        }

        public static uint Update(uint maxInstr, int* pReturnCode) 
        {
            tThread *pThread;
            tThread *pPrevThread;
            uint status;
            
            pThread = pAllThreads;
            // Set the initial thread to the RUNNING state.
            pThread->state = THREADSTATE_RUNNING;
            // Set the initial CurrentThread
            pCurrentThread = pThread;
            
            for (;;) {
                uint minSleepTime = 0xffffffff;
                int threadExitValue;
                
                status = JIT_Execute.Execute(pThread, maxInstr);
                switch (status) {
                    case Thread.THREAD_STATUS_EXIT:
                        threadExitValue = pThread->threadExitValue;
                        Sys.log_f(1, "Thread ID#%d exited. Return value: %d\n", (int)pThread->threadID, (int)threadExitValue);
                        // Remove the current thread from the running threads list.
                        // Note that this list may have changed since before the call to JitOps.JIT_Execute().
                        {
                            if (pAllThreads == pThread) {
                                pAllThreads = pAllThreads->pNextThread;
                            } else {
                                tThread *pThread1 = pAllThreads;
                                while (pThread1->pNextThread != pThread) {
                                    pThread1 = pThread1->pNextThread;
                                }
                                pThread1->pNextThread = pThread1->pNextThread->pNextThread;
                            }
                        }
                        // Delete the current thread
                        Thread.Delete(pThread);
                        // If there are no more threads left running, then exit application (by returning)
                        // Threads that are unstarted or background do not stop the exit
                        // [Steve edit] Threads that are suspended also do not stop the exit. This is because you'd just
                        // wait forever for them if they did. Note that 'exit' doesn't mean tearing down the process
                        // like in a regular .NET runtime case. The application state is still there and we can make
                        // further calls into it to create new threads.
                        {
                            tThread *pThread2 = pAllThreads;
                            uint canExit = 1;
                            while (pThread2 != null) {
                                if (
                                    ((pThread2->state & THREADSTATE_BACKGROUND) == 0)
                                    && ((pThread2->state & (~THREADSTATE_BACKGROUND)) != THREADSTATE_UNSTARTED)
                                    && ((pThread2->state & (~THREADSTATE_BACKGROUND)) != THREADSTATE_SUSPENDED))
                                {
                                    canExit = 0;
                                    break;
                                }
                                pThread2 = pThread2->pNextThread;
                            }
                            if (canExit != 0) {
                                if (pReturnCode != null) {
                                    *pReturnCode = threadExitValue;
                                }
                                return THREADSTATE_STOPPED;
                            }
                        }
                        pThread = pAllThreads; // This is not really correct, but it'll work for the time being
                        break;
                    case THREAD_STATUS_RUNNING:
                    case THREAD_STATUS_LOCK_EXIT:
                        // Nothing to do
                        break;
                    case THREAD_STATUS_ASYNC:
                        pThread->pAsync->startTime = Sys.msTime();
                        break;
                }
                
                // Move on to the next thread.
                // Find the next thread that isn't sleeping or blocked on IO
                pPrevThread = pThread;
                for (;;) {
                    pThread = pThread->pNextThread;
                    if (pThread == null) {
                        // That was the thread -- return!
                        return THREADSTATE_RUNNING;
                    }
                    // Set the CurrentThread correctly
                    pCurrentThread = pThread;
                    if ((pThread->state & (~THREADSTATE_BACKGROUND)) != 0) {
                        // Thread is not running
                        continue;
                    }
                    if (pThread->pAsync != null) {
                        // Discover if whatever is being waited for is finished
                        tAsyncCall *pAsync = pThread->pAsync;
                        if (pAsync->sleepTime >= 0) {
                            // This is a sleep
                            ulong nowTime = Sys.msTime();
                            int msSleepRemaining = pAsync->sleepTime - (int)(nowTime - pAsync->startTime);
                            if (msSleepRemaining <= 0) {
                                // Sleep is finished
                                break;
                            }
                            // Sleep is not finished, so continue to next thread
                            if ((uint)msSleepRemaining < minSleepTime) {
                                minSleepTime = (uint)msSleepRemaining;
                            }
                        } else {
                            // This is blocking IO, or a lock
                            tMethodState *pMethodState = pThread->pCurrentMethodState;
                            byte* pThis;
                            uint thisOfs;
                            uint unblocked;
                            
                            if (MetaData.METHOD_ISSTATIC(pMethodState->pMethod)) {
                                pThis = null;
                                thisOfs = 0;
                            } else {
                                pThis = *(byte**)pMethodState->pParamsLocals;
                                thisOfs = 4;
                            }
                            unblocked = ((fnInternalCallCheck)H.ToObj(pAsync->checkFn))(pThis, pMethodState->pParamsLocals + thisOfs, pMethodState->pEvalStack, pAsync);
                            if (unblocked != 0) {
                                // The IO has unblocked, and the return value is ready.
                                // So delete the async object.
                                // TODO: The async->state object needs to be deleted somehow (maybe)
                                Mem.free(pAsync);
                                // And remove it from the thread
                                pThread->pAsync = null;
                                break;
                            }
                            minSleepTime = 5;
                        }
                    } else {
                        // Thread is ready to run
                        break;
                    }
                    if (pThread == pPrevThread) {
                        // When it gets here, it means that all threads are currently blocked.
                        //printf("All blocked; sleep(%d)\n", minSleepTime);
                        Sys.SleepMS(minSleepTime);
                    }
                }
            }
        }

        public static void SetInstructionsPerThread(uint max) 
        {
            maxInstrPerThread = max;
        }

        public static int Execute() 
        {
            int returnCode = 0;
        	for (;;) {
                uint status = Thread.Update(maxInstrPerThread, &returnCode);
                if (status == THREADSTATE_STOPPED) {
                    return returnCode;
                }
            }
        }

        public static int Call(tMD_MethodDef *pMethod, byte* pParams, byte* pReturnValue) 
        {
            uint status;
            tThread* pThread;
            
            // Check to see if we have a call thread already available
            if (pCallThread == null) {
                pCallThread = Thread.New();
            }
            
            // Hang on to this thread in case the method returns async..
            pThread = pLastCallThread = pCallThread;
            pCallThread = null;
            
            // Set up the initial MethodState
            pThread->pCurrentMethodState = MethodState.Direct(pThread, pMethod, null, 0);
            // Insert initial parameters (if any)
            if (pParams == null) {
                MethodState.SetParameters(pThread->pCurrentMethodState, pMethod, pParams);
            }
            
            // Set the initial thread to the RUNNING state.
            pThread->state = THREADSTATE_RUNNING;
            // Set the initial CurrentThread
            pCurrentThread = pThread;
            
            pLastCallThread = pThread;
            
            status = THREAD_STATUS_RUNNING;
            while (status == THREAD_STATUS_RUNNING) {
                status = JIT_Execute.Execute(pThread, 1000000);
                switch (status) {
                    case Thread.THREAD_STATUS_EXIT:
                        if (pReturnValue != null) {
                            MethodState.GetReturnValue(pThread->pCurrentMethodState, pReturnValue);
                        }
                        // Thread exited normally, put it back so it can be reused by next call
                        Thread.Reset(pThread);
                        pCallThread = pThread;
                        break;
                    case Thread.THREAD_STATUS_RUNNING:
                    case Thread.THREAD_STATUS_LOCK_EXIT:
                        // Nothing to do
                        break;
                    case Thread.THREAD_STATUS_ASYNC:
                        pThread->pAsync->startTime = Sys.msTime();
                        break;
                }
            }
            
            return (int)status;
        }

        public static tThread* getLastCallThread() 
        {
            return pLastCallThread;
        }

        public static tThread* GetCurrent() 
        {
        	return pCurrentThread;
        }

        public static void GetHeapRoots(tHeapRoots *pHeapRoots) 
        {
        	tThread *pThread;

        	pThread = pAllThreads;
        	while (pThread != null) {
        		tMethodState *pMethodState;

        		pMethodState = pThread->pCurrentMethodState;
        		while (pMethodState != null) {
        			// Put the evaluation stack on the roots
        			Heap.SetRoots(pHeapRoots, pMethodState->pEvalStack, pMethodState->pMethod->pJITted->maxStack);
        			// Put the params/locals on the roots
        			Heap.SetRoots(pHeapRoots, pMethodState->pParamsLocals,
        				pMethodState->pMethod->parameterStackSize+pMethodState->pMethod->pJITted->localsStackSize);

        			pMethodState = pMethodState->pCaller;
        		}

        		pThread = pThread->pNextThread;
        	}
        }
    }
}
