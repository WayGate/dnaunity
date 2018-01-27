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

//#define TRACE

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif

    public unsafe static class JIT_Execute
    {
        static tThread *pThread;
        static tJITted *pJIT;
        static tMethodState *pCurrentMethodState;
        static byte* pParamsLocals;

        static byte* scNone;

        // Local copies of thread state variables, to speed up execution
        // Pointer to next op-code
        static uint *pOps;
        static uint *pCurOp;
        // Pointer to eval-stack position
        static byte* pCurEvalStack;
        static byte* pThrowExcept;

        public static void Init()
        {
            scNone = null;

            pOps = null;
            pCurOp = null;
            pCurEvalStack = null;
            pThrowExcept = null;

            // Initialise the JIT code addresses
            Execute(null, 0);
        }

        // Get the next op-code
        static uint GET_OP()
        {
            return (*(pCurOp++));
        }

        // Get a 32/64 bit pointer
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        static void* GET_PTR()
        {
            return (void*)(*(pCurOp++));
        }
        #else
        // NOTE: Technically this is undefined behavior having two increments in the same expression
        static void* GET_PTR()
        {
            return (void*)((PTR)((((ulong)*(pCurOp++)) | ((ulong)*(pCurOp++) << 32))));
        }
        #endif

        // Push a byte* value on the top of the stack
        static void PUSH_PTR(void* ptr) 
        {
            *(byte**)pCurEvalStack = (byte*)(ptr); 
            pCurEvalStack += sizeof(void*);
        }

        // Push an arbitrarily-sized value-type onto the top of the stack
        static void PUSH_VALUETYPE(void* ptr, uint valueSize, uint stackInc)
        {
            Mem.memcpy(pCurEvalStack, ptr, (SIZE_T)valueSize); 
            pCurEvalStack += stackInc;
        }

        // Push a uint value on the top of the stack
        static void PUSH_U32(uint value)
        {
            *(uint*)pCurEvalStack = (uint)(value); 
            pCurEvalStack += 4;
        }

        // Push a ulong value on the top of the stack
        static void PUSH_U64(ulong value)
        {
            *(ulong*)pCurEvalStack = (ulong)(value); 
            pCurEvalStack += 8;
        }

        // Push a float value on the top of the stack
        static void PUSH_FLOAT(float value) 
        {
            *(float*)pCurEvalStack = (float)(value); 
            pCurEvalStack += 4;
        }

        // Push a double value on the top of the stack
        static void PUSH_DOUBLE(double value)
        {
            *(double*)pCurEvalStack = (double)(value); 
            pCurEvalStack += 8;
        }

        // Push a heap pointer on to the top of the stack
        static void PUSH_O(void* pHeap) {
            *(void**)pCurEvalStack = (void*)(pHeap); 
            pCurEvalStack += sizeof(void*);
        }

        // DUP4() duplicates the top 4 bytes on the eval stack
        static void DUP4()
        {
            *(uint*)pCurEvalStack = *(uint*)(pCurEvalStack - 4); 
            pCurEvalStack += 4;
        }

        // DUP8() duplicates the top 4 bytes on the eval stack
        static void DUP8()
        {
            *(ulong*)pCurEvalStack = *(ulong*)(pCurEvalStack - 8); 
            pCurEvalStack += 8;
        }

        // DUP() duplicates numBytes bytes from the top of the stack
        static void DUP(uint numBytes)
        {
            Mem.memcpy(pCurEvalStack, pCurEvalStack - numBytes, numBytes); 
            pCurEvalStack += numBytes;
        }

        // Pop a uint value from the stack
        static uint POP_U32()
        {
            return (*(uint*)(pCurEvalStack -= 4));
        }

        // Pop a ulong value from the stack
        static ulong POP_U64() 
        {
            return (*(ulong*)(pCurEvalStack -= 8));
        }

        // Pop a float value from the stack
        static float POP_FLOAT()
        {
            return (*(float*)(pCurEvalStack -= 4));
        }

        // Pop a double value from the stack
        static double POP_DOUBLE()
        {
            return (*(double*)(pCurEvalStack -= 8));
        }

        // Pop 2 uint's from the stack
        static void POP_U32_U32(out uint v1, out uint v2)
        {
            pCurEvalStack -= 8; 
            v1 = *(uint*)pCurEvalStack; 
            v2 = *(uint*)(pCurEvalStack + 4);
        }

        // Pop 2 ulong's from the stack
        static void POP_U64_U64(out ulong v1, out ulong v2)
        {
            pCurEvalStack -= 16; 
            v1 = *(ulong*)pCurEvalStack; 
            v2 = *(ulong*)(pCurEvalStack + 8);
        }

        // Pop 2 F32's from the stack
        static void POP_F32_F32(out float v1, out float v2)
        {
            pCurEvalStack -= 8; 
            v1 = *(float*)pCurEvalStack; 
            v2 = *(float*)(pCurEvalStack + 4);
        }

        // Pop 2 F64's from the stack
        static void POP_F64_F64(out double v1, out double v2)
        {
            pCurEvalStack -= 16; 
            v1 = *(double*)pCurEvalStack; 
            v2 = *(double*)(pCurEvalStack + 8);
        }

        // Pop a byte* value from the stack
        static byte* POP_PTR()
        {
            return (*(byte**)(pCurEvalStack -= sizeof(void*)));
        }

        // Pop an arbitrarily-sized value-type from the stack (copies it to the specified memory location)
        static void POP_VALUETYPE(void* ptr, uint valueSize, uint stackDec) 
        {
            Mem.memcpy(ptr, pCurEvalStack -= stackDec, (SIZE_T)valueSize);
        }

        // Pop a Object (heap) pointer value from the stack
        static byte* POP_O()
        {
            return (*(/*HEAP_PTR*/byte**)(pCurEvalStack -= sizeof(void*)));
        }

        // POP() returns nothing - it just alters the stack offset correctly
        static void POP(uint numBytes)
        {
            pCurEvalStack -= numBytes;
        }

        // POP_ALL() empties the evaluation stack
        static void POP_ALL() 
        {
            pCurEvalStack = pCurrentMethodState->pEvalStack;
        }

        // General binary ops
//        const int BINARY_OP(returnType, type1, type2, op)
//        {
//          pCurEvalStack -= sizeof(type1) + sizeof(type2) - sizeof(returnType); 
//          *(returnType*)(pCurEvalStack - sizeof(returnType)) = *(type1*)(pCurEvalStack - sizeof(returnType)) op;
//            *(type2*)(pCurEvalStack - sizeof(returnType) + sizeof(type1));
//        }
        // pCurEvalStack -= sizeof($2) + sizeof($3) - sizeof($1); *($1*)(pCurEvalStack - sizeof($1)) = *($2*)(pCurEvalStack - sizeof($1)) $4 *($3*)(pCurEvalStack - sizeof($1) + sizeof($2));


        // General unary ops
//        s UNARY_OP(type, op)
//        {
//            STACK_ADDR(type) = op STACK_ADDR(type)
//        }

        // Set the new method state (for use when the method state changes - in calls mainly)
        static void SAVE_METHOD_STATE()
        {
            pCurrentMethodState->stackOfs = (uint)(pCurEvalStack - pCurrentMethodState->pEvalStack);
            pCurrentMethodState->ipOffset = (uint)(pCurOp - pOps);
        }

        static void LOAD_METHOD_STATE()
        {
            pCurrentMethodState = pThread->pCurrentMethodState;
            pParamsLocals = pCurrentMethodState->pParamsLocals;
            pCurEvalStack = pCurrentMethodState->pEvalStack + pCurrentMethodState->stackOfs;
            pJIT = pCurrentMethodState->pJIT;
            pOps = pJIT->pOps;
            pCurOp = pOps + pCurrentMethodState->ipOffset;
        }

        static void CHANGE_METHOD_STATE(tMethodState* pNewMethodState)
        {
            SAVE_METHOD_STATE();
            pThread->pCurrentMethodState = pNewMethodState;
            LOAD_METHOD_STATE();
        }

        // Easy access to method parameters and local variables
        static uint PARAMLOCAL_U32(uint offset)
        {
            return *(uint*)(pParamsLocals + offset);
        }

        // Easy access to method parameters and local variables
        static void SET_PARAMLOCAL_U32(uint offset, uint value)
        {
            *(uint*)(pParamsLocals + offset) = value;
        }

        static ulong PARAMLOCAL_U64(uint offset)
        {
            return *(ulong*)(pParamsLocals + offset);
        }

        static void SET_PARAMLOCAL_U64(uint offset, ulong value)
        {
            *(ulong*)(pParamsLocals + offset) = value;
        }

        static byte* THROW(tMD_TypeDef* exType)
        {
            return Heap.AllocType(exType); 
        }

        // Note: newObj is only set if a constructor is being called
        static void CreateParameters(byte* pParamsLocals, tMD_MethodDef *pCallMethod, byte* *ppCurEvalStack, /*HEAP_PTR*/byte* newObj) 
        {
            uint ofs;

            if (newObj != null) {
                // If this is being called from JitOps.JIT_NEW_OBJECT then need to specially push the new object
                // onto parameter stack position 0
                *(/*HEAP_PTR*/byte**)pParamsLocals = newObj;
                ofs = sizeof(PTR);
            } else {
                ofs = 0;
            }
            *ppCurEvalStack -= pCallMethod->parameterStackSize - ofs;
            Mem.memcpy(pParamsLocals + ofs, *ppCurEvalStack, (SIZE_T)(pCallMethod->parameterStackSize - ofs));
        }

        static tMethodState* RunFinalizer(tThread *pThread) 
        {
            /*HEAP_PTR*/byte* heapPtr = Finalizer.GetNextFinalizer();
            if (heapPtr != null) {
                // There is a pending finalizer, so create a MethodState for it and put it as next-to-run on the stack
                tMethodState *pFinalizerMethodState;
                tMD_TypeDef *pFinalizerType = Heap.GetType(heapPtr);

                pFinalizerMethodState = MethodState.Direct(pThread, pFinalizerType->pFinalizer, pThread->pCurrentMethodState, 0);
                // Mark this methodState as a Finalizer
                pFinalizerMethodState->finalizerThis = heapPtr;
                // Put the object on the stack (the object that is being Finalized)
                // Finalizers always have no parameters
                *(/*HEAP_PTR*/byte**)(pFinalizerMethodState->pParamsLocals) = heapPtr;
                //printf("--- FINALIZE ---\n");

                return pFinalizerMethodState;
            }
            return null;
        }

        #if DIAG_OPCODE_TIMES
        ulong opcodeTimes[JitOps.JIT_OPCODE_MAXNUM];
        static __inline unsigned __int64 __cdecl rdtsc() {
            __asm {
                rdtsc
            }
        }
        #endif

        #if DIAG_OPCODE_USE
        static uint opcodeNumUses[JitOps.JIT_OPCODE_MAXNUM];

        [System.Diagnostics.Conditional("TRACE")]
        static void OPCODE_USE(uint op) 
        {
            S.printf("%s %X op \n", (PTR)pCurrentMethodState->pMethod->name, (int)(pCurEvalStack - pCurrentMethodState->pEvalStack)); 
            opcodeNumUses[op]++;
        }

        #else

        [System.Diagnostics.Conditional("TRACE")]
        static void OPCODE_USE(uint op) 
        {
            Sys.printf("%s %X op \n", (PTR)pCurrentMethodState->pMethod->name, (int)(pCurEvalStack - pCurrentMethodState->pEvalStack));
        }

        #endif

        static void RUN_FINALIZER() 
        {
            tMethodState *pMS = RunFinalizer(pThread);
            if (pMS != null) {
                CHANGE_METHOD_STATE(pMS);
            }
        }

        public static uint Execute(tThread *pThread, uint numInst) 
        {
            JIT_Execute.pThread = pThread;
            uint op = 0;
            uint u32Value = 0;

            if (pThread == null) {
                return 0;
            }

        #if DIAG_OPCODE_TIMES
            ulong opcodeStartTime = rdtsc();
            uint realOp;
        #endif

            LOAD_METHOD_STATE();
         
            for(;;)
            {
                switch (*pCurOp++)
                {
                    case JitOps.JIT_NOP:
                    case JitOps.JIT_CONV_R32_R32:
                    case JitOps.JIT_CONV_R64_R64:
                    case JitOps.JIT_CONV_I64_U64:
                    case JitOps.JIT_CONV_U64_I64:
                        OPCODE_USE(JitOps.JIT_NOP);
                        break;

                    case JitOps.JIT_LOAD_NULL:
                        OPCODE_USE(JitOps.JIT_LOAD_NULL);
                        PUSH_O(null);
                        break;

                    case JitOps.JIT_DUP_4:
                        OPCODE_USE(JitOps.JIT_DUP_4);
                        DUP4();
                        break;

                    case JitOps.JIT_DUP_8:
                        OPCODE_USE(JitOps.JIT_DUP_8);
                        DUP8();
                        break;

                    case JitOps.JIT_DUP_GENERAL:
                        OPCODE_USE(JitOps.JIT_DUP_GENERAL);
                        {
                            uint dupSize = GET_OP();
                            DUP(dupSize);
                        }
                        break;

                    case JitOps.JIT_POP:
                        OPCODE_USE(JitOps.JIT_POP);
                        {
                            uint popSize = GET_OP();
                            POP(popSize);
                        }
                        break;

                    case JitOps.JIT_POP_4:
                        OPCODE_USE(JitOps.JIT_POP_4);
                        POP(4);
                        break;

                    case JitOps.JIT_LOAD_I32:
                    case JitOps.JIT_LOAD_F32:
                        OPCODE_USE(JitOps.JIT_LOAD_I32);
                        {
                            uint value = GET_OP();
                            PUSH_U32(value);
                        }
                        break;

                    case JitOps.JIT_LOAD_I4_M1:
                        OPCODE_USE(JitOps.JIT_LOAD_I4_M1);
                        PUSH_U32(0xFFFFFFFFU);
                        break;

                    case JitOps.JIT_LOAD_I4_0:
                        OPCODE_USE(JitOps.JIT_LOAD_I4_0);
                        PUSH_U32(0);
                        break;

                    case JitOps.JIT_LOAD_I4_1:
                        OPCODE_USE(JitOps.JIT_LOAD_I4_1);
                        PUSH_U32(1);
                        break;

                    case JitOps.JIT_LOAD_I4_2:
                        OPCODE_USE(JitOps.JIT_LOAD_I4_2);
                        PUSH_U32(2);
                        break;

                    case JitOps.JIT_LOAD_I64:
                    case JitOps.JIT_LOAD_F64:
                        OPCODE_USE(JitOps.JIT_LOAD_I64);
                        {
                            ulong value = *(ulong*)pCurOp;
                            pCurOp += 2;
                            PUSH_U64(value);
                        }
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_INT32:
                    case JitOps.JIT_LOADPARAMLOCAL_F32:
                    case JitOps.JIT_LOADPARAMLOCAL_INTNATIVE: // Only on 32-bit
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_INT32);
                        {
                            uint ofs = GET_OP();
                            uint value = PARAMLOCAL_U32(ofs);
                            PUSH_U32(value);
                        }
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_O:
                    case JitOps.JIT_LOADPARAMLOCAL_PTR:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_O);
                        {
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            uint ofs = GET_OP();
                            uint value = PARAMLOCAL_U32(ofs);
                            PUSH_U32(value);
                    #else
                            uint ofs = GET_OP();
                            ulong value = PARAMLOCAL_U64(ofs);
                            PUSH_U64(value);
                    #endif
                        }
                        break;
                        
                        
                    case JitOps.JIT_LOADPARAMLOCAL_INT64:
                    case JitOps.JIT_LOADPARAMLOCAL_F64:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_INT64);
                        {
                            uint ofs = GET_OP();
                            ulong value = PARAMLOCAL_U64(ofs);
                            PUSH_U64(value);
                        }
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_VALUETYPE);
                        {
                            tMD_TypeDef *pTypeDef;
                            uint ofs;
                            byte* pMem;

                            ofs = GET_OP();
                            pTypeDef = (tMD_TypeDef*)GET_PTR();
                            pMem = pParamsLocals + ofs;
                            PUSH_VALUETYPE(pMem, pTypeDef->stackSize, pTypeDef->stackSize);
                        }
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_0:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_0);
                        PUSH_U32(PARAMLOCAL_U32(0));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_1:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_1);
                        PUSH_U32(PARAMLOCAL_U32(4));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_2:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_2);
                        PUSH_U32(PARAMLOCAL_U32(8));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_3:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_3);
                        PUSH_U32(PARAMLOCAL_U32(12));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_4:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_4);
                        PUSH_U32(PARAMLOCAL_U32(16));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_5:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_5);
                        PUSH_U32(PARAMLOCAL_U32(20));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_6:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_6);
                        PUSH_U32(PARAMLOCAL_U32(24));
                        break;

                    case JitOps.JIT_LOADPARAMLOCAL_7:
                        OPCODE_USE(JitOps.JIT_LOADPARAMLOCAL_7);
                        PUSH_U32(PARAMLOCAL_U32(28));
                        break;

                    case JitOps.JIT_LOAD_PARAMLOCAL_ADDR:
                        OPCODE_USE(JitOps.JIT_LOAD_PARAMLOCAL_ADDR);
                        {
                            uint ofs = GET_OP();
                            byte* pMem = pParamsLocals + ofs;
                            PUSH_PTR(pMem);
                        }
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_INT32:
                    case JitOps.JIT_STOREPARAMLOCAL_F32:
                    case JitOps.JIT_STOREPARAMLOCAL_INTNATIVE: // Only on 32-bit
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_INT32);
                        {
                            uint ofs = GET_OP();
                            uint value = POP_U32();
                            SET_PARAMLOCAL_U32(ofs, value);
                        }
                        break;

                        
                    case JitOps.JIT_STOREPARAMLOCAL_O:
                    case JitOps.JIT_STOREPARAMLOCAL_PTR:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_PTR);
                        {
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            uint ofs = GET_OP();
                            uint value = POP_U32();
                            SET_PARAMLOCAL_U32(ofs, value);
                    #else
                            uint ofs = GET_OP();
                            ulong value = POP_U64();
                            SET_PARAMLOCAL_U64(ofs, value);
                    #endif
                        }
                        break;
                        
                    case JitOps.JIT_STOREPARAMLOCAL_INT64:
                    case JitOps.JIT_STOREPARAMLOCAL_F64:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_INT64);
                        {
                            uint ofs = GET_OP();
                            ulong value = POP_U64();
                            SET_PARAMLOCAL_U64(ofs, value);
                        }
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_VALUETYPE);
                        {
                            tMD_TypeDef *pTypeDef;
                            uint ofs;
                            byte* pMem;

                            ofs = GET_OP();
                            pTypeDef = (tMD_TypeDef*)GET_PTR();
                            pMem = pParamsLocals + ofs;
                            POP_VALUETYPE(pMem, pTypeDef->stackSize, pTypeDef->stackSize);
                        }
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_0:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_0);
                        SET_PARAMLOCAL_U32(0, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_1:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_1);
                        SET_PARAMLOCAL_U32(4, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_2:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_2);
                        SET_PARAMLOCAL_U32(8, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_3:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_3);
                        SET_PARAMLOCAL_U32(12, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_4:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_4);
                        SET_PARAMLOCAL_U32(16, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_5:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_5);
                        SET_PARAMLOCAL_U32(20, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_6:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_6);
                        SET_PARAMLOCAL_U32(24, POP_U32());
                        break;

                    case JitOps.JIT_STOREPARAMLOCAL_7:
                        OPCODE_USE(JitOps.JIT_STOREPARAMLOCAL_7);
                        SET_PARAMLOCAL_U32(28, POP_U32());
                        break;

                    case JitOps.JIT_LOADINDIRECT_I8:
                    case JitOps.JIT_LOADINDIRECT_I16:
                    case JitOps.JIT_LOADINDIRECT_I32:
                    case JitOps.JIT_LOADINDIRECT_U8:
                    case JitOps.JIT_LOADINDIRECT_U16:
                    case JitOps.JIT_LOADINDIRECT_U32:
                    case JitOps.JIT_LOADINDIRECT_R32:
                        OPCODE_USE(JitOps.JIT_LOADINDIRECT_U32);
                        {
                            byte* pMem = POP_PTR();
                            uint value = *(uint*)pMem;
                            PUSH_U32(value);
                        }
                        break;

                    case JitOps.JIT_LOADINDIRECT_REF:
                        OPCODE_USE(JitOps.JIT_LOADINDIRECT_U32);
                        {
                            byte* pMem = POP_PTR();
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            uint value = *(uint*)pMem;
                            PUSH_U32(value);
                    #else
                            ulong value = *(ulong*)pMem;
                            PUSH_U64(value);
                    #endif
                        }
                        break;
                        
                    case JitOps.JIT_LOADINDIRECT_R64:
                    case JitOps.JIT_LOADINDIRECT_I64:
                        OPCODE_USE(JitOps.JIT_LOADINDIRECT_I64);
                        {
                            byte* pMem = POP_PTR();
                            ulong value = *(ulong*)pMem;
                            PUSH_U64(value);
                        }
                        break;

                    case JitOps.JIT_STOREINDIRECT_U8:
                    case JitOps.JIT_STOREINDIRECT_U16:
                    case JitOps.JIT_STOREINDIRECT_U32:
                        OPCODE_USE(JitOps.JIT_STOREINDIRECT_U32);
                        {
                            uint value = POP_U32(); // The value to store
                            byte* pMem = POP_PTR(); // The address to store to
                            *(uint*)pMem = value;
                        }
                        break;

                    case JitOps.JIT_STOREINDIRECT_REF:
                        OPCODE_USE(JitOps.JIT_STOREINDIRECT_U32);
                        {
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            uint value = POP_U32(); // The value to store
                            byte* pMem = POP_PTR(); // The address to store to
                            *(uint*)pMem = value;
                    #else
                            ulong value = POP_U64(); // The value to store
                            byte* pMem = POP_PTR(); // The address to store to
                            *(ulong*)pMem = value;
                    #endif
                        }
                        break;
                        
                    case JitOps.JIT_STORE_OBJECT_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_STORE_OBJECT_VALUETYPE);
                        {
                            uint size = GET_OP(); // The size, in bytes, of the value-type to store
                            uint memSize = (size<4)?4:size;
                            byte* pMem = pCurEvalStack - memSize - sizeof(void*);
                            POP_VALUETYPE(*(void**)pMem, size, memSize);
                            POP(4);
                        }
                        break;

                    case JitOps.JIT_CALL_PINVOKE:
                        OPCODE_USE(JitOps.JIT_CALL_PINVOKE);
                            throw new System.NotImplementedException();
        //                {
        //                    throw new System.NotImplementedException();
        //                    tJITCallPInvoke *pCallPInvoke;
        //                    uint res;
        //
        //                    pCallPInvoke = (tJITCallPInvoke*)(pCurOp - 1);
        //                    res = PInvoke.Call(pCallPInvoke, pParamsLocals, pCurrentMethodState->pEvalStack, pThread);
        //                    pCurrentMethodState->stackOfs = res;
        //                }
        //                goto JIT_RETURN_start;

                    case JitOps.JIT_CALL_NATIVE:
                        OPCODE_USE(JitOps.JIT_CALL_NATIVE);
                        {
                            tJITCallNative *pCallNative;
                            byte* pThis;
                            uint thisOfs;
                            tAsyncCall *pAsync;

                            //pCallNative = (tJITCallNative*)&(pJIT->pOps[pCurrentMethodState->ipOffset - 1]);
                            pCallNative = (tJITCallNative*)(pCurOp - 1);
                            if (MetaData.METHOD_ISSTATIC(pCallNative->pMethodDef)) {
                                pThis = null;
                                thisOfs = 0;
                            } else {
                                pThis = *(byte**)pCurrentMethodState->pParamsLocals;
                                thisOfs = (uint)sizeof(void*);
                            }
                            // Internal constructors MUST leave the newly created object in the return value
                            // (ie on top of the evaluation stack)
                            fnInternalCall fn = (fnInternalCall)H.ToObj(pCallNative->fn);
                            pAsync = fn(pThis, pCurrentMethodState->pParamsLocals + thisOfs, pCurrentMethodState->pEvalStack);
                            if (pAsync != null) {
                                // Save the method state
                                SAVE_METHOD_STATE();
                                // Change the IP pointer to point to the return instruction
                                pCurrentMethodState->ipOffset = 3;
                                // Handle special async codes
                                if (pAsync == Thread.ASYNC_LOCK_EXIT()) {
                                    return Thread.THREAD_STATUS_LOCK_EXIT;
                                }
                                // Set the async in the thread
                                pThread->pAsync = pAsync;
                                return Thread.THREAD_STATUS_ASYNC;
                            }
                        }
                        // fall-through
                        goto JIT_RETURN_start;

                    case JitOps.JIT_RETURN:
                    JIT_RETURN_start:
                        OPCODE_USE(JitOps.JIT_RETURN);
                        {
                        #if TRACE
                                Sys.log_f(2, "Returned from %s() to %s()\n", (PTR)pCurrentMethodState->pMethod->name, (pCurrentMethodState->pCaller != null)?(PTR)pCurrentMethodState->pCaller->pMethod->name:(PTR)((byte*)(new S(ref scNone, "<none>"))));
                        #endif
                            if (pCurrentMethodState->pCaller == null) {
                                // End of thread!
                                if (pCurrentMethodState->pMethod->pReturnType == Type.types[Type.TYPE_SYSTEM_INT32]) {
                                    // If function returned an int32, then make it the thread exit-value
                                    pThread->threadExitValue = (int)POP_U32();
                                }
                                return Thread.THREAD_STATUS_EXIT;
                            }
                            // Make u32Value the number of bytes of the return value from the function
                            if (pCurrentMethodState->pMethod->pReturnType != null) {
                                u32Value = pCurrentMethodState->pMethod->pReturnType->stackSize;
                            } else if (pCurrentMethodState->isInternalNewObjCall != 0) {
                                u32Value = (uint)sizeof(void*);
                            } else {
                                u32Value = 0;
                            }
                            byte* pEvalStk = pCurrentMethodState->pEvalStack;
                            {
                                tMethodState *pOldMethodState = pCurrentMethodState;
                                pThread->pCurrentMethodState = pCurrentMethodState->pCaller;
                                LOAD_METHOD_STATE();
                                // Copy return value to callers evaluation stack
                                if (u32Value > 0) {
                                    Mem.memmove(pCurEvalStack, pEvalStk, (SIZE_T)u32Value);
                                    pCurEvalStack += u32Value;
                                }
                                // Delete the current method state and go back to callers method state
                                MethodState.Delete(pThread, ref pOldMethodState);
                            }
                            if (pCurrentMethodState->pNextDelegate == null) {
                                break;
                            }
                        }
                        // Fall-through if more delegate methods to invoke
                        goto JIT_INVOKE_DELEGATE_start;

                    case JitOps.JIT_INVOKE_DELEGATE:
                    JIT_INVOKE_DELEGATE_start:
                        OPCODE_USE(JitOps.JIT_INVOKE_DELEGATE);
                        {
                            tMD_MethodDef* pDelegateMethod, pCallMethod;
                            void* pDelegate;
                            /*HEAP_PTR*/byte* pDelegateThis;
                            tMethodState* pCallMethodState;
                            uint ofs;

                            if (pCurrentMethodState->pNextDelegate == null) {
                                // First delegate, so get the Invoke() method defined within the delegate class
                                pDelegateMethod = (tMD_MethodDef*)GET_PTR();
                                // Take the params off the stack. This is the pointer to the tDelegate & params
                                //pCurrentMethodState->stackOfs -= pDelegateMethod->parameterStackSize;
                                pCurEvalStack -= pDelegateMethod->parameterStackSize;
                                // Allocate memory for delegate params
                                pCurrentMethodState->pDelegateParams = Mem.malloc((SIZE_T)(pDelegateMethod->parameterStackSize - sizeof(void*)));
                                Mem.memcpy(
                                    pCurrentMethodState->pDelegateParams,
                                    //pCurrentMethodState->pEvalStack + pCurrentMethodState->stackOfs + sizeof(void*),
                                    pCurEvalStack + sizeof(void*),
                                    (SIZE_T)(pDelegateMethod->parameterStackSize - sizeof(void*)));
                                // Get the actual delegate heap pointer
                                pDelegate = *(void**)pCurEvalStack;
                            } else {
                                pDelegateMethod = Delegate.GetMethod(pCurrentMethodState->pNextDelegate);
                                if (pDelegateMethod->pReturnType != null) {
                                    pCurEvalStack -= pDelegateMethod->pReturnType->stackSize;
                                }
                                // Get the actual delegate heap pointer
                                pDelegate = pCurrentMethodState->pNextDelegate;
                            }
                            if (pDelegate == null) {
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_NULLREFERENCEEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            // Get the real method to call; the target of the delegate.
                            pCallMethod = Delegate.GetMethodAndStore(pDelegate, &pDelegateThis, &pCurrentMethodState->pNextDelegate);
                            // Set up the call method state for the call.
                            pCallMethodState = MethodState.Direct(pThread, pCallMethod, pCurrentMethodState, 0);
                            if (pDelegateThis != null) {
                                *(/*HEAP_PTR*/byte**)pCallMethodState->pParamsLocals = pDelegateThis;
                                ofs = (uint)sizeof(void*);
                            } else {
                                ofs = 0;
                            }
                            Mem.memcpy(pCallMethodState->pParamsLocals + ofs,
                                pCurrentMethodState->pDelegateParams,
                                pCallMethod->parameterStackSize - ofs);
                            CHANGE_METHOD_STATE(pCallMethodState);
                        }
                        break;

                    case JitOps.JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE:
                        OPCODE_USE(JitOps.JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE);
                        {
                            // Get the reference to MethodBase.Invoke
                            tMD_MethodDef *pInvokeMethod = (tMD_MethodDef*)GET_PTR();

                            // Take the MethodBase.Invoke params off the stack.
                            pCurEvalStack -= pInvokeMethod->parameterStackSize;

                            // Get a pointer to the MethodBase instance (e.g., a MethodInfo or ConstructorInfo),
                            // and from that, determine which method we're going to invoke
                            tMethodBase *pMethodBase = *(tMethodBase**)pCurEvalStack;
                            tMD_MethodDef *pCallMethod = pMethodBase->methodDef;

                            // Store the return type so that JitOps.JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE can
                            // interpret the stack after the invocation
                            pCurrentMethodState->pReflectionInvokeReturnType = pCallMethod->pReturnType;

                            // Get the 'this' pointer for the call and the params array
                            byte* invocationThis = (byte*)*(tMethodBase**)(pCurEvalStack + sizeof(/*HEAP_PTR*/byte*));
                            /*HEAP_PTR*/byte* invocationParamsArray = *(/*HEAP_PTR*/byte**)(pCurEvalStack + sizeof(/*HEAP_PTR*/byte*) + sizeof(byte*));     

                            // Put the new 'this' on the stack
                            byte* pPrevEvalStack = pCurEvalStack;
                            PUSH_PTR(invocationThis);

                            // Put any other params on the stack
                            if (invocationParamsArray != null) {
                                uint invocationParamsArrayLength = System_Array.GetLength(invocationParamsArray);
                                byte* invocationParamsArrayElements = System_Array.GetElements(invocationParamsArray);
                                for (uint paramIndex = 0; paramIndex < invocationParamsArrayLength; paramIndex++) {
                                    /*HEAP_PTR*/byte* currentParam = (byte*)((uint*)(invocationParamsArrayElements))[paramIndex];
                                    if (currentParam == null) {
                                        PUSH_O(null);
                                    } else {
                                        tMD_TypeDef *currentParamType = Heap.GetType(currentParam);

                                        if (Type.IsValueType(currentParamType) != 0) {
                                            PUSH_VALUETYPE(currentParam, currentParamType->stackSize, currentParamType->stackSize);
                                        } else {
                                            PUSH_O(currentParam);
                                        }
                                    }
                                }
                            }
                            pCurEvalStack = pPrevEvalStack;

                            // Change interpreter state so we continue execution inside the method being invoked
                            tMethodState *pCallMethodState = MethodState.Direct(pThread, pCallMethod, pCurrentMethodState, 0);
                            Mem.memcpy(pCallMethodState->pParamsLocals, pCurEvalStack, pCallMethod->parameterStackSize);
                            CHANGE_METHOD_STATE(pCallMethodState);
                        }
                        break;

                    case JitOps.JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE:
                        OPCODE_USE(JitOps.JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE);
                        {
                            tMD_TypeDef *pLastInvocationReturnType = pCurrentMethodState->pReflectionInvokeReturnType;
                            if (pLastInvocationReturnType == null) {
                                // It was a void method, so it won't have put anything on the stack. We need to put
                                // a null value there as a return value, because MethodBase.Invoke isn't void.
                                PUSH_O(null);
                            } else if (Type.IsValueType(pLastInvocationReturnType) != 0) {
                                // For value Type.types, remove the raw value data from the stack and replace it with a
                                // boxed copy, because MethodBase.Invoke returns object.
                                /*HEAP_PTR*/byte* heapPtr = Heap.AllocType(pLastInvocationReturnType);
                                POP_VALUETYPE(heapPtr, pLastInvocationReturnType->stackSize, pLastInvocationReturnType->stackSize);
                                PUSH_O(heapPtr);
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_DEREF_CALLVIRT:
                        op = JitOps.JIT_DEREF_CALLVIRT;
                        goto allCallStart;
                    case JitOps.JIT_BOX_CALLVIRT:
                        op = JitOps.JIT_BOX_CALLVIRT;
                        goto allCallStart;
                    case JitOps.JIT_CALL_PTR: // Note that JitOps.JIT_CALL_PTR cannot be virtual
                        op = JitOps.JIT_CALL_PTR;
                        goto allCallStart;
                    case JitOps.JIT_CALLVIRT_O:
                        op = JitOps.JIT_CALLVIRT_O;
                        goto allCallStart;
                    case JitOps.JIT_CALL_O:
                        op = JitOps.JIT_CALL_O;
                        goto allCallStart;
                    case JitOps.JIT_CALL_INTERFACE:
                        op = JitOps.JIT_CALL_INTERFACE;
                    allCallStart:
                        OPCODE_USE(JitOps.JIT_CALL_O);
                        {
                            tMD_MethodDef *pCallMethod = null;
                            tMethodState *pCallMethodState = null;
                            tMD_TypeDef *pBoxCallType = null;
                            byte* heapPtr = null;
                            byte* pMem = null;

                            if (op == JitOps.JIT_BOX_CALLVIRT) {
                                pBoxCallType = (tMD_TypeDef*)GET_PTR();
                            }

                            pCallMethod = (tMD_MethodDef*)GET_PTR();

                            if (op == JitOps.JIT_BOX_CALLVIRT) {
                                // Need to de-ref and box the value-type before calling the function
                                // TODO: Will this work on value-Type.types that are not 4 bytes long?
                                pMem = pCurEvalStack - pCallMethod->parameterStackSize;
                                heapPtr = Heap.Box(pBoxCallType, *(byte**)pMem);
                                *(/*HEAP_PTR**/byte**)pMem = heapPtr;
                            } else if (op == JitOps.JIT_DEREF_CALLVIRT) {
                                pMem = pCurEvalStack - pCallMethod->parameterStackSize;
                                // *(/*HEAP_PTR**/byte**)pMem = **(/*HEAP_PTR***/byte***)pMem;
                                heapPtr = *(byte**)pMem;    // NOTE: Need to do this in two steps or WebGL IL2CPP won't build..
                                heapPtr = *(byte**)heapPtr;
                                *(/*HEAP_PTR*/byte**)pMem = heapPtr;
                            }

                            // If it's a virtual call then find the real correct method to call
                            if (op == JitOps.JIT_CALLVIRT_O || op == JitOps.JIT_BOX_CALLVIRT || op == JitOps.JIT_DEREF_CALLVIRT) {
                                tMD_TypeDef *pThisType;
                                // Get the actual object that is becoming 'this'
                                if (heapPtr == null) {
                                    heapPtr = *(/*HEAP_PTR*/byte**)(pCurEvalStack - pCallMethod->parameterStackSize);
                                }
                                if (heapPtr == null) {
                                    //Sys.Crash("null 'this' in Virtual call: %s", Sys_GetMethodDesc(pCallMethod));
                                    pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_NULLREFERENCEEXCEPTION]);
                                    goto throwHeapPtr;
                                }
                                pThisType = Heap.GetType(heapPtr);
                                if (MetaData.METHOD_ISVIRTUAL(pCallMethod)) {
                                    pCallMethod = pThisType->pVTable[pCallMethod->vTableOfs];
                                }
                            } else if (op == JitOps.JIT_CALL_INTERFACE) {
                                tMD_TypeDef* pInterface, pThisType;
                                uint vIndex = 0xffffffff;
                                int i;

                                pInterface = pCallMethod->pParentType;
                                // Get the actual object that is becoming 'this'
                                heapPtr = *(/*HEAP_PTR*/byte**)(pCurEvalStack - pCallMethod->parameterStackSize);
                                pThisType = Heap.GetType(heapPtr);
                                // This must be searched backwards so if an interface is implemented more than
                                // once in the type hierarchy, the most recent definition gets called
                                for (i=(int)pThisType->numInterfaces-1; i >= 0; i--) {
                                    if (pThisType->pInterfaceMaps[i].pInterface == pInterface) {
                                        // Found the right interface map
                                        if (pThisType->pInterfaceMaps[i].pVTableLookup != null) {
                                            vIndex = pThisType->pInterfaceMaps[i].pVTableLookup[pCallMethod->vTableOfs];
                                            UnityEngine.Assertions.Assert.IsTrue(vIndex != 0xffffffff);
                                            pCallMethod = pThisType->pVTable[vIndex];
                                        } else {
                                            pCallMethod = pThisType->pInterfaceMaps[i].ppMethodVLookup[pCallMethod->vTableOfs];
                                        }
                                        break;
                                    }
                                }
                            }
                            //printf("Calling method: %s\n", Sys.GetMethodDesc(pCallMethod));
                            // Set up the new method state for the called method
                            pCallMethodState = MethodState.Direct(pThread, pCallMethod, pCurrentMethodState, 0);
                            // Set up the parameter stack for the method being called
                            byte* pTempPtr = pCurEvalStack;
                            CreateParameters(pCallMethodState->pParamsLocals, pCallMethod, &/*pCurEvalStack*/pTempPtr, null);
                            pCurEvalStack = pTempPtr;
                            // Set up the local variables for the new method state
                            CHANGE_METHOD_STATE(pCallMethodState);
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BRANCH:
                        OPCODE_USE(JitOps.JIT_BRANCH);
                        {
                            uint ofs = GET_OP();
                            pCurOp = pOps + ofs;
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_SWITCH:
                        OPCODE_USE(JitOps.JIT_SWITCH);
                        {
                            uint ofs;
                            // The number of jump targets
                            uint numTargets = GET_OP();
                            // The jump target selected
                            uint target = POP_U32();
                            if (target >= numTargets) {
                                // This is not a valid jump target, so fall-through
                                pCurOp += numTargets;
                            } else {
                                ofs = *(pCurOp + target);
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BRANCH_TRUE_U32:
                        OPCODE_USE(JitOps.JIT_BRANCH_TRUE_U32);
                        {
                            uint value = POP_U32();
                            uint ofs = GET_OP();
                            if (value != 0) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BRANCH_TRUE_U64:
                        OPCODE_USE(JitOps.JIT_BRANCH_TRUE_U64);
                        {
                            ulong value = POP_U64();
                            uint ofs = GET_OP();
                            if (value != 0) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BRANCH_FALSE_U32:
                        OPCODE_USE(JitOps.JIT_BRANCH_FALSE_U32);
                        {
                            uint value = POP_U32();
                            uint ofs = GET_OP();
                            if (value == 0) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;


                    case JitOps.JIT_BRANCH_FALSE_U64:
                        OPCODE_USE(JitOps.JIT_BRANCH_FALSE_U64);
                        {
                            ulong value = POP_U64();
                            uint ofs = GET_OP();
                            if (value == 0) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BEQ_I32I32:
                        OPCODE_USE(JitOps.JIT_BEQ_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if ((int)v1 == (int)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BEQ_I64I64:
                        OPCODE_USE(JitOps.JIT_BEQ_I64I64);
                        {
                            ulong v1, v2;
                            uint ofs;
                            POP_U64_U64(out v1, out v2);
                            ofs = GET_OP();
                            if ((long)v1 == (long)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BEQ_F32F32:
                        OPCODE_USE(JitOps.JIT_BEQ_F32F32);
                        {
                            float v1, v2;
                            uint ofs;
                            POP_F32_F32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 == v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BEQ_F64F64:
                        OPCODE_USE(JitOps.JIT_BEQ_F64F64);
                        {
                            double v1, v2;
                            uint ofs;
                            POP_F64_F64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 == v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGE_I32I32:
                        OPCODE_USE(JitOps.JIT_BGE_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if ((int)v1 >= (int)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGE_I64I64:
                        OPCODE_USE(JitOps.JIT_BGE_I64I64);
                        {
                            ulong v1, v2;
                            uint ofs;
                            POP_U64_U64(out v1, out v2);
                            ofs = GET_OP();
                            if ((long)v1 >= (long)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;


                    case JitOps.JIT_BGE_F32F32:
                    case JitOps.JIT_BGE_UN_F32F32:
                        OPCODE_USE(JitOps.JIT_BGE_F32F32);
                        {
                            float v1, v2;
                            uint ofs;
                            POP_F32_F32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 >= v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;


                    case JitOps.JIT_BGE_F64F64:
                    case JitOps.JIT_BGE_UN_F64F64:
                        OPCODE_USE(JitOps.JIT_BGE_F64F64);
                        {
                            double v1, v2;
                            uint ofs;
                            POP_F64_F64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 >= v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;


                    case JitOps.JIT_BGT_I32I32:
                        OPCODE_USE(JitOps.JIT_BGT_I32I32);
                        {
                            uint v1, v2;
                            uint ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if ((int)v1 > (int)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGT_I64I64:
                        OPCODE_USE(JitOps.JIT_BGT_I64I64);
                        {
                            ulong v1, v2;
                            uint ofs;
                            POP_U64_U64(out v1, out v2);
                            ofs = GET_OP();
                            if ((long)v1 > (long)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGT_F32F32:
                    case JitOps.JIT_BGT_UN_F32F32:
                        OPCODE_USE(JitOps.JIT_BGT_F32F32);
                        {
                            float v1, v2;
                            uint ofs;
                            POP_F32_F32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 > v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGT_F64F64:
                    case JitOps.JIT_BGT_UN_F64F64:
                        OPCODE_USE(JitOps.JIT_BGT_F64F64);
                        {
                            double v1, v2;
                            uint ofs;
                            POP_F64_F64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 > v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLE_I32I32:
                        OPCODE_USE(JitOps.JIT_BLE_I32I32);
                        {
                            uint v1, v2;
                            uint ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if ((int)v1 <= (int)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLE_I64I64:
                        OPCODE_USE(JitOps.JIT_BLE_I64I64);
                        {
                            ulong v1, v2;
                            uint ofs;
                            POP_U64_U64(out v1, out v2);
                            ofs = GET_OP();
                            if ((long)v1 <= (long)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLE_F32F32:
                    case JitOps.JIT_BLE_UN_F32F32:
                        OPCODE_USE(JitOps.JIT_BLE_F32F32);
                        {
                            float v1, v2;
                            uint ofs;
                            POP_F32_F32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 <= v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLE_F64F64:
                    case JitOps.JIT_BLE_UN_F64F64:
                        OPCODE_USE(JitOps.JIT_BLE_F64F64);
                        {
                            double v1, v2;
                            uint ofs;
                            POP_F64_F64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 <= v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLT_I32I32:
                        OPCODE_USE(JitOps.JIT_BLT_I32I32);
                        {
                            uint v1, v2;
                            uint ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if ((int)v1 < (int)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLT_I64I64:
                        OPCODE_USE(JitOps.JIT_BLT_I64I64);
                        {
                            ulong v1, v2;
                            uint ofs;
                            POP_U64_U64(out v1, out v2);
                            ofs = GET_OP();
                            if ((long)v1 < (long)v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLT_F32F32:
                    case JitOps.JIT_BLT_UN_F32F32:
                        OPCODE_USE(JitOps.JIT_BLT_F32F32);
                        {
                            float v1, v2;
                            uint ofs;
                            POP_F32_F32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 < v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLT_F64F64:
                    case JitOps.JIT_BLT_UN_F64F64:
                        OPCODE_USE(JitOps.JIT_BLT_F64F64);
                        {
                            double v1, v2;
                            uint ofs;
                            POP_F64_F64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 < v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BNE_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_BNE_UN_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 != v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BNE_UN_I64I64:
                        OPCODE_USE(JitOps.JIT_BNE_UN_I64I64);
                        {
                            ulong v1, v2;
                            uint ofs;
                            POP_U64_U64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 != v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BNE_UN_F32F32:
                        OPCODE_USE(JitOps.JIT_BNE_UN_F32F32);
                        {
                            float v1, v2;
                            uint ofs;
                            POP_F32_F32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 != v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BNE_UN_F64F64:
                        OPCODE_USE(JitOps.JIT_BNE_UN_F64F64);
                        {
                            double v1, v2;
                            uint ofs;
                            POP_F64_F64(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 != v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGE_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_BGE_UN_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 >= v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BGT_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_BGT_UN_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 > v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLE_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_BLE_UN_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 <= v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_BLT_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_BLT_UN_I32I32);
                        {
                            uint v1, v2, ofs;
                            POP_U32_U32(out v1, out v2);
                            ofs = GET_OP();
                            if (v1 < v2) {
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_CEQ_I32I32: // Handles int and O
                        OPCODE_USE(JitOps.JIT_CEQ_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) == *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CGT_I32I32:
                        OPCODE_USE(JitOps.JIT_CGT_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(int*)(pCurEvalStack - sizeof(uint)) > *(int*)(pCurEvalStack - sizeof(uint) + sizeof(int)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CGT_UN_I32I32: // Handles int and O
                        OPCODE_USE(JitOps.JIT_CGT_UN_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) > *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CLT_I32I32:
                        OPCODE_USE(JitOps.JIT_CLT_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(int*)(pCurEvalStack - sizeof(uint)) < *(int*)(pCurEvalStack - sizeof(uint) + sizeof(int)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CLT_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_CLT_UN_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) < *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CEQ_I64I64:
                        OPCODE_USE(JitOps.JIT_CEQ_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(ulong*)(pCurEvalStack - sizeof(uint)) == *(ulong*)(pCurEvalStack - sizeof(uint) + sizeof(ulong)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CGT_I64I64:
                        OPCODE_USE(JitOps.JIT_CGT_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(long*)(pCurEvalStack - sizeof(uint)) > *(long*)(pCurEvalStack - sizeof(uint) + sizeof(long)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CGT_UN_I64I64:
                        OPCODE_USE(JitOps.JIT_CGT_UN_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(ulong*)(pCurEvalStack - sizeof(uint)) > *(ulong*)(pCurEvalStack - sizeof(uint) + sizeof(ulong)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CLT_I64I64:
                        OPCODE_USE(JitOps.JIT_CLT_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(long*)(pCurEvalStack - sizeof(uint)) < *(long*)(pCurEvalStack - sizeof(uint) + sizeof(long)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CLT_UN_I64I64:
                        OPCODE_USE(JitOps.JIT_CLT_UN_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(ulong*)(pCurEvalStack - sizeof(uint)) < *(ulong*)(pCurEvalStack - sizeof(uint) + sizeof(ulong)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CEQ_F32F32:
                        OPCODE_USE(JitOps.JIT_CEQ_F32F32);
                        pCurEvalStack -= sizeof(float) + sizeof(float) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(float*)(pCurEvalStack - sizeof(uint)) == *(float*)(pCurEvalStack - sizeof(uint) + sizeof(float)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CEQ_F64F64:
                        OPCODE_USE(JitOps.JIT_CEQ_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(double*)(pCurEvalStack - sizeof(uint)) == *(double*)(pCurEvalStack - sizeof(uint) + sizeof(double)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CGT_F32F32:
                        OPCODE_USE(JitOps.JIT_CGT_F32F32);
                        pCurEvalStack -= sizeof(float) + sizeof(float) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(float*)(pCurEvalStack - sizeof(uint)) > *(float*)(pCurEvalStack - sizeof(uint) + sizeof(float)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CGT_F64F64:
                        OPCODE_USE(JitOps.JIT_CGT_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(double*)(pCurEvalStack - sizeof(uint)) > *(double*)(pCurEvalStack - sizeof(uint) + sizeof(double)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CLT_F32F32:
                        OPCODE_USE(JitOps.JIT_CLT_F32F32);
                        pCurEvalStack -= sizeof(float) + sizeof(float) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(float*)(pCurEvalStack - sizeof(uint)) < *(float*)(pCurEvalStack - sizeof(uint) + sizeof(float)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_CLT_F64F64:
                        OPCODE_USE(JitOps.JIT_CLT_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(double*)(pCurEvalStack - sizeof(uint)) < *(double*)(pCurEvalStack - sizeof(uint) + sizeof(double)) ? 1U : 0U;
                        break;

                    case JitOps.JIT_ADD_OVF_I32I32:
                        OPCODE_USE(JitOps.JIT_ADD_OVF_I32I32);
                        {
                            uint v1, v2;
                            long res;
                            POP_U32_U32(out v1, out v2);
                            res = (long)(int)v1 + (long)(int)v2;
                            if ((ulong)res > 0x7fffffffUL || (ulong)res < 0xffffffff80000000UL) {
                                // Overflowed, so throw exception
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            PUSH_U32((uint)(int)res);
                        }
                        break;

                    case JitOps.JIT_ADD_OVF_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_ADD_OVF_UN_I32I32);
                        {
                            uint v1, v2;
                            ulong res;
                            POP_U32_U32(out v1, out v2);
                            res = (ulong)v1 + (ulong)v2;
                            if (res > 0xffffffffUL) {
                                // Overflowed, so throw exception
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            PUSH_U32((uint)(int)res);
                        }
                        break;

                    case JitOps.JIT_MUL_OVF_I32I32:
                        OPCODE_USE(JitOps.JIT_MUL_OVF_I32I32);
                        {
                            uint v1, v2;
                            long res;
                            POP_U32_U32(out v1, out v2);
                            res = (long)(int)v1 * (long)(int)v2;
                            if ((ulong)res > 0x7fffffffUL || (ulong)res < 0xffffffff80000000UL) {
                                // Overflowed, so throw exception
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            PUSH_U32((uint)res);
                        }
                        break;

                    case JitOps.JIT_MUL_OVF_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_MUL_OVF_UN_I32I32);
                        {
                            uint v1, v2;
                            ulong res;
                            POP_U32_U32(out v1, out v2);
                            res = (ulong)v1 * (ulong)v2;
                            if (res > (ulong)0xffffffff) {
                                // Overflowed, so throw exception
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            PUSH_U32((uint)res);
                        }
                        break;

                    case JitOps.JIT_SUB_OVF_I32I32:
                        OPCODE_USE(JitOps.JIT_SUB_OVF_I32I32);
                        {
                            uint v1, v2;
                            long res;
                            POP_U32_U32(out v1, out v2);
                            res = (long)(int)v1 - (long)(int)v2;
                            if ((ulong)res > 0x7fffffffUL || (ulong)res < 0xffffffff80000000UL) {
                                // Overflowed, so throw exception
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            PUSH_U32((uint)(int)res);
                        }
                        break;

                    case JitOps.JIT_SUB_OVF_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_SUB_OVF_UN_I32I32);
                        {
                            uint v1, v2;
                            ulong res;
                            POP_U32_U32(out v1, out v2);
                            res = (ulong)v1 - (ulong)v2;
                            if (res > (ulong)0xffffffff) {
                                // Overflowed, so throw exception
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
                                goto throwHeapPtr;
                            }
                            PUSH_U32((uint)res);
                        }
                        break;

                    case JitOps.JIT_ADD_I32I32:
                        OPCODE_USE(JitOps.JIT_ADD_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(int); 
                        *(int*)(pCurEvalStack - sizeof(int)) = *(int*)(pCurEvalStack - sizeof(int)) + *(int*)(pCurEvalStack - sizeof(int) + sizeof(int));
                        break;

                    case JitOps.JIT_ADD_I64I64:
                        OPCODE_USE(JitOps.JIT_ADD_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(long); 
                        *(long*)(pCurEvalStack - sizeof(long)) = *(long*)(pCurEvalStack - sizeof(long)) + *(long*)(pCurEvalStack - sizeof(long) + sizeof(long));
                        break;

                    case JitOps.JIT_ADD_F32F32:
                        OPCODE_USE(JitOps.JIT_ADD_F32F32);
                        pCurEvalStack -= sizeof(float) + sizeof(float) - sizeof(float); 
                        *(float*)(pCurEvalStack - sizeof(float)) = *(float*)(pCurEvalStack - sizeof(float)) + *(float*)(pCurEvalStack - sizeof(float) + sizeof(float));
                        break;

                    case JitOps.JIT_ADD_F64F64:
                        OPCODE_USE(JitOps.JIT_ADD_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(double); 
                        *(double*)(pCurEvalStack - sizeof(double)) = *(double*)(pCurEvalStack - sizeof(double)) + *(double*)(pCurEvalStack - sizeof(double) + sizeof(double));
                        break;

                    case JitOps.JIT_SUB_I32I32:
                        OPCODE_USE(JitOps.JIT_SUB_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(int); 
                        *(int*)(pCurEvalStack - sizeof(int)) = *(int*)(pCurEvalStack - sizeof(int)) - *(int*)(pCurEvalStack - sizeof(int) + sizeof(int));
                        break;

                    case JitOps.JIT_SUB_I64I64:
                        OPCODE_USE(JitOps.JIT_SUB_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(long);
                        *(long*)(pCurEvalStack - sizeof(long)) = *(long*)(pCurEvalStack - sizeof(long)) - *(long*)(pCurEvalStack - sizeof(long) + sizeof(long));
                        break;

                    case JitOps.JIT_SUB_F32F32:
                        OPCODE_USE(JitOps.JIT_SUB_F32F32);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(double);
                        *(double*)(pCurEvalStack - sizeof(double)) = *(double*)(pCurEvalStack - sizeof(double)) - *(double*)(pCurEvalStack - sizeof(double) + sizeof(double));
                        break;

                    case JitOps.JIT_SUB_F64F64:
                        OPCODE_USE(JitOps.JIT_SUB_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(double); 
                        *(double*)(pCurEvalStack - sizeof(double)) = *(double*)(pCurEvalStack - sizeof(double)) - *(double*)(pCurEvalStack - sizeof(double) + sizeof(double));
                        break;

                    case JitOps.JIT_MUL_I32I32:
                        OPCODE_USE(JitOps.JIT_MUL_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(int); 
                        *(int*)(pCurEvalStack - sizeof(int)) = *(int*)(pCurEvalStack - sizeof(int)) * *(int*)(pCurEvalStack - sizeof(int) + sizeof(int));
                        break;

                    case JitOps.JIT_MUL_I64I64:
                        OPCODE_USE(JitOps.JIT_MUL_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(long); 
                        *(long*)(pCurEvalStack - sizeof(long)) = *(long*)(pCurEvalStack - sizeof(long)) * *(long*)(pCurEvalStack - sizeof(long) + sizeof(long));
                        break;

                    case JitOps.JIT_MUL_F32F32:
                        OPCODE_USE(JitOps.JIT_MUL_F32F32);
                        pCurEvalStack -= sizeof(float) + sizeof(float) - sizeof(float); 
                        *(float*)(pCurEvalStack - sizeof(float)) = *(float*)(pCurEvalStack - sizeof(float)) * *(float*)(pCurEvalStack - sizeof(float) + sizeof(float));
                        break;

                    case JitOps.JIT_MUL_F64F64:
                        OPCODE_USE(JitOps.JIT_MUL_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(double); 
                        *(double*)(pCurEvalStack - sizeof(double)) = *(double*)(pCurEvalStack - sizeof(double)) * *(double*)(pCurEvalStack - sizeof(double) + sizeof(double));
                        break;

                    case JitOps.JIT_DIV_I32I32:
                        OPCODE_USE(JitOps.JIT_DIV_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(int); 
                        *(int*)(pCurEvalStack - sizeof(int)) = *(int*)(pCurEvalStack - sizeof(int)) / *(int*)(pCurEvalStack - sizeof(int) + sizeof(int));
                        break;

                    case JitOps.JIT_DIV_I64I64:
                        OPCODE_USE(JitOps.JIT_DIV_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(long); 
                        *(long*)(pCurEvalStack - sizeof(long)) = *(long*)(pCurEvalStack - sizeof(long)) / *(long*)(pCurEvalStack - sizeof(long) + sizeof(long));
                        break;

                    case JitOps.JIT_DIV_F32F32:
                        OPCODE_USE(JitOps.JIT_DIV_F32F32);
                        pCurEvalStack -= sizeof(float) + sizeof(float) - sizeof(float); 
                        *(float*)(pCurEvalStack - sizeof(float)) = *(float*)(pCurEvalStack - sizeof(float)) / *(float*)(pCurEvalStack - sizeof(float) + sizeof(float));
                        break;

                    case JitOps.JIT_DIV_F64F64:
                        OPCODE_USE(JitOps.JIT_DIV_F64F64);
                        pCurEvalStack -= sizeof(double) + sizeof(double) - sizeof(double); 
                        *(double*)(pCurEvalStack - sizeof(double)) = *(double*)(pCurEvalStack - sizeof(double)) / *(double*)(pCurEvalStack - sizeof(double) + sizeof(double));
                        break;

                    case JitOps.JIT_DIV_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_DIV_UN_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) / *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_DIV_UN_I64I64:
                        OPCODE_USE(JitOps.JIT_DIV_UN_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(ulong); 
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) / *(ulong*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                    case JitOps.JIT_REM_I32I32:
                        OPCODE_USE(JitOps.JIT_REM_I32I32);
                        pCurEvalStack -= sizeof(int) + sizeof(int) - sizeof(int); 
                        *(int*)(pCurEvalStack - sizeof(int)) = *(int*)(pCurEvalStack - sizeof(int)) % *(int*)(pCurEvalStack - sizeof(int) + sizeof(int));
                        break;

                    case JitOps.JIT_REM_I64I64:
                        OPCODE_USE(JitOps.JIT_REM_I64I64);
                        pCurEvalStack -= sizeof(long) + sizeof(long) - sizeof(long); 
                        *(long*)(pCurEvalStack - sizeof(long)) = *(long*)(pCurEvalStack - sizeof(long)) % *(long*)(pCurEvalStack - sizeof(long) + sizeof(long));
                        break;

                    case JitOps.JIT_REM_UN_I32I32:
                        OPCODE_USE(JitOps.JIT_REM_UN_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) % *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_REM_UN_I64I64:
                        OPCODE_USE(JitOps.JIT_REM_UN_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(ulong);
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) % *(ulong*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                    case JitOps.JIT_AND_I32I32:
                        OPCODE_USE(JitOps.JIT_AND_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) & *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_AND_I64I64:
                        OPCODE_USE(JitOps.JIT_AND_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(ulong); 
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) & *(ulong*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                    case JitOps.JIT_OR_I32I32:
                        OPCODE_USE(JitOps.JIT_OR_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) | *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_OR_I64I64:
                        OPCODE_USE(JitOps.JIT_OR_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(ulong); 
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) | *(ulong*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                    case JitOps.JIT_XOR_I32I32:
                        OPCODE_USE(JitOps.JIT_XOR_I32I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) ^ *(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_XOR_I64I64:
                        OPCODE_USE(JitOps.JIT_XOR_I64I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(ulong) - sizeof(ulong); 
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) ^ *(ulong*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                    case JitOps.JIT_NEG_I32:
                        OPCODE_USE(JitOps.JIT_NEG_I32);
                        *(int*)(pCurEvalStack - sizeof(int)) = - *(int*)(pCurEvalStack - sizeof(int));
                        break;

                    case JitOps.JIT_NEG_I64:
                        OPCODE_USE(JitOps.JIT_NEG_I64);
                        *(long*)(pCurEvalStack - sizeof(long)) = - *(long*)(pCurEvalStack - sizeof(long));
                        break;

                    case JitOps.JIT_NOT_I32:
                        OPCODE_USE(JitOps.JIT_NOT_I32);
                        *(uint*)(pCurEvalStack - sizeof(uint)) = ~ *(uint*)(pCurEvalStack - sizeof(uint));
                        break;

                    case JitOps.JIT_NOT_I64:
                        OPCODE_USE(JitOps.JIT_NOT_I64);
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = ~ *(ulong*)(pCurEvalStack - sizeof(ulong));
                        break;

                    case JitOps.JIT_SHL_I32:
                        OPCODE_USE(JitOps.JIT_SHL_I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) << (int)*(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_SHR_I32:
                        OPCODE_USE(JitOps.JIT_SHR_I32);
                        pCurEvalStack -= sizeof(int) + sizeof(uint) - sizeof(int); 
                        *(int*)(pCurEvalStack - sizeof(int)) = *(int*)(pCurEvalStack - sizeof(int)) >> (int)*(uint*)(pCurEvalStack - sizeof(int) + sizeof(int));
                        break;

                    case JitOps.JIT_SHR_UN_I32:
                        OPCODE_USE(JitOps.JIT_SHR_UN_I32);
                        pCurEvalStack -= sizeof(uint) + sizeof(uint) - sizeof(uint); 
                        *(uint*)(pCurEvalStack - sizeof(uint)) = *(uint*)(pCurEvalStack - sizeof(uint)) >> (int)*(uint*)(pCurEvalStack - sizeof(uint) + sizeof(uint));
                        break;

                    case JitOps.JIT_SHL_I64:
                        OPCODE_USE(JitOps.JIT_SHL_I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(uint) - sizeof(ulong); 
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) << (int)*(uint*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                    case JitOps.JIT_SHR_I64:
                        OPCODE_USE(JitOps.JIT_SHR_I64);
                        pCurEvalStack -= sizeof(long) + sizeof(uint) - sizeof(long); 
                        *(long*)(pCurEvalStack - sizeof(long)) = *(long*)(pCurEvalStack - sizeof(long)) >> (int)*(uint*)(pCurEvalStack - sizeof(long) + sizeof(long));
                        break;

                    case JitOps.JIT_SHR_UN_I64:
                        OPCODE_USE(JitOps.JIT_SHR_UN_I64);
                        pCurEvalStack -= sizeof(ulong) + sizeof(uint) - sizeof(ulong); 
                        *(ulong*)(pCurEvalStack - sizeof(ulong)) = *(ulong*)(pCurEvalStack - sizeof(ulong)) >> (int)*(uint*)(pCurEvalStack - sizeof(ulong) + sizeof(ulong));
                        break;

                        // Conversion operations

                    case JitOps.JIT_CONV_U32_U32:
                    case JitOps.JIT_CONV_I32_U32:
                        OPCODE_USE(JitOps.JIT_CONV_I32_U32);
                        {
                            uint mask = GET_OP();
                            *(uint*)(pCurEvalStack - sizeof(uint)) &= mask;
                        }
                        break;

                    case JitOps.JIT_CONV_U32_I32:
                    case JitOps.JIT_CONV_I32_I32:
                        OPCODE_USE(JitOps.JIT_CONV_I32_I32);
                        {
                            int shift = (int)GET_OP();
                            *(int*)(pCurEvalStack - sizeof(int)) = (*(int*)(pCurEvalStack - sizeof(int)) << shift) >> shift;
                        }
                        break;

                    case JitOps.JIT_CONV_I32_I64:
                        OPCODE_USE(JitOps.JIT_CONV_I32_I64);
                        {
                            int value = (int)POP_U32();
                            PUSH_U64((ulong)(long)value);
                        }
                        break;

                    case JitOps.JIT_CONV_I32_U64:
                    case JitOps.JIT_CONV_U32_U64:
                    case JitOps.JIT_CONV_U32_I64:
                        OPCODE_USE(JitOps.JIT_CONV_U32_I64);
                        {
                            uint value = POP_U32();
                            PUSH_U64(value);
                        }
                        break;

                    case JitOps.JIT_CONV_I32_R32:
                        OPCODE_USE(JitOps.JIT_CONV_I32_R32);
                        {
                            int value = (int)POP_U32();
                            PUSH_FLOAT(value);
                        }
                        break;

                    case JitOps.JIT_CONV_I32_R64:
                        OPCODE_USE(JitOps.JIT_CONV_I32_R64);
                        {
                            int value = (int)POP_U32();
                            PUSH_DOUBLE(value);
                        }
                        break;

                    case JitOps.JIT_CONV_U32_R32:
                        OPCODE_USE(JitOps.JIT_CONV_U32_R32);
                        {
                            uint value = POP_U32();
                            PUSH_FLOAT(value);
                        }
                        break;

                    case JitOps.JIT_CONV_U32_R64:
                        OPCODE_USE(JitOps.JIT_CONV_U32_R64);
                        {
                            uint value = POP_U32();
                            PUSH_DOUBLE(value);
                        }
                        break;

                    case JitOps.JIT_CONV_I64_U32:
                    case JitOps.JIT_CONV_U64_U32:
                        OPCODE_USE(JitOps.JIT_CONV_I64_U32);
                        {
                            uint mask = GET_OP();
                            ulong value = POP_U64();
                            PUSH_U32((uint)(value & mask));
                        }
                        break;

                    case JitOps.JIT_CONV_I64_I32:
                    case JitOps.JIT_CONV_U64_I32:
                        OPCODE_USE(JitOps.JIT_CONV_I64_U32);
                        {
                            int shift = (int)GET_OP();
                            int value = (int)POP_U64();
                            value = (value << shift) >> shift;
                            PUSH_U32((uint)value);
                        }
                        break;

                    case JitOps.JIT_CONV_I64_R32:
                        OPCODE_USE(JitOps.JIT_CONV_I64_R32);
                        {
                            long value = (long)POP_U64();
                            PUSH_FLOAT(value);
                        }
                        break;

                    case JitOps.JIT_CONV_I64_R64:
                        OPCODE_USE(JitOps.JIT_CONV_I64_R64);
                        {
                            long value = (long)POP_U64();
                            PUSH_DOUBLE(value);
                        }
                        break;

                    case JitOps.JIT_CONV_U64_R32:
                        OPCODE_USE(JitOps.JIT_CONV_U64_R32);
                        {
                            ulong value = POP_U64();
                            PUSH_FLOAT(value);
                        }
                        break;

                    case JitOps.JIT_CONV_U64_R64:
                        OPCODE_USE(JitOps.JIT_CONV_U64_R64);
                        {
                            ulong value = POP_U64();
                            PUSH_DOUBLE(value);
                        }
                        break;

                    case JitOps.JIT_CONV_R32_I32:
                        OPCODE_USE(JitOps.JIT_CONV_R32_I32);
                        {
                            int shift = (int)GET_OP();
                            int result;
                            float value = POP_FLOAT();
                            result = (int)value;
                            result = (result << shift) >> shift;
                            PUSH_U32((uint)result);
                        }
                        break;

                    case JitOps.JIT_CONV_R32_U32:
                        OPCODE_USE(JitOps.JIT_CONV_R32_U32);
                        {
                            uint mask = GET_OP();
                            float value = POP_FLOAT();
                            PUSH_U32(((uint)value) & mask);
                        }
                        break;

                    case JitOps.JIT_CONV_R32_I64:
                        OPCODE_USE(JitOps.JIT_CONV_R32_I64);
                        {
                            float value = POP_FLOAT();
                            PUSH_U64((ulong)(long)value);
                        }
                        break;

                    case JitOps.JIT_CONV_R32_U64:
                        OPCODE_USE(JitOps.JIT_CONV_R32_U64);
                        {
                            float value = POP_FLOAT();
                            PUSH_U64((ulong)value);
                        }
                        break;

                    case JitOps.JIT_CONV_R32_R64:
                        OPCODE_USE(JitOps.JIT_CONV_R32_R64);
                        {
                            float value = POP_FLOAT();
                            PUSH_DOUBLE(value);
                        }
                        break;

                    case JitOps.JIT_CONV_R64_I32:
                        OPCODE_USE(JitOps.JIT_CONV_R64_I32);
                        {
                            int shift = (int)GET_OP();
                            int result;
                            double value = POP_DOUBLE();
                            result = (int)value;
                            result = (result << shift) >> shift;
                            PUSH_U32((uint)result);
                        }
                        break;

                    case JitOps.JIT_CONV_R64_U32:
                        OPCODE_USE(JitOps.JIT_CONV_R64_U32);
                        {
                            uint mask = GET_OP();
                            double value = POP_DOUBLE();
                            PUSH_U32(((uint)value) & mask);
                        }
                        break;

                    case JitOps.JIT_CONV_R64_I64:
                        OPCODE_USE(JitOps.JIT_CONV_R64_I64);
                        {
                            float value = POP_FLOAT();
                            PUSH_U64((ulong)(long)value);
                        }
                        break;

                    case JitOps.JIT_CONV_R64_U64:
                        OPCODE_USE(JitOps.JIT_CONV_R64_U64);
                        {
                            double value = POP_DOUBLE();
                            PUSH_U64((ulong)value);
                        }
                        break;

                    case JitOps.JIT_CONV_R64_R32:
                        OPCODE_USE(JitOps.JIT_CONV_R64_R32);
                        {
                            float value = (float)POP_DOUBLE();
                            PUSH_FLOAT(value);
                        }
                        break;

                    case JitOps.JIT_LOADFUNCTION:
                        OPCODE_USE(JitOps.JIT_LOADFUNCTION);
                        {
                            // This is actually a pointer not a uint
                            uint value = GET_OP();
                            PUSH_U32(value);
                        }
                        break;

                    case JitOps.JIT_LOADOBJECT:
                        OPCODE_USE(JitOps.JIT_LOADOBJECT);
                        {
                            tMD_TypeDef *pTypeDef;
                            byte* pMem;

                            pMem = POP_PTR(); // address of value-type
                            pTypeDef = (tMD_TypeDef*)GET_PTR(); //type of the value-type
                            //if (pTypeDef->stackSize != pTypeDef->arrayElementSize) {
                                // For bytes and int16s we need some special code to ensure that the stack
                                // does not contain rubbish in the bits unused in this type.
                                // But there is no harm in running this for all Type.types, and it's smaller and probably faster
                                *(uint*)pCurEvalStack = 0;
                            //}
                            PUSH_VALUETYPE(pMem, pTypeDef->arrayElementSize, pTypeDef->stackSize);
                        }
                        break;

                    case JitOps.JIT_LOAD_STRING:
                        OPCODE_USE(JitOps.JIT_LOAD_STRING);
                        {
                            uint value = GET_OP();
                            byte* heapPtr = System_String.FromUserStrings(pCurrentMethodState->pMetaData, value);
                            PUSH_O(heapPtr);
                        }
                        break;

                    case JitOps.JIT_NEWOBJECT:
                        OPCODE_USE(JitOps.JIT_NEWOBJECT);
                        {
                            tMD_MethodDef *pConstructorDef;
                            /*HEAP_PTR*/byte* obj;
                            tMethodState *pCallMethodState;
                            bool isInternalConstructor;
                            byte* pTempPtr;

                            pConstructorDef = (tMD_MethodDef*)GET_PTR();
                            isInternalConstructor = (pConstructorDef->implFlags & MetaData.METHODIMPLATTRIBUTES_INTERNALCALL) != 0;

                            if (!isInternalConstructor) {
                                // All internal constructors MUST allocate their own 'this' objects
                                obj = Heap.AllocType(pConstructorDef->pParentType);
                            } else {
                                // Need to set this to something non-null so that CreateParameters() works properly
                                obj = (/*HEAP_PTR*/byte*)-1;
                            }

                            // Set up the new method state for the called method
                            pCallMethodState = MethodState.Direct(pThread, pConstructorDef, pCurrentMethodState, isInternalConstructor ? 1U : 0U);
                            // Fill in the parameters
                            pTempPtr = pCurEvalStack;
                            CreateParameters(pCallMethodState->pParamsLocals, pConstructorDef, &pTempPtr, obj);
                            pCurEvalStack = pTempPtr;
                            if (!isInternalConstructor) {
                                // Push the object here, so it's on the stack when the constructor returns
                                PUSH_O(obj);
                            }
                            // Set up the local variables for the new method state (for the obj constructor)
                            CHANGE_METHOD_STATE(pCallMethodState);
                            // Run any pending Finalizers
                            RUN_FINALIZER();
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_NEWOBJECT_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_NEWOBJECT_VALUETYPE);
                        {
                            tMD_MethodDef *pConstructorDef;
                            tMethodState *pCallMethodState;
                            bool isInternalConstructor;
                            byte* pTempPtr, pMem;

                            pConstructorDef = (tMD_MethodDef*)GET_PTR();
                            isInternalConstructor = (pConstructorDef->implFlags & MetaData.METHODIMPLATTRIBUTES_INTERNALCALL) != 0;

                            // Allocate space on the eval-stack for the new value-type here
                            pMem = pCurEvalStack - (pConstructorDef->parameterStackSize - sizeof(byte*));

                            // Set up the new method state for the called method
                            pCallMethodState = MethodState.Direct(pThread, pConstructorDef, pCurrentMethodState, isInternalConstructor ? 1U : 0U);
                            // Fill in the parameters
                            pTempPtr = pCurEvalStack;
                            CreateParameters(pCallMethodState->pParamsLocals, pConstructorDef, &pTempPtr, pMem);
                            pCurEvalStack = pTempPtr;
                            // Set the stack state so it's correct for the constructor return
                            pCurEvalStack += pConstructorDef->pParentType->stackSize;
                            // Set up the local variables for the new method state
                            CHANGE_METHOD_STATE(pCallMethodState);
                            // Run any pending Finalizers
                            RUN_FINALIZER();
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;


                    case JitOps.JIT_IS_INSTANCE:
                        op = JitOps.JIT_IS_INSTANCE;
                        goto jitCastClass;
                    case JitOps.JIT_CAST_CLASS:
                        op = JitOps.JIT_CAST_CLASS;
                    jitCastClass:
                        OPCODE_USE(JitOps.JIT_CAST_CLASS);
                        {
                            tMD_TypeDef* pToType, pTestType;
                            /*HEAP_PTR*/byte* heapPtr;

                            pToType = (tMD_TypeDef*)GET_PTR();
                            heapPtr = POP_O();
                            if (heapPtr == null) {
                                PUSH_O(null);
                                goto JIT_IS_INSTANCE_end;
                            }
                            pTestType = Heap.GetType(heapPtr);
                            if (MetaData.TYPE_ISARRAY(pTestType) && MetaData.TYPE_ISARRAY(pToType)) {
                                // Arrays are handled specially - check if the element type is compatible
                                if (Type.IsAssignableFrom(pToType->pArrayElementType, pTestType->pArrayElementType) != 0) {
                                    PUSH_O(heapPtr);
                                    goto JIT_IS_INSTANCE_end;
                                }
                            } else {
                                if (Type.IsAssignableFrom(pToType, pTestType) != 0 ||
                                    (pToType->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE] &&
                                    pToType->ppClassTypeArgs[0] == pTestType)) {
                                    // If derived class, interface, or nullable type compatible.
                                    PUSH_O(heapPtr);
                                    goto JIT_IS_INSTANCE_end;
                                }
                            }
                            if (op == JitOps.JIT_IS_INSTANCE) {
                                PUSH_O(null);
                            } else {
                                pThrowExcept = THROW(Type.types[Type.TYPE_SYSTEM_INVALIDCASTEXCEPTION]);
                                goto throwHeapPtr;
                            }
                        }
                    JIT_IS_INSTANCE_end:
                        break;

                    case JitOps.JIT_NEW_VECTOR: // Array with 1 dimension, zero-based
                        OPCODE_USE(JitOps.JIT_NEW_VECTOR);
                        {
                            tMD_TypeDef *pArrayTypeDef;
                            uint numElements;
                            /*HEAP_PTR*/byte* heapPtr;

                            pArrayTypeDef = (tMD_TypeDef*)GET_PTR();
                            numElements = POP_U32();
                            heapPtr = System_Array.NewVector(pArrayTypeDef, numElements);
                            PUSH_O(heapPtr);
                            // Run any pending Finalizers
                            RUN_FINALIZER();
                        }
                        break;

                    case JitOps.JIT_LOAD_VECTOR_LEN: // Load the length of a vector array
                        OPCODE_USE(JitOps.JIT_LOAD_VECTOR_LEN);
                        {
                            byte* heapPtr = POP_O();
                            uint value = System_Array.GetLength(heapPtr);
                            PUSH_U32(value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_I8:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_I8);
                        {
                            uint value, idx = POP_U32(); // Array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            System_Array.LoadElement(heapPtr, idx, (byte*)&value);
                            PUSH_U32((uint)(sbyte)value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_U8:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_U8);
                        {
                            uint value, idx = POP_U32(); // Array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            System_Array.LoadElement(heapPtr, idx, (byte*)&value);
                            PUSH_U32((byte)value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_I16:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_I16);
                        {
                            uint value, idx = POP_U32(); // Array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            System_Array.LoadElement(heapPtr, idx, (byte*)&value);
                            PUSH_U32((uint)(short)value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_U16:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_U16);
                        {
                            uint value, idx = POP_U32(); // Array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            System_Array.LoadElement(heapPtr, idx, (byte*)&value);
                            PUSH_U32((ushort)value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_I32:
                    case JitOps.JIT_LOAD_ELEMENT_U32:
                    case JitOps.JIT_LOAD_ELEMENT_R32:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_I32);
                        {
                            uint value, idx = POP_U32(); // Array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            System_Array.LoadElement(heapPtr, idx, (byte*)&value);
                            PUSH_U32(value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_I64:
                    case JitOps.JIT_LOAD_ELEMENT_R64:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_I64);
                        {
                            uint idx = POP_U32(); // array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            ulong value;
                            System_Array.LoadElement(heapPtr, idx, (byte*)&value);
                            PUSH_U64(value);
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT);
                        {
                            uint idx = POP_U32(); // Array index
                            /*HEAP_PTR*/byte* heapPtr = POP_O(); // array object
                            uint size = GET_OP(); // size of type on stack
                            *(uint*)pCurEvalStack = 0; // This is required to zero out the stack for type that are stored in <4 bytes in arrays
                            System_Array.LoadElement(heapPtr, idx, pCurEvalStack);
                            pCurEvalStack += size;
                        }
                        break;

                    case JitOps.JIT_LOAD_ELEMENT_ADDR:
                        OPCODE_USE(JitOps.JIT_LOAD_ELEMENT_ADDR);
                        {
                            uint idx = POP_U32(); // Array index
                            byte* heapPtr = POP_O();
                            byte* pMem = System_Array.LoadElementAddress(heapPtr, idx);
                            PUSH_PTR(pMem);
                        }
                        break;

                    case JitOps.JIT_STORE_ELEMENT_32:
                        OPCODE_USE(JitOps.JIT_STORE_ELEMENT_32);
                        {
                            uint value = POP_U32(); // Value
                            uint idx = POP_U32(); // Array index
                            byte* heapPtr = POP_O();
                            System_Array.StoreElement(heapPtr, idx, (byte*)&value);
                        }
                        break;

                    case JitOps.JIT_STORE_ELEMENT_64:
                        OPCODE_USE(JitOps.JIT_STORE_ELEMENT_64);
                        {
                            ulong value = POP_U64(); // Value
                            uint idx = POP_U32(); // Array index
                            byte* heapPtr = POP_O();
                    #if TRACE
                            Sys.printf("  val 0x%llx idx %d ptr 0x%llx\n", value, idx, (ulong)heapPtr);
                    #endif
                            System_Array.StoreElement(heapPtr, idx, (byte*)&value);
                        }
                        break;

                    case JitOps.JIT_STORE_ELEMENT:
                        OPCODE_USE(JitOps.JIT_STORE_ELEMENT);
                        {
                            /*HEAP_PTR*/byte* heapPtr;
                            byte* pMem;
                            uint idx, size = GET_OP(); // Size in bytes of value on stack
                            POP(size);
                            pMem = pCurEvalStack;
                            idx = POP_U32(); // Array index
                            heapPtr = POP_O(); // Array on heap
                            System_Array.StoreElement(heapPtr, idx, pMem);
                        }
                        break;

                    case JitOps.JIT_STOREFIELD_INT32:
                    case JitOps.JIT_STOREFIELD_INTNATIVE: // only for 32-bit
                    case JitOps.JIT_STOREFIELD_F32:
                        OPCODE_USE(JitOps.JIT_STOREFIELD_INT32);
                        {
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            uint value;
                            /*HEAP_PTR*/byte* heapPtr;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U32();
                            heapPtr = POP_O();
                            pMem = heapPtr + pFieldDef->memOffset;
                    #if TRACE
                            Sys.printf("  val 0x%x off %d ptr 0x%llx\n", value, pFieldDef->memOffset, (ulong)heapPtr);
                    #endif
                            *(uint*)pMem = value;
                        }
                        break;

                    case JitOps.JIT_STOREFIELD_O:
                    case JitOps.JIT_STOREFIELD_PTR:
                        OPCODE_USE(JitOps.JIT_STOREFIELD_PTR);
                        {
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            uint value;
                            /*HEAP_PTR*/byte* heapPtr;
                            
                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U32();
                            heapPtr = POP_O();
                            pMem = heapPtr + pFieldDef->memOffset;
                    //        printf("  val 0x%x off %d ptr 0x%llx\n", value, pFieldDef->memOffset, (ulong)heapPtr);
                            *(uint*)pMem = value;
                    #else
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            ulong value;
                            /*HEAP_PTR*/byte* heapPtr;
                            
                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U64();
                            heapPtr = POP_O();
                            pMem = heapPtr + pFieldDef->memOffset;
                    #if TRACE
                            Sys.printf("  val 0x%llx off %d ptr 0x%llx\n", value, pFieldDef->memOffset, (ulong)heapPtr);
                    #endif
                            *(ulong*)pMem = value;
                    #endif
                        }
                        break;
                        
                    case JitOps.JIT_STOREFIELD_INT64:
                    case JitOps.JIT_STOREFIELD_F64:
                        OPCODE_USE(JitOps.JIT_STOREFIELD_F64);
                        {
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            ulong value;
                            /*HEAP_PTR*/byte* heapPtr;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U64();
                            heapPtr = POP_O();
                            pMem = heapPtr + pFieldDef->memOffset;
                            *(ulong*)pMem = value;
                        }
                        break;

                    case JitOps.JIT_STOREFIELD_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_STOREFIELD_VALUETYPE);
                        {
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            pCurEvalStack -= pFieldDef->memSize;
                            pMem = pCurEvalStack;
                            byte* heapPtr = POP_O();
                            Mem.memcpy(heapPtr + pFieldDef->memOffset, pMem, pFieldDef->memSize);
                        }
                        break;

                    case JitOps.JIT_LOADFIELD:
                        OPCODE_USE(JitOps.JIT_LOADFIELD);
                        // TODO: Optimize into LOADFIELD of different type O, INT32, INT64, F, etc...)
                        {
                            tMD_FieldDef *pFieldDef;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            byte* heapPtr = POP_O();
                            byte* pMem = heapPtr + pFieldDef->memOffset;
                            // It may not be a value-type, but this'll work anyway
                            PUSH_VALUETYPE(pMem, pFieldDef->memSize, pFieldDef->memSize);
                        }
                        break;

                    case JitOps.JIT_LOADFIELD_4:
                        OPCODE_USE(JitOps.JIT_LOADFIELD_4);
                        {
                            uint ofs = GET_OP();
                            byte* heapPtr = POP_O();
                    //        printf("  ofs %d ptr 0x%llx val 0x%x\n", ofs, (ulong)heapPtr, *(uint*)(heapPtr + ofs));
                            PUSH_U32(*(uint*)(heapPtr + ofs));
                        }
                        break;

                    case JitOps.JIT_LOADFIELD_8:
                        OPCODE_USE(JitOps.JIT_LOADFIELD_8);
                        {
                            uint ofs = GET_OP();
                            byte* heapPtr = POP_O();
                    //        printf("  ofs %d ptr 0x%llx val 0x%llx\n", ofs, (ulong)heapPtr, *(ulong*)(heapPtr + ofs));
                            PUSH_U64(*(ulong*)(heapPtr + ofs));
                        }
                        break;
                        
                    case JitOps.JIT_LOADFIELD_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_LOADFIELD_VALUETYPE);
                        {
                            tMD_FieldDef *pFieldDef;

                            u32Value = GET_OP(); // Get the size of the value-type on the eval stack
                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            
                            // [Steve edit] The following line used to be:
                            //     pCurrentMethodState->stackOfs -= u32Value;
                            // ... but this seems to result in calculating the wrong pMem value and getting garbage results.
                            // My guess is that at some point they refactored from using 'pEvalStack' to 'pCurEvalStack', but
                            // didn't update this method (because nothing in corlib reads fields from structs).
                            // I think the following line moves the stack pointer along correctly instead:
                            pCurEvalStack -= u32Value;
                            
                            //pMem = pEvalStack + pCurrentMethodState->stackOfs + pFieldDef->memOffset;
                            byte* pMem = pCurEvalStack + pFieldDef->memOffset;
                            // It may not be a value-type, but this'll work anyway
                            PUSH_VALUETYPE(pMem, pFieldDef->memSize, pFieldDef->memSize);
                        }
                        break;

                    case JitOps.JIT_LOAD_FIELD_ADDR:
                        OPCODE_USE(JitOps.JIT_LOAD_FIELD_ADDR);
                        {
                            uint ofs = GET_OP();
                            /*HEAP_PTR*/byte* heapPtr = POP_O();
                            byte* pMem = heapPtr + ofs;
                            PUSH_PTR(pMem);
                        }
                        break;

                    case JitOps.JIT_STORESTATICFIELD_INT32:
                    case JitOps.JIT_STORESTATICFIELD_F32:
                    case JitOps.JIT_STORESTATICFIELD_INTNATIVE: // only for 32-bit
                        OPCODE_USE(JitOps.JIT_STORESTATICFIELD_INT32);
                        {
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            uint value;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U32();
                            pMem = pFieldDef->pMemory;
                            *(uint*)pMem = value;
                        }
                        break;

                    case JitOps.JIT_STORESTATICFIELD_O: // only for 32-bit
                    case JitOps.JIT_STORESTATICFIELD_PTR: // only for 32-bit
                        OPCODE_USE(JitOps.JIT_STORESTATICFIELD_INT32);
                        {
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            uint value;
                            
                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U32();
                            pMem = pFieldDef->pMemory;
                            *(uint*)pMem = value;
                    #else
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            ulong value;
                            
                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U64();
                            pMem = pFieldDef->pMemory;
                            *(ulong*)pMem = value;
                    #endif
                        }
                        break;
                        
                    case JitOps.JIT_STORESTATICFIELD_F64:
                    case JitOps.JIT_STORESTATICFIELD_INT64:
                        OPCODE_USE(JitOps.JIT_STORESTATICFIELD_INT64);
                        {
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;
                            ulong value;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            value = POP_U64();
                            //pMem = pFieldDef->pParentType->pStaticFields + pFieldDef->memOffset;
                            pMem = pFieldDef->pMemory;
                            *(ulong*)pMem = value;
                        }
                        break;

                    case JitOps.JIT_STORESTATICFIELD_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_STORESTATICFIELD_VALUETYPE);
                        {
                            tMD_FieldDef *pFieldDef;
                            byte* pMem;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            pMem = pFieldDef->pMemory;
                            POP_VALUETYPE(pMem, pFieldDef->memSize, pFieldDef->memSize);
                        }
                        break;

                    case JitOps.JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT:
                        op = JitOps.JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT;
                        goto loadStaticFieldStart;
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE:
                        op = JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE;
                        goto loadStaticFieldStart;
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64:
                        op = JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64;
                        goto loadStaticFieldStart;
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_O: // Only for 32-bit
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR: // Only for 32-bit
                        op = JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR;
                        goto loadStaticFieldStart;
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32:
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_F32:
                    case JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_INTNATIVE: // Only for 32-bit
                        op = 0;
                    loadStaticFieldStart:
                        OPCODE_USE(JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32);
                        {
                            tMD_FieldDef *pFieldDef;
                            tMD_TypeDef *pParentType;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            pParentType = pFieldDef->pParentType;
                            // Check that any type (static) constructor has been called
                            if (pParentType->isTypeInitialised == 0) {
                                // Set the state to initialised
                                pParentType->isTypeInitialised = 1;
                                // Initialise the type (if there is a static constructor)
                                if (pParentType->pStaticConstructor != null) {
                                    tMethodState *pCallMethodState;

                                    // Call static constructor
                                    // Need to re-run this instruction when we return from static constructor call
                                    //pCurrentMethodState->ipOffset -= op + ptr;
                                    pCurOp -= 1 + (sizeof(void*) >> 2);
                                    pCallMethodState = MethodState.Direct(pThread, pParentType->pStaticConstructor, pCurrentMethodState, 0);
                                    // There can be no parameters, so don't need to set them up
                                    CHANGE_METHOD_STATE(pCallMethodState);
                                    if (--numInst == 0) 
                                        goto done;
                                }
                            }
                            if (op == JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64) {
                                ulong value;
                                value = *(ulong*)(pFieldDef->pMemory);
                                PUSH_U64(value);
                            } else if (op == JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR ||
                                       op == JitOps.JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT) {
                    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                                uint value;
                                if (op == JitOps.JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT) {
                                    value = (uint)(pFieldDef->pMemory);
                                } else {
                                    value = *(uint*)(pFieldDef->pMemory);
                                }
                                PUSH_U32(value);
                    #else
                                ulong value;
                                if (op == JitOps.JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT) {
                                    value = (ulong)(pFieldDef->pMemory);
                                } else {
                                    value = *(ulong*)(pFieldDef->pMemory);
                                }
                                PUSH_U64(value);
                    #endif
                            } else if (op == JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE) {
                                PUSH_VALUETYPE(pFieldDef->pMemory, pFieldDef->memSize, pFieldDef->memSize);
                            } else {
                                uint value;
                                value = *(uint*)pFieldDef->pMemory;
                                PUSH_U32(value);
                            }
                        }
                        break;

                    case JitOps.JIT_INIT_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_INIT_VALUETYPE);
                        {
                            tMD_TypeDef *pTypeDef;

                            pTypeDef = (tMD_TypeDef*)GET_PTR();
                            byte* pMem = POP_PTR();
                            Mem.memset(pMem, 0, pTypeDef->instanceMemSize);
                        }
                        break;

                    case JitOps.JIT_INIT_OBJECT:
                        OPCODE_USE(JitOps.JIT_INIT_OBJECT);
                        {
                            byte* pMem = POP_PTR();
                            *(void**)pMem = null;
                        }
                        break;

                    case JitOps.JIT_BOX_INT32:
                    case JitOps.JIT_BOX_F32:
                    case JitOps.JIT_BOX_INTNATIVE:
                        OPCODE_USE(JitOps.JIT_BOX_INT32);
                        {
                            tMD_TypeDef *pTypeDef;

                            pTypeDef = (tMD_TypeDef*)GET_PTR();
                            byte* heapPtr = Heap.AllocType(pTypeDef);
                            u32Value = POP_U32();
                            *(uint*)heapPtr = u32Value;
                            PUSH_O(heapPtr);
                        }
                        break;

                    case JitOps.JIT_BOX_INT64:
                    case JitOps.JIT_BOX_F64:
                    OPCODE_USE(JitOps.JIT_BOX_INT64);
                        {
                            tMD_TypeDef *pTypeDef = (tMD_TypeDef*)GET_PTR();
                            byte* heapPtr = Heap.AllocType(pTypeDef);
                            *(ulong*)heapPtr = POP_U64();
                            PUSH_O(heapPtr);
                        }
                        break;

                    case JitOps.JIT_BOX_VALUETYPE:
                        OPCODE_USE(JitOps.JIT_BOX_VALUETYPE);
                        {
                            tMD_TypeDef *pTypeDef;

                            pTypeDef = (tMD_TypeDef*)GET_PTR();
                            byte* heapPtr = Heap.AllocType(pTypeDef);
                            POP_VALUETYPE(heapPtr, pTypeDef->stackSize, pTypeDef->stackSize);
                            PUSH_O(heapPtr);
                        }
                        break;

                    case JitOps.JIT_BOX_O:
                        pCurOp++;
                        goto JIT_UNBOX2OBJECT_start;
                        // Fall-through
                    case JitOps.JIT_UNBOX2OBJECT: // TODO: This is not correct - it should check the type, just like CAST_CLASS
                    JIT_UNBOX2OBJECT_start:
                        OPCODE_USE(JitOps.JIT_UNBOX2OBJECT);
                        break;

                    case JitOps.JIT_BOX_NULLABLE:
                        OPCODE_USE(JitOps.JIT_BOX_NULLABLE);
                        {
                            // Get the underlying type of the nullable type
                            tMD_TypeDef *pType = (tMD_TypeDef*)GET_PTR();

                            // Take the nullable type off the stack. The +4 is because the of the HasValue field (Bool, size = 4 bytes)
                            pCurEvalStack -= pType->stackSize + 4;
                            // If .HasValue
                            if (*(uint*)pCurEvalStack != 0) {
                                // Box the underlying type
                                /*HEAP_PTR*/byte* boxed;
                                boxed = Heap.Box(pType, pCurEvalStack + 4);
                                PUSH_O(boxed);
                            } else {
                                // Put a null pointer on the stack
                                PUSH_O(null);
                            }
                        }
                        break;

                    case JitOps.JIT_UNBOX2VALUETYPE:
                        OPCODE_USE(JitOps.JIT_UNBOX2VALUETYPE);
                        {
                            tMD_TypeDef *pTypeDef;
                            /*HEAP_PTR*/byte* heapPtr;

                            heapPtr = POP_O();
                            pTypeDef = Heap.GetType(heapPtr);
                            PUSH_VALUETYPE(heapPtr, pTypeDef->stackSize, pTypeDef->stackSize);
                        }
                        break;

                    case JitOps.JIT_UNBOX_NULLABLE:
                        OPCODE_USE(JitOps.JIT_UNBOX_NULLABLE);
                        {
                            tMD_TypeDef *pTypeDef = (tMD_TypeDef*)GET_PTR();
                            /*HEAP_PTR*/byte* heapPtr;
                            heapPtr = POP_O();
                            if (heapPtr == null) {
                                // Push .HasValue (= false)
                                PUSH_U32(0);
                                // And increase the stack pointer by the size of the underlying type
                                // (the contents don't matter)
                                pCurEvalStack += pTypeDef->stackSize;
                            } else {
                                // Push .HasValue (= true)
                                PUSH_U32(1);
                                // Push the contents of .Value
                                PUSH_VALUETYPE(heapPtr, pTypeDef->stackSize, pTypeDef->stackSize);
                            }
                        }
                        break;

                    case JitOps.JIT_LOADTOKEN_TYPE:
                        OPCODE_USE(JitOps.JIT_LOADTOKEN_TYPE);
                        {
                            tMD_TypeDef *pTypeDef;

                            pTypeDef = (tMD_TypeDef*)GET_PTR();
                            // Push new valuetype onto evaluation stack
                            PUSH_PTR((byte*)pTypeDef);
                        }
                        break;

                    case JitOps.JIT_LOADTOKEN_FIELD:
                        OPCODE_USE(JitOps.JIT_LOADTOKEN_FIELD);
                        {
                            tMD_FieldDef *pFieldDef;

                            pFieldDef = (tMD_FieldDef*)GET_PTR();
                            // Push new valuetype onto evaluation stack - only works on static fields.
                            PUSH_PTR(pFieldDef->pMemory);
                        }
                        break;

                    case JitOps.JIT_RETHROW:
                        op = JitOps.JIT_RETHROW;
                        goto throwStart;
                    case JitOps.JIT_THROW:
                        op = JitOps.JIT_THROW;
                    throwStart:
                    throwHeapPtr:
                        OPCODE_USE(JitOps.JIT_THROW);
                        {
                            uint i;
                            tExceptionHeader *pCatch;
                            tMethodState *pCatchMethodState;
                            tMD_TypeDef *pExType;
                            byte* pExcept = null;

                            // Get the exception object
                            if (pThrowExcept != null) {
                                pExcept = pThrowExcept;
                                pThread->pCurrentExceptionObject = pThrowExcept;
                            } else {
                                if (op == JitOps.JIT_RETHROW) {
                                    pExcept = pThread->pCurrentExceptionObject;
                                } else {
                                    pExcept = POP_O();
                                    pThread->pCurrentExceptionObject = pExcept;
                                }
                            }
                            SAVE_METHOD_STATE();
                            pExType = Heap.GetType(pExcept);
                            // Find any catch exception clauses; look in the complete call stack
                            pCatch = null;
                            pCatchMethodState = pCurrentMethodState;
                            for(;;) {
                                for (i=0; i<pCatchMethodState->pMethod->pJITted->numExceptionHandlers; i++) {
                                    tJITExceptionHeader *pEx = &pCatchMethodState->pMethod->pJITted->pExceptionHeaders[i];
                                    if (pEx->flags == JIT.COR_ILEXCEPTION_CLAUSE_EXCEPTION &&
                                        pCatchMethodState->ipOffset - 1 >= pEx->tryStart &&
                                        pCatchMethodState->ipOffset - 1 < pEx->tryEnd &&
                                        Type.IsDerivedFromOrSame(pEx->pCatchTypeDef, pExType) != 0) {
                                        
                                        // Found the correct catch clause to jump to
                                        pCatch = (tExceptionHeader*)pEx;
                                        break;
                                    }
                                }
                                if (pCatch != null) {
                                    // Found a suitable exception handler
                                    break;
                                }
                                pCatchMethodState = pCatchMethodState->pCaller;
                                if (pCatchMethodState == null) {
                                    Sys.Crash("Unhandled exception in %s.%s(): %s.%s",
                                        (PTR)pCurrentMethodState->pMethod->pParentType->name,
                                        (PTR)pCurrentMethodState->pMethod->name, (PTR)pExType->nameSpace, (PTR)pExType->name);
                                }
                            }
                            // Unwind the stack down to the exception handler's stack frame (MethodState)
                            // Run all finally clauses during unwinding
                            pThread->pCatchMethodState = pCatchMethodState;
                            pThread->pCatchExceptionHandler = pCatch;
                            // Have to use the pThread->pCatchMethodState, as we could be getting here from END_FINALLY
                            while (pCurrentMethodState != pThread->pCatchMethodState) {
                                tMethodState *pPrevState;

                    // finallyUnwindStack:
                                for (i=pThread->nextFinallyUnwindStack; i<pCurrentMethodState->pMethod->pJITted->numExceptionHandlers; i++) {
                                    tExceptionHeader *pEx;

                                    pEx = (tExceptionHeader*)&pCurrentMethodState->pMethod->pJITted->pExceptionHeaders[i];
                                    if (pEx->flags == JIT.COR_ILEXCEPTION_CLAUSE_FINALLY &&
                                        pCurrentMethodState->ipOffset - 1 >= pEx->tryStart &&
                                        pCurrentMethodState->ipOffset - 1 < pEx->tryEnd) {

                                        // Found a finally handler
                                        POP_ALL();
                                        CHANGE_METHOD_STATE(pCurrentMethodState);
                                        pCurrentMethodState->ipOffset = pEx->handlerStart;
                                        // Keep track of which finally clause should be executed next
                                        pThread->nextFinallyUnwindStack = i + 1;
                                        goto throwEnd;
                                    }
                                }

                                pPrevState = pCurrentMethodState->pCaller;
                                MethodState.Delete(pThread, ref pCurrentMethodState);
                                pCurrentMethodState = pPrevState;
                                // Reset the stack unwind tracker
                                pThread->nextFinallyUnwindStack = 0;
                            }
                            // Set the IP to the catch handler
                            pCurrentMethodState->ipOffset = pThread->pCatchExceptionHandler->handlerStart;
                            // Set the current method state
                            LOAD_METHOD_STATE();
                            // Push onto this stack-frame's evaluation stack the opject thrown
                            POP_ALL();
                            PUSH_O(pThread->pCurrentExceptionObject);
                        }
                    throwEnd:
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_LEAVE:
                        OPCODE_USE(JitOps.JIT_LEAVE);
                        {
                            uint i;
                            tExceptionHeader *pFinally;

                            // Find any finally exception clauses
                            pFinally = null;
                            for (i=0; i<pJIT->numExceptionHandlers; i++) {
                                if (pJIT->pExceptionHeaders[i].flags == JIT.COR_ILEXCEPTION_CLAUSE_FINALLY &&
                                    pCurrentMethodState->ipOffset - 1 >= pJIT->pExceptionHeaders[i].tryStart &&
                                    pCurrentMethodState->ipOffset - 1 < pJIT->pExceptionHeaders[i].tryEnd) {
                                    // Found the correct finally clause to jump to
                                    pFinally = (tExceptionHeader*)&pJIT->pExceptionHeaders[i];
                                    break;
                                }
                            }
                            POP_ALL();
                            uint ofs = GET_OP();
                            if (pFinally != null) {
                                // Jump to 'finally' section
                                pCurOp = pOps + pFinally->handlerStart;
                                pCurrentMethodState->pOpEndFinally = pOps + ofs;
                            } else {
                                // just branch
                                pCurOp = pOps + ofs;
                            }
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    case JitOps.JIT_END_FINALLY:
                        OPCODE_USE(JitOps.JIT_END_FINALLY);
                        if (pThread->nextFinallyUnwindStack > 0) {
                            // Copy of unwind code above^^ in exception where the following line starts with..
                            // finallyUnwindStack:
                            uint i;
                            for (i=pThread->nextFinallyUnwindStack; i<pCurrentMethodState->pMethod->pJITted->numExceptionHandlers; i++) {
                                tExceptionHeader *pEx = (tExceptionHeader*)&pCurrentMethodState->pMethod->pJITted->pExceptionHeaders[i];
                                if (pEx->flags == JIT.COR_ILEXCEPTION_CLAUSE_FINALLY &&
                                    pCurrentMethodState->ipOffset - 1 >= pEx->tryStart &&
                                    pCurrentMethodState->ipOffset - 1 < pEx->tryEnd) {

                                    // Found a finally handler
                                    POP_ALL();
                                    CHANGE_METHOD_STATE(pCurrentMethodState);
                                    pCurrentMethodState->ipOffset = pEx->handlerStart;
                                    // Keep track of which finally clause should be executed next
                                    pThread->nextFinallyUnwindStack = i + 1;
                                    goto throwEnd;
                                }
                            }
                            tMethodState* pPrevState = pCurrentMethodState->pCaller;
                            MethodState.Delete(pThread, ref pCurrentMethodState);
                            pCurrentMethodState = pPrevState;
                            // Reset the stack unwind tracker
                            pThread->nextFinallyUnwindStack = 0;
                            // Set the IP to the catch handler
                            pCurrentMethodState->ipOffset = pThread->pCatchExceptionHandler->handlerStart;
                            // Set the current method state
                            LOAD_METHOD_STATE();
                            // Push onto this stack-frame's evaluation stack the opject thrown
                            POP_ALL();
                            PUSH_O(pThread->pCurrentExceptionObject);
                        } else {
                            // Just empty the evaluation stack and continue on to the next opcode
                            // (finally blocks are always after catch blocks, so execution can just continue)
                            POP_ALL();
                            // And jump to the correct instruction, as specified in the leave instruction
                            pCurOp = pCurrentMethodState->pOpEndFinally;
                        }
                        if (--numInst == 0) 
                            goto done;
                        break;

                    }

                }

            done:
                SAVE_METHOD_STATE();

            return Thread.THREAD_STATUS_RUNNING;
        }

    } 
}
