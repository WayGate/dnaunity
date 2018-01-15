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

    // [Steve] This overly-specific-looking int-returning 3-/*STRING*/byte*-arg signature is because it's difficult
    // to support arbitrary signatures given Emscripten's limitations around needing to know the original
    // type of a function pointer when invoking it: https://kripken.github.io/emscripten-site/docs/porting/guidelines/function_pointer_issues.html
    // My workaround is just to hard-code this as the only possible PInvoke method signature and then skip
    // the code in PInvoke.c that tries to dynamically select a function pointer type.
    //
    // With more work I'm sure it would be possible to figure out a mechanism for getting the original
    // Pinvoke.c logic to work. It might even just be as simple as changing the return type of fnPInvoke from int
    // to ulong, since it looks like that's hardcoded as the return type in Pinvoke.c. But I don't need to deal
    // with that now.
    //typedef int(*fnPInvoke)(/*STRING*/byte* libName, /*STRING*/byte* funcName, /*STRING*/byte* arg0);


    #if GEN_COMBINED_OPCODES
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tCombinedOpcodesMem
    {
        public void *pMem;
        public tCombinedOpcodesMem *pNext;
    };
    #endif

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tJITted
    {
        // The JITted opcodes
        public uint *pOps;
        // The maximum size of the evaluation stack
        public uint maxStack;
        // The required size of the locals stack
        public uint localsStackSize;
        // Number of exception handler headers
        public uint numExceptionHandlers;
        // Pointer to the exception handler headers (null if none)
        public tJITExceptionHeader *pExceptionHeaders;
        #if GEN_COMBINED_OPCODES
        // The number of bytes used by this JITted method - to include ALL bytes:
        // The size of the opcodes, plus the size of the combined opcodes.
        public uint opsMemSize;
        // Store all memory used to store combined opcodes, so they can be Mem.free()d later
        public tCombinedOpcodesMem *pCombinedOpcodesMem;
        #endif
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tExceptionHeader
    {
        public uint flags;
        public uint tryStart;
        public uint tryEnd;
        public uint handlerStart;
        public uint handlerEnd;
        // Class token for type-based exception handler
        public /*IDX_TABLE*/uint classTokenOrFilterOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tJITExceptionHeader
    {
        public uint flags;
        public uint tryStart;
        public uint tryEnd;
        public uint handlerStart;
        public uint handlerEnd;
        public /*IDX_TABLE*/uint classTokenOrFilterOffset;
        // The TypeDef of the catch type
        public tMD_TypeDef *pCatchTypeDef;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tJITCallNative
    {
        public uint opCode;
        // The method meta-data
        public tMD_MethodDef *pMethodDef;
        // the native pointer to the function
        public fnInternalCall fn;
        // The RET instruction. This is needed when the native function has blocking IO or sleep
        public uint retOpCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tJITCallPInvoke
    {
        public uint opCode;
        // The native function to call
        public /*fnPInvoke*/ void* fn;
        // The method that is being called
        public tMD_MethodDef *pMethod;
        // The ImplMap of the function that's being called
        public tMD_ImplMap *pImplMap;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tJITCodeInfo 
    {
        // The beginning and end of the actual native code to run the JIT opcode.
        public void *pStart;
        public void *pEnd;
        public uint isDynamic;
    };

    public unsafe static class JIT
    {
        public const int COR_ILEXCEPTION_CLAUSE_EXCEPTION = 0;
        public const int COR_ILEXCEPTION_CLAUSE_FINALLY = 2;

        public static void Prepare(tMD_MethodDef *pMethodDef, uint genCombinedOpcodes) 
        {
            throw new System.NotImplementedException();
        }

        public static void Execute_Init()
        {
            throw new System.NotImplementedException();
        }

        public static uint Execute(tThread *pThread, uint numInst)
        {
            throw new System.NotImplementedException();
        }

#if NO

extern tJITCodeInfo jitCodeInfo[JIT_OPCODE_MAXNUM];
extern tJITCodeInfo jitCodeGoNext;

void JIT_Execute_Init();

//void JIT_Prepare(tMD_MethodDef *pMethodDef);
void JIT_Prepare(tMD_MethodDef *pMethodDef, uint genCombinedOpcodes);

uint JIT_Execute(tThread *pThread, uint numInst);

#if DIAG_OPCODE_TIMES
extern ulong opcodeTimes[JIT_OPCODE_MAXNUM];
#endif

#if DIAG_OPCODE_USE
extern uint opcodeNumUses[JIT_OPCODE_MAXNUM];
#endif

const int CorILMethod_TinyFormat = 0x02;
const int CorILMethod_MoreSects = 0x08;

const int CorILMethod_Sect_EHTable = 0x01;
const int CorILMethod_Sect_FatFormat = 0x40;
const int CorILMethod_Sect_MoreSects = 0x80;

const int DYNAMIC_OK = 0x100;
const int DYNAMIC_JUMP_TARGET = 0x200;
const int DYNAMIC_EX_START = 0x400;
const int DYNAMIC_EX_END = 0x800;
const int DYNAMIC_BYTE_COUNT_MASK = 0xff;

typedef struct tOps_ tOps;
struct tOps_ {
	uint *p;
	uint capacity;
	uint ofs;
};

typedef struct tTypeStack_ tTypeStack;
struct tTypeStack_ {
	tMD_TypeDef **ppTypes;
	uint ofs;
	uint maxBytes; // The max size of the stack in bytes
};

const int InitOps(ops_, initialCapacity) ops_.capacity = initialCapacity; ops_.ofs = 0; ops_.p = Mem.malloc((initialCapacity) * sizeof(int));
const int DeleteOps(ops_) Mem.free(ops_.p)

// Turn this into a MACRO at some point?
/* static uint Translate(uint op, uint getDynamic) {
	if (op >= JIT_OPCODE_MAXNUM) {
		Sys.Crash("Illegal opcode: %d", op);
	}
	if (jitCodeInfo[op].pEnd == null) {
		Sys.Crash("Opcode not available: 0x%08x", op);
	}
	if (getDynamic) {
		return (uint)jitCodeInfo[op].isDynamic;
	} else {
		return (uint)jitCodeInfo[op].pStart;
	}
} */

#if GEN_COMBINED_OPCODES
const int PushU32(v) PushU32_(&ops, (uint)(v)); PushU32_(&isDynamic, 0)
const int PushI32(v) PushU32_(&ops, (uint)(v)); PushU32_(&isDynamic, 0)
const int PushFloat(v) convFloat.f=(float)(v); PushU32_(&ops, convFloat.u32); PushU32_(&isDynamic, 0)
const int PushDouble(v) convDouble.d=(double)(v); PushU32_(&ops, convDouble.u32.a); PushU32_(&ops, convDouble.u32.b); PushU32_(&isDynamic, 0); PushU32_(&isDynamic, 0)
#if UNITY_WEBGL || DNA_32BIT
const int PushPTR(ptr) PushU32_(&ops, (uint)(ptr)); PushU32_(&isDynamic, 0)
#else
const int PushPTR(ptr) PushU32_(&ops, (uint)(ptr)); PushU32_(&isDynamic, 0); PushU32_(&ops, (uint)((ulong)(ptr) >> 32)); PushU32_(&isDynamic, 0)
#endif
const int PushOp(op) PushU32_(&ops, (uint)(op)); PushU32_(&isDynamic, (uint)(op))
const int PushOpParam(op, param) PushOp(op); PushU32_(&ops, (uint)(param)); PushU32_(&isDynamic, 0)
#else
const int PushU32(v) PushU32_(&ops, (uint)(v))
const int PushI32(v) PushU32_(&ops, (uint)(v))
const int PushFloat(v) convFloat.f=(float)(v); PushU32_(&ops, convFloat.u32)
const int PushDouble(v) convDouble.d=(double)(v); PushU32_(&ops, convDouble.u32.a); PushU32_(&ops, convDouble.u32.b)
#if UNITY_WEBGL || DNA_32BIT
const int PushPTR(ptr) PushU32_(&ops, (uint)(ptr))
#else
const int PushPTR(ptr) PushU32_(&ops, (uint)(ptr)); PushU32_(&ops, (uint)((ulong)(ptr) >> 32))
#endif
const int PushOp(op) PushU32_(&ops, (uint)(op))
const int PushOpParam(op, param) PushOp(op); PushU32_(&ops, (uint)(param))
#endif

const int PushBranch() PushU32_(&branchOffsets, ops.ofs)

const int PushStackType(type) PushStackType_(&typeStack, type);
const int PopStackType() (typeStack.ppTypes[--typeStack.ofs])
const int PopStackTypeDontCare() typeStack.ofs--
const int PopStackTypeMulti(number) typeStack.ofs -= number
const int PopStackTypeAll() typeStack.ofs = 0;

const int MayCopyTypeStack() if (u32Value > cilOfs) ppTypeStacks[u32Value] = DeepCopyTypeStack(&typeStack)

static void PushStackType_(tTypeStack *pTypeStack, tMD_TypeDef *pType) {
	uint i, size;

	MetaData.Fill_TypeDef(pType, null, null);
	pTypeStack->ppTypes[pTypeStack->ofs++] = pType;
	// Count current stack size in bytes
	size = 0;
	for (i=0; i<pTypeStack->ofs; i++) {
		size += pTypeStack->ppTypes[i]->stackSize;
	}
	if (size > pTypeStack->maxBytes) {
		pTypeStack->maxBytes = size;
	}
	//printf("Stack ofs = %d; Max stack size: %d (0x%x)\n", pTypeStack->ofs, size, size);
}

static void PushU32_(tOps *pOps, uint v) {
	if (pOps->ofs >= pOps->capacity) {
		pOps->capacity <<= 1;
//		printf("a.pOps->p = 0x%08x size=%d\n", pOps->p, pOps->capacity * sizeof(uint));
		pOps->p = realloc(pOps->p, pOps->capacity * sizeof(uint));
	}
	pOps->p[pOps->ofs++] = v;
}

static uint GetUnalignedU32(byte *pCIL, uint *pCILOfs) {
	uint a,b,c,d;
	a = pCIL[(*pCILOfs)++];
	b = pCIL[(*pCILOfs)++];
	c = pCIL[(*pCILOfs)++];
	d = pCIL[(*pCILOfs)++];
	return a | (b << 8) | (c << 16) | (d << 24);
}

static tTypeStack* DeepCopyTypeStack(tTypeStack *pToCopy) {
	tTypeStack *pCopy;

	pCopy = ((tTypeStack*)Mem.malloc(sizeof(tTypeStack)));
	pCopy->maxBytes = pToCopy->maxBytes;
	pCopy->ofs = pToCopy->ofs;
	if (pToCopy->ofs > 0) {
		pCopy->ppTypes = Mem.malloc(pToCopy->ofs * sizeof(tMD_TypeDef*));
		Mem.memcpy(pCopy->ppTypes, pToCopy->ppTypes, pToCopy->ofs * sizeof(tMD_TypeDef*));
	} else {
		pCopy->ppTypes = null;
	}
	return pCopy;
}

static void RestoreTypeStack(tTypeStack *pMainStack, tTypeStack *pCopyFrom) {
	// This does not effect maxBytes, as the current value will always be equal
	// or greater than the value being copied from.
	if (pCopyFrom == null) {
		pMainStack->ofs = 0;
	} else {
		pMainStack->ofs = pCopyFrom->ofs;
		if (pCopyFrom->ppTypes != null) {
			Mem.memcpy(pMainStack->ppTypes, pCopyFrom->ppTypes, pCopyFrom->ofs * sizeof(tMD_TypeDef*));
		}
	}
}

#if GEN_COMBINED_OPCODES
static uint FindOpCode(void *pAddr) {
	uint i;
	for (i=0; i<JIT_OPCODE_MAXNUM; i++) {
		if (jitCodeInfo[i].pStart == pAddr) {
			return i;
		}
	}
	Sys.Crash("Cannot find opcode for address: 0x%08x", (uint)pAddr);
	FAKE_RETURN;
}

static uint combinedMemSize = 0;
static uint GenCombined(tOps *pOps, tOps *pIsDynamic, uint startOfs, uint count, uint *pCombinedSize, void **ppMem) {
	uint memSize;
	uint ofs;
	void *pCombined;
	uint opCopyToOfs;
	uint shrinkOpsBy;
	uint goNextSize = (uint)((byte*)jitCodeGoNext.pEnd - (byte*)jitCodeGoNext.pStart);

	// Get length of final combined code chunk
	memSize = 0;
	for (ofs=0; ofs < count; ofs++) {
		uint opcode = FindOpCode((void*)pOps->p[startOfs + ofs]);
		uint size = (uint)((byte*)jitCodeInfo[opcode].pEnd - (byte*)jitCodeInfo[opcode].pStart);
		memSize += size;
		ofs += (pIsDynamic->p[startOfs + ofs] & DYNAMIC_BYTE_COUNT_MASK) >> 2;
	}
	// Add length of GoNext code
	memSize += goNextSize;

	pCombined = Mem.malloc(memSize);
	*ppMem = pCombined;
	combinedMemSize += memSize;
	*pCombinedSize = memSize;
	//Sys.log_f(0, "Combined JIT size: %d\n", combinedMemSize);

	// Copy the bits of code into place
	memSize = 0;
	opCopyToOfs = 1;
	for (ofs=0; ofs < count; ofs++) {
		uint extraOpBytes;
		uint opcode = FindOpCode((void*)pOps->p[startOfs + ofs]);
		uint size = (uint)((byte*)jitCodeInfo[opcode].pEnd - (byte*)jitCodeInfo[opcode].pStart);
		Mem.memcpy((byte*)pCombined + memSize, jitCodeInfo[opcode].pStart, size);
		memSize += size;
		extraOpBytes = pIsDynamic->p[startOfs + ofs] & DYNAMIC_BYTE_COUNT_MASK;
		memmove(&pOps->p[startOfs + opCopyToOfs], &pOps->p[startOfs + ofs + 1], extraOpBytes);
		opCopyToOfs += extraOpBytes >> 2;
		ofs += extraOpBytes >> 2;
	}
	shrinkOpsBy = ofs - opCopyToOfs;
	// Add GoNext code
	Mem.memcpy((byte*)pCombined + memSize, jitCodeGoNext.pStart, goNextSize);
	pOps->p[startOfs] = (uint)pCombined;

	return shrinkOpsBy;
}
#endif

static uint* JITit(tMD_MethodDef *pMethodDef, byte *pCIL, uint codeSize, tParameter *pLocals, tJITted *pJITted, uint genCombinedOpcodes) {
	uint maxStack = pJITted->maxStack;
	uint i;
	uint cilOfs;
	tOps ops; // The JITted op-codes
	tOps branchOffsets; // Filled with all the branch instructions that need offsets fixing
	uint *pJITOffsets;	// To store the JITted code offset of each CIL byte.
						// Only CIL bytes that are the first byte of an instruction will have meaningful data
	tTypeStack **ppTypeStacks; // To store the evaluation stack state for forward jumps
	uint *pFinalOps;
	tMD_TypeDef *pStackType;
	tTypeStack typeStack;

#if GEN_COMBINED_OPCODES
	tOps isDynamic;
#endif

	int i32Value;
	uint u32Value, u32Value2, ofs;
	uConvFloat convFloat;
	uConvDouble convDouble;
	tMD_TypeDef *pTypeA, *pTypeB;
	byte* pMem;
	tMetaData *pMetaData;

	pMetaData = pMethodDef->pMetaData;
	pJITOffsets = Mem.malloc(codeSize * sizeof(uint));
	// + 1 to handle cases where the stack is being restored at the last instruction in a method
	ppTypeStacks = Mem.malloc((codeSize + 1) * sizeof(tTypeStack*));
	Mem.memset(ppTypeStacks, 0, (codeSize + 1) * sizeof(tTypeStack*));
	typeStack.maxBytes = 0;
	typeStack.ofs = 0;
	typeStack.ppTypes = Mem.malloc(maxStack * sizeof(tMD_TypeDef*));

	// Set up all exception 'catch' blocks with the correct stack information,
	// So they'll have just the exception type on the stack when entered
	for (i=0; i<pJITted->numExceptionHandlers; i++) {
		tJITExceptionHeader *pEx;

		pEx = &pJITted->pExceptionHeaders[i]; 
		if (pEx->flags == COR_ILEXCEPTION_CLAUSE_EXCEPTION) {
			tTypeStack *pTypeStack;

			ppTypeStacks[pEx->handlerStart] = pTypeStack = ((tTypeStack*)Mem.malloc(sizeof(tTypeStack)));
			pTypeStack->maxBytes = 4;
			pTypeStack->ofs = 1;
			pTypeStack->ppTypes = TMALLOC(tMD_TypeDef*);
			pTypeStack->ppTypes[0] = pEx->pCatchTypeDef;
		}
	}

	InitOps(ops, 32);
	InitOps(branchOffsets, 16);
#if GEN_COMBINED_OPCODES
	InitOps(isDynamic, 32);
#endif

	cilOfs = 0;

	do {
		byte op;

		// Set the JIT offset for this CIL opcode
		pJITOffsets[cilOfs] = ops.ofs;

		op = pCIL[cilOfs++];
		//printf("Opcode: 0x%02x\n", op);

		switch (op) {
			case OpCodes.CIL_NOP:
				PushOp(JIT_NOP);
				break;

			case OpCodes.CIL_LDNULL:
				PushOp(JIT_LOAD_NULL);
				PushStackType(Type.types[Type.TYPE_SYSTEM_OBJECT]);
				break;

			case OpCodes.CIL_DUP:
				pStackType = PopStackType();
				PushStackType(pStackType);
				PushStackType(pStackType);
				switch (pStackType->stackSize) {
				case 4:
					PushOp(JIT_DUP_4);
					break;
				case 8:
					PushOp(JIT_DUP_8);
					break;
				default:
					PushOpParam(JIT_DUP_GENERAL, pStackType->stackSize);
					break;
				}
				break;

			case OpCodes.CIL_POP:
				pStackType = PopStackType();
				if (pStackType->stackSize == 4) {
					PushOp(JIT_POP_4);
				} else {
					PushOpParam(JIT_POP, pStackType->stackSize);
				}
				break;

			case OpCodes.CIL_LDC_I4_M1:
			case OpCodes.CIL_LDC_I4_0:
			case OpCodes.CIL_LDC_I4_1:
			case OpCodes.CIL_LDC_I4_2:
			case OpCodes.CIL_LDC_I4_3:
			case OpCodes.CIL_LDC_I4_4:
			case OpCodes.CIL_LDC_I4_5:
			case OpCodes.CIL_LDC_I4_6:
			case OpCodes.CIL_LDC_I4_7:
			case OpCodes.CIL_LDC_I4_8:
				i32Value = (sbyte)op - (sbyte)CIL_LDC_I4_0;
				goto cilLdcI4;

			case OpCodes.CIL_LDC_I4_S:
				i32Value = (sbyte)pCIL[cilOfs++];
				goto cilLdcI4;

			case OpCodes.CIL_LDC_I4:
				i32Value = (int)GetUnalignedU32(pCIL, &cilOfs);
cilLdcI4:
				if (i32Value >= -1 && i32Value <= 2) {
					PushOp(JIT_LOAD_I4_0 + i32Value);
				} else {
					PushOp(JIT_LOAD_I32);
					PushI32(i32Value);
				}
				PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
				break;

			case OpCodes.CIL_LDC_I8:
				PushOp(JIT_LOAD_I64);
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
				PushU32(u32Value);
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
				PushU32(u32Value);
				PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
				break;

			case OpCodes.CIL_LDC_R4:
				convFloat.u32 = GetUnalignedU32(pCIL, &cilOfs);
				PushStackType(Type.types[Type.TYPE_SYSTEM_SINGLE]);
				PushOp(JIT_LOAD_F32);
				PushFloat(convFloat.f);
				break;

			case OpCodes.CIL_LDC_R8:
				convDouble.u32.a = GetUnalignedU32(pCIL, &cilOfs);
				convDouble.u32.b = GetUnalignedU32(pCIL, &cilOfs);
				PushStackType(Type.types[Type.TYPE_SYSTEM_DOUBLE]);
				PushOp(JIT_LOAD_F64);
				PushDouble(convDouble.d);
				break;

			case OpCodes.CIL_LDARG_0:
			case OpCodes.CIL_LDARG_1:
			case OpCodes.CIL_LDARG_2:
			case OpCodes.CIL_LDARG_3:
				u32Value = op - CIL_LDARG_0;
				goto cilLdArg;

			case OpCodes.CIL_LDARG_S:
				u32Value = pCIL[cilOfs++];
cilLdArg:
				pStackType = pMethodDef->pParams[u32Value].pTypeDef;
				ofs = pMethodDef->pParams[u32Value].offset;
				if (pStackType->stackSize == 4 && ofs < 32) {
					PushOp(JIT_LOADPARAMLOCAL_0 + (ofs >> 2));
				} else {
					PushOpParam(JIT_LOADPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
					// if it's a valuetype then push the TypeDef of it afterwards
					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
						PushPTR(pStackType);
					}
				}
				PushStackType(pStackType);
				break;

			case OpCodes.CIL_LDARGA_S:
				// Get the argument number to load the address of
				u32Value = pCIL[cilOfs++];
				PushOpParam(JIT_LOAD_PARAMLOCAL_ADDR, pMethodDef->pParams[u32Value].offset);
				PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
				break;

			case OpCodes.CIL_STARG_S:
				// Get the argument number to store the arg of
				u32Value = pCIL[cilOfs++];
				pStackType = PopStackType();
				ofs = pMethodDef->pParams[u32Value].offset;
				if (pStackType->stackSize == 4 && ofs < 32) {
					PushOp(JIT_STOREPARAMLOCAL_0 + (ofs >> 2));
				} else {
					PushOpParam(JIT_STOREPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
					// if it's a valuetype then push the TypeDef of it afterwards
					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
						PushPTR(pStackType);
					}
				}
				break;

			case OpCodes.CIL_LDLOC_0:
			case OpCodes.CIL_LDLOC_1:
			case OpCodes.CIL_LDLOC_2:
			case OpCodes.CIL_LDLOC_3:
				// Push opcode and offset into locals memory
				u32Value = op - CIL_LDLOC_0;
				goto cilLdLoc;

			case OpCodes.CIL_LDLOC_S:
				// Push opcode and offset into locals memory
				u32Value = pCIL[cilOfs++];
cilLdLoc:
				pStackType = pLocals[u32Value].pTypeDef;
				ofs = pMethodDef->parameterStackSize + pLocals[u32Value].offset;
				if (pStackType->stackSize == 4 && ofs < 32) {
					PushOp(JIT_LOADPARAMLOCAL_0 + (ofs >> 2));
				} else {
					PushOpParam(JIT_LOADPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
					// if it's a valuetype then push the TypeDef of it afterwards
					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
						PushPTR(pStackType);
					}
				}
				PushStackType(pStackType);
				break;

			case OpCodes.CIL_STLOC_0:
			case OpCodes.CIL_STLOC_1:
			case OpCodes.CIL_STLOC_2:
			case OpCodes.CIL_STLOC_3:
				u32Value = op - CIL_STLOC_0;
				goto cilStLoc;

			case OpCodes.CIL_STLOC_S:
				u32Value = pCIL[cilOfs++];
cilStLoc:
				pStackType = PopStackType();
				ofs = pMethodDef->parameterStackSize + pLocals[u32Value].offset;
				if (pStackType->stackSize == 4 && ofs < 32) {
					PushOp(JIT_STOREPARAMLOCAL_0 + (ofs >> 2));
				} else {
					PushOpParam(JIT_STOREPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
					// if it's a valuetype then push the TypeDef of it afterwards
					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
						PushPTR(pStackType);
					}
				}
				break;

			case OpCodes.CIL_LDLOCA_S:
				// Get the local number to load the address of
				u32Value = pCIL[cilOfs++];
				PushOpParam(JIT_LOAD_PARAMLOCAL_ADDR, pMethodDef->parameterStackSize + pLocals[u32Value].offset);
				PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
				break;

			case OpCodes.CIL_LDIND_I1:
				u32Value = Type.TYPE_SYSTEM_SBYTE;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_U1:
				u32Value = Type.TYPE_SYSTEM_BYTE;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_I2:
				u32Value = Type.TYPE_SYSTEM_INT16;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_U2:
				u32Value = Type.TYPE_SYSTEM_UINT16;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_I4:
				u32Value = Type.TYPE_SYSTEM_INT32;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_U4:
				u32Value = Type.TYPE_SYSTEM_UINT32;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_I8:
				u32Value = Type.TYPE_SYSTEM_INT64;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_R4:
				u32Value = Type.TYPE_SYSTEM_SINGLE;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_R8:
				u32Value = Type.TYPE_SYSTEM_DOUBLE;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_REF:
				u32Value = Type.TYPE_SYSTEM_OBJECT;
				goto cilLdInd;
			case OpCodes.CIL_LDIND_I:
				u32Value = Type.TYPE_SYSTEM_INTPTR;
cilLdInd:
				PopStackTypeDontCare(); // don't care what it is
				PushOp(JIT_LOADINDIRECT_I8 + (op - CIL_LDIND_I1));
				PushStackType(Type.types[u32Value]);
				break;

			case OpCodes.CIL_STIND_REF:
			case OpCodes.CIL_STIND_I1:
			case OpCodes.CIL_STIND_I2:
			case OpCodes.CIL_STIND_I4:
				PopStackTypeMulti(2); // Don't care what they are
				PushOp(JIT_STOREINDIRECT_REF + (op - CIL_STIND_REF));
				break;

			case OpCodes.CIL_RET:
				PushOp(JIT_RETURN);
				RestoreTypeStack(&typeStack, ppTypeStacks[cilOfs]);
				break;

			case OpCodes.CIL_CALL:
			case OpCodes.CIL_CALLVIRT:
				{
					tMD_MethodDef *pCallMethod;
					tMD_TypeDef *pBoxCallType;
					uint derefRefType;
					byte dynamicallyBoxReturnValue = 0;

					u32Value2 = 0;

cilCallVirtConstrained:
					pBoxCallType = null;
					derefRefType = 0;

					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pCallMethod = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					if (pCallMethod->isFilled == 0) {
						tMD_TypeDef *pTypeDef;
						
						pTypeDef = MetaData.GetTypeDefFromMethodDef(pCallMethod);
						MetaData.Fill_TypeDef(pTypeDef, null, null);
					}

					if (u32Value2 != 0) {
						// There is a 'constrained' prefix
						tMD_TypeDef *pConstrainedType;

						pConstrainedType = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, u32Value2, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
						if (MetaData.TYPE_ISINTERFACE(pCallMethod->pParentType)) {
							u32Value2 = 0xffffffff;
							// Find the interface that we're dealing with
							for (i=0; i<pConstrainedType->numInterfaces; i++) {
								if (pConstrainedType->pInterfaceMaps[i].pInterface == pCallMethod->pParentType) {
									u32Value2 = pConstrainedType->pInterfaceMaps[i].pVTableLookup[pCallMethod->vTableOfs];
									break;
								}
							}
							Assert(u32Value2 != 0xffffffff);
							if (pConstrainedType->pVTable[u32Value2]->pParentType == pConstrainedType) {
								// This method is implemented on this class, so make it a normal CALL op
								op = CIL_CALL;
								pCallMethod = pConstrainedType->pVTable[u32Value2];
							}
						} else {
							if (pConstrainedType->isValueType) {
								tMD_MethodDef *pImplMethod;
								// If pConstraintedType directly implements the call then don't do anything
								// otherwise the 'this' pointer must be boxed (BoxedCall)
								pImplMethod = pConstrainedType->pVTable[pCallMethod->vTableOfs];
								if (pImplMethod->pParentType == pConstrainedType) {
									op = CIL_CALL;
									pCallMethod = pImplMethod;
								} else {
									pBoxCallType = pConstrainedType;
								}
							} else {
								// Reference-type, so dereference the byte* to 'this' and use that for the 'this' for the call.
								derefRefType = 1;
							}
						}
					}

					// Pop stack type for each argument. Don't actually care what these are,
					// except the last one which will be the 'this' object type of a non-static method
					//printf("Call %s() - popping %d stack args\n", pCallMethod->name, pCallMethod->numberOfParameters);
					for (i=0; i<pCallMethod->numberOfParameters; i++) {
						pStackType = PopStackType();
					}
					// the stack type of the 'this' object will now be in stackType (if there is one)
					if (MetaData.MetaData.METHOD_ISSTATIC(pCallMethod)) {
						pStackType = Type.types[Type.TYPE_SYSTEM_OBJECT];
					}
					MetaData.Fill_TypeDef(pStackType, null, null);
					if (MetaData.TYPE_ISINTERFACE(pCallMethod->pParentType) && op == CIL_CALLVIRT) {
						PushOp(JIT_CALL_INTERFACE);
					} else if (pCallMethod->pParentType->pParent == Type.types[Type.TYPE_SYSTEM_MULTICASTDELEGATE]) {
						PushOp(JIT_INVOKE_DELEGATE);
					} else if (pCallMethod->pParentType == Type.types[Type.TYPE_SYSTEM_REFLECTION_METHODBASE] && S.strcmp(pCallMethod->name, "Invoke") == 0) {
						PushOp(JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE);
						dynamicallyBoxReturnValue = 1;
					} else {
						switch (pStackType->stackType)
						{
						case EvalStack.EVALSTACK_INTNATIVE: // Not really right, but it'll work on 32-bit
						case EvalStack.EVALSTACK_O:
							if (derefRefType) {
								PushOp(JIT_DEREF_CALLVIRT);
							} else {
								if (pBoxCallType != null) {
									PushOp(JIT_BOX_CALLVIRT);
									PushPTR(pBoxCallType);
								} else {
									PushOp((op == CIL_CALL)?JIT_CALL_O:JIT_CALLVIRT_O);
								}
							}
							break;
						case EvalStack.EVALSTACK_PTR:
						case EvalStack.EVALSTACK_VALUETYPE:
							if (derefRefType) {
								PushOp(JIT_DEREF_CALLVIRT);
							} else if (pBoxCallType != null) {
								PushOp(JIT_BOX_CALLVIRT);
								PushPTR(pBoxCallType);
							} else {
								PushOp(JIT_CALL_PTR);
							}
							break;
						default:
							Sys.Crash("JITit(): Cannot CALL or CALLVIRT with stack type: %d", pStackType->stackType);
						}
					}
					PushPTR(pCallMethod);

					if (pCallMethod->pReturnType != null) {
						PushStackType(pCallMethod->pReturnType);
					}

					if (dynamicallyBoxReturnValue) {
						PushOp(JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE);
					}
				}
				break;

			case OpCodes.CIL_BR_S: // unconditional branch
				u32Value = (sbyte)pCIL[cilOfs++];
				goto cilBr;

			case OpCodes.CIL_BR:
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
cilBr:
				// Put a temporary CIL offset value into the JITted code. This will be updated later
				u32Value = cilOfs + (int)u32Value;
				MayCopyTypeStack();
				PushOp(JIT_BRANCH);
				PushBranch();
				PushU32(u32Value);
				// Restore the stack state
				RestoreTypeStack(&typeStack, ppTypeStacks[cilOfs]);
				break;

			case OpCodes.CIL_SWITCH:
				// This is the int containing the switch value. Don't care what it is.
				PopStackTypeDontCare();
				// The number of switch jump targets
				i32Value = (int)GetUnalignedU32(pCIL, &cilOfs);
				// Set up the offset from which the jump offsets are calculated
				u32Value2 = cilOfs + (i32Value << 2);
				PushOpParam(JIT_SWITCH, i32Value);
				for (i=0; i<(uint)i32Value; i++) {
					// A jump target
					u32Value = u32Value2 + (int)GetUnalignedU32(pCIL, &cilOfs);
					PushBranch();
					MayCopyTypeStack();
					// Push the jump target.
					// It is needed to allow the branch offset to be correctly updated later.
					PushU32(u32Value);
				}
				break;

			case OpCodes.CIL_BRFALSE_S:
			case OpCodes.CIL_BRTRUE_S:
				u32Value = (sbyte)pCIL[cilOfs++];
				u32Value2 = JIT_BRANCH_FALSE_U32 + (op - CIL_BRFALSE_S);
				goto cilBrFalseTrue;

			case OpCodes.CIL_BRFALSE:
			case OpCodes.CIL_BRTRUE:
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
				u32Value2 = JIT_BRANCH_FALSE_U32 + (op - CIL_BRFALSE);
cilBrFalseTrue:
				pTypeA = PopStackType();
                if (pTypeA->stackSize == 8)
                    u32Value2 += 2;
				// Put a temporary CIL offset value into the JITted code. This will be updated later
				u32Value = cilOfs + (int)u32Value;
				MayCopyTypeStack();
				PushOp(u32Value2);
				PushBranch();
				PushU32(u32Value);
				break;

			case OpCodes.CIL_BEQ_S:
			case OpCodes.CIL_BGE_S:
			case OpCodes.CIL_BGT_S:
			case OpCodes.CIL_BLE_S:
			case OpCodes.CIL_BLT_S:
			case OpCodes.CIL_BNE_UN_S:
			case OpCodes.CIL_BGE_UN_S:
			case OpCodes.CIL_BGT_UN_S:
			case OpCodes.CIL_BLE_UN_S:
			case OpCodes.CIL_BLT_UN_S:
				u32Value = (sbyte)pCIL[cilOfs++];
				u32Value2 = CIL_BEQ_S;
				goto cilBrCond;

			case OpCodes.CIL_BEQ:
			case OpCodes.CIL_BGE:
			case OpCodes.CIL_BGT:
			case OpCodes.CIL_BLE:
			case OpCodes.CIL_BLT:
			case OpCodes.CIL_BNE_UN:
			case OpCodes.CIL_BGE_UN:
			case OpCodes.CIL_BGT_UN:
			case OpCodes.CIL_BLE_UN:
			case OpCodes.CIL_BLT_UN:
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
				u32Value2 = CIL_BEQ;
cilBrCond:
				pTypeB = PopStackType();
				pTypeA = PopStackType();
				u32Value = cilOfs + (int)u32Value;
				MayCopyTypeStack();
#if UNITY_WEBGL || DNA_32BIT
				if ((pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) ||
					(pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O)) {
#else
                if (pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) {
#endif
					PushOp(JIT_BEQ_I32I32 + (op - u32Value2));
#if UNITY_WEBGL || DNA_32BIT
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) {
#else
                } else if ((pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) ||
                           (pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O)) {
#endif
					PushOp(JIT_BEQ_I64I64 + (op - u32Value2));
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F32 && pTypeB->stackType == EvalStack.EVALSTACK_F32) {
					PushOp(JIT_BEQ_F32F32 + (op - u32Value2));
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F64 && pTypeB->stackType == EvalStack.EVALSTACK_F64) {
					PushOp(JIT_BEQ_F64F64 + (op - u32Value2));
				} else {
					Sys.Crash("JITit(): Cannot perform conditional branch on stack Type.types: %d and %d", pTypeA->stackType, pTypeB->stackType);
				}
				PushBranch();
				PushU32(u32Value);
				break;

			case OpCodes.CIL_ADD_OVF:
			case OpCodes.CIL_ADD_OVF_UN:
			case OpCodes.CIL_MUL_OVF:
			case OpCodes.CIL_MUL_OVF_UN:
			case OpCodes.CIL_SUB_OVF:
			case OpCodes.CIL_SUB_OVF_UN:
				u32Value = (CIL_ADD_OVF - CIL_ADD) + (JIT_ADD_I32I32 - JIT_ADD_OVF_I32I32);
				goto cilBinaryArithOp;
			case OpCodes.CIL_ADD:
			case OpCodes.CIL_SUB:
			case OpCodes.CIL_MUL:
			case OpCodes.CIL_DIV:
			case OpCodes.CIL_DIV_UN:
			case OpCodes.CIL_REM:
			case OpCodes.CIL_REM_UN:
			case OpCodes.CIL_AND:
			case OpCodes.CIL_OR:
			case OpCodes.CIL_XOR:
				u32Value = 0;
cilBinaryArithOp:
				pTypeB = PopStackType();
				pTypeA = PopStackType();
				if (pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) {
					PushOp(JIT_ADD_I32I32 + (op - CIL_ADD) - u32Value);
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) {
					PushOp(JIT_ADD_I64I64 + (op - CIL_ADD) - u32Value);
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F32 && pTypeB->stackType == EvalStack.EVALSTACK_F32) {
					PushOp(JIT_ADD_F32F32 + (op - CIL_ADD) - u32Value);
					PushStackType(pTypeA);
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F64 && pTypeB->stackType == EvalStack.EVALSTACK_F64) {
					PushOp(JIT_ADD_F64F64 + (op - CIL_ADD) - u32Value);
					PushStackType(pTypeA);
				} else {
					Sys.Crash("JITit(): Cannot perform binary numeric operand on stack Type.types: %d and %d", pTypeA->stackType, pTypeB->stackType);
				}
				break;

			case OpCodes.CIL_NEG:
			case OpCodes.CIL_NOT:
				pTypeA = PopStackType();
				if (pTypeA->stackType == EvalStack.EVALSTACK_INT32) {
					PushOp(JIT_NEG_I32 + (op - CIL_NEG));
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64) {
					PushOp(JIT_NEG_I64 + (op - CIL_NEG));
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
				} else {
					Sys.Crash("JITit(): Cannot perform unary operand on stack Type.types: %d", pTypeA->stackType);
				}
				break;

			case OpCodes.CIL_SHL:
			case OpCodes.CIL_SHR:
			case OpCodes.CIL_SHR_UN:
				PopStackTypeDontCare(); // Don't care about the shift amount
				pTypeA = PopStackType(); // Do care about the value to shift
				if (pTypeA->stackType == EvalStack.EVALSTACK_INT32) {
					PushOp(JIT_SHL_I32 - CIL_SHL + op);
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64) {
					PushOp(JIT_SHL_I64 - CIL_SHL + op);
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
				} else {
					Sys.Crash("JITit(): Cannot perform shift operation on type: %s", pTypeA->name);
				}
				break;

				// Conversion operations
				{
					uint toType;
					uint toBitCount;
					uint convOpOffset;
			case OpCodes.CIL_CONV_I1:
			case OpCodes.CIL_CONV_OVF_I1: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_I1_UN: // Fix this later - will never overflow
				toBitCount = 8;
				toType = Type.TYPE_SYSTEM_SBYTE;
				goto cilConvInt32;
			case OpCodes.CIL_CONV_I2:
			case OpCodes.CIL_CONV_OVF_I2: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_I2_UN: // Fix this later - will never overflow
				toBitCount = 16;
				toType = Type.TYPE_SYSTEM_INT16;
				goto cilConvInt32;
			case OpCodes.CIL_CONV_I4:
			case OpCodes.CIL_CONV_OVF_I4: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_I4_UN: // Fix this later - will never overflow
#if UNITY_WEBGL || DNA_32BIT
			case OpCodes.CIL_CONV_I: // Only on 32-bit
			case OpCodes.CIL_CONV_OVF_I_UN: // Only on 32-bit; Fix this later - will never overflow
#endif
				toBitCount = 32;
				toType = Type.TYPE_SYSTEM_INT32;
cilConvInt32:
				convOpOffset = JIT_CONV_OFFSET_I32;
				goto cilConv;
			case OpCodes.CIL_CONV_U1:
			case OpCodes.CIL_CONV_OVF_U1: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_U1_UN: // Fix this later - will never overflow
				toBitCount = 8;
				toType = Type.TYPE_SYSTEM_BYTE;
				goto cilConvUInt32;
			case OpCodes.CIL_CONV_U2:
			case OpCodes.CIL_CONV_OVF_U2: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_U2_UN: // Fix this later - will never overflow
				toBitCount = 16;
				toType = Type.TYPE_SYSTEM_UINT16;
				goto cilConvUInt32;
			case OpCodes.CIL_CONV_U4:
			case OpCodes.CIL_CONV_OVF_U4: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_U4_UN: // Fix this later - will never overflow
#if UNITY_WEBGL || DNA_32BIT
			case OpCodes.CIL_CONV_U:
			case OpCodes.CIL_CONV_OVF_U_UN:
#endif
				toBitCount = 32;
				toType = Type.TYPE_SYSTEM_UINT32;
cilConvUInt32:
				convOpOffset = JIT_CONV_OFFSET_U32;
				goto cilConv;
                    
			case OpCodes.CIL_CONV_I8:
			case OpCodes.CIL_CONV_OVF_I8: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_I8_UN: // Fix this later - will never overflow
#if !(UNITY_WEBGL || DNA_32BIT)
            case OpCodes.CIL_CONV_I:
            case OpCodes.CIL_CONV_OVF_I_UN:
#endif
				toType = Type.TYPE_SYSTEM_INT64;
				convOpOffset = JIT_CONV_OFFSET_I64;
				goto cilConv;
			case OpCodes.CIL_CONV_U8:
			case OpCodes.CIL_CONV_OVF_U8: // Fix this later - will never overflow
			case OpCodes.CIL_CONV_OVF_U8_UN: // Fix this later - will never overflow
#if !(UNITY_WEBGL || DNA_32BIT)
            case OpCodes.CIL_CONV_U:
            case OpCodes.CIL_CONV_OVF_U_UN:
#endif
				toType = Type.TYPE_SYSTEM_UINT64;
				convOpOffset = JIT_CONV_OFFSET_U64;
				goto cilConv;
			case OpCodes.CIL_CONV_R4:
				toType = Type.TYPE_SYSTEM_SINGLE;
				convOpOffset = JIT_CONV_OFFSET_R32;
				goto cilConv;
			case OpCodes.CIL_CONV_R8:
			case OpCodes.CIL_CONV_R_UN:
				toType = Type.TYPE_SYSTEM_DOUBLE;
				convOpOffset = JIT_CONV_OFFSET_R64;
				goto cilConv;
cilConv:
				pStackType = PopStackType();
				{
					uint opCodeBase;
					uint useParam = 0, param;
					// This is the type that the conversion is from.
					switch (pStackType->stackType) {
					case EvalStack.EVALSTACK_INT64:
						opCodeBase = (pStackType == Type.types[Type.TYPE_SYSTEM_INT64])?JIT_CONV_FROM_I64:JIT_CONV_FROM_U64;
						break;
					case EvalStack.EVALSTACK_INT32:
                        opCodeBase =
                            (pStackType == Type.types[Type.TYPE_SYSTEM_BYTE] ||
                             pStackType == Type.types[Type.TYPE_SYSTEM_UINT16] ||
                             pStackType == Type.types[Type.TYPE_SYSTEM_UINT32])
                                ?JIT_CONV_FROM_U32:JIT_CONV_FROM_I32;
                            break;
					case EvalStack.EVALSTACK_PTR:
						opCodeBase =
							(pStackType == Type.types[Type.TYPE_SYSTEM_UINTPTR])
#if UNITY_WEBGL || DNA_32BIT
                                ?JIT_CONV_FROM_U32:JIT_CONV_FROM_I32;
#else
                                ?JIT_CONV_FROM_U64:JIT_CONV_FROM_I64;
#endif
						break;
					case EvalStack.EVALSTACK_F64:
						opCodeBase = JIT_CONV_FROM_R64;
						break;
					case EvalStack.EVALSTACK_F32:
						opCodeBase = JIT_CONV_FROM_R32;
						break;
					default:
                        opCodeBase = 0;
						Sys.Crash("JITit() Conv cannot handle stack type %d", pStackType->stackType);
					}
					// This is the type that the conversion is to.
					switch (convOpOffset) {
					case JIT_CONV_OFFSET_I32:
						useParam = 1;
						param = 32 - toBitCount;
						break;
					case JIT_CONV_OFFSET_U32:
						useParam = 1;
						// Next line is really (1 << toBitCount) - 1
						// But it's done like this to work when toBitCount == 32
						param = (((1 << (toBitCount - 1)) - 1) << 1) + 1;
						break;
					case JIT_CONV_OFFSET_I64:
					case JIT_CONV_OFFSET_U64:
					case JIT_CONV_OFFSET_R32:
					case JIT_CONV_OFFSET_R64:
						break;
					default:
						Sys.Crash("JITit() Conv cannot handle convOpOffset %d", convOpOffset);
					}
					PushOp(opCodeBase + convOpOffset);
					if (useParam) {
						PushU32(param);
					}
				}
				PushStackType(Type.types[toType]);
				break;
				}

			case OpCodes.CIL_LDOBJ:
				{
					tMD_TypeDef *pTypeDef;

					PopStackTypeDontCare(); // Don't care what this is
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PushOp(JIT_LOADOBJECT);
					PushPTR(pTypeDef);
					PushStackType(pTypeDef);
				}
				break;

			case OpCodes.CIL_STOBJ:
				{
					tMD_TypeDef *pTypeDef;

					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PopStackTypeMulti(2);
					if (pTypeDef->isValueType && pTypeDef->arrayElementSize != 4) {
						// If it's a value-type then do this
						PushOpParam(JIT_STORE_OBJECT_VALUETYPE, pTypeDef->arrayElementSize);
					} else {
						// If it's a ref type, or a value-type with size 4, then can do this instead
						// (it executes faster)
						PushOp(JIT_STOREINDIRECT_REF);
					}
					break;
				}

			case OpCodes.CIL_LDSTR:
				u32Value = GetUnalignedU32(pCIL, &cilOfs) & 0x00ffffff;
				PushOpParam(JIT_LOAD_STRING, u32Value);
				PushStackType(Type.types[Type.TYPE_SYSTEM_STRING]);
				break;

			case OpCodes.CIL_NEWOBJ:
				{
					tMD_MethodDef *pConstructorDef;

					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pConstructorDef = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					if (pConstructorDef->isFilled == 0) {
						tMD_TypeDef *pTypeDef;

						pTypeDef = MetaData.GetTypeDefFromMethodDef(pConstructorDef);
						MetaData.Fill_TypeDef(pTypeDef, null, null);
					}
					if (pConstructorDef->pParentType->isValueType) {
						PushOp(JIT_NEWOBJECT_VALUETYPE);
					} else {
						PushOp(JIT_NEWOBJECT);
					}
					// -1 because the param count includes the 'this' parameter that is sent to the constructor
					PopStackTypeMulti(pConstructorDef->numberOfParameters - 1);
					PushPTR(pConstructorDef);
					PushStackType(pConstructorDef->pParentType);
				}
				break;

			case OpCodes.CIL_CASTCLASS:
				{
					tMD_TypeDef *pCastToType;

					PushOp(JIT_CAST_CLASS);
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pCastToType = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PushPTR(pCastToType);
				}
				break;

			case OpCodes.CIL_ISINST:
				{
					tMD_TypeDef *pIsInstanceOfType;

					PushOp(JIT_IS_INSTANCE);
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pIsInstanceOfType = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PushPTR(pIsInstanceOfType);
				}
				break;

			case OpCodes.CIL_NEWARR:
				{
					tMD_TypeDef *pTypeDef;

					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PopStackTypeDontCare(); // Don't care what it is
					PushOp(JIT_NEW_VECTOR);
					MetaData.Fill_TypeDef(pTypeDef, null, null);
					pTypeDef = Type.GetArrayTypeDef(pTypeDef, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PushPTR(pTypeDef);
					PushStackType(pTypeDef);
				}
				break;

			case OpCodes.CIL_LDLEN:
				PopStackTypeDontCare(); // Don't care what it is
				PushOp(JIT_LOAD_VECTOR_LEN);
				PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
				break;

			case OpCodes.CIL_LDELEM_I1:
			case OpCodes.CIL_LDELEM_U1:
			case OpCodes.CIL_LDELEM_I2:
			case OpCodes.CIL_LDELEM_U2:
			case OpCodes.CIL_LDELEM_I4:
			case OpCodes.CIL_LDELEM_U4:
				PopStackTypeMulti(2); // Don't care what any of these are
				PushOp(JIT_LOAD_ELEMENT_I8 + (op - CIL_LDELEM_I1));
				PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
				break;

			case OpCodes.CIL_LDELEM_I8:
				PopStackTypeMulti(2); // Don't care what any of these are
				PushOp(JIT_LOAD_ELEMENT_I64);
				PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
				break;

			case OpCodes.CIL_LDELEM_R4:
				PopStackTypeMulti(2); // Don't care what any of these are
				PushOp(JIT_LOAD_ELEMENT_R32);
				PushStackType(Type.types[Type.TYPE_SYSTEM_SINGLE]);
				break;

			case OpCodes.CIL_LDELEM_R8:
				PopStackTypeMulti(2); // Don't care what any of these are
				PushOp(JIT_LOAD_ELEMENT_R64);
				PushStackType(Type.types[Type.TYPE_SYSTEM_DOUBLE]);
				break;

			case OpCodes.CIL_LDELEM_REF:
				PopStackTypeMulti(2); // Don't care what any of these are
#if UNITY_WEBGL || DNA_32BIT
				PushOp(JIT_LOAD_ELEMENT_U32);
#else
                PushOp(JIT_LOAD_ELEMENT_I64);
#endif
				PushStackType(Type.types[Type.TYPE_SYSTEM_OBJECT]);
				break;

			case OpCodes.CIL_LDELEM_ANY:
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
				pStackType = (tMD_TypeDef*)MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
				PopStackTypeMulti(2); // Don't care what these are
				PushOpParam(JIT_LOAD_ELEMENT, pStackType->stackSize);
				PushStackType(pStackType);
				break;

			case OpCodes.CIL_LDELEMA:
				PopStackTypeMulti(2); // Don't care what any of these are
				GetUnalignedU32(pCIL, &cilOfs); // Don't care what this is
				PushOp(JIT_LOAD_ELEMENT_ADDR);
				PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
				break;

			case OpCodes.CIL_STELEM_I1:
			case OpCodes.CIL_STELEM_I2:
			case OpCodes.CIL_STELEM_I4:
			case OpCodes.CIL_STELEM_R4:
				PopStackTypeMulti(3); // Don't care what any of these are
				PushOp(JIT_STORE_ELEMENT_32);
				break;

			case OpCodes.CIL_STELEM_I8:
			case OpCodes.CIL_STELEM_R8:
				PopStackTypeMulti(3); // Don't care what any of these are
				PushOp(JIT_STORE_ELEMENT_64);
				break;

            case OpCodes.CIL_STELEM_REF:
#if UNITY_WEBGL || DNA_32BIT
                PopStackTypeMulti(3); // Don't care what any of these are
                PushOp(JIT_STORE_ELEMENT_32);
#else
                PopStackTypeMulti(3); // Don't care what any of these are
                PushOp(JIT_STORE_ELEMENT_64);
#endif
                break;
                
			case OpCodes.CIL_STELEM_ANY:
				GetUnalignedU32(pCIL, &cilOfs); // Don't need this token, as the type stack will contain the same type
				pStackType = PopStackType(); // This is the type to store
				PopStackTypeMulti(2); // Don't care what these are
				PushOpParam(JIT_STORE_ELEMENT, pStackType->stackSize);
				break;

			case OpCodes.CIL_STFLD:
				{
					tMD_FieldDef *pFieldDef;

					// Get the stack type of the value to store
					pStackType = PopStackType();
					PushOp(JIT_STOREFIELD_TYPEID + pStackType->stackType);
					// Get the FieldRef or FieldDef of the field to store
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					PushPTR(pFieldDef);
					// Pop the object/valuetype on which to store the field. Don't care what it is
					PopStackTypeDontCare();
				}
				break;

			case OpCodes.CIL_LDFLD:
				{
					tMD_FieldDef *pFieldDef;

					// Get the FieldRef or FieldDef of the field to load
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					// Pop the object/valuetype on which to load the field.
					pStackType = PopStackType();
					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
						PushOpParam(JIT_LOADFIELD_VALUETYPE, pStackType->stackSize);
						PushPTR(pFieldDef);
					} else {
						if (pFieldDef->memSize <= 4) {
							PushOp(JIT_LOADFIELD_4);
							PushU32(pFieldDef->memOffset);
                        } else if (pFieldDef->memSize == 8) {
                            PushOp(JIT_LOADFIELD_8);
                            PushU32(pFieldDef->memOffset);
                        } else {
							PushOp(JIT_LOADFIELD);
							PushPTR(pFieldDef);
						}
					}
					// Push the stack type of the just-read field
					PushStackType(pFieldDef->pType);
				}
				break;

			case OpCodes.CIL_LDFLDA:
				{
					tMD_FieldDef *pFieldDef;
					tMD_TypeDef *pTypeDef;

					// Get the FieldRef or FieldDef of the field to load
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					// Sometimes, the type def will not have been filled, so ensure it's filled.
					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
					MetaData.Fill_TypeDef(pTypeDef, null, null);
					PopStackTypeDontCare(); // Don't care what it is
					PushOpParam(JIT_LOAD_FIELD_ADDR, pFieldDef->memOffset);
					PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
				}
				break;

			case OpCodes.CIL_STSFLD: // Store static field
				{
					tMD_FieldDef *pFieldDef;
					tMD_TypeDef *pTypeDef;

					// Get the FieldRef or FieldDef of the static field to store
					PopStackTypeDontCare(); // Don't care what it is
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					// Sometimes, the type def will not have been filled, so ensure it's filled.
					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
					MetaData.Fill_TypeDef(pTypeDef, null, null);
					pStackType = pFieldDef->pType;
					PushOp(JIT_STORESTATICFIELD_TYPEID + pStackType->stackType);
					PushPTR(pFieldDef);
				}
				break;

			case OpCodes.CIL_LDSFLD: // Load static field
				{
					tMD_FieldDef *pFieldDef;
					tMD_TypeDef *pTypeDef;

					// Get the FieldRef or FieldDef of the static field to load
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					// Sometimes, the type def will not have been filled, so ensure it's filled.
					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
					MetaData.Fill_TypeDef(pTypeDef, null, null);
					pStackType = pFieldDef->pType;
					PushOp(JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + pStackType->stackType);
					PushPTR(pFieldDef);
					PushStackType(pStackType);
				}
				break;

			case OpCodes.CIL_LDSFLDA: // Load static field address
				{
					tMD_FieldDef *pFieldDef;
					tMD_TypeDef *pTypeDef;

					// Get the FieldRef or FieldDef of the field to load
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					// Sometimes, the type def will not have been filled, so ensure it's filled.
					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
					MetaData.Fill_TypeDef(pTypeDef, null, null);
					PushOp(JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT);
					PushPTR(pFieldDef);
					PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
				}
				break;

			case OpCodes.CIL_BOX:
				{
					tMD_TypeDef *pTypeDef;

					pStackType = PopStackType();
					// Get the TypeDef(or Ref) token of the valuetype to box
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					MetaData.Fill_TypeDef(pTypeDef, null, null);
					if (pTypeDef->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE]) {
						// This is a nullable type, so special boxing code is needed.
						PushOp(JIT_BOX_NULLABLE);
						// Push the underlying type of the nullable type, not the nullable type itself
						PushPTR(pTypeDef->ppClassTypeArgs[0]);
					} else {
						PushOp(JIT_BOX_TYPEID + pStackType->stackType);
						PushPTR(pTypeDef);
					}
					// This is correct - cannot push underlying type, as then references are treated as value-Type.types
					PushStackType(Type.types[Type.TYPE_SYSTEM_OBJECT]);
				}
				break;

			case OpCodes.CIL_UNBOX_ANY:
				{
					tMD_TypeDef *pTypeDef;

					PopStackTypeDontCare(); // Don't care what it is
					u32Value = GetUnalignedU32(pCIL, &cilOfs);
					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
					if (pTypeDef->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE]) {
						// This is a nullable type, so special unboxing is required.
						PushOp(JIT_UNBOX_NULLABLE);
						// For nullable Type.types, push the underlying type
						PushPTR(pTypeDef->ppClassTypeArgs[0]);
					} else if (pTypeDef->isValueType) {
						PushOp(JIT_UNBOX2VALUETYPE);
					} else {
						PushOp(JIT_UNBOX2OBJECT);
					}
					PushStackType(pTypeDef);
				}
				break;

			case OpCodes.CIL_LDTOKEN:
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
				pMem = MetaData.GetTypeMethodField(pMethodDef->pMetaData, u32Value, &u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
				PushOp(JIT_LOADTOKEN_BASE + u32Value);
				PushPTR(pMem);
				PushStackType(Type.types[
					(u32Value==0)?Type.TYPE_SYSTEM_RUNTIMETYPEHANDLE:
						((u32Value==1)?Type.TYPE_SYSTEM_RUNTIMEFIELDHANDLE:Type.TYPE_SYSTEM_RUNTIMEMETHODHANDLE)
				]);
				break;

			case OpCodes.CIL_THROW:
				PopStackTypeDontCare(); // Don't care what it is
				PushOp(JIT_THROW);
				RestoreTypeStack(&typeStack, ppTypeStacks[cilOfs]);
				break;

			case OpCodes.CIL_LEAVE_S:
				u32Value = (sbyte)pCIL[cilOfs++];
				goto cilLeave;

			case OpCodes.CIL_LEAVE:
				u32Value = GetUnalignedU32(pCIL, &cilOfs);
cilLeave:
				// Put a temporary CIL offset value into the JITted code. This will be updated later
				u32Value = cilOfs + (int)u32Value;
				MayCopyTypeStack();
				RestoreTypeStack(&typeStack, ppTypeStacks[cilOfs]);
				PushOp(JIT_LEAVE);
				PushBranch();
				PushU32(u32Value);
				break;

			case OpCodes.CIL_ENDFINALLY:
				PushOp(JIT_END_FINALLY);
				RestoreTypeStack(&typeStack, ppTypeStacks[cilOfs]);
				break;

			case OpCodes.CIL_EXTENDED:
				op = pCIL[cilOfs++];

				switch (op)
				{
				case CILX_INITOBJ:
					{
						tMD_TypeDef *pTypeDef;

						PopStackTypeDontCare(); // Don't care what it is
						u32Value = GetUnalignedU32(pCIL, &cilOfs);
						pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
						if (pTypeDef->isValueType) {
							PushOp(JIT_INIT_VALUETYPE);
							PushPTR(pTypeDef);
						} else {
							PushOp(JIT_INIT_OBJECT);
						}
					}
					break;

				case CILX_LOADFUNCTION:
					{
						tMD_MethodDef *pFuncMethodDef;

						u32Value = GetUnalignedU32(pCIL, &cilOfs);
						pFuncMethodDef = MetaData.GetMethodDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
						PushOp(JIT_LOADFUNCTION);
						PushPTR(pFuncMethodDef);
						PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
					}
					break;

				case CILX_CEQ:
				case CILX_CGT:
				case CILX_CGT_UN:
				case CILX_CLT:
				case CILX_CLT_UN:
					pTypeB = PopStackType();
					pTypeA = PopStackType();
#if UNITY_WEBGL || DNA_32BIT
					if ((pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) ||
						(pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O) ||
						(pTypeA->stackType == EvalStack.EVALSTACK_PTR && pTypeB->stackType == EvalStack.EVALSTACK_PTR)) {
#else
                        if (pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) {
#endif
						PushOp(JIT_CEQ_I32I32 + (op - CILX_CEQ));
#if UNITY_WEBGL || DNA_32BIT
                    } else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) {
#else
                    } else if ((pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) ||
                        (pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O) ||
                        (pTypeA->stackType == EvalStack.EVALSTACK_PTR && pTypeB->stackType == EvalStack.EVALSTACK_PTR)) {
#endif
						PushOp(JIT_CEQ_I64I64 + (op - CILX_CEQ));
					} else if (pTypeA->stackType == EvalStack.EVALSTACK_F32 && pTypeB->stackType == EvalStack.EVALSTACK_F32) {
						PushOp(JIT_CEQ_F32F32 + (op - CILX_CEQ));
					} else if (pTypeA->stackType == EvalStack.EVALSTACK_F64 && pTypeB->stackType == EvalStack.EVALSTACK_F64) {
						PushOp(JIT_CEQ_F64F64 + (op - CILX_CEQ));
					} else {
						Sys.Crash("JITit(): Cannot perform comparison operand on stack Type.types: %s and %s", pTypeA->name, pTypeB->name);
					}
					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
					break;
					
				case CILX_RETHROW:
					PushOp(JIT_RETHROW);
					break;

				case CILX_CONSTRAINED:
					u32Value2 = GetUnalignedU32(pCIL, &cilOfs);
					cilOfs++;
					goto cilCallVirtConstrained;

				case CILX_READONLY:
					// Do nothing
					break;

				default:
					Sys.Crash("JITit(): JITter cannot handle extended op-code:0x%02x", op);

				}
				break;

			default:
				Sys.Crash("JITit(): JITter cannot handle op-code: 0x%02x", op);
		}

	} while (cilOfs < codeSize);

	// Apply branch offset fixes
	for (i=0; i<branchOffsets.ofs; i++) {
		uint ofs, jumpTarget;

		ofs = branchOffsets.p[i];
		jumpTarget = ops.p[ofs];
		// Rewrite the branch offset
		jumpTarget = pJITOffsets[jumpTarget];
		ops.p[ofs] = jumpTarget;
#if GEN_COMBINED_OPCODES
		isDynamic.p[jumpTarget] |= DYNAMIC_JUMP_TARGET;
#endif
	}

	// Apply expection handler offset fixes
	for (i=0; i<pJITted->numExceptionHandlers; i++) {
		tExceptionHeader *pEx;

		pEx = &pJITted->pExceptionHeaders[i];
		pEx->tryEnd = pJITOffsets[pEx->tryStart + pEx->tryEnd];
		pEx->tryStart = pJITOffsets[pEx->tryStart];
		pEx->handlerEnd = pJITOffsets[pEx->handlerStart + pEx->handlerEnd];
		pEx->handlerStart = pJITOffsets[pEx->handlerStart];
#if GEN_COMBINED_OPCODES
		isDynamic.p[pEx->tryStart] |= DYNAMIC_EX_START | DYNAMIC_JUMP_TARGET;
		isDynamic.p[pEx->tryEnd] |= DYNAMIC_EX_END | DYNAMIC_JUMP_TARGET;
		isDynamic.p[pEx->handlerStart] |= DYNAMIC_EX_START | DYNAMIC_JUMP_TARGET;
		isDynamic.p[pEx->handlerEnd] |= DYNAMIC_EX_END | DYNAMIC_JUMP_TARGET;
#endif
	}

#if GEN_COMBINED_OPCODES
	// Find any candidates for instruction combining
	if (genCombinedOpcodes) {
		uint inst0 = 0;
		while (inst0 < ops.ofs) {
			uint opCodeCount = 0;
			uint instCount = 0;
			uint shrinkOpsBy;
			uint isFirstInst;
			while (!(isDynamic.p[inst0] & DYNAMIC_OK)) {
				inst0++;
				if (inst0 >= ops.ofs) {
					goto combineDone;
				}
			}
			isFirstInst = 1;
			while (isDynamic.p[inst0 + instCount] & DYNAMIC_OK) {
				if (isFirstInst) {
					isFirstInst = 0;
				} else {
					if (isDynamic.p[inst0 + instCount] & DYNAMIC_JUMP_TARGET) {
						// Cannot span a jump target
						break;
					}
				}
				instCount += 1 + ((isDynamic.p[inst0 + instCount] & DYNAMIC_BYTE_COUNT_MASK) >> 2);
				opCodeCount++;
			}
			shrinkOpsBy = 0;
			if (opCodeCount > 1) {
				uint combinedSize;
				tCombinedOpcodesMem *pCOMem = ((tCombinedOpcodesMem*)Mem.malloc(sizeof(tCombinedOpcodesMem)));
				shrinkOpsBy = GenCombined(&ops, &isDynamic, inst0, instCount, &combinedSize, &pCOMem->pMem);
				pCOMem->pNext = pJITted->pCombinedOpcodesMem;
				pJITted->pCombinedOpcodesMem = pCOMem;
				pJITted->opsMemSize += combinedSize;
				memmove(&ops.p[inst0 + instCount - shrinkOpsBy], &ops.p[inst0 + instCount], (ops.ofs - inst0 - instCount) << 2);
				memmove(&isDynamic.p[inst0 + instCount - shrinkOpsBy], &isDynamic.p[inst0 + instCount], (ops.ofs - inst0 - instCount) << 2);
				ops.ofs -= shrinkOpsBy;
				isDynamic.ofs -= shrinkOpsBy;
				for (i=0; i<branchOffsets.ofs; i++) {
					uint ofs;
					if (branchOffsets.p[i] > inst0) {
						branchOffsets.p[i] -= shrinkOpsBy;
					}
					ofs = branchOffsets.p[i];
					if (ops.p[ofs] > inst0) {
						ops.p[ofs] -= shrinkOpsBy;
					}
				}
				for (i=0; i<pJITted->numExceptionHandlers; i++) {
					tExceptionHeader *pEx;

					pEx = &pJITted->pExceptionHeaders[i];
					if (pEx->tryStart > inst0) {
						pEx->tryStart -= shrinkOpsBy;
					}
					if (pEx->tryEnd > inst0) {
						pEx->tryEnd -= shrinkOpsBy;
					}
					if (pEx->handlerStart > inst0) {
						pEx->handlerStart -= shrinkOpsBy;
					}
					if (pEx->handlerEnd > inst0) {
						pEx->handlerEnd -= shrinkOpsBy;
					}
				}
			}
			inst0 += instCount - shrinkOpsBy;
		}
	}
combineDone:
#endif

	// Change maxStack to indicate the number of bytes needed on the evaluation stack.
	// This is the largest number of bytes needed by all objects/value-Type.types on the stack,
	pJITted->maxStack = typeStack.maxBytes;

	Mem.free(typeStack.ppTypes);

	for (i=0; i<codeSize; i++) {
		if (ppTypeStacks[i] != null) {
			Mem.free(ppTypeStacks[i]->ppTypes);
		}
	}
	Mem.free(ppTypeStacks);

	DeleteOps(branchOffsets);
	Mem.free(pJITOffsets);

	// Copy ops to some memory of exactly the correct size. To not waste memory.
	u32Value = ops.ofs * sizeof(uint);
	pFinalOps = genCombinedOpcodes?Mem.malloc(u32Value):Mem.mallocForever(u32Value);
	Mem.memcpy(pFinalOps, ops.p, u32Value);
	DeleteOps(ops);
#if GEN_COMBINED_OPCODES
	pJITted->opsMemSize += u32Value;
	DeleteOps(isDynamic);
#endif

	return pFinalOps;
}

// Prepare a method for execution
// This makes sure that the method has been JITed.
void JIT_Prepare(tMD_MethodDef *pMethodDef, uint genCombinedOpcodes) {
	tMetaData *pMetaData;
	byte *pMethodHeader;
	tJITted *pJITted;
	/*FLAGS16*/ushort flags;
	uint codeSize;
	/*IDX_TABLE*/uint localsToken;
	byte *pCIL;
	/*SIG*/byte* sig;
	uint i, sigLength, numLocals;
	tParameter *pLocals;

	Sys.log_f(2, "JIT:   %s\n", Sys_GetMethodDesc(pMethodDef));

	pMetaData = pMethodDef->pMetaData;
	pJITted = (genCombinedOpcodes)?((tJITted*)Mem.malloc(sizeof(tJITted))) : ((tJITted*)Mem.mallocForever(sizeof(tJITted)));
#if GEN_COMBINED_OPCODES
	pJITted->pCombinedOpcodesMem = null;
	pJITted->opsMemSize = 0;
	if (genCombinedOpcodes) {
		pMethodDef->pJITtedCombined = pJITted;
	} else {
		pMethodDef->pJITted = pJITted;
	}
#else
	pMethodDef->pJITted = pJITted;
#endif

	if ((pMethodDef->implFlags & METHODIMPLATTRIBUTES_INTERNALCALL) ||
		((pMethodDef->implFlags & METHODIMPLATTRIBUTES_CODETYPE_MASK) == METHODIMPLATTRIBUTES_CODETYPE_RUNTIME)) {
		tJITCallNative *pCallNative;

		// Internal call
		if (S.strcmp(pMethodDef->name, ".ctor") == 0) {
			// Internal constructor needs enough evaluation stack space to return itself
			pJITted->maxStack = pMethodDef->pParentType->stackSize;
		} else {
			pJITted->maxStack = (pMethodDef->pReturnType == null)?0:pMethodDef->pReturnType->stackSize; // For return value
		}
		pCallNative = ((tJITCallNative*)Mem.mallocForever(sizeof(tJITCallNative)));
		pCallNative->opCode = JIT_CALL_NATIVE;
		pCallNative->pMethodDef = pMethodDef;
		pCallNative->fn = InternalCall_Map(pMethodDef);
		pCallNative->retOpCode = JIT_RETURN;

		pJITted->localsStackSize = 0;
		pJITted->pOps = (uint*)pCallNative;

		return;
	}
	if (pMethodDef->flags & METHODATTRIBUTES_PINVOKEIMPL) {
		tJITCallPInvoke *pCallPInvoke;

		// PInvoke call
		tMD_ImplMap *pImplMap = MetaData.GetImplMap(pMetaData, pMethodDef->tableIndex);
		fnPInvoke fn = PInvoke_GetFunction(pMetaData, pImplMap);
		if (fn == null) {
			Sys.Crash("PInvoke library or function not found: %s()", pImplMap->importName);
		}

		pCallPInvoke = ((tJITCallPInvoke*)Mem.mallocForever(sizeof(tJITCallPInvoke)));
		pCallPInvoke->opCode = JIT_CALL_PINVOKE;
		pCallPInvoke->fn = fn;
		pCallPInvoke->pMethod = pMethodDef;
		pCallPInvoke->pImplMap = pImplMap;

		pJITted->localsStackSize = 0;
		pJITted->maxStack = (pMethodDef->pReturnType == null)?0:pMethodDef->pReturnType->stackSize; // For return value
		pJITted->pOps = (uint*)pCallPInvoke;

		return;
	}

	pMethodHeader = (byte*)pMethodDef->pCIL;
	if ((*pMethodHeader & 0x3) == CorILMethod_TinyFormat) {
		// Tiny header
		flags = *pMethodHeader & 0x3;
		pJITted->maxStack = 8;
		codeSize = (*pMethodHeader & 0xfc) >> 2;
		localsToken = 0;
		pCIL = pMethodHeader + 1;
	} else {
		// Fat header
		flags = *(ushort*)pMethodHeader & 0x0fff;
		pJITted->maxStack = *(ushort*)&pMethodHeader[2];
		codeSize = *(uint*)&pMethodHeader[4];
		localsToken = *(/*IDX_TABLE*/uint*)&pMethodHeader[8];
		pCIL = pMethodHeader + ((pMethodHeader[1] & 0xf0) >> 2);
	}
	if (flags & CorILMethod_MoreSects) {
		uint numClauses;

		pMethodHeader = pCIL + ((codeSize + 3) & (~0x3));
		if (*pMethodHeader & CorILMethod_Sect_FatFormat) {
            tExceptionHeader* pOrigExHeaders;
			uint exSize;

			// Fat header
			numClauses = ((*(uint*)pMethodHeader >> 8) - 4) / 24;
			//pJITted->pExceptionHeaders = (tExceptionHeader*)(pMethodHeader + 4);
			exSize = numClauses * sizeof(tJITExceptionHeader);
            
            // Copy ex header into slightly larger JIT ex header which has typedef ptr at end
			pJITted->pExceptionHeaders =
				(tJITExceptionHeader*)(genCombinedOpcodes?Mem.malloc(exSize):Mem.mallocForever(exSize));
            Mem.memset(pJITted->pExceptionHeaders, 0, exSize);
            pOrigExHeaders = (tExceptionHeader*)(pMethodHeader + 4);
            for (i=0; i<numClauses; i++) {
                Mem.memcpy(pJITted->pExceptionHeaders + i, pOrigExHeaders + i, sizeof(tExceptionHeader));
            }
		} else {
			// Thin header
			tJITExceptionHeader *pExHeaders;
			uint exSize;

			numClauses = (((byte*)pMethodHeader)[1] - 4) / 12;
			exSize = numClauses * sizeof(tJITExceptionHeader);
			pMethodHeader += 4;
			//pExHeaders = pJITted->pExceptionHeaders = (tExceptionHeader*)Mem.mallocForever(numClauses * sizeof(tExceptionHeader));
			pExHeaders = pJITted->pExceptionHeaders =
				(tJITExceptionHeader*)(genCombinedOpcodes?Mem.malloc(exSize):Mem.mallocForever(exSize));
            Mem.memset(pJITted->pExceptionHeaders, 0, exSize);
			for (i=0; i<numClauses; i++) {
				pExHeaders[i].flags = ((ushort*)pMethodHeader)[0];
				pExHeaders[i].tryStart = ((ushort*)pMethodHeader)[1];
				pExHeaders[i].tryEnd = ((byte*)pMethodHeader)[4];
				pExHeaders[i].handlerStart = ((byte*)pMethodHeader)[5] | (((byte*)pMethodHeader)[6] << 8);
				pExHeaders[i].handlerEnd = ((byte*)pMethodHeader)[7];
				pExHeaders[i].u.classToken = ((uint*)pMethodHeader)[2];

				pMethodHeader += 12;
			}
		}
		pJITted->numExceptionHandlers = numClauses;
		// replace all classToken's with the actual tMD_TypeDef*
		for (i=0; i<numClauses; i++) {
			if (pJITted->pExceptionHeaders[i].flags == COR_ILEXCEPTION_CLAUSE_EXCEPTION) {
				pJITted->pExceptionHeaders[i].pCatchTypeDef =
					MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, pJITted->pExceptionHeaders[i].u.classToken, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
			}
		}
	} else {
		pJITted->numExceptionHandlers = 0;
		pJITted->pExceptionHeaders = null;
	}

	// Analyse the locals
	if (localsToken == 0) {
		// No locals
		pJITted->localsStackSize = 0;
		pLocals = null;
	} else {
		tMD_StandAloneSig *pStandAloneSig;
		uint i, totalSize;

		pStandAloneSig = (tMD_StandAloneSig*)MetaData.GetTableRow(pMethodDef->pMetaData, localsToken);
		sig = MetaData.GetBlob(pStandAloneSig->signature, &sigLength);
		MetaData.DecodeSigEntry(&sig); // Always 0x07
		numLocals = MetaData.DecodeSigEntry(&sig);
		pLocals = (tParameter*)Mem.malloc(numLocals * sizeof(tParameter));
		totalSize = 0;
		for (i=0; i<numLocals; i++) {
			tMD_TypeDef *pTypeDef;

			pTypeDef = Type.GetTypeFromSig(pMethodDef->pMetaData, &sig, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
			MetaData.Fill_TypeDef(pTypeDef, null, null);
			pLocals[i].pTypeDef = pTypeDef;
			pLocals[i].offset = totalSize;
			pLocals[i].size = pTypeDef->stackSize;
			totalSize += pTypeDef->stackSize;
		}
		pJITted->localsStackSize = totalSize;
	}

	// JIT the CIL code
	pJITted->pOps = JITit(pMethodDef, pCIL, codeSize, pLocals, pJITted, genCombinedOpcodes);
    
    pJITted->maxStack += 64;
    
	Mem.free(pLocals);
}
  
#endif

    }
}
