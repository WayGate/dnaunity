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

#if NO

//const int TRACE 1

// Global array which stores the absolute addresses of the start and end of all JIT code
// fragment machine code.
tJITCodeInfo jitCodeInfo[JIT_OPCODE_MAXNUM];
tJITCodeInfo jitCodeGoNext;

// Get the next op-code
const int GET_OP() (*(pCurOp++))

// Get a 32/64 bit pointer
#if UNITY_WEBGL || DNA_32BIT
const int GET_PTR() (*(pCurOp++))
#else
// NOTE: Technically this is undefined behavior having two increments in the same expression
const int GET_PTR() (((ulong)*(pCurOp++)) | ((ulong)*(pCurOp++) << 32));
#endif

// Push a byte* value on the top of the stack
const int PUSH_PTR(ptr) *(byte**)pCurEvalStack = (byte*)(ptr); pCurEvalStack += sizeof(void*)
// Push an arbitrarily-sized value-type onto the top of the stack
const int PUSH_VALUETYPE(ptr, valueSize, stackInc) Mem.memcpy(pCurEvalStack, ptr, valueSize); pCurEvalStack += stackInc
// Push a uint value on the top of the stack
const int PUSH_U32(value) *(uint*)pCurEvalStack = (uint)(value); pCurEvalStack += 4
// Push a ulong value on the top of the stack
const int PUSH_U64(value) *(ulong*)pCurEvalStack = (ulong)(value); pCurEvalStack += 8
// Push a float value on the top of the stack
const int PUSH_FLOAT(value) *(float*)pCurEvalStack = (float)(value); pCurEvalStack += 4;
// Push a double value on the top of the stack
const int PUSH_DOUBLE(value) *(double*)pCurEvalStack = (double)(value); pCurEvalStack += 8;
// Push a heap pointer on to the top of the stack
const int PUSH_O(pHeap) *(void**)pCurEvalStack = (void*)(pHeap); pCurEvalStack += sizeof(void*)
// DUP4() duplicates the top 4 bytes on the eval stack
const int DUP4() *(uint*)pCurEvalStack = *(uint*)(pCurEvalStack - 4); pCurEvalStack += 4
// DUP8() duplicates the top 4 bytes on the eval stack
const int DUP8() *(ulong*)pCurEvalStack = *(ulong*)(pCurEvalStack - 8); pCurEvalStack += 8
// DUP() duplicates numBytes bytes from the top of the stack
const int DUP(numBytes) Mem.memcpy(pCurEvalStack, pCurEvalStack - numBytes, numBytes); pCurEvalStack += numBytes
// Pop a uint value from the stack
const int POP_U32() (*(uint*)(pCurEvalStack -= 4))
// Pop a ulong value from the stack
const int POP_U64() (*(ulong*)(pCurEvalStack -= 8))
// Pop a float value from the stack
const int POP_FLOAT() (*(float*)(pCurEvalStack -= 4))
// Pop a double value from the stack
const int POP_DOUBLE() (*(double*)(pCurEvalStack -= 8))
// Pop 2 uint's from the stack
const int POP_U32_U32(v1,v2) pCurEvalStack -= 8; v1 = *(uint*)pCurEvalStack; v2 = *(uint*)(pCurEvalStack + 4)
// Pop 2 ulong's from the stack
const int POP_U64_U64(v1,v2) pCurEvalStack -= 16; v1 = *(ulong*)pCurEvalStack; v2 = *(ulong*)(pCurEvalStack + 8)
// Pop 2 F32's from the stack
const int POP_F32_F32(v1,v2) pCurEvalStack -= 8; v1 = *(float*)pCurEvalStack; v2 = *(float*)(pCurEvalStack + 4)
// Pop 2 F64's from the stack
const int POP_F64_F64(v1,v2) pCurEvalStack -= 16; v1 = *(double*)pCurEvalStack; v2 = *(double*)(pCurEvalStack + 8)
// Pop a byte* value from the stack
const int POP_PTR() (*(byte**)(pCurEvalStack -= sizeof(void*)))
// Pop an arbitrarily-sized value-type from the stack (copies it to the specified memory location)
const int POP_VALUETYPE(ptr, valueSize, stackDec) Mem.memcpy(ptr, pCurEvalStack -= stackDec, valueSize)
// Pop a Object (heap) pointer value from the stack
const int POP_O() (*(/*HEAP_PTR*/byte**)(pCurEvalStack -= sizeof(void*)))
// POP() returns nothing - it just alters the stack offset correctly
const int POP(numBytes) pCurEvalStack -= numBytes
// POP_ALL() empties the evaluation stack
const int POP_ALL() pCurEvalStack = pCurrentMethodState->pEvalStack

const int STACK_ADDR(type) *(type*)(pCurEvalStack - sizeof(type))
// General binary ops
const int BINARY_OP(returnType, type1, type2, op) \
	pCurEvalStack -= sizeof(type1) + sizeof(type2) - sizeof(returnType); \
	*(returnType*)(pCurEvalStack - sizeof(returnType)) = \
	*(type1*)(pCurEvalStack - sizeof(returnType)) op \
	*(type2*)(pCurEvalStack - sizeof(returnType) + sizeof(type1))
// General unary ops
const int UNARY_OP(type, op) STACK_ADDR(type) = op STACK_ADDR(type)

// Set the new method state (for use when the method state changes - in calls mainly)
const int SAVE_METHOD_STATE() \
	pCurrentMethodState->stackOfs = (uint)(pCurEvalStack - pCurrentMethodState->pEvalStack); \
	pCurrentMethodState->ipOffset = (uint)(pCurOp - pOps)

const int LOAD_METHOD_STATE() \
	pCurrentMethodState = pThread->pCurrentMethodState; \
	pParamsLocals = pCurrentMethodState->pParamsLocals; \
	pCurEvalStack = pCurrentMethodState->pEvalStack + pCurrentMethodState->stackOfs; \
	pJIT = pCurrentMethodState->pJIT; \
	pOps = pJIT->pOps; \
	pCurOp = pOps + pCurrentMethodState->ipOffset

const int CHANGE_METHOD_STATE(pNewMethodState) \
	SAVE_METHOD_STATE(); \
	pThread->pCurrentMethodState = pNewMethodState; \
	LOAD_METHOD_STATE()

// Easy access to method parameters and local variables
const int PARAMLOCAL_U32(offset) *(uint*)(pParamsLocals + offset)
const int PARAMLOCAL_U64(offset) *(ulong*)(pParamsLocals + offset)

const int THROW(exType) heapPtr = Heap.AllocType(exType); goto throwHeapPtr

// Note: newObj is only set if a constructor is being called
static void CreateParameters(byte* pParamsLocals, tMD_MethodDef *pCallMethod, byte* *ppCurEvalStack, /*HEAP_PTR*/byte* newObj) {
	uint ofs;

	if (newObj != null) {
		// If this is being called from JIT_NEW_OBJECT then need to specially push the new object
		// onto parameter stack position 0
		*(/*HEAP_PTR*/byte**)pParamsLocals = newObj;
		ofs = sizeof(/*HEAP_PTR*/byte**);
	} else {
		ofs = 0;
	}
	*ppCurEvalStack -= pCallMethod->parameterStackSize - ofs;
	Mem.memcpy(pParamsLocals + ofs, *ppCurEvalStack, pCallMethod->parameterStackSize - ofs);
}

static tMethodState* RunFinalizer(tThread *pThread) {
	/*HEAP_PTR*/byte* heapPtr = GetNextFinalizer();
	if (heapPtr != null) {
		// There is a pending finalizer, so create a MethodState for it and put it as next-to-run on the stack
		tMethodState *pFinalizerMethodState;
		tMD_TypeDef *pFinalizerType = Heap.GetType(heapPtr);

		pFinalizerMethodState = MethodState_Direct(pThread, pFinalizerType->pFinalizer, pThread->pCurrentMethodState, 0);
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
ulong opcodeTimes[JIT_OPCODE_MAXNUM];
static __inline unsigned __int64 __cdecl rdtsc() {
	__asm {
		rdtsc
	}
}
#endif

#if DIAG_OPCODE_USE
uint opcodeNumUses[JIT_OPCODE_MAXNUM];

#if TRACE
const int OPCODE_USE(op) printf("%s %X "#op "\n", pCurrentMethodState->pMethod->name, (int)(pCurEvalStack - pCurrentMethodState->pEvalStack)); opcodeNumUses[op]++;
#else
const int OPCODE_USE(op) opcodeNumUses[op]++;
#endif

#else

#if TRACE
const int OPCODE_USE(op) printf("%s %X "#op "\n", pCurrentMethodState->pMethod->name, (int)(pCurEvalStack - pCurrentMethodState->pEvalStack));
#else
const int OPCODE_USE(op)
#endif

#endif

const int GET_LABEL(var, label) var = &&label

const int GO_NEXT() goto *(void*)jitCodeInfo[*pCurOp++].pStart;

//const int GO_NEXT() goto *(void*)jitCodeInfo[*pCurOp++].pStart;

const int GO_NEXT_CHECK() \
	if (--numInst == 0) goto done; \
	GO_NEXT()

const int GET_LABELS(op) \
	GET_LABEL(pAddr, op##_start); \
	jitCodeInfo[op].pStart = pAddr; \
	GET_LABEL(pAddr, op##_end); \
	jitCodeInfo[op].pEnd = pAddr; \
	jitCodeInfo[op].isDynamic = 0

const int GET_LABELS_DYNAMIC(op, extraBytes) \
	GET_LABEL(pAddr, op##_start); \
	jitCodeInfo[op].pStart = pAddr; \
	GET_LABEL(pAddr, op##_end); \
	jitCodeInfo[op].pEnd = pAddr; \
	jitCodeInfo[op].isDynamic = 0x100 | (extraBytes & 0xff)

const int RUN_FINALIZER() {tMethodState *pMS = RunFinalizer(pThread);if(pMS) {CHANGE_METHOD_STATE(pMS);}}

uint JIT_Execute(tThread *pThread, uint numInst) {
	tJITted *pJIT;
	tMethodState *pCurrentMethodState;
	byte* pParamsLocals;

	// Local copies of thread state variables, to speed up execution
	// Pointer to next op-code
	uint *pOps;
	register uint *pCurOp;
	// Pointer to eval-stack position
	register byte* pCurEvalStack;
	byte* pTempPtr;

	uint op;
	// General purpose variables
	//int i32Value;
	uint u32Value; //, u32Value2;
	//ulong u64Value;
	//double dValue;
	//float fValue;
	//uConvDouble convDouble;
	uint ofs;
	/*HEAP_PTR*/byte* heapPtr;
	byte* pMem;

	if (pThread == null) {
		void *pAddr;
		// Special case to get all the label addresses
		// Default all op-codes to noCode.
		GET_LABEL(pAddr, noCode);
		for (u32Value = 0; u32Value < JIT_OPCODE_MAXNUM; u32Value++) {
			jitCodeInfo[u32Value].pStart = pAddr;
			jitCodeInfo[u32Value].pEnd = null;
			jitCodeInfo[u32Value].isDynamic = 0;
		}

		// Get GoNext code
		GET_LABEL(jitCodeGoNext.pStart, JIT_GoNext_start);
		GET_LABEL(jitCodeGoNext.pEnd, JIT_GoNext_end);
		jitCodeGoNext.isDynamic = 0;

		// Get all defined opcodes
		GET_LABELS_DYNAMIC(JIT_NOP, 0);
		GET_LABELS(JIT_RETURN);
		GET_LABELS_DYNAMIC(JIT_LOAD_I32, 4);
		GET_LABELS(JIT_BRANCH);
		GET_LABELS(JIT_LOAD_STRING);
		GET_LABELS(JIT_CALLVIRT_O);
		GET_LABELS(JIT_CALL_NATIVE);
		GET_LABELS(JIT_CALL_O);
		GET_LABELS(JIT_NEWOBJECT);
		GET_LABELS(JIT_LOAD_PARAMLOCAL_ADDR);
		GET_LABELS(JIT_CALL_PTR);
		GET_LABELS(JIT_BOX_CALLVIRT);
		GET_LABELS(JIT_INIT_VALUETYPE);
		GET_LABELS(JIT_NEW_VECTOR);
		GET_LABELS(JIT_NEWOBJECT_VALUETYPE);
		GET_LABELS(JIT_IS_INSTANCE);
		GET_LABELS(JIT_LOAD_NULL);
		GET_LABELS(JIT_UNBOX2VALUETYPE);
		GET_LABELS(JIT_UNBOX2OBJECT);
		GET_LABELS(JIT_LOAD_FIELD_ADDR);
		GET_LABELS(JIT_DUP_GENERAL);
		GET_LABELS_DYNAMIC(JIT_POP, 4);
		GET_LABELS(JIT_STORE_OBJECT_VALUETYPE);
		GET_LABELS(JIT_DEREF_CALLVIRT);
		GET_LABELS(JIT_STORE_ELEMENT);
		GET_LABELS(JIT_LEAVE);
		GET_LABELS(JIT_END_FINALLY);
		GET_LABELS(JIT_THROW);
		GET_LABELS(JIT_RETHROW);
		GET_LABELS(JIT_LOADOBJECT);
		GET_LABELS(JIT_LOAD_VECTOR_LEN);
		GET_LABELS(JIT_SWITCH);
		GET_LABELS(JIT_LOAD_ELEMENT_ADDR);
		GET_LABELS(JIT_CALL_INTERFACE);
		GET_LABELS(JIT_CAST_CLASS);
		GET_LABELS(JIT_LOAD_ELEMENT);
		GET_LABELS(JIT_LOADFIELD_VALUETYPE);
		GET_LABELS(JIT_LOADFIELD);
		GET_LABELS(JIT_LOADFUNCTION);
		GET_LABELS(JIT_INVOKE_DELEGATE);
		GET_LABELS(JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE);
		GET_LABELS(JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE);
		GET_LABELS(JIT_CALL_PINVOKE);
		GET_LABELS_DYNAMIC(JIT_LOAD_I64, 8);
		GET_LABELS(JIT_INIT_OBJECT);
		GET_LABELS_DYNAMIC(JIT_DUP_4, 0);
		GET_LABELS_DYNAMIC(JIT_DUP_8, 0);
		GET_LABELS(JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT);
		GET_LABELS_DYNAMIC(JIT_POP_4, 0);
		GET_LABELS_DYNAMIC(JIT_LOAD_F32, 4);

		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_INT64, 4);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_INT32, 4);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_INTNATIVE, 4);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_F32, 4);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_F64, 4);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_PTR, 4);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_O, 4);
		GET_LABELS(JIT_LOADPARAMLOCAL_VALUETYPE);

		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_0, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_1, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_2, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_3, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_4, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_5, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_6, 0);
		GET_LABELS_DYNAMIC(JIT_LOADPARAMLOCAL_7, 0);

		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_INT64, 4);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_INT32, 4);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_INTNATIVE, 4);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_F32, 4);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_F64, 4);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_PTR, 4);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_O, 4);
		GET_LABELS(JIT_STOREPARAMLOCAL_VALUETYPE);

		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_0, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_1, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_2, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_3, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_4, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_5, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_6, 0);
		GET_LABELS_DYNAMIC(JIT_STOREPARAMLOCAL_7, 0);

		GET_LABELS(JIT_STOREFIELD_INT64);
		GET_LABELS(JIT_STOREFIELD_INT32);
		GET_LABELS(JIT_STOREFIELD_INTNATIVE);
		GET_LABELS(JIT_STOREFIELD_F32);
		GET_LABELS(JIT_STOREFIELD_F64);
		GET_LABELS(JIT_STOREFIELD_PTR);
		GET_LABELS(JIT_STOREFIELD_O);
		GET_LABELS(JIT_STOREFIELD_VALUETYPE);

		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32);
		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE);
		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_O);
		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_INTNATIVE);
		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR);
		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_F32);
		GET_LABELS(JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64);

		GET_LABELS(JIT_STORESTATICFIELD_INT32);
		GET_LABELS(JIT_STORESTATICFIELD_INT64);
		GET_LABELS(JIT_STORESTATICFIELD_O);
		GET_LABELS(JIT_STORESTATICFIELD_F32);
		GET_LABELS(JIT_STORESTATICFIELD_F64);
		GET_LABELS(JIT_STORESTATICFIELD_INTNATIVE);
		GET_LABELS(JIT_STORESTATICFIELD_PTR);
		GET_LABELS(JIT_STORESTATICFIELD_VALUETYPE);

		GET_LABELS(JIT_BOX_INT64);
		GET_LABELS(JIT_BOX_INT32);
		GET_LABELS(JIT_BOX_INTNATIVE);
		GET_LABELS(JIT_BOX_F32);
		GET_LABELS(JIT_BOX_F64);
		GET_LABELS(JIT_BOX_O);
		GET_LABELS(JIT_BOX_VALUETYPE);

		GET_LABELS_DYNAMIC(JIT_CEQ_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_CGT_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_CGT_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_CLT_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_CLT_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_CEQ_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_CGT_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_CGT_UN_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_CLT_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_CLT_UN_I64I64, 0);

		GET_LABELS_DYNAMIC(JIT_ADD_OVF_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_ADD_OVF_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_MUL_OVF_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_MUL_OVF_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_SUB_OVF_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_SUB_OVF_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_ADD_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_SUB_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_MUL_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_DIV_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_DIV_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_REM_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_REM_UN_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_AND_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_OR_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_XOR_I32I32, 0);
		GET_LABELS_DYNAMIC(JIT_NEG_I32, 0);
		GET_LABELS_DYNAMIC(JIT_NOT_I32, 0);
		GET_LABELS_DYNAMIC(JIT_NEG_I64, 0);
		GET_LABELS_DYNAMIC(JIT_NOT_I64, 0);

		GET_LABELS(JIT_BOX_NULLABLE);
		GET_LABELS_DYNAMIC(JIT_LOAD_F64, 8);
		GET_LABELS(JIT_UNBOX_NULLABLE);

		GET_LABELS(JIT_BEQ_I32I32);
		GET_LABELS(JIT_BEQ_I64I64);
		GET_LABELS(JIT_BEQ_F32F32);
		GET_LABELS(JIT_BEQ_F64F64);

		GET_LABELS(JIT_BGE_I32I32);
		GET_LABELS(JIT_BGE_I64I64);
		GET_LABELS(JIT_BGE_F32F32);
		GET_LABELS(JIT_BGE_F64F64);
		GET_LABELS(JIT_BGE_UN_F32F32);
		GET_LABELS(JIT_BGE_UN_F64F64);

		GET_LABELS(JIT_BGT_I32I32);
		GET_LABELS(JIT_BGT_I64I64);
		GET_LABELS(JIT_BGT_F32F32);
		GET_LABELS(JIT_BGT_F64F64);
		GET_LABELS(JIT_BGT_UN_F32F32);
		GET_LABELS(JIT_BGT_UN_F64F64);

		GET_LABELS(JIT_BLE_I32I32);
		GET_LABELS(JIT_BLE_I64I64);
		GET_LABELS(JIT_BLE_F32F32);
		GET_LABELS(JIT_BLE_F64F64);
		GET_LABELS(JIT_BLE_UN_F32F32);
		GET_LABELS(JIT_BLE_UN_F64F64);

		GET_LABELS(JIT_BLT_I32I32);
		GET_LABELS(JIT_BLT_I64I64);
		GET_LABELS(JIT_BLT_F32F32);
		GET_LABELS(JIT_BLT_F64F64);
		GET_LABELS(JIT_BLT_UN_F32F32);
		GET_LABELS(JIT_BLT_UN_F64F64);

		GET_LABELS(JIT_BNE_UN_I32I32);
		GET_LABELS(JIT_BNE_UN_I64I64);
		GET_LABELS(JIT_BNE_UN_F32F32);
		GET_LABELS(JIT_BNE_UN_F64F64);

		GET_LABELS(JIT_BGE_UN_I32I32);
		GET_LABELS(JIT_BGT_UN_I32I32);
		GET_LABELS(JIT_BLE_UN_I32I32);
		GET_LABELS(JIT_BLT_UN_I32I32);

		GET_LABELS_DYNAMIC(JIT_SHL_I32, 0);
		GET_LABELS_DYNAMIC(JIT_SHR_I32, 0);
		GET_LABELS_DYNAMIC(JIT_SHR_UN_I32, 0);
		GET_LABELS_DYNAMIC(JIT_SHL_I64, 0);
		GET_LABELS_DYNAMIC(JIT_SHR_I64, 0);
		GET_LABELS_DYNAMIC(JIT_SHR_UN_I64, 0);

		GET_LABELS(JIT_LOADTOKEN_TYPE);
		
		GET_LABELS(JIT_LOADTOKEN_FIELD);
		GET_LABELS(JIT_LOADINDIRECT_I8);
		GET_LABELS(JIT_LOADINDIRECT_U8);
		GET_LABELS(JIT_LOADINDIRECT_I16);
		GET_LABELS(JIT_LOADINDIRECT_U16);
		GET_LABELS(JIT_LOADINDIRECT_I32);
		GET_LABELS(JIT_LOADINDIRECT_U32);
		GET_LABELS(JIT_LOADINDIRECT_I64);

		GET_LABELS(JIT_LOADINDIRECT_R32);
		GET_LABELS(JIT_LOADINDIRECT_R64);
		GET_LABELS(JIT_LOADINDIRECT_REF);
		GET_LABELS(JIT_STOREINDIRECT_REF);
		GET_LABELS(JIT_STOREINDIRECT_U8);
		GET_LABELS(JIT_STOREINDIRECT_U16);
		GET_LABELS(JIT_STOREINDIRECT_U32);

		GET_LABELS_DYNAMIC(JIT_CONV_I32_I32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_I32_U32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_I32_I64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_I32_U64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_I32_R32, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_I32_R64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U32_I32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_U32_U32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_U32_I64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U32_U64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U32_R32, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U32_R64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_I64_I32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_I64_U32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_I64_U64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_I64_R32, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_I64_R64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U64_I32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_U64_U32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_U64_I64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U64_R32, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_U64_R64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R32_I32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_R32_U32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_R32_I64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R32_U64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R32_R32, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R32_R64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R64_I32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_R64_U32, 4);
		GET_LABELS_DYNAMIC(JIT_CONV_R64_I64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R64_U64, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R64_R32, 0);
		GET_LABELS_DYNAMIC(JIT_CONV_R64_R64, 0);

		GET_LABELS(JIT_STORE_ELEMENT_32);
		GET_LABELS(JIT_STORE_ELEMENT_64);

		GET_LABELS(JIT_LOAD_ELEMENT_I8);
		GET_LABELS(JIT_LOAD_ELEMENT_U8);
		GET_LABELS(JIT_LOAD_ELEMENT_I16);
		GET_LABELS(JIT_LOAD_ELEMENT_U16);
		GET_LABELS(JIT_LOAD_ELEMENT_I32);
		GET_LABELS(JIT_LOAD_ELEMENT_U32);
		GET_LABELS(JIT_LOAD_ELEMENT_I64);
		GET_LABELS(JIT_LOAD_ELEMENT_R32);
		GET_LABELS(JIT_LOAD_ELEMENT_R64);

		GET_LABELS_DYNAMIC(JIT_ADD_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_SUB_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_MUL_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_DIV_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_DIV_UN_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_REM_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_REM_UN_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_AND_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_OR_I64I64, 0);
		GET_LABELS_DYNAMIC(JIT_XOR_I64I64, 0);

		GET_LABELS_DYNAMIC(JIT_CEQ_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_CGT_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_CLT_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_CEQ_F64F64, 0);
		GET_LABELS_DYNAMIC(JIT_CGT_F64F64, 0);
		GET_LABELS_DYNAMIC(JIT_CLT_F64F64, 0);

		GET_LABELS_DYNAMIC(JIT_ADD_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_ADD_F64F64, 0);
		GET_LABELS_DYNAMIC(JIT_SUB_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_SUB_F64F64, 0);
		GET_LABELS_DYNAMIC(JIT_MUL_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_MUL_F64F64, 0);
		GET_LABELS_DYNAMIC(JIT_DIV_F32F32, 0);
		GET_LABELS_DYNAMIC(JIT_DIV_F64F64, 0);

		GET_LABELS_DYNAMIC(JIT_LOAD_I4_M1, 0);
		GET_LABELS_DYNAMIC(JIT_LOAD_I4_0, 0);
		GET_LABELS_DYNAMIC(JIT_LOAD_I4_1, 0);
		GET_LABELS_DYNAMIC(JIT_LOAD_I4_2, 0);

        GET_LABELS_DYNAMIC(JIT_LOADFIELD_4, 4);
		GET_LABELS_DYNAMIC(JIT_LOADFIELD_8, 4);

        GET_LABELS(JIT_BRANCH_FALSE_U32);
        GET_LABELS(JIT_BRANCH_TRUE_U32);
        GET_LABELS(JIT_BRANCH_FALSE_U64);
        GET_LABELS(JIT_BRANCH_TRUE_U64);
        
		return 0;
	}

#if DIAG_OPCODE_TIMES
	ulong opcodeStartTime = rdtsc();
	uint realOp;
#endif

	LOAD_METHOD_STATE();

	GO_NEXT();

noCode:
	Sys.Crash("No code for op-code");

JIT_NOP_start:
JIT_CONV_R32_R32_start:
JIT_CONV_R64_R64_start:
JIT_CONV_I64_U64_start:
JIT_CONV_U64_I64_start:
	OPCODE_USE(JIT_NOP);
JIT_NOP_end:
JIT_CONV_R32_R32_end:
JIT_CONV_R64_R64_end:
JIT_CONV_I64_U64_end:
JIT_CONV_U64_I64_end:
JIT_GoNext_start:
	GO_NEXT();
JIT_GoNext_end:

JIT_LOAD_NULL_start:
	OPCODE_USE(JIT_LOAD_NULL);
	PUSH_O(null);
JIT_LOAD_NULL_end:
	GO_NEXT();

JIT_DUP_4_start:
	OPCODE_USE(JIT_DUP_4);
	DUP4();
JIT_DUP_4_end:
	GO_NEXT();

JIT_DUP_8_start:
	OPCODE_USE(JIT_DUP_8);
	DUP8();
JIT_DUP_8_end:
	GO_NEXT();

JIT_DUP_GENERAL_start:
	OPCODE_USE(JIT_DUP_GENERAL);
	{
		uint dupSize = GET_OP();
		DUP(dupSize);
	}
JIT_DUP_GENERAL_end:
	GO_NEXT();

JIT_POP_start:
	OPCODE_USE(JIT_POP);
	{
		uint popSize = GET_OP();
		POP(popSize);
	}
JIT_POP_end:
	GO_NEXT();

JIT_POP_4_start:
	OPCODE_USE(JIT_POP_4);
	POP(4);
JIT_POP_4_end:
	GO_NEXT();

JIT_LOAD_I32_start:
JIT_LOAD_F32_start:
	OPCODE_USE(JIT_LOAD_I32);
	{
		int value = GET_OP();
		PUSH_U32(value);
	}
JIT_LOAD_I32_end:
JIT_LOAD_F32_end:
	GO_NEXT();

JIT_LOAD_I4_M1_start:
	OPCODE_USE(JIT_LOAD_I4_M1);
	PUSH_U32(-1);
JIT_LOAD_I4_M1_end:
	GO_NEXT();

JIT_LOAD_I4_0_start:
	OPCODE_USE(JIT_LOAD_I4_0);
	PUSH_U32(0);
JIT_LOAD_I4_0_end:
	GO_NEXT();

JIT_LOAD_I4_1_start:
	OPCODE_USE(JIT_LOAD_I4_1);
	PUSH_U32(1);
JIT_LOAD_I4_1_end:
	GO_NEXT();

JIT_LOAD_I4_2_start:
	OPCODE_USE(JIT_LOAD_I4_2);
	PUSH_U32(2);
JIT_LOAD_I4_2_end:
	GO_NEXT();

JIT_LOAD_I64_start:
JIT_LOAD_F64_start:
	OPCODE_USE(JIT_LOAD_I64);
	{
		ulong value = *(ulong*)pCurOp;
		pCurOp += 2;
		PUSH_U64(value);
	}
JIT_LOAD_I64_end:
JIT_LOAD_F64_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_INT32_start:
JIT_LOADPARAMLOCAL_F32_start:
JIT_LOADPARAMLOCAL_INTNATIVE_start: // Only on 32-bit
	OPCODE_USE(JIT_LOADPARAMLOCAL_INT32);
	{
		uint ofs = GET_OP();
		uint value = PARAMLOCAL_U32(ofs);
		PUSH_U32(value);
	}
JIT_LOADPARAMLOCAL_INT32_end:
JIT_LOADPARAMLOCAL_F32_end:
JIT_LOADPARAMLOCAL_INTNATIVE_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_O_start:
JIT_LOADPARAMLOCAL_PTR_start:
    OPCODE_USE(JIT_LOADPARAMLOCAL_O);
    {
#if UNITY_WEBGL || DNA_32BIT
        uint ofs = GET_OP();
        uint value = PARAMLOCAL_U32(ofs);
        PUSH_U32(value);
#else
        uint ofs = GET_OP();
        ulong value = PARAMLOCAL_U64(ofs);
        PUSH_U64(value);
#endif
    }
JIT_LOADPARAMLOCAL_O_end:
JIT_LOADPARAMLOCAL_PTR_end:
    GO_NEXT();
    
    
JIT_LOADPARAMLOCAL_INT64_start:
JIT_LOADPARAMLOCAL_F64_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_INT64);
	{
		uint ofs = GET_OP();
		ulong value = PARAMLOCAL_U64(ofs);
		PUSH_U64(value);
	}
JIT_LOADPARAMLOCAL_INT64_end:
JIT_LOADPARAMLOCAL_F64_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_VALUETYPE_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_VALUETYPE);
	{
		tMD_TypeDef *pTypeDef;
		uint ofs;
		byte* pMem;

		ofs = GET_OP();
		pTypeDef = (tMD_TypeDef*)GET_PTR();
		pMem = pParamsLocals + ofs;
		PUSH_VALUETYPE(pMem, pTypeDef->stackSize, pTypeDef->stackSize);
	}
JIT_LOADPARAMLOCAL_VALUETYPE_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_0_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_0);
	PUSH_U32(PARAMLOCAL_U32(0));
JIT_LOADPARAMLOCAL_0_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_1_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_1);
	PUSH_U32(PARAMLOCAL_U32(4));
JIT_LOADPARAMLOCAL_1_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_2_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_2);
	PUSH_U32(PARAMLOCAL_U32(8));
JIT_LOADPARAMLOCAL_2_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_3_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_3);
	PUSH_U32(PARAMLOCAL_U32(12));
JIT_LOADPARAMLOCAL_3_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_4_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_4);
	PUSH_U32(PARAMLOCAL_U32(16));
JIT_LOADPARAMLOCAL_4_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_5_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_5);
	PUSH_U32(PARAMLOCAL_U32(20));
JIT_LOADPARAMLOCAL_5_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_6_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_6);
	PUSH_U32(PARAMLOCAL_U32(24));
JIT_LOADPARAMLOCAL_6_end:
	GO_NEXT();

JIT_LOADPARAMLOCAL_7_start:
	OPCODE_USE(JIT_LOADPARAMLOCAL_7);
	PUSH_U32(PARAMLOCAL_U32(28));
JIT_LOADPARAMLOCAL_7_end:
	GO_NEXT();

JIT_LOAD_PARAMLOCAL_ADDR_start:
	OPCODE_USE(JIT_LOAD_PARAMLOCAL_ADDR);
	{
		uint ofs = GET_OP();
		byte* pMem = pParamsLocals + ofs;
		PUSH_PTR(pMem);
	}
JIT_LOAD_PARAMLOCAL_ADDR_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_INT32_start:
JIT_STOREPARAMLOCAL_F32_start:
JIT_STOREPARAMLOCAL_INTNATIVE_start: // Only on 32-bit
	OPCODE_USE(JIT_STOREPARAMLOCAL_INT32);
	{
		uint ofs = GET_OP();
		uint value = POP_U32();
		PARAMLOCAL_U32(ofs) = value;
	}
JIT_STOREPARAMLOCAL_INT32_end:
JIT_STOREPARAMLOCAL_F32_end:
JIT_STOREPARAMLOCAL_INTNATIVE_end:
	GO_NEXT();

    
JIT_STOREPARAMLOCAL_O_start:
JIT_STOREPARAMLOCAL_PTR_start:
    OPCODE_USE(JIT_STOREPARAMLOCAL_PTR);
    {
#if UNITY_WEBGL || DNA_32BIT
        uint ofs = GET_OP();
        uint value = POP_U32();
        PARAMLOCAL_U32(ofs) = value;
#else
        uint ofs = GET_OP();
        ulong value = POP_U64();
        PARAMLOCAL_U64(ofs) = value;
#endif
    }
JIT_STOREPARAMLOCAL_O_end:
JIT_STOREPARAMLOCAL_PTR_end:
    GO_NEXT();
    
JIT_STOREPARAMLOCAL_INT64_start:
JIT_STOREPARAMLOCAL_F64_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_INT64);
	{
		uint ofs = GET_OP();
		ulong value = POP_U64();
		PARAMLOCAL_U64(ofs) = value;
	}
JIT_STOREPARAMLOCAL_INT64_end:
JIT_STOREPARAMLOCAL_F64_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_VALUETYPE_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_VALUETYPE);
	{
		tMD_TypeDef *pTypeDef;
		uint ofs;
		byte* pMem;

		ofs = GET_OP();
		pTypeDef = (tMD_TypeDef*)GET_PTR();
		pMem = pParamsLocals + ofs;
		POP_VALUETYPE(pMem, pTypeDef->stackSize, pTypeDef->stackSize);
	}
JIT_STOREPARAMLOCAL_VALUETYPE_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_0_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_0);
	PARAMLOCAL_U32(0) = POP_U32();
JIT_STOREPARAMLOCAL_0_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_1_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_1);
	PARAMLOCAL_U32(4) = POP_U32();
JIT_STOREPARAMLOCAL_1_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_2_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_2);
	PARAMLOCAL_U32(8) = POP_U32();
JIT_STOREPARAMLOCAL_2_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_3_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_3);
	PARAMLOCAL_U32(12) = POP_U32();
JIT_STOREPARAMLOCAL_3_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_4_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_4);
	PARAMLOCAL_U32(16) = POP_U32();
JIT_STOREPARAMLOCAL_4_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_5_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_5);
	PARAMLOCAL_U32(20) = POP_U32();
JIT_STOREPARAMLOCAL_5_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_6_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_6);
	PARAMLOCAL_U32(24) = POP_U32();
JIT_STOREPARAMLOCAL_6_end:
	GO_NEXT();

JIT_STOREPARAMLOCAL_7_start:
	OPCODE_USE(JIT_STOREPARAMLOCAL_7);
	PARAMLOCAL_U32(28) = POP_U32();
JIT_STOREPARAMLOCAL_7_end:
	GO_NEXT();

JIT_LOADINDIRECT_I8_start:
JIT_LOADINDIRECT_I16_start:
JIT_LOADINDIRECT_I32_start:
JIT_LOADINDIRECT_U8_start:
JIT_LOADINDIRECT_U16_start:
JIT_LOADINDIRECT_U32_start:
JIT_LOADINDIRECT_R32_start:
	OPCODE_USE(JIT_LOADINDIRECT_U32);
	{
		byte* pMem = POP_PTR();
		uint value = *(uint*)pMem;
		PUSH_U32(value);
	}
JIT_LOADINDIRECT_I8_end:
JIT_LOADINDIRECT_I16_end:
JIT_LOADINDIRECT_I32_end:
JIT_LOADINDIRECT_U8_end:
JIT_LOADINDIRECT_U16_end:
JIT_LOADINDIRECT_U32_end:
JIT_LOADINDIRECT_R32_end:
	GO_NEXT();

JIT_LOADINDIRECT_REF_start:
    OPCODE_USE(JIT_LOADINDIRECT_U32);
    {
        byte* pMem = POP_PTR();
#if UNITY_WEBGL || DNA_32BIT
        uint value = *(uint*)pMem;
        PUSH_U32(value);
#else
        ulong value = *(ulong*)pMem;
        PUSH_U64(value);
#endif
    }
JIT_LOADINDIRECT_REF_end:
    GO_NEXT();
    
JIT_LOADINDIRECT_R64_start:
JIT_LOADINDIRECT_I64_start:
	OPCODE_USE(JIT_LOADINDIRECT_I64);
	{
		byte* pMem = POP_PTR();
		ulong value = *(ulong*)pMem;
		PUSH_U64(value);
	}
JIT_LOADINDIRECT_R64_end:
JIT_LOADINDIRECT_I64_end:
	GO_NEXT();

JIT_STOREINDIRECT_U8_start:
JIT_STOREINDIRECT_U16_start:
JIT_STOREINDIRECT_U32_start:
	OPCODE_USE(JIT_STOREINDIRECT_U32);
	{
		uint value = POP_U32(); // The value to store
		byte* pMem = POP_PTR(); // The address to store to
		*(uint*)pMem = value;
	}
JIT_STOREINDIRECT_U8_end:
JIT_STOREINDIRECT_U16_end:
JIT_STOREINDIRECT_U32_end:
	GO_NEXT();

JIT_STOREINDIRECT_REF_start:
    OPCODE_USE(JIT_STOREINDIRECT_U32);
    {
#if UNITY_WEBGL || DNA_32BIT
        uint value = POP_U32(); // The value to store
        byte* pMem = POP_PTR(); // The address to store to
        *(uint*)pMem = value;
#else
        ulong value = POP_U64(); // The value to store
        byte* pMem = POP_PTR(); // The address to store to
        *(ulong*)pMem = value;
#endif
    }
JIT_STOREINDIRECT_REF_end:
    GO_NEXT();
    
JIT_STORE_OBJECT_VALUETYPE_start:
	OPCODE_USE(JIT_STORE_OBJECT_VALUETYPE);
	{
		uint size = GET_OP(); // The size, in bytes, of the value-type to store
		uint memSize = (size<4)?4:size;
		byte* pMem = pCurEvalStack - memSize - sizeof(void*);
		POP_VALUETYPE(*(void**)pMem, size, memSize);
		POP(4);
	}
JIT_STORE_OBJECT_VALUETYPE_end:
	GO_NEXT();

JIT_CALL_PINVOKE_start:
	OPCODE_USE(JIT_CALL_PINVOKE);
	{
		tJITCallPInvoke *pCallPInvoke;
		uint res;

		pCallPInvoke = (tJITCallPInvoke*)(pCurOp - 1);
		res = PInvoke_Call(pCallPInvoke, pParamsLocals, pCurrentMethodState->pEvalStack, pThread);
		pCurrentMethodState->stackOfs = res;
	}
	goto JIT_RETURN_start;
JIT_CALL_PINVOKE_end:

JIT_CALL_NATIVE_start:
	OPCODE_USE(JIT_CALL_NATIVE);
	{
		tJITCallNative *pCallNative;
		byte* pThis;
		uint thisOfs;
		tAsyncCall *pAsync;

		//pCallNative = (tJITCallNative*)&(pJIT->pOps[pCurrentMethodState->ipOffset - 1]);
		pCallNative = (tJITCallNative*)(pCurOp - 1);
		if (MetaData.MetaData.METHOD_ISSTATIC(pCallNative->pMethodDef)) {
			pThis = null;
			thisOfs = 0;
		} else {
			pThis = *(byte**)pCurrentMethodState->pParamsLocals;
			thisOfs = sizeof(void*);
		}
		// Internal constructors MUST leave the newly created object in the return value
		// (ie on top of the evaluation stack)
		pAsync = pCallNative->fn(pThis, pCurrentMethodState->pParamsLocals + thisOfs, pCurrentMethodState->pEvalStack);
		if (pAsync != null) {
			// Save the method state
			SAVE_METHOD_STATE();
			// Change the IP pointer to point to the return instruction
			pCurrentMethodState->ipOffset = 3;
			// Handle special async codes
            if (pAsync == Thread.ASYNC_LOCK_EXIT()) {
				return THREAD_STATUS_LOCK_EXIT;
			}
			// Set the async in the thread
			pThread->pAsync = pAsync;
			return THREAD_STATUS_ASYNC;
		}
	}
	// fall-through
JIT_CALL_NATIVE_end:

JIT_RETURN_start:
	OPCODE_USE(JIT_RETURN);
#if TRACE
	Sys.log_f(2, "Returned from %s() to %s()\n", pCurrentMethodState->pMethod->name, (pCurrentMethodState->pCaller)?pCurrentMethodState->pCaller->pMethod->name:"<none>");
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
	} else if (pCurrentMethodState->isInternalNewObjCall) {
		u32Value = sizeof(void*);
	} else {
		u32Value = 0;
	}
	pMem = pCurrentMethodState->pEvalStack;
	{
		tMethodState *pOldMethodState = pCurrentMethodState;
		pThread->pCurrentMethodState = pCurrentMethodState->pCaller;
		LOAD_METHOD_STATE();
		// Copy return value to callers evaluation stack
		if (u32Value > 0) {
			memmove(pCurEvalStack, pMem, u32Value);
			pCurEvalStack += u32Value;
		}
		// Delete the current method state and go back to callers method state
		MethodState_Delete(pThread, &pOldMethodState);
	}
	if (pCurrentMethodState->pNextDelegate == null) {
		GO_NEXT();
	}
	// Fall-through if more delegate methods to invoke
JIT_RETURN_end:

JIT_INVOKE_DELEGATE_start:
	OPCODE_USE(JIT_INVOKE_DELEGATE);
	{
		tMD_MethodDef *pDelegateMethod, *pCallMethod;
		void *pDelegate;
		/*HEAP_PTR*/byte* pDelegateThis;
		tMethodState *pCallMethodState;
		uint ofs;

		if (pCurrentMethodState->pNextDelegate == null) {
			// First delegate, so get the Invoke() method defined within the delegate class
			pDelegateMethod = (tMD_MethodDef*)GET_PTR();
			// Take the params off the stack. This is the pointer to the tDelegate & params
			//pCurrentMethodState->stackOfs -= pDelegateMethod->parameterStackSize;
			pCurEvalStack -= pDelegateMethod->parameterStackSize;
			// Allocate memory for delegate params
			pCurrentMethodState->pDelegateParams = Mem.malloc(pDelegateMethod->parameterStackSize - sizeof(void*));
			Mem.memcpy(
				pCurrentMethodState->pDelegateParams,
				//pCurrentMethodState->pEvalStack + pCurrentMethodState->stackOfs + sizeof(void*),
				pCurEvalStack + sizeof(void*),
				pDelegateMethod->parameterStackSize - sizeof(void*));
			// Get the actual delegate heap pointer
			pDelegate = *(void**)pCurEvalStack;
		} else {
			pDelegateMethod = Delegate_GetMethod(pCurrentMethodState->pNextDelegate);
			if (pDelegateMethod->pReturnType != null) {
				pCurEvalStack -= pDelegateMethod->pReturnType->stackSize;
			}
			// Get the actual delegate heap pointer
			pDelegate = pCurrentMethodState->pNextDelegate;
		}
		if (pDelegate == null) {
			THROW(Type.types[Type.TYPE_SYSTEM_NULLREFERENCEEXCEPTION]);
		}
		// Get the real method to call; the target of the delegate.
		pCallMethod = Delegate_GetMethodAndStore(pDelegate, &pDelegateThis, &pCurrentMethodState->pNextDelegate);
		// Set up the call method state for the call.
		pCallMethodState = MethodState_Direct(pThread, pCallMethod, pCurrentMethodState, 0);
		if (pDelegateThis != null) {
			*(/*HEAP_PTR*/byte**)pCallMethodState->pParamsLocals = pDelegateThis;
			ofs = sizeof(void*);
		} else {
			ofs = 0;
		}
		Mem.memcpy(pCallMethodState->pParamsLocals + ofs,
			pCurrentMethodState->pDelegateParams,
			pCallMethod->parameterStackSize - ofs);
		CHANGE_METHOD_STATE(pCallMethodState);
	}
JIT_INVOKE_DELEGATE_end:
	GO_NEXT();

JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE_start:
	OPCODE_USE(JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE);
	{
		// Get the reference to MethodBase.Invoke
		tMD_MethodDef *pInvokeMethod = (tMD_MethodDef*)GET_PTR();

		// Take the MethodBase.Invoke params off the stack.
		pCurEvalStack -= pInvokeMethod->parameterStackSize;

		// Get a pointer to the MethodBase instance (e.g., a MethodInfo or ConstructorInfo),
		// and from that, determine which method we're going to invoke
		tMethodBase *pMethodBase = *(tMethodBase**)pCurEvalStack;
		tMD_MethodDef *pCallMethod = pMethodBase->methodDef;

		// Store the return type so that JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE can
		// interpret the stack after the invocation
		pCurrentMethodState->pReflectionInvokeReturnType = pCallMethod->pReturnType;

		// Get the 'this' pointer for the call and the params array
		byte* invocationThis = *(tMethodBase**)(pCurEvalStack + sizeof(/*HEAP_PTR*/byte*));
		/*HEAP_PTR*/byte* invocationParamsArray = *(/*HEAP_PTR*/byte**)(pCurEvalStack + sizeof(/*HEAP_PTR*/byte*) + sizeof(byte*));		

		// Put the new 'this' on the stack
		byte* pPrevEvalStack = pCurEvalStack;
		PUSH_PTR(invocationThis);

		// Put any other params on the stack
		if (invocationParamsArray != null) {
			uint invocationParamsArrayLength = SystemArray.GetLength(invocationParamsArray);
			byte* invocationParamsArrayElements = SystemArray.GetElements(invocationParamsArray);
			for (uint paramIndex = 0; paramIndex < invocationParamsArrayLength; paramIndex++) {
				/*HEAP_PTR*/byte* currentParam = ((uint*)(invocationParamsArrayElements))[paramIndex];
				if (currentParam == null) {
					PUSH_O(null);
				} else {
					tMD_TypeDef *currentParamType = Heap.GetType(currentParam);

					if (Type_IsValueType(currentParamType)) {
						PUSH_VALUETYPE(currentParam, currentParamType->stackSize, currentParamType->stackSize);
					} else {
						PUSH_O(currentParam);
					}
				}
			}
		}
		pCurEvalStack = pPrevEvalStack;

		// Change interpreter state so we continue execution inside the method being invoked
		tMethodState *pCallMethodState = MethodState_Direct(pThread, pCallMethod, pCurrentMethodState, 0);
		Mem.memcpy(pCallMethodState->pParamsLocals, pCurEvalStack, pCallMethod->parameterStackSize);
		CHANGE_METHOD_STATE(pCallMethodState);
	}
JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE_end:
	GO_NEXT();

JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE_start:
	OPCODE_USE(JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE);
	{
		tMD_TypeDef *pLastInvocationReturnType = pCurrentMethodState->pReflectionInvokeReturnType;
		if (pLastInvocationReturnType == null) {
			// It was a void method, so it won't have put anything on the stack. We need to put
			// a null value there as a return value, because MethodBase.Invoke isn't void.
			PUSH_O(null);
		} else if (Type_IsValueType(pLastInvocationReturnType)) {
			// For value Type.types, remove the raw value data from the stack and replace it with a
			// boxed copy, because MethodBase.Invoke returns object.
			/*HEAP_PTR*/byte* heapPtr = Heap.AllocType(pLastInvocationReturnType);
			POP_VALUETYPE(heapPtr, pLastInvocationReturnType->stackSize, pLastInvocationReturnType->stackSize);
			PUSH_O(heapPtr);
		}
	}

JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE_end:
	GO_NEXT_CHECK();

JIT_DEREF_CALLVIRT_start:
	op = JIT_DEREF_CALLVIRT;
	goto allCallStart;
JIT_BOX_CALLVIRT_start:
	op = JIT_BOX_CALLVIRT;
	goto allCallStart;
JIT_CALL_PTR_start: // Note that JIT_CALL_PTR cannot be virtual
	op = JIT_CALL_PTR;
	goto allCallStart;
JIT_CALLVIRT_O_start:
	op = JIT_CALLVIRT_O;
	goto allCallStart;
JIT_CALL_O_start:
	op = JIT_CALL_O;
	goto allCallStart;
JIT_CALL_INTERFACE_start:
	op = JIT_CALL_INTERFACE;
allCallStart:
	OPCODE_USE(JIT_CALL_O);
	{
		tMD_MethodDef *pCallMethod;
		tMethodState *pCallMethodState;
		tMD_TypeDef *pBoxCallType;

		if (op == JIT_BOX_CALLVIRT) {
			pBoxCallType = (tMD_TypeDef*)GET_PTR();
		}

		pCallMethod = (tMD_MethodDef*)GET_PTR();
		heapPtr = null;

		if (op == JIT_BOX_CALLVIRT) {
			// Need to de-ref and box the value-type before calling the function
			// TODO: Will this work on value-Type.types that are not 4 bytes long?
			pMem = pCurEvalStack - pCallMethod->parameterStackSize;
			heapPtr = Heap_Box(pBoxCallType, *(byte**)pMem);
			*(/*HEAP_PTR*/byte**)pMem = heapPtr;
		} else if (op == JIT_DEREF_CALLVIRT) {
			pMem = pCurEvalStack - pCallMethod->parameterStackSize;
			*(/*HEAP_PTR*/byte**)pMem = **(/*HEAP_PTR*/byte***)pMem;
		}

		// If it's a virtual call then find the real correct method to call
		if (op == JIT_CALLVIRT_O || op == JIT_BOX_CALLVIRT || op == JIT_DEREF_CALLVIRT) {
			tMD_TypeDef *pThisType;
			// Get the actual object that is becoming 'this'
			if (heapPtr == null) {
				heapPtr = *(/*HEAP_PTR*/byte**)(pCurEvalStack - pCallMethod->parameterStackSize);
			}
			if (heapPtr == null) {
				//Sys.Crash("null 'this' in Virtual call: %s", Sys_GetMethodDesc(pCallMethod));
				THROW(Type.types[Type.TYPE_SYSTEM_NULLREFERENCEEXCEPTION]);
			}
			pThisType = Heap.GetType(heapPtr);
			if (MetaData.MetaData.METHOD_ISVIRTUAL(pCallMethod)) {
				pCallMethod = pThisType->pVTable[pCallMethod->vTableOfs];
			}
		} else if (op == JIT_CALL_INTERFACE) {
			tMD_TypeDef *pInterface, *pThisType;
			uint vIndex;
			int i;

			pInterface = pCallMethod->pParentType;
			// Get the actual object that is becoming 'this'
			heapPtr = *(/*HEAP_PTR*/byte**)(pCurEvalStack - pCallMethod->parameterStackSize);
			pThisType = Heap.GetType(heapPtr);
			// Find the interface mapping on the 'this' type.
			vIndex = 0xffffffff;
			// This must be searched backwards so if an interface is implemented more than
			// once in the type hierarchy, the most recent definition gets called
			for (i=(int)pThisType->numInterfaces-1; i >= 0; i--) {
				if (pThisType->pInterfaceMaps[i].pInterface == pInterface) {
					// Found the right interface map
					if (pThisType->pInterfaceMaps[i].pVTableLookup != null) {
						vIndex = pThisType->pInterfaceMaps[i].pVTableLookup[pCallMethod->vTableOfs];
						break;
					}
					pCallMethod = pThisType->pInterfaceMaps[i].ppMethodVLookup[pCallMethod->vTableOfs];
					goto callMethodSet;
				}
			}
			Assert(vIndex != 0xffffffff);
			pCallMethod = pThisType->pVTable[vIndex];
		}
callMethodSet:
		//printf("Calling method: %s\n", Sys_GetMethodDesc(pCallMethod));
		// Set up the new method state for the called method
		pCallMethodState = MethodState_Direct(pThread, pCallMethod, pCurrentMethodState, 0);
		// Set up the parameter stack for the method being called
		pTempPtr = pCurEvalStack;
		CreateParameters(pCallMethodState->pParamsLocals, pCallMethod, &/*pCurEvalStack*/pTempPtr, null);
		pCurEvalStack = pTempPtr;
		// Set up the local variables for the new method state
		CHANGE_METHOD_STATE(pCallMethodState);
	}
JIT_DEREF_CALLVIRT_end:
JIT_BOX_CALLVIRT_end:
JIT_CALL_PTR_end:
JIT_CALLVIRT_O_end:
JIT_CALL_O_end:
JIT_CALL_INTERFACE_end:
	GO_NEXT_CHECK();

JIT_BRANCH_start:
	OPCODE_USE(JIT_BRANCH);
	{
		uint ofs = GET_OP();
		pCurOp = pOps + ofs;
	}
JIT_BRANCH_end:
	GO_NEXT_CHECK();

JIT_SWITCH_start:
	OPCODE_USE(JIT_SWITCH);
	{
		uint ofs;
		// The number of jump targets
		uint numTargets = GET_OP();
		// The jump target selected
		uint target = POP_U32();
		if (target >= numTargets) {
			// This is not a valid jump target, so fall-through
			pCurOp += numTargets;
			goto JIT_SWITCH_end;
		}
		ofs = *(pCurOp + target);
		pCurOp = pOps + ofs;
	}
JIT_SWITCH_end:
	GO_NEXT_CHECK();

JIT_BRANCH_TRUE_U32_start:
	OPCODE_USE(JIT_BRANCH_TRUE);
	{
		uint value = POP_U32();
		uint ofs = GET_OP();
		if (value != 0) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BRANCH_TRUE_U32_end:
	GO_NEXT_CHECK();

JIT_BRANCH_TRUE_U64_start:
    OPCODE_USE(JIT_BRANCH_TRUE);
    {
        ulong value = POP_U64();
        uint ofs = GET_OP();
        if (value != 0) {
            pCurOp = pOps + ofs;
        }
    }
JIT_BRANCH_TRUE_U64_end:
    GO_NEXT_CHECK();
    
JIT_BRANCH_FALSE_U32_start:
	OPCODE_USE(JIT_BRANCH_FALSE);
	{
		uint value = POP_U32();
		uint ofs = GET_OP();
		if (value == 0) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BRANCH_FALSE_U32_end:
	GO_NEXT_CHECK();

JIT_BRANCH_FALSE_U64_start:
    OPCODE_USE(JIT_BRANCH_FALSE);
    {
        ulong value = POP_U64();
        uint ofs = GET_OP();
        if (value == 0) {
            pCurOp = pOps + ofs;
        }
    }
JIT_BRANCH_FALSE_U64_end:
    GO_NEXT_CHECK();
    
JIT_BEQ_I32I32_start:
	OPCODE_USE(JIT_BEQ_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if ((int)v1 == (int)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BEQ_I32I32_end:
	GO_NEXT_CHECK();

JIT_BEQ_I64I64_start:
	OPCODE_USE(JIT_BEQ_I64I64);
	{
		ulong v1, v2;
		uint ofs;
		POP_U64_U64(v1, v2);
		ofs = GET_OP();
		if ((I64)v1 == (I64)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BEQ_I64I64_end:
	GO_NEXT_CHECK();

JIT_BEQ_F32F32_start:
	OPCODE_USE(JIT_BEQ_F32F32);
	{
		float v1, v2;
		uint ofs;
		POP_F32_F32(v1, v2);
		ofs = GET_OP();
		if (v1 == v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BEQ_F32F32_end:
	GO_NEXT_CHECK();

JIT_BEQ_F64F64_start:
	OPCODE_USE(JIT_BEQ_F64F64);
	{
		double v1, v2;
		uint ofs;
		POP_F64_F64(v1, v2);
		ofs = GET_OP();
		if (v1 == v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BEQ_F64F64_end:
	GO_NEXT_CHECK();

JIT_BGE_I32I32_start:
	OPCODE_USE(JIT_BGE_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if ((int)v1 >= (int)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGE_I32I32_end:
	GO_NEXT_CHECK();

JIT_BGE_I64I64_start:
	OPCODE_USE(JIT_BGE_I64I64);
	{
		ulong v1, v2;
		uint ofs;
		POP_U64_U64(v1, v2);
		ofs = GET_OP();
		if ((I64)v1 >= (I64)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGE_I64I64_end:
	GO_NEXT_CHECK();

JIT_BGE_F32F32_start:
JIT_BGE_UN_F32F32_start:
	OPCODE_USE(JIT_BGE_F32F32);
	{
		float v1, v2;
		uint ofs;
		POP_F32_F32(v1, v2);
		ofs = GET_OP();
		if (v1 >= v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGE_F32F32_end:
JIT_BGE_UN_F32F32_end:
	GO_NEXT_CHECK();

JIT_BGE_F64F64_start:
JIT_BGE_UN_F64F64_start:
	OPCODE_USE(JIT_BGE_F64F64);
	{
		double v1, v2;
		uint ofs;
		POP_F64_F64(v1, v2);
		ofs = GET_OP();
		if (v1 >= v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGE_F64F64_end:
JIT_BGE_UN_F64F64_end:
	GO_NEXT_CHECK();

JIT_BGT_I32I32_start:
	OPCODE_USE(JIT_BGT_I32I32);
	{
		uint v1, v2;
		uint ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if ((int)v1 > (int)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGT_I32I32_end:
	GO_NEXT_CHECK();

JIT_BGT_I64I64_start:
	OPCODE_USE(JIT_BGT_I64I64);
	{
		ulong v1, v2;
		uint ofs;
		POP_U64_U64(v1, v2);
		ofs = GET_OP();
		if ((I64)v1 > (I64)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGT_I64I64_end:
	GO_NEXT_CHECK();

JIT_BGT_F32F32_start:
JIT_BGT_UN_F32F32_start:
	OPCODE_USE(JIT_BGT_F32F32);
	{
		float v1, v2;
		uint ofs;
		POP_F32_F32(v1, v2);
		ofs = GET_OP();
		if (v1 > v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGT_F32F32_end:
JIT_BGT_UN_F32F32_end:
	GO_NEXT_CHECK();

JIT_BGT_F64F64_start:
JIT_BGT_UN_F64F64_start:
	OPCODE_USE(JIT_BGT_F64F64);
	{
		double v1, v2;
		uint ofs;
		POP_F64_F64(v1, v2);
		ofs = GET_OP();
		if (v1 > v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGT_F64F64_end:
JIT_BGT_UN_F64F64_end:
	GO_NEXT_CHECK();

JIT_BLE_I32I32_start:
	OPCODE_USE(JIT_BLE_I32I32);
	{
		uint v1, v2;
		uint ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if ((int)v1 <= (int)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLE_I32I32_end:
	GO_NEXT_CHECK();

JIT_BLE_I64I64_start:
	OPCODE_USE(JIT_BLE_I64I64);
	{
		ulong v1, v2;
		uint ofs;
		POP_U64_U64(v1, v2);
		ofs = GET_OP();
		if ((I64)v1 <= (I64)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLE_I64I64_end:
	GO_NEXT_CHECK();

JIT_BLE_F32F32_start:
JIT_BLE_UN_F32F32_start:
	OPCODE_USE(JIT_BLE_F32F32);
	{
		float v1, v2;
		uint ofs;
		POP_F32_F32(v1, v2);
		ofs = GET_OP();
		if (v1 <= v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLE_F32F32_end:
JIT_BLE_UN_F32F32_end:
	GO_NEXT_CHECK();

JIT_BLE_F64F64_start:
JIT_BLE_UN_F64F64_start:
	OPCODE_USE(JIT_BLE_F64F64);
	{
		double v1, v2;
		uint ofs;
		POP_F64_F64(v1, v2);
		ofs = GET_OP();
		if (v1 <= v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLE_F64F64_end:
JIT_BLE_UN_F64F64_end:
	GO_NEXT_CHECK();

JIT_BLT_I32I32_start:
	OPCODE_USE(JIT_BLT_I32I32);
	{
		uint v1, v2;
		uint ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if ((int)v1 < (int)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLT_I32I32_end:
	GO_NEXT_CHECK();

JIT_BLT_I64I64_start:
	OPCODE_USE(JIT_BLT_I64I64);
	{
		ulong v1, v2;
		uint ofs;
		POP_U64_U64(v1, v2);
		ofs = GET_OP();
		if ((I64)v1 < (I64)v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLT_I64I64_end:
	GO_NEXT_CHECK();

JIT_BLT_F32F32_start:
JIT_BLT_UN_F32F32_start:
	OPCODE_USE(JIT_BLT_F32F32);
	{
		float v1, v2;
		uint ofs;
		POP_F32_F32(v1, v2);
		ofs = GET_OP();
		if (v1 < v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLT_F32F32_end:
JIT_BLT_UN_F32F32_end:
	GO_NEXT_CHECK();

JIT_BLT_F64F64_start:
JIT_BLT_UN_F64F64_start:
	OPCODE_USE(JIT_BLT_F64F64);
	{
		double v1, v2;
		uint ofs;
		POP_F64_F64(v1, v2);
		ofs = GET_OP();
		if (v1 < v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLT_F64F64_end:
JIT_BLT_UN_F64F64_end:
	GO_NEXT_CHECK();

JIT_BNE_UN_I32I32_start:
	OPCODE_USE(JIT_BNE_UN_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if (v1 != v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BNE_UN_I32I32_end:
	GO_NEXT_CHECK();

JIT_BNE_UN_I64I64_start:
	OPCODE_USE(JIT_BNE_UN_I64I64);
	{
		ulong v1, v2;
		uint ofs;
		POP_U64_U64(v1, v2);
		ofs = GET_OP();
		if (v1 != v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BNE_UN_I64I64_end:
	GO_NEXT_CHECK();

JIT_BNE_UN_F32F32_start:
	OPCODE_USE(JIT_BNE_UN_F32F32);
	{
		float v1, v2;
		uint ofs;
		POP_F32_F32(v1, v2);
		ofs = GET_OP();
		if (v1 != v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BNE_UN_F32F32_end:
	GO_NEXT_CHECK();

JIT_BNE_UN_F64F64_start:
	OPCODE_USE(JIT_BNE_UN_F64F64);
	{
		double v1, v2;
		uint ofs;
		POP_F64_F64(v1, v2);
		ofs = GET_OP();
		if (v1 != v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BNE_UN_F64F64_end:
	GO_NEXT_CHECK();

JIT_BGE_UN_I32I32_start:
	OPCODE_USE(JIT_BGE_UN_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if (v1 >= v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGE_UN_I32I32_end:
	GO_NEXT_CHECK();

JIT_BGT_UN_I32I32_start:
	OPCODE_USE(JIT_BGT_UN_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if (v1 > v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BGT_UN_I32I32_end:
	GO_NEXT_CHECK();

JIT_BLE_UN_I32I32_start:
	OPCODE_USE(JIT_BLE_UN_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if (v1 <= v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLE_UN_I32I32_end:
	GO_NEXT_CHECK();

JIT_BLT_UN_I32I32_start:
	OPCODE_USE(JIT_BLT_UN_I32I32);
	{
		uint v1, v2, ofs;
		POP_U32_U32(v1, v2);
		ofs = GET_OP();
		if (v1 < v2) {
			pCurOp = pOps + ofs;
		}
	}
JIT_BLT_UN_I32I32_end:
	GO_NEXT_CHECK();

JIT_CEQ_I32I32_start: // Handles int and O
	OPCODE_USE(JIT_CEQ_I32I32);
	BINARY_OP(uint, uint, uint, ==);
JIT_CEQ_I32I32_end:
	GO_NEXT();

JIT_CGT_I32I32_start:
	OPCODE_USE(JIT_CGT_I32I32);
	BINARY_OP(uint, int, int, >);
JIT_CGT_I32I32_end:
	GO_NEXT();

JIT_CGT_UN_I32I32_start: // Handles int and O
	OPCODE_USE(JIT_CGT_UN_I32I32);
	BINARY_OP(uint, uint, uint, >);
JIT_CGT_UN_I32I32_end:
	GO_NEXT();

JIT_CLT_I32I32_start:
	OPCODE_USE(JIT_CLT_I32I32);
	BINARY_OP(uint, int, int, <);
JIT_CLT_I32I32_end:
	GO_NEXT();

JIT_CLT_UN_I32I32_start:
	OPCODE_USE(JIT_CLT_UN_I32I32);
	BINARY_OP(uint, uint, uint, <);
JIT_CLT_UN_I32I32_end:
	GO_NEXT();

JIT_CEQ_I64I64_start:
	OPCODE_USE(JIT_CEQ_I64I64);
	BINARY_OP(uint, ulong, ulong, ==);
JIT_CEQ_I64I64_end:
	GO_NEXT();

JIT_CGT_I64I64_start:
	OPCODE_USE(JIT_CGT_I64I64);
	BINARY_OP(uint, I64, I64, >);
JIT_CGT_I64I64_end:
	GO_NEXT();

JIT_CGT_UN_I64I64_start:
	OPCODE_USE(JIT_CGT_UN_I64I64);
	BINARY_OP(uint, ulong, ulong, >);
JIT_CGT_UN_I64I64_end:
	GO_NEXT();

JIT_CLT_I64I64_start:
	OPCODE_USE(JIT_CLT_I64I64);
	BINARY_OP(uint, I64, I64, <);
JIT_CLT_I64I64_end:
	GO_NEXT();

JIT_CLT_UN_I64I64_start:
	OPCODE_USE(JIT_CLT_UN_I64I64);
	BINARY_OP(uint, ulong, ulong, <);
JIT_CLT_UN_I64I64_end:
	GO_NEXT();

JIT_CEQ_F32F32_start:
	OPCODE_USE(JIT_CEQ_F32F32);
	BINARY_OP(uint, float, float, ==);
JIT_CEQ_F32F32_end:
	GO_NEXT();

JIT_CEQ_F64F64_start:
	OPCODE_USE(JIT_CEQ_F64F64);
	BINARY_OP(uint, double, double, ==);
JIT_CEQ_F64F64_end:
	GO_NEXT();

JIT_CGT_F32F32_start:
	OPCODE_USE(JIT_CGT_F32F32);
	BINARY_OP(uint, float, float, >);
JIT_CGT_F32F32_end:
	GO_NEXT();

JIT_CGT_F64F64_start:
	OPCODE_USE(JIT_CGT_F64F64);
	BINARY_OP(uint, double, double, >);
JIT_CGT_F64F64_end:
	GO_NEXT();

JIT_CLT_F32F32_start:
	OPCODE_USE(JIT_CLT_F32F32);
	BINARY_OP(uint, float, float, <);
JIT_CLT_F32F32_end:
	GO_NEXT();

JIT_CLT_F64F64_start:
	OPCODE_USE(JIT_CLT_F64F64);
	BINARY_OP(uint, double, double, <);
JIT_CLT_F64F64_end:
	GO_NEXT();

JIT_ADD_OVF_I32I32_start:
	OPCODE_USE(JIT_ADD_OVF_I32I32);
	{
		uint v1, v2;
		I64 res;
		POP_U32_U32(v1, v2);
		res = (I64)(int)v1 + (I64)(int)v2;
		if (res > (I64)0x7fffffff || res < (I64)0xffffffff80000000) {
			// Overflowed, so throw exception
			THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
		}
		PUSH_U32((int)res);
	}
JIT_ADD_OVF_I32I32_end:
	GO_NEXT();

JIT_ADD_OVF_UN_I32I32_start:
	OPCODE_USE(JIT_ADD_OVF_UN_I32I32);
	{
		uint v1, v2;
		ulong res;
		POP_U32_U32(v1, v2);
		res = (ulong)v1 + (ulong)v2;
		if (res > (ulong)0xffffffff) {
			// Overflowed, so throw exception
			THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
		}
		PUSH_U32(res);
	}
JIT_ADD_OVF_UN_I32I32_end:
	GO_NEXT();

JIT_MUL_OVF_I32I32_start:
	OPCODE_USE(JIT_MUL_OVF_I32I32);
	{
		uint v1, v2;
		I64 res;
		POP_U32_U32(v1, v2);
		res = (I64)(int)v1 * (I64)(int)v2;
		if (res > (I64)0x7fffffff || res < (I64)0xffffffff80000000) {
			// Overflowed, so throw exception
			THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
		}
		PUSH_U32((int)res);
	}
JIT_MUL_OVF_I32I32_end:
	GO_NEXT();

JIT_MUL_OVF_UN_I32I32_start:
	OPCODE_USE(JIT_MUL_OVF_UN_I32I32);
	{
		uint v1, v2;
		ulong res;
		POP_U32_U32(v1, v2);
		res = (ulong)v1 * (ulong)v2;
		if (res > (ulong)0xffffffff) {
			// Overflowed, so throw exception
			THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
		}
		PUSH_U32(res);
	}
JIT_MUL_OVF_UN_I32I32_end:
	GO_NEXT();

JIT_SUB_OVF_I32I32_start:
	OPCODE_USE(JIT_SUB_OVF_I32I32);
	{
		uint v1, v2;
		I64 res;
		POP_U32_U32(v1, v2);
		res = (I64)(int)v1 - (I64)(int)v2;
		if (res > (I64)0x7fffffff || res < (I64)0xffffffff80000000) {
			// Overflowed, so throw exception
			THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
		}
		PUSH_U32((int)res);
	}
JIT_SUB_OVF_I32I32_end:
	GO_NEXT();

JIT_SUB_OVF_UN_I32I32_start:
	OPCODE_USE(JIT_SUB_OVF_UN_I32I32);
	{
		uint v1, v2;
		ulong res;
		POP_U32_U32(v1, v2);
		res = (ulong)v1 - (ulong)v2;
		if (res > (ulong)0xffffffff) {
			// Overflowed, so throw exception
			THROW(Type.types[Type.TYPE_SYSTEM_OVERFLOWEXCEPTION]);
		}
		PUSH_U32(res);
	}
JIT_SUB_OVF_UN_I32I32_end:
	GO_NEXT();

JIT_ADD_I32I32_start:
	OPCODE_USE(JIT_ADD_I32I32);
	BINARY_OP(int, int, int, +);
JIT_ADD_I32I32_end:
	GO_NEXT();

JIT_ADD_I64I64_start:
	OPCODE_USE(JIT_ADD_I64I64);
	BINARY_OP(I64, I64, I64, +);
JIT_ADD_I64I64_end:
	GO_NEXT();

JIT_ADD_F32F32_start:
	OPCODE_USE(JIT_ADD_F32F32);
	BINARY_OP(float, float, float, +);
JIT_ADD_F32F32_end:
	GO_NEXT();

JIT_ADD_F64F64_start:
	OPCODE_USE(JIT_ADD_F64F64);
	BINARY_OP(double, double, double, +);
JIT_ADD_F64F64_end:
	GO_NEXT();

JIT_SUB_I32I32_start:
	OPCODE_USE(JIT_SUB_I32I32);
	BINARY_OP(int, int, int, -);
JIT_SUB_I32I32_end:
	GO_NEXT();

JIT_SUB_I64I64_start:
	OPCODE_USE(JIT_SUB_I64I64);
	BINARY_OP(I64, I64, I64, -);
JIT_SUB_I64I64_end:
	GO_NEXT();

JIT_SUB_F32F32_start:
	OPCODE_USE(JIT_SUB_F32F32);
	BINARY_OP(double, double, double, -);
JIT_SUB_F32F32_end:
	GO_NEXT();

JIT_SUB_F64F64_start:
	OPCODE_USE(JIT_SUB_F64F64);
	BINARY_OP(double, double, double, -);
JIT_SUB_F64F64_end:
	GO_NEXT();

JIT_MUL_I32I32_start:
	OPCODE_USE(JIT_MUL_I32I32);
	BINARY_OP(int, int, int, *);
JIT_MUL_I32I32_end:
	GO_NEXT();

JIT_MUL_I64I64_start:
	OPCODE_USE(JIT_MUL_I64I64);
	BINARY_OP(I64, I64, I64, *);
JIT_MUL_I64I64_end:
	GO_NEXT();

JIT_MUL_F32F32_start:
	OPCODE_USE(JIT_MUL_F32F32);
	BINARY_OP(float, float, float, *);
JIT_MUL_F32F32_end:
	GO_NEXT();

JIT_MUL_F64F64_start:
	OPCODE_USE(JIT_MUL_F64F64);
	BINARY_OP(double, double, double, *);
JIT_MUL_F64F64_end:
	GO_NEXT();

JIT_DIV_I32I32_start:
	OPCODE_USE(JIT_DIV_I32I32);
	BINARY_OP(int, int, int, /);
JIT_DIV_I32I32_end:
	GO_NEXT();

JIT_DIV_I64I64_start:
	OPCODE_USE(JIT_DIV_I64I64);
	BINARY_OP(I64, I64, I64, /);
JIT_DIV_I64I64_end:
	GO_NEXT();

JIT_DIV_F32F32_start:
	OPCODE_USE(JIT_DIV_F32F32);
	BINARY_OP(float, float, float, /);
JIT_DIV_F32F32_end:
	GO_NEXT();

JIT_DIV_F64F64_start:
	OPCODE_USE(JIT_DIV_F64F64);
	BINARY_OP(double, double, double, /);
JIT_DIV_F64F64_end:
	GO_NEXT();

JIT_DIV_UN_I32I32_start:
	OPCODE_USE(JIT_DIV_UN_I32I32);
	BINARY_OP(uint, uint, uint, /);
JIT_DIV_UN_I32I32_end:
	GO_NEXT();

JIT_DIV_UN_I64I64_start:
	OPCODE_USE(JIT_DIV_UN_I64I64);
	BINARY_OP(ulong, ulong, ulong, /);
JIT_DIV_UN_I64I64_end:
	GO_NEXT();

JIT_REM_I32I32_start:
	OPCODE_USE(JIT_REM_I32I32);
	BINARY_OP(int, int, int, %);
JIT_REM_I32I32_end:
	GO_NEXT();

JIT_REM_I64I64_start:
	OPCODE_USE(JIT_REM_I64I64);
	BINARY_OP(I64, I64, I64, %);
JIT_REM_I64I64_end:
	GO_NEXT();

JIT_REM_UN_I32I32_start:
	OPCODE_USE(JIT_REM_UN_I32I32);
	BINARY_OP(uint, uint, uint, %);
JIT_REM_UN_I32I32_end:
	GO_NEXT();

JIT_REM_UN_I64I64_start:
	OPCODE_USE(JIT_REM_UN_I64I64);
	BINARY_OP(ulong, ulong, ulong, %);
JIT_REM_UN_I64I64_end:
	GO_NEXT();

JIT_AND_I32I32_start:
	OPCODE_USE(JIT_AND_I32I32);
	BINARY_OP(uint, uint, uint, &);
JIT_AND_I32I32_end:
	GO_NEXT();

JIT_AND_I64I64_start:
	OPCODE_USE(JIT_AND_I64I64);
	BINARY_OP(ulong, ulong, ulong, &);
JIT_AND_I64I64_end:
	GO_NEXT();

JIT_OR_I32I32_start:
	OPCODE_USE(JIT_OR_I32I32);
	BINARY_OP(uint, uint, uint, |);
JIT_OR_I32I32_end:
	GO_NEXT();

JIT_OR_I64I64_start:
	OPCODE_USE(JIT_OR_I64I64);
	BINARY_OP(ulong, ulong, ulong, |);
JIT_OR_I64I64_end:
	GO_NEXT();

JIT_XOR_I32I32_start:
	OPCODE_USE(JIT_XOR_I32I32);
	BINARY_OP(uint, uint, uint, ^);
JIT_XOR_I32I32_end:
	GO_NEXT();

JIT_XOR_I64I64_start:
	OPCODE_USE(JIT_XOR_I64I64);
	BINARY_OP(ulong, ulong, ulong, ^);
JIT_XOR_I64I64_end:
	GO_NEXT();

JIT_NEG_I32_start:
	OPCODE_USE(JIT_NEG_I32);
	UNARY_OP(int, -);
JIT_NEG_I32_end:
	GO_NEXT();

JIT_NEG_I64_start:
	OPCODE_USE(JIT_NEG_I64);
	UNARY_OP(I64, -);
JIT_NEG_I64_end:
	GO_NEXT();

JIT_NOT_I32_start:
	OPCODE_USE(JIT_NOT_I32);
	UNARY_OP(uint, ~);
JIT_NOT_I32_end:
	GO_NEXT();

JIT_NOT_I64_start:
	OPCODE_USE(JIT_NOT_I64);
	UNARY_OP(ulong, ~);
JIT_NOT_I64_end:
	GO_NEXT();

JIT_SHL_I32_start:
	OPCODE_USE(JIT_SHL_I32);
	BINARY_OP(uint, uint, uint, <<);
JIT_SHL_I32_end:
	GO_NEXT();

JIT_SHR_I32_start:
	OPCODE_USE(JIT_SHR_I32);
	BINARY_OP(int, int, uint, >>);
JIT_SHR_I32_end:
	GO_NEXT();

JIT_SHR_UN_I32_start:
	OPCODE_USE(JIT_SHR_UN_I32);
	BINARY_OP(uint, uint, uint, >>);
JIT_SHR_UN_I32_end:
	GO_NEXT();

JIT_SHL_I64_start:
	OPCODE_USE(JIT_SHL_I64);
	BINARY_OP(ulong, ulong, uint, <<);
JIT_SHL_I64_end:
	GO_NEXT();

JIT_SHR_I64_start:
	OPCODE_USE(JIT_SHR_I64);
	BINARY_OP(I64, I64, uint, >>);
JIT_SHR_I64_end:
	GO_NEXT();

JIT_SHR_UN_I64_start:
	OPCODE_USE(JIT_SHR_UN_I64);
	BINARY_OP(ulong, ulong, uint, >>);
JIT_SHR_UN_I64_end:
	GO_NEXT();

	// Conversion operations

JIT_CONV_U32_U32_start:
JIT_CONV_I32_U32_start:
	OPCODE_USE(JIT_CONV_I32_U32);
	{
		uint mask = GET_OP();
		STACK_ADDR(uint) &= mask;
	}
JIT_CONV_U32_U32_end:
JIT_CONV_I32_U32_end:
	GO_NEXT();

JIT_CONV_U32_I32_start:
JIT_CONV_I32_I32_start:
	OPCODE_USE(JIT_CONV_I32_I32);
	{
		uint shift = GET_OP();
		STACK_ADDR(int) = (STACK_ADDR(int) << shift) >> shift;
	}
JIT_CONV_U32_I32_end:
JIT_CONV_I32_I32_end:
	GO_NEXT();

JIT_CONV_I32_I64_start:
	OPCODE_USE(JIT_CONV_I32_I64);
	{
		int value = (int)POP_U32();
		PUSH_U64((I64)value);
	}
JIT_CONV_I32_I64_end:
	GO_NEXT();

JIT_CONV_I32_U64_start:
JIT_CONV_U32_U64_start:
JIT_CONV_U32_I64_start:
	OPCODE_USE(JIT_CONV_U32_I64);
	{
		uint value = POP_U32();
		PUSH_U64(value);
	}
JIT_CONV_I32_U64_end:
JIT_CONV_U32_U64_end:
JIT_CONV_U32_I64_end:
	GO_NEXT();

JIT_CONV_I32_R32_start:
	OPCODE_USE(JIT_CONV_I32_R32);
	{
		int value = (int)POP_U32();
		PUSH_FLOAT(value);
	}
JIT_CONV_I32_R32_end:
	GO_NEXT();

JIT_CONV_I32_R64_start:
	OPCODE_USE(JIT_CONV_I32_R64);
	{
		int value = (int)POP_U32();
		PUSH_DOUBLE(value);
	}
JIT_CONV_I32_R64_end:
	GO_NEXT();

JIT_CONV_U32_R32_start:
	OPCODE_USE(JIT_CONV_U32_R32);
	{
		uint value = POP_U32();
		PUSH_FLOAT(value);
	}
JIT_CONV_U32_R32_end:
	GO_NEXT();

JIT_CONV_U32_R64_start:
	OPCODE_USE(JIT_CONV_U32_R64);
	{
		uint value = POP_U32();
		PUSH_DOUBLE(value);
	}
JIT_CONV_U32_R64_end:
	GO_NEXT();

JIT_CONV_I64_U32_start:
JIT_CONV_U64_U32_start:
	OPCODE_USE(JIT_CONV_I64_U32);
	{
		uint mask = GET_OP();
		ulong value = POP_U64();
		PUSH_U32(value & mask);
	}
JIT_CONV_I64_U32_end:
JIT_CONV_U64_U32_end:
	GO_NEXT();

JIT_CONV_I64_I32_start:
JIT_CONV_U64_I32_start:
	OPCODE_USE(JIT_CONV_I64_U32);
	{
		uint shift = GET_OP();
		int value = (int)POP_U64();
		value = (value << shift) >> shift;
		PUSH_U32(value);
	}
JIT_CONV_I64_I32_end:
JIT_CONV_U64_I32_end:
	GO_NEXT();

JIT_CONV_I64_R32_start:
	OPCODE_USE(JIT_CONV_I64_R32);
	{
		I64 value = (I64)POP_U64();
		PUSH_FLOAT(value);
	}
JIT_CONV_I64_R32_end:
	GO_NEXT();

JIT_CONV_I64_R64_start:
	OPCODE_USE(JIT_CONV_I64_R64);
	{
		I64 value = (I64)POP_U64();
		PUSH_DOUBLE(value);
	}
JIT_CONV_I64_R64_end:
	GO_NEXT();

JIT_CONV_U64_R32_start:
	OPCODE_USE(JIT_CONV_U64_R32);
	{
		ulong value = POP_U64();
		PUSH_FLOAT(value);
	}
JIT_CONV_U64_R32_end:
	GO_NEXT();

JIT_CONV_U64_R64_start:
	OPCODE_USE(JIT_CONV_U64_R64);
	{
		ulong value = POP_U64();
		PUSH_DOUBLE(value);
	}
JIT_CONV_U64_R64_end:
	GO_NEXT();

JIT_CONV_R32_I32_start:
	OPCODE_USE(JIT_CONV_R32_I32);
	{
		uint shift = GET_OP();
		int result;
		float value = POP_FLOAT();
		result = (int)value;
		result = (result << shift) >> shift;
		PUSH_U32(result);
	}
JIT_CONV_R32_I32_end:
	GO_NEXT();

JIT_CONV_R32_U32_start:
	OPCODE_USE(JIT_CONV_R32_U32);
	{
		uint mask = GET_OP();
		float value = POP_FLOAT();
		PUSH_U32(((uint)value) & mask);
	}
JIT_CONV_R32_U32_end:
	GO_NEXT();

JIT_CONV_R32_I64_start:
	OPCODE_USE(JIT_CONV_R32_I64);
	{
		float value = POP_FLOAT();
		PUSH_U64((I64)value);
	}
JIT_CONV_R32_I64_end:
	GO_NEXT();

JIT_CONV_R32_U64_start:
	OPCODE_USE(JIT_CONV_R32_U64);
	{
		float value = POP_FLOAT();
		PUSH_U64(value);
	}
JIT_CONV_R32_U64_end:
	GO_NEXT();

JIT_CONV_R32_R64_start:
	OPCODE_USE(JIT_CONV_R32_R64);
	{
		float value = POP_FLOAT();
		PUSH_DOUBLE(value);
	}
JIT_CONV_R32_R64_end:
	GO_NEXT();

JIT_CONV_R64_I32_start:
	OPCODE_USE(JIT_CONV_R64_I32);
	{
		uint shift = GET_OP();
		int result;
		double value = POP_DOUBLE();
		result = (int)value;
		result = (result << shift) >> shift;
		PUSH_U32(result);
	}
JIT_CONV_R64_I32_end:
	GO_NEXT();

JIT_CONV_R64_U32_start:
	OPCODE_USE(JIT_CONV_R64_U32);
	{
		uint mask = GET_OP();
		double value = POP_DOUBLE();
		PUSH_U32(((uint)value) & mask);
	}
JIT_CONV_R64_U32_end:
	GO_NEXT();

JIT_CONV_R64_I64_start:
	OPCODE_USE(JIT_CONV_R64_I64);
	{
		float value = POP_FLOAT();
		PUSH_U64((I64)value);
	}
JIT_CONV_R64_I64_end:
	GO_NEXT();

JIT_CONV_R64_U64_start:
	OPCODE_USE(JIT_CONV_R64_U64);
	{
		double value = POP_DOUBLE();
		PUSH_U64(value);
	}
JIT_CONV_R64_U64_end:
	GO_NEXT();

JIT_CONV_R64_R32_start:
	OPCODE_USE(JIT_CONV_R64_R32);
	{
		float value = (float)POP_DOUBLE();
		PUSH_FLOAT(value);
	}
JIT_CONV_R64_R32_end:
	GO_NEXT();

JIT_LOADFUNCTION_start:
	OPCODE_USE(JIT_LOADFUNCTION);
	{
		// This is actually a pointer not a uint
		uint value = GET_OP();
		PUSH_U32(value);
	}
JIT_LOADFUNCTION_end:
	GO_NEXT();

JIT_LOADOBJECT_start:
	OPCODE_USE(JIT_LOADOBJECT);
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
JIT_LOADOBJECT_end:
	GO_NEXT();

JIT_LOAD_STRING_start:
	OPCODE_USE(JIT_LOAD_STRING);
	{
		uint value = GET_OP();
		byte* heapPtr = SystemString.FromUserStrings(pCurrentMethodState->pMetaData, value);
		PUSH_O(heapPtr);
	}
JIT_LOAD_STRING_end:
	GO_NEXT();

JIT_NEWOBJECT_start:
	OPCODE_USE(JIT_NEWOBJECT);
	{
		tMD_MethodDef *pConstructorDef;
		/*HEAP_PTR*/byte* obj;
		tMethodState *pCallMethodState;
		uint isInternalConstructor;
		byte* pTempPtr;

		pConstructorDef = (tMD_MethodDef*)GET_PTR();
		isInternalConstructor = (pConstructorDef->implFlags & METHODIMPLATTRIBUTES_INTERNALCALL) != 0;

		if (!isInternalConstructor) {
			// All internal constructors MUST allocate their own 'this' objects
			obj = Heap.AllocType(pConstructorDef->pParentType);
		} else {
			// Need to set this to something non-null so that CreateParameters() works properly
			obj = (/*HEAP_PTR*/byte*)-1;
		}

		// Set up the new method state for the called method
		pCallMethodState = MethodState_Direct(pThread, pConstructorDef, pCurrentMethodState, isInternalConstructor);
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
JIT_NEWOBJECT_end:
	GO_NEXT_CHECK();

JIT_NEWOBJECT_VALUETYPE_start:
	OPCODE_USE(JIT_NEWOBJECT_VALUETYPE);
	{
		tMD_MethodDef *pConstructorDef;
		tMethodState *pCallMethodState;
		uint isInternalConstructor;
		byte* pTempPtr, pMem;

		pConstructorDef = (tMD_MethodDef*)GET_PTR();
		isInternalConstructor = (pConstructorDef->implFlags & METHODIMPLATTRIBUTES_INTERNALCALL) != 0;

		// Allocate space on the eval-stack for the new value-type here
		pMem = pCurEvalStack - (pConstructorDef->parameterStackSize - sizeof(byte*));

		// Set up the new method state for the called method
		pCallMethodState = MethodState_Direct(pThread, pConstructorDef, pCurrentMethodState, isInternalConstructor);
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
JIT_NEWOBJECT_VALUETYPE_end:
	GO_NEXT_CHECK();

JIT_IS_INSTANCE_start:
	op = JIT_IS_INSTANCE;
	goto jitCastClass;
JIT_CAST_CLASS_start:
	op = JIT_CAST_CLASS;
jitCastClass:
	OPCODE_USE(JIT_CAST_CLASS);
	{
		tMD_TypeDef *pToType, *pTestType;
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
			if (Type.IsAssignableFrom(pToType->pArrayElementType, pTestType->pArrayElementType)) {
				PUSH_O(heapPtr);
				goto JIT_IS_INSTANCE_end;
			}
		} else {
			if (Type.IsAssignableFrom(pToType, pTestType) ||
				(pToType->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE] &&
				pToType->ppClassTypeArgs[0] == pTestType)) {
				// If derived class, interface, or nullable type compatible.
				PUSH_O(heapPtr);
				goto JIT_IS_INSTANCE_end;
			}
		}
		if (op == JIT_IS_INSTANCE) {
			PUSH_O(null);
		} else {
			THROW(Type.types[Type.TYPE_SYSTEM_INVALIDCASTEXCEPTION]);
		}
	}
JIT_IS_INSTANCE_end:
JIT_CAST_CLASS_end:
	GO_NEXT();

JIT_NEW_VECTOR_start: // Array with 1 dimension, zero-based
	OPCODE_USE(JIT_NEW_VECTOR);
	{
		tMD_TypeDef *pArrayTypeDef;
		uint numElements;
		/*HEAP_PTR*/byte* heapPtr;

		pArrayTypeDef = (tMD_TypeDef*)GET_PTR();
		numElements = POP_U32();
		heapPtr = SystemArray.NewVector(pArrayTypeDef, numElements);
		PUSH_O(heapPtr);
		// Run any pending Finalizers
		RUN_FINALIZER();
	}
JIT_NEW_VECTOR_end:
	GO_NEXT();

JIT_LOAD_VECTOR_LEN_start: // Load the length of a vector array
	OPCODE_USE(JIT_LOAD_VECTOR_LEN);
	{
		byte* heapPtr = POP_O();
		uint value = SystemArray.GetLength(heapPtr);
		PUSH_U32(value);
	}
JIT_LOAD_VECTOR_LEN_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_I8_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_I8);
	{
		uint value, idx = POP_U32(); // Array index
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		SystemArray.LoadElement(heapPtr, idx, (byte*)&value);
		PUSH_U32((sbyte)value);
	}
JIT_LOAD_ELEMENT_I8_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_U8_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_U8);
	{
		uint value, idx = POP_U32(); // Array index
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		SystemArray.LoadElement(heapPtr, idx, (byte*)&value);
		PUSH_U32((byte)value);
	}
JIT_LOAD_ELEMENT_U8_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_I16_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_I16);
	{
		uint value, idx = POP_U32(); // Array index
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		SystemArray.LoadElement(heapPtr, idx, (byte*)&value);
		PUSH_U32((short)value);
	}
JIT_LOAD_ELEMENT_I16_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_U16_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_U16);
	{
		uint value, idx = POP_U32(); // Array index
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		SystemArray.LoadElement(heapPtr, idx, (byte*)&value);
		PUSH_U32((ushort)value);
	}
JIT_LOAD_ELEMENT_U16_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_I32_start:
JIT_LOAD_ELEMENT_U32_start:
JIT_LOAD_ELEMENT_R32_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_I32);
	{
		uint value, idx = POP_U32(); // Array index
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		SystemArray.LoadElement(heapPtr, idx, (byte*)&value);
		PUSH_U32(value);
	}
JIT_LOAD_ELEMENT_I32_end:
JIT_LOAD_ELEMENT_U32_end:
JIT_LOAD_ELEMENT_R32_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_I64_start:
JIT_LOAD_ELEMENT_R64_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_I64);
	{
		uint idx = POP_U32(); // array index
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		ulong value;
		SystemArray.LoadElement(heapPtr, idx, (byte*)&value);
		PUSH_U64(value);
	}
JIT_LOAD_ELEMENT_I64_end:
JIT_LOAD_ELEMENT_R64_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_start:
	OPCODE_USE(JIT_LOAD_ELEMENT);
	{
		uint idx = POP_U32(); // Array index
		/*HEAP_PTR*/byte* heapPtr = POP_O(); // array object
		uint size = GET_OP(); // size of type on stack
		*(uint*)pCurEvalStack = 0; // This is required to zero out the stack for type that are stored in <4 bytes in arrays
		SystemArray.LoadElement(heapPtr, idx, pCurEvalStack);
		pCurEvalStack += size;
	}
JIT_LOAD_ELEMENT_end:
	GO_NEXT();

JIT_LOAD_ELEMENT_ADDR_start:
	OPCODE_USE(JIT_LOAD_ELEMENT_ADDR);
	{
		uint idx = POP_U32(); // Array index
		byte* heapPtr = POP_O();
		byte* pMem = SystemArray.LoadElementAddress(heapPtr, idx);
		PUSH_PTR(pMem);
	}
JIT_LOAD_ELEMENT_ADDR_end:
	GO_NEXT();

JIT_STORE_ELEMENT_32_start:
	OPCODE_USE(JIT_STORE_ELEMENT_32);
	{
		uint value = POP_U32(); // Value
		uint idx = POP_U32(); // Array index
		byte* heapPtr = POP_O();
		SystemArray.StoreElement(heapPtr, idx, (byte*)&value);
	}
JIT_STORE_ELEMENT_32_end:
	GO_NEXT();

JIT_STORE_ELEMENT_64_start:
	OPCODE_USE(JIT_STORE_ELEMENT_64);
	{
		ulong value = POP_U64(); // Value
		uint idx = POP_U32(); // Array index
		byte* heapPtr = POP_O();
#if TRACE
        printf("  val 0x%llx idx %d ptr 0x%llx\n", value, idx, (ulong)heapPtr);
#endif
		SystemArray.StoreElement(heapPtr, idx, (byte*)&value);
	}
JIT_STORE_ELEMENT_64_end:
	GO_NEXT();

JIT_STORE_ELEMENT_start:
	OPCODE_USE(JIT_STORE_ELEMENT);
	{
		/*HEAP_PTR*/byte* heapPtr;
		byte* pMem;
		uint idx, size = GET_OP(); // Size in bytes of value on stack
		POP(size);
		pMem = pCurEvalStack;
		idx = POP_U32(); // Array index
		heapPtr = POP_O(); // Array on heap
		SystemArray.StoreElement(heapPtr, idx, pMem);
	}
JIT_STORE_ELEMENT_end:
	GO_NEXT();

JIT_STOREFIELD_INT32_start:
JIT_STOREFIELD_INTNATIVE_start: // only for 32-bit
JIT_STOREFIELD_F32_start:
	OPCODE_USE(JIT_STOREFIELD_INT32);
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
        printf("  val 0x%x off %d ptr 0x%llx\n", value, pFieldDef->memOffset, (ulong)heapPtr);
#endif
		*(uint*)pMem = value;
	}
JIT_STOREFIELD_INT32_end:
JIT_STOREFIELD_INTNATIVE_end:
JIT_STOREFIELD_F32_end:
	GO_NEXT();

JIT_STOREFIELD_O_start:
JIT_STOREFIELD_PTR_start:
    OPCODE_USE(JIT_STOREFIELD_PTR);
    {
#if UNITY_WEBGL || DNA_32BIT
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
        printf("  val 0x%llx off %d ptr 0x%llx\n", value, pFieldDef->memOffset, (ulong)heapPtr);
#endif
        *(ulong*)pMem = value;
#endif
    }
JIT_STOREFIELD_O_end:
JIT_STOREFIELD_PTR_end:
    GO_NEXT();
    
JIT_STOREFIELD_INT64_start:
JIT_STOREFIELD_F64_start:
	OPCODE_USE(JIT_STOREFIELD_F64);
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
JIT_STOREFIELD_INT64_end:
JIT_STOREFIELD_F64_end:
	GO_NEXT();

JIT_STOREFIELD_VALUETYPE_start:
	OPCODE_USE(JIT_STOREFIELD_VALUETYPE);
	{
		tMD_FieldDef *pFieldDef;
		byte* pMem;

		pFieldDef = (tMD_FieldDef*)GET_PTR();
		pCurEvalStack -= pFieldDef->memSize;
		pMem = pCurEvalStack;
		heapPtr = POP_O();
		Mem.memcpy(heapPtr + pFieldDef->memOffset, pMem, pFieldDef->memSize);
	}
JIT_STOREFIELD_VALUETYPE_end:
	GO_NEXT();

JIT_LOADFIELD_start:
	OPCODE_USE(JIT_LOADFIELD);
	// TODO: Optimize into LOADFIELD of different type O, INT32, INT64, F, etc...)
	{
		tMD_FieldDef *pFieldDef;

		pFieldDef = (tMD_FieldDef*)GET_PTR();
		heapPtr = POP_O();
		pMem = heapPtr + pFieldDef->memOffset;
		// It may not be a value-type, but this'll work anyway
		PUSH_VALUETYPE(pMem, pFieldDef->memSize, pFieldDef->memSize);
	}
JIT_LOADFIELD_end:
	GO_NEXT();

JIT_LOADFIELD_4_start:
	OPCODE_USE(JIT_LOADFIELD_4);
	{
		uint ofs = GET_OP();
		byte* heapPtr = POP_O();
//        printf("  ofs %d ptr 0x%llx val 0x%x\n", ofs, (ulong)heapPtr, *(uint*)(heapPtr + ofs));
		PUSH_U32(*(uint*)(heapPtr + ofs));
	}
JIT_LOADFIELD_4_end:
	GO_NEXT();

JIT_LOADFIELD_8_start:
    OPCODE_USE(JIT_LOADFIELD_8);
    {
        uint ofs = GET_OP();
        byte* heapPtr = POP_O();
//        printf("  ofs %d ptr 0x%llx val 0x%llx\n", ofs, (ulong)heapPtr, *(ulong*)(heapPtr + ofs));
        PUSH_U64(*(ulong*)(heapPtr + ofs));
    }
JIT_LOADFIELD_8_end:
    GO_NEXT();
    
JIT_LOADFIELD_VALUETYPE_start:
	OPCODE_USE(JIT_LOADFIELD_VALUETYPE);
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
		pMem = pCurEvalStack + pFieldDef->memOffset;
		// It may not be a value-type, but this'll work anyway
		PUSH_VALUETYPE(pMem, pFieldDef->memSize, pFieldDef->memSize);
	}
JIT_LOADFIELD_VALUETYPE_end:
	GO_NEXT();

JIT_LOAD_FIELD_ADDR_start:
	OPCODE_USE(JIT_LOAD_FIELD_ADDR);
	{
		uint ofs = GET_OP();
		/*HEAP_PTR*/byte* heapPtr = POP_O();
		byte* pMem = heapPtr + ofs;
		PUSH_PTR(pMem);
	}
JIT_LOAD_FIELD_ADDR_end:
	GO_NEXT();

JIT_STORESTATICFIELD_INT32_start:
JIT_STORESTATICFIELD_F32_start:
JIT_STORESTATICFIELD_INTNATIVE_start: // only for 32-bit
	OPCODE_USE(JIT_STORESTATICFIELD_INT32);
	{
		tMD_FieldDef *pFieldDef;
		byte* pMem;
		uint value;

		pFieldDef = (tMD_FieldDef*)GET_PTR();
		value = POP_U32();
		pMem = pFieldDef->pMemory;
		*(uint*)pMem = value;
	}
JIT_STORESTATICFIELD_INT32_end:
JIT_STORESTATICFIELD_F32_end:
JIT_STORESTATICFIELD_INTNATIVE_end:
	GO_NEXT();

JIT_STORESTATICFIELD_O_start: // only for 32-bit
JIT_STORESTATICFIELD_PTR_start: // only for 32-bit
    OPCODE_USE(JIT_STORESTATICFIELD_INT32);
    {
#if UNITY_WEBGL || DNA_32BIT
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
JIT_STORESTATICFIELD_O_end:
JIT_STORESTATICFIELD_PTR_end:
    GO_NEXT();
    
JIT_STORESTATICFIELD_F64_start:
JIT_STORESTATICFIELD_INT64_start:
	OPCODE_USE(JIT_STORESTATICFIELD_INT64);
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
JIT_STORESTATICFIELD_F64_end:
JIT_STORESTATICFIELD_INT64_end:
	GO_NEXT();

JIT_STORESTATICFIELD_VALUETYPE_start:
	OPCODE_USE(JIT_STORESTATICFIELD_VALUETYPE);
	{
		tMD_FieldDef *pFieldDef;
		byte* pMem;

		pFieldDef = (tMD_FieldDef*)GET_PTR();
		pMem = pFieldDef->pMemory;
		POP_VALUETYPE(pMem, pFieldDef->memSize, pFieldDef->memSize);
	}
JIT_STORESTATICFIELD_VALUETYPE_end:
	GO_NEXT();

JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT_start:
	op = JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT;
	goto loadStaticFieldStart;
JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE_start:
	op = JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE;
	goto loadStaticFieldStart;
JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64_start:
	op = JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64;
	goto loadStaticFieldStart;
JIT_LOADSTATICFIELD_CHECKTYPEINIT_O_start: // Only for 32-bit
JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR_start: // Only for 32-bit
	op = JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR;
    goto loadStaticFieldStart;
JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32_start:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_F32_start:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_INTNATIVE_start: // Only for 32-bit
	op = 0;
loadStaticFieldStart:
	OPCODE_USE(JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32);
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
				pCallMethodState = MethodState_Direct(pThread, pParentType->pStaticConstructor, pCurrentMethodState, 0);
				// There can be no parameters, so don't need to set them up
				CHANGE_METHOD_STATE(pCallMethodState);
				GO_NEXT_CHECK();
			}
		}
		if (op == JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64) {
			ulong value;
			value = *(ulong*)(pFieldDef->pMemory);
			PUSH_U64(value);
        } else if (op == JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR ||
                   op == JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT) {
#if UNITY_WEBGL || DNA_32BIT
            uint value;
            if (op == JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT) {
                value = (uint)(pFieldDef->pMemory);
            } else {
                value = *(uint*)(pFieldDef->pMemory);
            }
            PUSH_U32(value);
#else
            ulong value;
            if (op == JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT) {
                value = (ulong)(pFieldDef->pMemory);
            } else {
                value = *(ulong*)(pFieldDef->pMemory);
            }
            PUSH_U64(value);
#endif
		} else if (op == JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE) {
			PUSH_VALUETYPE(pFieldDef->pMemory, pFieldDef->memSize, pFieldDef->memSize);
		} else {
			uint value;
			value = *(uint*)pFieldDef->pMemory;
			PUSH_U32(value);
		}
	}
JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_F32_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_O_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_INTNATIVE_end:
JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR_end:
	GO_NEXT();

JIT_INIT_VALUETYPE_start:
	OPCODE_USE(JIT_INIT_VALUETYPE);
	{
		tMD_TypeDef *pTypeDef;

		pTypeDef = (tMD_TypeDef*)GET_PTR();
		pMem = POP_PTR();
		Mem.memset(pMem, 0, pTypeDef->instanceMemSize);
	}
JIT_INIT_VALUETYPE_end:
	GO_NEXT();

JIT_INIT_OBJECT_start:
	OPCODE_USE(JIT_INIT_OBJECT);
	{
		byte* pMem = POP_PTR();
		*(void**)pMem = null;
	}
JIT_INIT_OBJECT_end:
	GO_NEXT();

JIT_BOX_INT32_start:
JIT_BOX_F32_start:
JIT_BOX_INTNATIVE_start:
	OPCODE_USE(JIT_BOX_INT32);
	{
		tMD_TypeDef *pTypeDef;

		pTypeDef = (tMD_TypeDef*)GET_PTR();
		heapPtr = Heap.AllocType(pTypeDef);
		u32Value = POP_U32();
		*(uint*)heapPtr = u32Value;
		PUSH_O(heapPtr);
	}
JIT_BOX_INT32_end:
JIT_BOX_F32_end:
JIT_BOX_INTNATIVE_end:
	GO_NEXT();

JIT_BOX_INT64_start:
JIT_BOX_F64_start:
OPCODE_USE(JIT_BOX_INT64);
	{
		tMD_TypeDef *pTypeDef = (tMD_TypeDef*)GET_PTR();
		heapPtr = Heap.AllocType(pTypeDef);
		*(ulong*)heapPtr = POP_U64();
		PUSH_O(heapPtr);
	}
JIT_BOX_INT64_end:
JIT_BOX_F64_end:
	GO_NEXT();

JIT_BOX_VALUETYPE_start:
	OPCODE_USE(JIT_BOX_VALUETYPE);
	{
		tMD_TypeDef *pTypeDef;

		pTypeDef = (tMD_TypeDef*)GET_PTR();
		heapPtr = Heap.AllocType(pTypeDef);
		POP_VALUETYPE(heapPtr, pTypeDef->stackSize, pTypeDef->stackSize);
		PUSH_O(heapPtr);
	}
JIT_BOX_VALUETYPE_end:
	GO_NEXT();

JIT_BOX_O_start:
	pCurOp++;
	// Fall-through
JIT_UNBOX2OBJECT_start: // TODO: This is not correct - it should check the type, just like CAST_CLASS
	OPCODE_USE(JIT_UNBOX2OBJECT);
	// Nothing to do
JIT_BOX_O_end:
JIT_UNBOX2OBJECT_end:
	GO_NEXT();

JIT_BOX_NULLABLE_start:
	OPCODE_USE(JIT_BOX_NULLABLE);
	{
		// Get the underlying type of the nullable type
		tMD_TypeDef *pType = (tMD_TypeDef*)GET_PTR();

		// Take the nullable type off the stack. The +4 is because the of the HasValue field (Bool, size = 4 bytes)
		pCurEvalStack -= pType->stackSize + 4;
		// If .HasValue
		if (*(uint*)pCurEvalStack) {
			// Box the underlying type
			/*HEAP_PTR*/byte* boxed;
			boxed = Heap_Box(pType, pCurEvalStack + 4);
			PUSH_O(boxed);
		} else {
			// Put a null pointer on the stack
			PUSH_O(null);
		}
	}
JIT_BOX_NULLABLE_end:
	GO_NEXT();

JIT_UNBOX2VALUETYPE_start:
	OPCODE_USE(JIT_UNBOX2VALUETYPE);
	{
		tMD_TypeDef *pTypeDef;
		/*HEAP_PTR*/byte* heapPtr;

		heapPtr = POP_O();
		pTypeDef = Heap.GetType(heapPtr);
		PUSH_VALUETYPE(heapPtr, pTypeDef->stackSize, pTypeDef->stackSize);
	}
JIT_UNBOX2VALUETYPE_end:
	GO_NEXT();

JIT_UNBOX_NULLABLE_start:
	OPCODE_USE(JIT_UNBOX_NULLABLE);
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
JIT_UNBOX_NULLABLE_end:
	GO_NEXT();

JIT_LOADTOKEN_TYPE_start:
	OPCODE_USE(JIT_LOADTOKEN_TYPE);
	{
		tMD_TypeDef *pTypeDef;

		pTypeDef = (tMD_TypeDef*)GET_PTR();
		// Push new valuetype onto evaluation stack
		PUSH_PTR((byte*)pTypeDef);
	}
JIT_LOADTOKEN_TYPE_end:
	GO_NEXT();

JIT_LOADTOKEN_FIELD_start:
	OPCODE_USE(JIT_LOADTOKEN_FIELD);
	{
		tMD_FieldDef *pFieldDef;

		pFieldDef = (tMD_FieldDef*)GET_PTR();
		// Push new valuetype onto evaluation stack - only works on static fields.
		PUSH_PTR(pFieldDef->pMemory);
	}
JIT_LOADTOKEN_FIELD_end:
	GO_NEXT();

JIT_RETHROW_start:
	op = JIT_RETHROW;
	goto throwStart;
JIT_THROW_start:
	op = JIT_THROW;
throwStart:
	OPCODE_USE(JIT_THROW);
	{
		uint i;
		tExceptionHeader *pCatch;
		tMethodState *pCatchMethodState;
		tMD_TypeDef *pExType;

		// Get the exception object
		if (op == JIT_RETHROW) {
			heapPtr = pThread->pCurrentExceptionObject;
		} else {
			heapPtr = POP_O();
throwHeapPtr:
			pThread->pCurrentExceptionObject = heapPtr;
		}
		SAVE_METHOD_STATE();
		pExType = Heap.GetType(heapPtr);
		// Find any catch exception clauses; look in the complete call stack
		pCatch = null;
		pCatchMethodState = pCurrentMethodState;
		for(;;) {
			for (i=0; i<pCatchMethodState->pMethod->pJITted->numExceptionHandlers; i++) {
				tJITExceptionHeader *pEx = &pCatchMethodState->pMethod->pJITted->pExceptionHeaders[i];
				if (pEx->flags == COR_ILEXCEPTION_CLAUSE_EXCEPTION &&
					pCatchMethodState->ipOffset - 1 >= pEx->tryStart &&
					pCatchMethodState->ipOffset - 1 < pEx->tryEnd &&
					Type_IsDerivedFromOrSame(pEx->pCatchTypeDef, pExType)) {
					
					// Found the correct catch clause to jump to
					pCatch = pEx;
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
					pCurrentMethodState->pMethod->pParentType->name,
					pCurrentMethodState->pMethod->name, pExType->nameSpace, pExType->name);
			}
		}
		// Unwind the stack down to the exception handler's stack frame (MethodState)
		// Run all finally clauses during unwinding
		pThread->pCatchMethodState = pCatchMethodState;
		pThread->pCatchExceptionHandler = pCatch;
		// Have to use the pThread->pCatchMethodState, as we could be getting here from END_FINALLY
		while (pCurrentMethodState != pThread->pCatchMethodState) {
			tMethodState *pPrevState;

finallyUnwindStack:
			for (i=pThread->nextFinallyUnwindStack; i<pCurrentMethodState->pMethod->pJITted->numExceptionHandlers; i++) {
				tExceptionHeader *pEx;

				pEx = &pCurrentMethodState->pMethod->pJITted->pExceptionHeaders[i];
				if (pEx->flags == COR_ILEXCEPTION_CLAUSE_FINALLY &&
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
			MethodState_Delete(pThread, &pCurrentMethodState);
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
JIT_THROW_end:
JIT_RETHROW_end:
	GO_NEXT_CHECK();

JIT_LEAVE_start:
	OPCODE_USE(JIT_LEAVE);
	{
		uint i;
		tExceptionHeader *pFinally;

		// Find any finally exception clauses
		pFinally = null;
		for (i=0; i<pJIT->numExceptionHandlers; i++) {
			if (pJIT->pExceptionHeaders[i].flags == COR_ILEXCEPTION_CLAUSE_FINALLY &&
				pCurrentMethodState->ipOffset - 1 >= pJIT->pExceptionHeaders[i].tryStart &&
				pCurrentMethodState->ipOffset - 1 < pJIT->pExceptionHeaders[i].tryEnd) {
				// Found the correct finally clause to jump to
				pFinally = &pJIT->pExceptionHeaders[i];
				break;
			}
		}
		POP_ALL();
		ofs = GET_OP();
		if (pFinally != null) {
			// Jump to 'finally' section
			pCurOp = pOps + pFinally->handlerStart;
			pCurrentMethodState->pOpEndFinally = pOps + ofs;
		} else {
			// just branch
			pCurOp = pOps + ofs;
		}
	}
JIT_LEAVE_end:
	GO_NEXT_CHECK();

JIT_END_FINALLY_start:
	OPCODE_USE(JIT_END_FINALLY);
	if (pThread->nextFinallyUnwindStack > 0) {
		// unwinding stack, so jump back to unwind code
		goto finallyUnwindStack;
	} else {
		// Just empty the evaluation stack and continue on to the next opcode
		// (finally blocks are always after catch blocks, so execution can just continue)
		POP_ALL();
		// And jump to the correct instruction, as specified in the leave instruction
		pCurOp = pCurrentMethodState->pOpEndFinally;
	}
JIT_END_FINALLY_end:
	GO_NEXT_CHECK();

done:
	SAVE_METHOD_STATE();

	return THREAD_STATUS_RUNNING;
}

void JIT_Execute_Init() {
	// Initialise the JIT code addresses
	JIT_Execute(null, 0);
}

#endif