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
        public /*fnInternalCall*/void* fn;
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

        unsafe struct tOps
        {
            public uint *p;
            public uint capacity;
            public uint ofs;
        }

        unsafe struct tTypeStack
        {
            public tMD_TypeDef **ppTypes;
            public uint ofs;
            public uint maxBytes; // The max size of the stack in bytes
        }

        static uint maxStack;
        static uint cilOfs;
        static tOps ops; // The JITted op-codes
        static tOps branchOffsets; // Filled with all the branch instructions that need offsets fixing
        static uint *pJITOffsets;  // To store the JITted code offset of each CIL byte.
        // Only CIL bytes that are the first byte of an instruction will have meaningful data
        static tTypeStack **ppTypeStacks; // To store the evaluation stack state for forward jumps
        static uint *pFinalOps;
        static tMD_TypeDef *pStackType;
        static tTypeStack typeStack;
        static uConvFloat convFloat;
        static uConvDouble convDouble;
        static tMetaData* pMetaData;

        public static void Init()
        {
            pJITOffsets = null;
            ppTypeStacks = null;
            pFinalOps = null;
            pStackType = null;
            pMetaData = null;
        }

        static void InitOps(ref tOps ops, uint initialCapacity) 
        {
            ops.capacity = initialCapacity; 
            ops.ofs = 0; 
            ops.p = (uint*)Mem.malloc((SIZE_T)((initialCapacity) * sizeof(int)));
        }

        static void DeleteOps(ref tOps ops) 
        {
            Mem.free(ops.p);
        }

        // Turn this into a MACRO at some point?
        /* static uint Translate(uint op, uint getDynamic) {
        	if (op >= JitOps.JIT_OPCODE_MAXNUM) {
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

        static void PushU32(uint v)
        {
            PushU32_(ref ops, (uint)(v));
        }

        static void PushI32(int v)
        {
            PushU32_(ref ops, (uint)(v));
        }

        static void PushFloat(float v) 
        {
            convFloat.f = (float)(v); 
            PushU32_(ref ops, convFloat.u32);
        }

        static void PushDouble(double v)
        {
            convDouble.d = (double)(v);
            PushU32_(ref ops, convDouble.u32a); 
            PushU32_(ref ops, convDouble.u32b);
        }

        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT

            static void PushPTR(void* ptr)
            {
                PushU32_(ref ops, (uint)((PTR)ptr));
            }

        #else

            static void PushPTR(void* ptr)
            {
                PushU32_(ref ops, (uint)((PTR)ptr)); 
                PushU32_(ref ops, (uint)((PTR)(ptr) >> 32));
            }

        #endif

        static void PushOp(uint op)
        {
            PushU32_(ref ops, (uint)(op));
        }

        static void PushOpParam(uint op, uint param)
        {
            PushOp(op); 
            PushU32_(ref ops, (uint)(param));
        }

        static void PushBranch()
        {
            PushU32_(ref branchOffsets, ops.ofs);
        }

        static void PushStackType(tMD_TypeDef* type)
        {
            PushStackType_(ref typeStack, type);
        }

        static tMD_TypeDef* PopStackType()
        {
            return (typeStack.ppTypes[--typeStack.ofs]);
        }

        static void PopStackTypeDontCare()
        {
            typeStack.ofs--;
        }

        static void PopStackTypeMulti(int number)
        {
            typeStack.ofs -= (uint)number;
        }

        static void PopStackTypeAll()
        {
            typeStack.ofs = 0;
        }

        static void MayCopyTypeStack(uint v)
        {
            if (v > cilOfs)
                ppTypeStacks[v] = DeepCopyTypeStack(ref typeStack);
        }

        static void PushStackType_(ref tTypeStack tstack, tMD_TypeDef *pType) 
        {
        	uint i, size;

        	MetaData.Fill_TypeDef(pType, null, null);
        	tstack.ppTypes[tstack.ofs++] = pType;
        	// Count current stack size in bytes
        	size = 0;
        	for (i=0; i<tstack.ofs; i++) {
        		size += tstack.ppTypes[i]->stackSize;
        	}
        	if (size > tstack.maxBytes) {
        		tstack.maxBytes = size;
        	}
        	//printf("Stack ofs = %d; Max stack size: %d (0x%x)\n", pTypeStack->ofs, size, size);
        }

        static void PushU32_(ref tOps ops, uint v) 
        {
        	if (ops.ofs >= ops.capacity) {
        		ops.capacity <<= 1;
        //		printf("a.pOps->p = 0x%08x size=%d\n", pOps->p, pOps->capacity * sizeof(uint));
                ops.p = (uint*)Mem.realloc(ops.p, (SIZE_T)(ops.capacity * sizeof(uint)));
        	}
        	ops.p[ops.ofs++] = v;
        }

        static uint GetUnalignedU32(byte *pCIL, ref uint pCILOfs) {
        	uint a,b,c,d;
        	a = pCIL[pCILOfs++];
        	b = pCIL[pCILOfs++];
        	c = pCIL[pCILOfs++];
        	d = pCIL[pCILOfs++];
        	return a | (b << 8) | (c << 16) | (d << 24);
        }

        static tTypeStack* DeepCopyTypeStack(ref tTypeStack toCopy) {
        	tTypeStack *pCopy;

            pCopy = ((tTypeStack*)Mem.malloc((SIZE_T)(sizeof(tTypeStack))));
        	pCopy->maxBytes = toCopy.maxBytes;
        	pCopy->ofs = toCopy.ofs;
        	if (toCopy.ofs > 0) {
                pCopy->ppTypes = (tMD_TypeDef**)Mem.malloc((SIZE_T)(toCopy.ofs * sizeof(tMD_TypeDef*)));
                Mem.memcpy(pCopy->ppTypes, toCopy.ppTypes, (SIZE_T)(toCopy.ofs * sizeof(tMD_TypeDef*)));
        	} else {
        		pCopy->ppTypes = null;
        	}
        	return pCopy;
        }

        static void RestoreTypeStack(ref tTypeStack tstack, tTypeStack *pCopyFrom) {
        	// This does not effect maxBytes, as the current value will always be equal
        	// or greater than the value being copied from.
        	if (pCopyFrom == null) {
        		tstack.ofs = 0;
        	} else {
        		tstack.ofs = pCopyFrom->ofs;
        		if (pCopyFrom->ppTypes != null) {
                    Mem.memcpy(tstack.ppTypes, pCopyFrom->ppTypes, (SIZE_T)(pCopyFrom->ofs * sizeof(tMD_TypeDef*)));
        		}
        	}
        }

        static uint* JITit(tMD_MethodDef *pMethodDef, byte *pCIL, uint codeSize, tParameter *pLocals, tJITted *pJITted, uint genCombinedOpcodes) {
        	maxStack = pJITted->maxStack;
        	uint i;

        	int i32Value;
        	uint u32Value, u32Value2, ofs;
            tMD_TypeDef* pTypeA, pTypeB;
        	byte* pMem;

            uint toType = 0;
            uint toBitCount = 0;
            uint convOpOffset = 0;

        	pMetaData = pMethodDef->pMetaData;
            pJITOffsets = (uint*)Mem.malloc((SIZE_T)(codeSize * sizeof(uint)));
        	// + 1 to handle cases where the stack is being restored at the last instruction in a method
            ppTypeStacks = (tTypeStack**)Mem.malloc((SIZE_T)((codeSize + 1) * sizeof(tTypeStack*)));
            Mem.memset(ppTypeStacks, 0, (SIZE_T)((codeSize + 1) * sizeof(tTypeStack*)));
        	typeStack.maxBytes = 0;
        	typeStack.ofs = 0;
            typeStack.ppTypes = (tMD_TypeDef**)Mem.malloc((SIZE_T)(maxStack * sizeof(tMD_TypeDef*)));

        	// Set up all exception 'catch' blocks with the correct stack information,
        	// So they'll have just the exception type on the stack when entered
        	for (i=0; i<pJITted->numExceptionHandlers; i++) {
        		tJITExceptionHeader *pEx;

        		pEx = &pJITted->pExceptionHeaders[i]; 
        		if (pEx->flags == COR_ILEXCEPTION_CLAUSE_EXCEPTION) {
        			tTypeStack *pTypeStack;

                    ppTypeStacks[pEx->handlerStart] = pTypeStack = ((tTypeStack*)Mem.malloc((SIZE_T)sizeof(tTypeStack)));
        			pTypeStack->maxBytes = 4;
        			pTypeStack->ofs = 1;
                    pTypeStack->ppTypes = (tMD_TypeDef**)Mem.malloc((SIZE_T)sizeof(tMD_TypeDef));
        			pTypeStack->ppTypes[0] = pEx->pCatchTypeDef;
        		}
        	}

        	InitOps(ref ops, 32);
        	InitOps(ref branchOffsets, 16);

        	cilOfs = 0;

        	do {
        		byte op;

        		// Set the JIT offset for this CIL opcode
        		pJITOffsets[cilOfs] = ops.ofs;

        		op = pCIL[cilOfs++];
        		//printf("Opcode: 0x%02x\n", op);

        		switch (op) {
        			case OpCodes.NOP:
        				PushOp(JitOps.JIT_NOP);
        				break;

        			case OpCodes.LDNULL:
        				PushOp(JitOps.JIT_LOAD_NULL);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_OBJECT]);
        				break;

        			case OpCodes.DUP:
        				pStackType = PopStackType();
        				PushStackType(pStackType);
        				PushStackType(pStackType);
        				switch (pStackType->stackSize) {
        				case 4:
        					PushOp(JitOps.JIT_DUP_4);
        					break;
        				case 8:
        					PushOp(JitOps.JIT_DUP_8);
        					break;
        				default:
        					PushOpParam(JitOps.JIT_DUP_GENERAL, pStackType->stackSize);
        					break;
        				}
        				break;

        			case OpCodes.POP:
        				pStackType = PopStackType();
        				if (pStackType->stackSize == 4) {
        					PushOp(JitOps.JIT_POP_4);
        				} else {
        					PushOpParam(JitOps.JIT_POP, pStackType->stackSize);
        				}
        				break;

        			case OpCodes.LDC_I4_M1:
        			case OpCodes.LDC_I4_0:
        			case OpCodes.LDC_I4_1:
        			case OpCodes.LDC_I4_2:
        			case OpCodes.LDC_I4_3:
        			case OpCodes.LDC_I4_4:
        			case OpCodes.LDC_I4_5:
        			case OpCodes.LDC_I4_6:
        			case OpCodes.LDC_I4_7:
        			case OpCodes.LDC_I4_8:
        				i32Value = (sbyte)op - (sbyte)OpCodes.LDC_I4_0;
        				goto cilLdcI4;

        			case OpCodes.LDC_I4_S:
        				i32Value = (sbyte)pCIL[cilOfs++];
        				goto cilLdcI4;

        			case OpCodes.LDC_I4:
        				i32Value = (int)GetUnalignedU32(pCIL, ref cilOfs);
        cilLdcI4:
        				if (i32Value >= -1 && i32Value <= 2) {
                            PushOp((uint)(JitOps.JIT_LOAD_I4_0 + i32Value));
        				} else {
        					PushOp(JitOps.JIT_LOAD_I32);
        					PushI32(i32Value);
        				}
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        				break;

        			case OpCodes.LDC_I8:
        				PushOp(JitOps.JIT_LOAD_I64);
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        				PushU32(u32Value);
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        				PushU32(u32Value);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
        				break;

        			case OpCodes.LDC_R4:
        				convFloat.u32 = GetUnalignedU32(pCIL, ref cilOfs);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_SINGLE]);
        				PushOp(JitOps.JIT_LOAD_F32);
        				PushFloat(convFloat.f);
        				break;

        			case OpCodes.LDC_R8:
        				convDouble.u32a = GetUnalignedU32(pCIL, ref cilOfs);
        				convDouble.u32b = GetUnalignedU32(pCIL, ref cilOfs);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_DOUBLE]);
        				PushOp(JitOps.JIT_LOAD_F64);
        				PushDouble(convDouble.d);
        				break;

        			case OpCodes.LDARG_0:
        			case OpCodes.LDARG_1:
        			case OpCodes.LDARG_2:
        			case OpCodes.LDARG_3:
                        u32Value = (uint)(op - OpCodes.LDARG_0);
        				goto cilLdArg;

        			case OpCodes.LDARG_S:
        				u32Value = pCIL[cilOfs++];
        cilLdArg:
        				pStackType = pMethodDef->pParams[u32Value].pTypeDef;
        				ofs = pMethodDef->pParams[u32Value].offset;
        				if (pStackType->stackSize == 4 && ofs < 32) {
        					PushOp(JitOps.JIT_LOADPARAMLOCAL_0 + (ofs >> 2));
        				} else {
        					PushOpParam(JitOps.JIT_LOADPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
        					// if it's a valuetype then push the TypeDef of it afterwards
        					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
        						PushPTR(pStackType);
        					}
        				}
        				PushStackType(pStackType);
        				break;

        			case OpCodes.LDARGA_S:
        				// Get the argument number to load the address of
        				u32Value = pCIL[cilOfs++];
        				PushOpParam(JitOps.JIT_LOAD_PARAMLOCAL_ADDR, pMethodDef->pParams[u32Value].offset);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
        				break;

        			case OpCodes.STARG_S:
        				// Get the argument number to store the arg of
        				u32Value = pCIL[cilOfs++];
        				pStackType = PopStackType();
        				ofs = pMethodDef->pParams[u32Value].offset;
        				if (pStackType->stackSize == 4 && ofs < 32) {
        					PushOp(JitOps.JIT_STOREPARAMLOCAL_0 + (ofs >> 2));
        				} else {
        					PushOpParam(JitOps.JIT_STOREPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
        					// if it's a valuetype then push the TypeDef of it afterwards
        					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
        						PushPTR(pStackType);
        					}
        				}
        				break;

        			case OpCodes.LDLOC_0:
        			case OpCodes.LDLOC_1:
        			case OpCodes.LDLOC_2:
        			case OpCodes.LDLOC_3:
        				// Push opcode and offset into locals memory
                        u32Value = (uint)(op - OpCodes.LDLOC_0);
        				goto cilLdLoc;

        			case OpCodes.LDLOC_S:
        				// Push opcode and offset into locals memory
        				u32Value = pCIL[cilOfs++];
        cilLdLoc:
        				pStackType = pLocals[u32Value].pTypeDef;
        				ofs = pMethodDef->parameterStackSize + pLocals[u32Value].offset;
        				if (pStackType->stackSize == 4 && ofs < 32) {
        					PushOp(JitOps.JIT_LOADPARAMLOCAL_0 + (ofs >> 2));
        				} else {
        					PushOpParam(JitOps.JIT_LOADPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
        					// if it's a valuetype then push the TypeDef of it afterwards
        					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
        						PushPTR(pStackType);
        					}
        				}
        				PushStackType(pStackType);
        				break;

        			case OpCodes.STLOC_0:
        			case OpCodes.STLOC_1:
        			case OpCodes.STLOC_2:
        			case OpCodes.STLOC_3:
                        u32Value = (uint)(op - OpCodes.STLOC_0);
        				goto cilStLoc;

        			case OpCodes.STLOC_S:
        				u32Value = pCIL[cilOfs++];
        cilStLoc:
        				pStackType = PopStackType();
        				ofs = pMethodDef->parameterStackSize + pLocals[u32Value].offset;
        				if (pStackType->stackSize == 4 && ofs < 32) {
                            PushOp(JitOps.JIT_STOREPARAMLOCAL_0 + (ofs >> 2));
        				} else {
                            PushOpParam(JitOps.JIT_STOREPARAMLOCAL_TYPEID + pStackType->stackType, ofs);
        					// if it's a valuetype then push the TypeDef of it afterwards
        					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
        						PushPTR(pStackType);
        					}
        				}
        				break;

        			case OpCodes.LDLOCA_S:
        				// Get the local number to load the address of
        				u32Value = pCIL[cilOfs++];
                        PushOpParam(JitOps.JIT_LOAD_PARAMLOCAL_ADDR, pMethodDef->parameterStackSize + pLocals[u32Value].offset);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
        				break;

        			case OpCodes.LDIND_I1:
        				u32Value = Type.TYPE_SYSTEM_SBYTE;
        				goto cilLdInd;
        			case OpCodes.LDIND_U1:
        				u32Value = Type.TYPE_SYSTEM_BYTE;
        				goto cilLdInd;
        			case OpCodes.LDIND_I2:
        				u32Value = Type.TYPE_SYSTEM_INT16;
        				goto cilLdInd;
        			case OpCodes.LDIND_U2:
        				u32Value = Type.TYPE_SYSTEM_UINT16;
        				goto cilLdInd;
        			case OpCodes.LDIND_I4:
        				u32Value = Type.TYPE_SYSTEM_INT32;
        				goto cilLdInd;
        			case OpCodes.LDIND_U4:
        				u32Value = Type.TYPE_SYSTEM_UINT32;
        				goto cilLdInd;
        			case OpCodes.LDIND_I8:
        				u32Value = Type.TYPE_SYSTEM_INT64;
        				goto cilLdInd;
        			case OpCodes.LDIND_R4:
        				u32Value = Type.TYPE_SYSTEM_SINGLE;
        				goto cilLdInd;
        			case OpCodes.LDIND_R8:
        				u32Value = Type.TYPE_SYSTEM_DOUBLE;
        				goto cilLdInd;
        			case OpCodes.LDIND_REF:
        				u32Value = Type.TYPE_SYSTEM_OBJECT;
        				goto cilLdInd;
        			case OpCodes.LDIND_I:
        				u32Value = Type.TYPE_SYSTEM_INTPTR;
        cilLdInd:
        				PopStackTypeDontCare(); // don't care what it is
                        PushOp((uint)(JitOps.JIT_LOADINDIRECT_I8 + (op - OpCodes.LDIND_I1)));
        				PushStackType(Type.types[u32Value]);
        				break;

        			case OpCodes.STIND_REF:
        			case OpCodes.STIND_I1:
        			case OpCodes.STIND_I2:
        			case OpCodes.STIND_I4:
        				PopStackTypeMulti(2); // Don't care what they are
                        PushOp((uint)(JitOps.JIT_STOREINDIRECT_REF + (op - OpCodes.STIND_REF)));
        				break;

        			case OpCodes.RET:
                        PushOp(JitOps.JIT_RETURN);
       				    RestoreTypeStack(ref typeStack, ppTypeStacks[cilOfs]);
        				break;

        			case OpCodes.CALL:
        			case OpCodes.CALLVIRT:
                        u32Value2 = 0;
cilCallVirtConstrained:
        				{
        					tMD_MethodDef *pCallMethod;
        					tMD_TypeDef *pBoxCallType;
        					uint derefRefType;
        					byte dynamicallyBoxReturnValue = 0;
        					pBoxCallType = null;
        					derefRefType = 0;

        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
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
                                    System.Diagnostics.Debug.Assert(u32Value2 != 0xffffffff);
        							if (pConstrainedType->pVTable[u32Value2]->pParentType == pConstrainedType) {
        								// This method is implemented on this class, so make it a normal CALL op
        								op = OpCodes.CALL;
        								pCallMethod = pConstrainedType->pVTable[u32Value2];
        							}
        						} else {
        							if (pConstrainedType->isValueType != 0) {
        								tMD_MethodDef *pImplMethod;
        								// If pConstraintedType directly implements the call then don't do anything
        								// otherwise the 'this' pointer must be boxed (BoxedCall)
        								pImplMethod = pConstrainedType->pVTable[pCallMethod->vTableOfs];
        								if (pImplMethod->pParentType == pConstrainedType) {
        									op = OpCodes.CALL;
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
        					if (MetaData.METHOD_ISSTATIC(pCallMethod)) {
        						pStackType = Type.types[Type.TYPE_SYSTEM_OBJECT];
        					}
        					MetaData.Fill_TypeDef(pStackType, null, null);
        					if (MetaData.TYPE_ISINTERFACE(pCallMethod->pParentType) && op == OpCodes.CALLVIRT) {
                                PushOp(JitOps.JIT_CALL_INTERFACE);
        					} else if (pCallMethod->pParentType->pParent == Type.types[Type.TYPE_SYSTEM_MULTICASTDELEGATE]) {
                                PushOp(JitOps.JIT_INVOKE_DELEGATE);
                            } else if (pCallMethod->pParentType == Type.types[Type.TYPE_SYSTEM_REFLECTION_METHODBASE] && S.strcmp(pCallMethod->name, "Invoke") == 0) {
                                PushOp(JitOps.JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE);
        						dynamicallyBoxReturnValue = 1;
        					} else {
        						switch (pStackType->stackType)
        						{
        						case EvalStack.EVALSTACK_INTNATIVE: // Not really right, but it'll work on 32-bit
        						case EvalStack.EVALSTACK_O:
        							if (derefRefType != 0) {
                                        PushOp(JitOps.JIT_DEREF_CALLVIRT);
        							} else {
        								if (pBoxCallType != null) {
                                            PushOp(JitOps.JIT_BOX_CALLVIRT);
        									PushPTR(pBoxCallType);
        								} else {
                                            PushOp((op == OpCodes.CALL)?JitOps.JIT_CALL_O:JitOps.JIT_CALLVIRT_O);
        								}
        							}
        							break;
        						case EvalStack.EVALSTACK_PTR:
        						case EvalStack.EVALSTACK_VALUETYPE:
        							if (derefRefType != 0) {
        								PushOp(JitOps.JIT_DEREF_CALLVIRT);
        							} else if (pBoxCallType != null) {
        								PushOp(JitOps.JIT_BOX_CALLVIRT);
        								PushPTR(pBoxCallType);
        							} else {
        								PushOp(JitOps.JIT_CALL_PTR);
        							}
        							break;
        						default:
        							Sys.Crash("JITit(): Cannot CALL or CALLVIRT with stack type: %d", pStackType->stackType);
                                    break;
        						}
        					}
        					PushPTR(pCallMethod);

        					if (pCallMethod->pReturnType != null) {
        						PushStackType(pCallMethod->pReturnType);
        					}

        					if (dynamicallyBoxReturnValue != 0) {
        						PushOp(JitOps.JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE);
        					}
        				}
        				break;

        			case OpCodes.BR_S: // unconditional branch
                        u32Value = (uint)((sbyte)pCIL[cilOfs++]);
        				goto cilBr;

        			case OpCodes.BR:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        cilBr:
        				// Put a temporary CIL offset value into the JITted code. This will be updated later
                        u32Value = (uint)(cilOfs + (int)u32Value);
        				MayCopyTypeStack(u32Value);
        				PushOp(JitOps.JIT_BRANCH);
        				PushBranch();
        				PushU32(u32Value);
        				// Restore the stack state
        				RestoreTypeStack(ref typeStack, ppTypeStacks[cilOfs]);
        				break;

        			case OpCodes.SWITCH:
        				// This is the int containing the switch value. Don't care what it is.
        				PopStackTypeDontCare();
        				// The number of switch jump targets
        				i32Value = (int)GetUnalignedU32(pCIL, ref cilOfs);
        				// Set up the offset from which the jump offsets are calculated
                        u32Value2 = (uint)(cilOfs + (i32Value << 2));
                        PushOpParam(JitOps.JIT_SWITCH, (uint)i32Value);
        				for (i=0; i<(uint)i32Value; i++) {
        					// A jump target
                            u32Value = (uint)(u32Value2 + (int)GetUnalignedU32(pCIL, ref cilOfs));
        					PushBranch();
        					MayCopyTypeStack(u32Value);
        					// Push the jump target.
        					// It is needed to allow the branch offset to be correctly updated later.
        					PushU32(u32Value);
        				}
        				break;

        			case OpCodes.BRFALSE_S:
        			case OpCodes.BRTRUE_S:
                        u32Value = (uint)((sbyte)pCIL[cilOfs++]);
                        u32Value2 = (uint)(JitOps.JIT_BRANCH_FALSE_U32 + (op - OpCodes.BRFALSE_S));
        				goto cilBrFalseTrue;

        			case OpCodes.BRFALSE:
        			case OpCodes.BRTRUE:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
                        u32Value2 = (uint)(JitOps.JIT_BRANCH_FALSE_U32 + (op - OpCodes.BRFALSE));
        cilBrFalseTrue:
        				pTypeA = PopStackType();
                        if (pTypeA->stackSize == 8)
                            u32Value2 += 2;
        				// Put a temporary CIL offset value into the JITted code. This will be updated later
                        u32Value = (uint)(cilOfs + (int)u32Value);
        				MayCopyTypeStack(u32Value);
        				PushOp(u32Value2);
        				PushBranch();
        				PushU32(u32Value);
        				break;

        			case OpCodes.BEQ_S:
        			case OpCodes.BGE_S:
        			case OpCodes.BGT_S:
        			case OpCodes.BLE_S:
        			case OpCodes.BLT_S:
        			case OpCodes.BNE_UN_S:
        			case OpCodes.BGE_UN_S:
        			case OpCodes.BGT_UN_S:
        			case OpCodes.BLE_UN_S:
        			case OpCodes.BLT_UN_S:
                        u32Value = (uint)((sbyte)pCIL[cilOfs++]);
        				u32Value2 = OpCodes.BEQ_S;
        				goto cilBrCond;

        			case OpCodes.BEQ:
        			case OpCodes.BGE:
        			case OpCodes.BGT:
        			case OpCodes.BLE:
        			case OpCodes.BLT:
        			case OpCodes.BNE_UN:
        			case OpCodes.BGE_UN:
        			case OpCodes.BGT_UN:
        			case OpCodes.BLE_UN:
        			case OpCodes.BLT_UN:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        				u32Value2 = OpCodes.BEQ;
        cilBrCond:
        				pTypeB = PopStackType();
        				pTypeA = PopStackType();
                        u32Value = (uint)(cilOfs + (int)u32Value);
        				MayCopyTypeStack(u32Value);
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        				if ((pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) ||
        					(pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O)) {
        #else
                        if (pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) {
        #endif
        					PushOp(JitOps.JIT_BEQ_I32I32 + (op - u32Value2));
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) {
        #else
                        } else if ((pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) ||
                                   (pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O)) {
        #endif
        					PushOp(JitOps.JIT_BEQ_I64I64 + (op - u32Value2));
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F32 && pTypeB->stackType == EvalStack.EVALSTACK_F32) {
        					PushOp(JitOps.JIT_BEQ_F32F32 + (op - u32Value2));
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F64 && pTypeB->stackType == EvalStack.EVALSTACK_F64) {
        					PushOp(JitOps.JIT_BEQ_F64F64 + (op - u32Value2));
        				} else {
        					Sys.Crash("JITit(): Cannot perform conditional branch on stack Type.types: %d and %d", pTypeA->stackType, pTypeB->stackType);
        				}
        				PushBranch();
        				PushU32(u32Value);
        				break;

        			case OpCodes.ADD_OVF:
        			case OpCodes.ADD_OVF_UN:
        			case OpCodes.MUL_OVF:
        			case OpCodes.MUL_OVF_UN:
        			case OpCodes.SUB_OVF:
        			case OpCodes.SUB_OVF_UN:
        				u32Value = (OpCodes.ADD_OVF - OpCodes.ADD) + (JitOps.JIT_ADD_I32I32 - JitOps.JIT_ADD_OVF_I32I32);
        				goto cilBinaryArithOp;
        			case OpCodes.ADD:
        			case OpCodes.SUB:
        			case OpCodes.MUL:
        			case OpCodes.DIV:
        			case OpCodes.DIV_UN:
        			case OpCodes.REM:
        			case OpCodes.REM_UN:
        			case OpCodes.AND:
        			case OpCodes.OR:
        			case OpCodes.XOR:
        				u32Value = 0;
        cilBinaryArithOp:
        				pTypeB = PopStackType();
        				pTypeA = PopStackType();
        				if (pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) {
                            PushOp((uint)(JitOps.JIT_ADD_I32I32 + (op - OpCodes.ADD) - u32Value));
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) {
                            PushOp((uint)(JitOps.JIT_ADD_I64I64 + (op - OpCodes.ADD) - u32Value));
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F32 && pTypeB->stackType == EvalStack.EVALSTACK_F32) {
                            PushOp((uint)(JitOps.JIT_ADD_F32F32 + (op - OpCodes.ADD) - u32Value));
        					PushStackType(pTypeA);
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_F64 && pTypeB->stackType == EvalStack.EVALSTACK_F64) {
                            PushOp((uint)(JitOps.JIT_ADD_F64F64 + (op - OpCodes.ADD) - u32Value));
        					PushStackType(pTypeA);
        				} else {
        					Sys.Crash("JITit(): Cannot perform binary numeric operand on stack Type.types: %d and %d", pTypeA->stackType, pTypeB->stackType);
        				}
        				break;

        			case OpCodes.NEG:
        			case OpCodes.NOT:
        				pTypeA = PopStackType();
        				if (pTypeA->stackType == EvalStack.EVALSTACK_INT32) {
                            PushOp((uint)(JitOps.JIT_NEG_I32 + (op - OpCodes.NEG)));
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64) {
                            PushOp((uint)(JitOps.JIT_NEG_I64 + (op - OpCodes.NEG)));
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
        				} else {
        					Sys.Crash("JITit(): Cannot perform unary operand on stack Type.types: %d", pTypeA->stackType);
        				}
        				break;

        			case OpCodes.SHL:
        			case OpCodes.SHR:
        			case OpCodes.SHR_UN:
        				PopStackTypeDontCare(); // Don't care about the shift amount
        				pTypeA = PopStackType(); // Do care about the value to shift
        				if (pTypeA->stackType == EvalStack.EVALSTACK_INT32) {
        					PushOp(JitOps.JIT_SHL_I32 - OpCodes.SHL + op);
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        				} else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64) {
        					PushOp(JitOps.JIT_SHL_I64 - OpCodes.SHL + op);
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
        				} else {
                            Sys.Crash("JITit(): Cannot perform shift operation on type: %s", (PTR)pTypeA->name);
        				}
        				break;

        				// Conversion operations

        			case OpCodes.CONV_I1:
        			case OpCodes.CONV_OVF_I1: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_I1_UN: // Fix this later - will never overflow
        				toBitCount = 8;
        				toType = Type.TYPE_SYSTEM_SBYTE;
        				goto cilConvInt32;
        			case OpCodes.CONV_I2:
        			case OpCodes.CONV_OVF_I2: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_I2_UN: // Fix this later - will never overflow
        				toBitCount = 16;
        				toType = Type.TYPE_SYSTEM_INT16;
        				goto cilConvInt32;
        			case OpCodes.CONV_I4:
        			case OpCodes.CONV_OVF_I4: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_I4_UN: // Fix this later - will never overflow
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        			case OpCodes.CONV_I: // Only on 32-bit
        			case OpCodes.CONV_OVF_I_UN: // Only on 32-bit; Fix this later - will never overflow
        #endif
        				toBitCount = 32;
        				toType = Type.TYPE_SYSTEM_INT32;
        cilConvInt32:
        				convOpOffset = JitOps.JIT_CONV_OFFSET_I32;
        				goto cilConv;
        			case OpCodes.CONV_U1:
        			case OpCodes.CONV_OVF_U1: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_U1_UN: // Fix this later - will never overflow
        				toBitCount = 8;
        				toType = Type.TYPE_SYSTEM_BYTE;
        				goto cilConvUInt32;
        			case OpCodes.CONV_U2:
        			case OpCodes.CONV_OVF_U2: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_U2_UN: // Fix this later - will never overflow
        				toBitCount = 16;
        				toType = Type.TYPE_SYSTEM_UINT16;
        				goto cilConvUInt32;
        			case OpCodes.CONV_U4:
        			case OpCodes.CONV_OVF_U4: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_U4_UN: // Fix this later - will never overflow
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        			case OpCodes.CONV_U:
        			case OpCodes.CONV_OVF_U_UN:
        #endif
        				toBitCount = 32;
        				toType = Type.TYPE_SYSTEM_UINT32;
        cilConvUInt32:
        				convOpOffset = JitOps.JIT_CONV_OFFSET_U32;
        				goto cilConv;
                            
        			case OpCodes.CONV_I8:
        			case OpCodes.CONV_OVF_I8: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_I8_UN: // Fix this later - will never overflow
        #if !(UNITY_WEBGL || DNA_32BIT)
                    case OpCodes.CONV_I:
                    case OpCodes.CONV_OVF_I_UN:
        #endif
        				toType = Type.TYPE_SYSTEM_INT64;
        				convOpOffset = JitOps.JIT_CONV_OFFSET_I64;
        				goto cilConv;
        			case OpCodes.CONV_U8:
        			case OpCodes.CONV_OVF_U8: // Fix this later - will never overflow
        			case OpCodes.CONV_OVF_U8_UN: // Fix this later - will never overflow
        #if !(UNITY_WEBGL || DNA_32BIT)
                    case OpCodes.CONV_U:
                    case OpCodes.CONV_OVF_U_UN:
        #endif
        				toType = Type.TYPE_SYSTEM_UINT64;
        				convOpOffset = JitOps.JIT_CONV_OFFSET_U64;
        				goto cilConv;
        			case OpCodes.CONV_R4:
        				toType = Type.TYPE_SYSTEM_SINGLE;
        				convOpOffset = JitOps.JIT_CONV_OFFSET_R32;
        				goto cilConv;
        			case OpCodes.CONV_R8:
        			case OpCodes.CONV_R_UN:
        				toType = Type.TYPE_SYSTEM_DOUBLE;
        				convOpOffset = JitOps.JIT_CONV_OFFSET_R64;
        				goto cilConv;
        cilConv:
        				pStackType = PopStackType();
        				{
        					uint opCodeBase;
                            uint useParam = 0;
                            uint param = 0;
        					// This is the type that the conversion is from.
        					switch (pStackType->stackType) {
        					case EvalStack.EVALSTACK_INT64:
        						opCodeBase = (pStackType == Type.types[Type.TYPE_SYSTEM_INT64])?JitOps.JIT_CONV_FROM_I64:JitOps.JIT_CONV_FROM_U64;
        						break;
        					case EvalStack.EVALSTACK_INT32:
                                opCodeBase =
                                    (pStackType == Type.types[Type.TYPE_SYSTEM_BYTE] ||
                                     pStackType == Type.types[Type.TYPE_SYSTEM_UINT16] ||
                                     pStackType == Type.types[Type.TYPE_SYSTEM_UINT32])
                                        ?JitOps.JIT_CONV_FROM_U32:JitOps.JIT_CONV_FROM_I32;
                                    break;
        					case EvalStack.EVALSTACK_PTR:
        						opCodeBase =
        							(pStackType == Type.types[Type.TYPE_SYSTEM_UINTPTR])
#if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                                        ?JitOps.JIT_CONV_FROM_U32:JitOps.JIT_CONV_FROM_I32;
#else
                                        ?JitOps.JIT_CONV_FROM_U64:JitOps.JIT_CONV_FROM_I64;
#endif
        						break;
        					case EvalStack.EVALSTACK_F64:
        						opCodeBase = JitOps.JIT_CONV_FROM_R64;
        						break;
        					case EvalStack.EVALSTACK_F32:
        						opCodeBase = JitOps.JIT_CONV_FROM_R32;
        						break;
        					default:
                                opCodeBase = 0;
        						Sys.Crash("JITit() Conv cannot handle stack type %d", pStackType->stackType);
                                break;
        					}
        					// This is the type that the conversion is to.
        					switch (convOpOffset) {
        					case JitOps.JIT_CONV_OFFSET_I32:
        						useParam = 1;
        						param = 32 - toBitCount;
        						break;
        					case JitOps.JIT_CONV_OFFSET_U32:
        						useParam = 1;
        						// Next line is really (1 << toBitCount) - 1
        						// But it's done like this to work when toBitCount == 32
                                param = (uint)((((1 << ((int)toBitCount - 1)) - 1) << 1) + 1);
        						break;
        					case JitOps.JIT_CONV_OFFSET_I64:
        					case JitOps.JIT_CONV_OFFSET_U64:
        					case JitOps.JIT_CONV_OFFSET_R32:
        					case JitOps.JIT_CONV_OFFSET_R64:
        						break;
        					default:
        						Sys.Crash("JITit() Conv cannot handle convOpOffset %d", convOpOffset);
                                break;
        					}
        					PushOp(opCodeBase + convOpOffset);
        					if (useParam != 0) {
        						PushU32(param);
        					}
        				}
        				PushStackType(Type.types[toType]);
        				break;

        			case OpCodes.LDOBJ:
        				{
        					tMD_TypeDef *pTypeDef;

        					PopStackTypeDontCare(); // Don't care what this is
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PushOp(JitOps.JIT_LOADOBJECT);
        					PushPTR(pTypeDef);
        					PushStackType(pTypeDef);
        				}
        				break;

        			case OpCodes.STOBJ:
        				{
        					tMD_TypeDef *pTypeDef;

        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PopStackTypeMulti(2);
        					if (pTypeDef->isValueType != 0 && pTypeDef->arrayElementSize != 4) {
        						// If it's a value-type then do this
        						PushOpParam(JitOps.JIT_STORE_OBJECT_VALUETYPE, pTypeDef->arrayElementSize);
        					} else {
        						// If it's a ref type, or a value-type with size 4, then can do this instead
        						// (it executes faster)
        						PushOp(JitOps.JIT_STOREINDIRECT_REF);
        					}
        					break;
        				}

        			case OpCodes.LDSTR:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs) & 0x00ffffffU;
        				PushOpParam(JitOps.JIT_LOAD_STRING, u32Value);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_STRING]);
        				break;

        			case OpCodes.NEWOBJ:
        				{
        					tMD_MethodDef *pConstructorDef;

        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pConstructorDef = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					if (pConstructorDef->isFilled == 0) {
        						tMD_TypeDef *pTypeDef;

        						pTypeDef = MetaData.GetTypeDefFromMethodDef(pConstructorDef);
        						MetaData.Fill_TypeDef(pTypeDef, null, null);
        					}
        					if (pConstructorDef->pParentType->isValueType != 0) {
        						PushOp(JitOps.JIT_NEWOBJECT_VALUETYPE);
        					} else {
        						PushOp(JitOps.JIT_NEWOBJECT);
        					}
        					// -1 because the param count includes the 'this' parameter that is sent to the constructor
        					PopStackTypeMulti(pConstructorDef->numberOfParameters - 1);
        					PushPTR(pConstructorDef);
        					PushStackType(pConstructorDef->pParentType);
        				}
        				break;

        			case OpCodes.CASTCLASS:
        				{
        					tMD_TypeDef *pCastToType;

        					PushOp(JitOps.JIT_CAST_CLASS);
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pCastToType = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PushPTR(pCastToType);
        				}
        				break;

        			case OpCodes.ISINST:
        				{
        					tMD_TypeDef *pIsInstanceOfType;

        					PushOp(JitOps.JIT_IS_INSTANCE);
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pIsInstanceOfType = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PushPTR(pIsInstanceOfType);
        				}
        				break;

        			case OpCodes.NEWARR:
        				{
        					tMD_TypeDef *pTypeDef;

        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PopStackTypeDontCare(); // Don't care what it is
        					PushOp(JitOps.JIT_NEW_VECTOR);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        					pTypeDef = Type.GetArrayTypeDef(pTypeDef, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PushPTR(pTypeDef);
        					PushStackType(pTypeDef);
        				}
        				break;

        			case OpCodes.LDLEN:
        				PopStackTypeDontCare(); // Don't care what it is
        				PushOp(JitOps.JIT_LOAD_VECTOR_LEN);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        				break;

        			case OpCodes.LDELEM_I1:
        			case OpCodes.LDELEM_U1:
        			case OpCodes.LDELEM_I2:
        			case OpCodes.LDELEM_U2:
        			case OpCodes.LDELEM_I4:
        			case OpCodes.LDELEM_U4:
        				PopStackTypeMulti(2); // Don't care what any of these are
                        PushOp((uint)(JitOps.JIT_LOAD_ELEMENT_I8 + (op - OpCodes.LDELEM_I1)));
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        				break;

        			case OpCodes.LDELEM_I8:
        				PopStackTypeMulti(2); // Don't care what any of these are
        				PushOp(JitOps.JIT_LOAD_ELEMENT_I64);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INT64]);
        				break;

        			case OpCodes.LDELEM_R4:
        				PopStackTypeMulti(2); // Don't care what any of these are
        				PushOp(JitOps.JIT_LOAD_ELEMENT_R32);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_SINGLE]);
        				break;

        			case OpCodes.LDELEM_R8:
        				PopStackTypeMulti(2); // Don't care what any of these are
        				PushOp(JitOps.JIT_LOAD_ELEMENT_R64);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_DOUBLE]);
        				break;

        			case OpCodes.LDELEM_REF:
        				PopStackTypeMulti(2); // Don't care what any of these are
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        				PushOp(JitOps.JIT_LOAD_ELEMENT_U32);
        #else
                        PushOp(JitOps.JIT_LOAD_ELEMENT_I64);
        #endif
        				PushStackType(Type.types[Type.TYPE_SYSTEM_OBJECT]);
        				break;

        			case OpCodes.LDELEM_ANY:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        				pStackType = (tMD_TypeDef*)MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        				PopStackTypeMulti(2); // Don't care what these are
        				PushOpParam(JitOps.JIT_LOAD_ELEMENT, pStackType->stackSize);
        				PushStackType(pStackType);
        				break;

        			case OpCodes.LDELEMA:
        				PopStackTypeMulti(2); // Don't care what any of these are
        				GetUnalignedU32(pCIL, ref cilOfs); // Don't care what this is
        				PushOp(JitOps.JIT_LOAD_ELEMENT_ADDR);
        				PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
        				break;

        			case OpCodes.STELEM_I1:
        			case OpCodes.STELEM_I2:
        			case OpCodes.STELEM_I4:
        			case OpCodes.STELEM_R4:
        				PopStackTypeMulti(3); // Don't care what any of these are
        				PushOp(JitOps.JIT_STORE_ELEMENT_32);
        				break;

        			case OpCodes.STELEM_I8:
        			case OpCodes.STELEM_R8:
        				PopStackTypeMulti(3); // Don't care what any of these are
        				PushOp(JitOps.JIT_STORE_ELEMENT_64);
        				break;

                    case OpCodes.STELEM_REF:
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                        PopStackTypeMulti(3); // Don't care what any of these are
                        PushOp(JitOps.JIT_STORE_ELEMENT_32);
        #else
                        PopStackTypeMulti(3); // Don't care what any of these are
                        PushOp(JitOps.JIT_STORE_ELEMENT_64);
        #endif
                        break;
                        
        			case OpCodes.STELEM_ANY:
        				GetUnalignedU32(pCIL, ref cilOfs); // Don't need this token, as the type stack will contain the same type
        				pStackType = PopStackType(); // This is the type to store
        				PopStackTypeMulti(2); // Don't care what these are
        				PushOpParam(JitOps.JIT_STORE_ELEMENT, pStackType->stackSize);
        				break;

        			case OpCodes.STFLD:
        				{
        					tMD_FieldDef *pFieldDef;

        					// Get the stack type of the value to store
        					pStackType = PopStackType();
        					PushOp(JitOps.JIT_STOREFIELD_TYPEID + pStackType->stackType);
        					// Get the FieldRef or FieldDef of the field to store
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					PushPTR(pFieldDef);
        					// Pop the object/valuetype on which to store the field. Don't care what it is
        					PopStackTypeDontCare();
        				}
        				break;

        			case OpCodes.LDFLD:
        				{
        					tMD_FieldDef *pFieldDef;

        					// Get the FieldRef or FieldDef of the field to load
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					// Pop the object/valuetype on which to load the field.
        					pStackType = PopStackType();
        					if (pStackType->stackType == EvalStack.EVALSTACK_VALUETYPE) {
        						PushOpParam(JitOps.JIT_LOADFIELD_VALUETYPE, pStackType->stackSize);
        						PushPTR(pFieldDef);
        					} else {
        						if (pFieldDef->memSize <= 4) {
        							PushOp(JitOps.JIT_LOADFIELD_4);
        							PushU32(pFieldDef->memOffset);
                                } else if (pFieldDef->memSize == 8) {
                                    PushOp(JitOps.JIT_LOADFIELD_8);
                                    PushU32(pFieldDef->memOffset);
                                } else {
        							PushOp(JitOps.JIT_LOADFIELD);
        							PushPTR(pFieldDef);
        						}
        					}
        					// Push the stack type of the just-read field
        					PushStackType(pFieldDef->pType);
        				}
        				break;

        			case OpCodes.LDFLDA:
        				{
        					tMD_FieldDef *pFieldDef;
        					tMD_TypeDef *pTypeDef;

        					// Get the FieldRef or FieldDef of the field to load
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					// Sometimes, the type def will not have been filled, so ensure it's filled.
        					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        					PopStackTypeDontCare(); // Don't care what it is
        					PushOpParam(JitOps.JIT_LOAD_FIELD_ADDR, pFieldDef->memOffset);
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
        				}
        				break;

        			case OpCodes.STSFLD: // Store static field
        				{
        					tMD_FieldDef *pFieldDef;
        					tMD_TypeDef *pTypeDef;

        					// Get the FieldRef or FieldDef of the static field to store
        					PopStackTypeDontCare(); // Don't care what it is
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					// Sometimes, the type def will not have been filled, so ensure it's filled.
        					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        					pStackType = pFieldDef->pType;
        					PushOp(JitOps.JIT_STORESTATICFIELD_TYPEID + pStackType->stackType);
        					PushPTR(pFieldDef);
        				}
        				break;

        			case OpCodes.LDSFLD: // Load static field
        				{
        					tMD_FieldDef *pFieldDef;
        					tMD_TypeDef *pTypeDef;

        					// Get the FieldRef or FieldDef of the static field to load
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					// Sometimes, the type def will not have been filled, so ensure it's filled.
        					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        					pStackType = pFieldDef->pType;
        					PushOp(JitOps.JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + pStackType->stackType);
        					PushPTR(pFieldDef);
        					PushStackType(pStackType);
        				}
        				break;

        			case OpCodes.LDSFLDA: // Load static field address
        				{
        					tMD_FieldDef *pFieldDef;
        					tMD_TypeDef *pTypeDef;

        					// Get the FieldRef or FieldDef of the field to load
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					// Sometimes, the type def will not have been filled, so ensure it's filled.
        					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        					PushOp(JitOps.JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT);
        					PushPTR(pFieldDef);
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
        				}
        				break;

        			case OpCodes.BOX:
        				{
        					tMD_TypeDef *pTypeDef;

        					pStackType = PopStackType();
        					// Get the TypeDef(or Ref) token of the valuetype to box
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        					if (pTypeDef->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE]) {
        						// This is a nullable type, so special boxing code is needed.
        						PushOp(JitOps.JIT_BOX_NULLABLE);
        						// Push the underlying type of the nullable type, not the nullable type itself
        						PushPTR(pTypeDef->ppClassTypeArgs[0]);
        					} else {
        						PushOp(JitOps.JIT_BOX_TYPEID + pStackType->stackType);
        						PushPTR(pTypeDef);
        					}
        					// This is correct - cannot push underlying type, as then references are treated as value-Type.types
        					PushStackType(Type.types[Type.TYPE_SYSTEM_OBJECT]);
        				}
        				break;

        			case OpCodes.UNBOX_ANY:
        				{
        					tMD_TypeDef *pTypeDef;

        					PopStackTypeDontCare(); // Don't care what it is
        					u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        					pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        					if (pTypeDef->pGenericDefinition == Type.types[Type.TYPE_SYSTEM_NULLABLE]) {
        						// This is a nullable type, so special unboxing is required.
        						PushOp(JitOps.JIT_UNBOX_NULLABLE);
        						// For nullable Type.types, push the underlying type
        						PushPTR(pTypeDef->ppClassTypeArgs[0]);
        					} else if (pTypeDef->isValueType != 0) {
        						PushOp(JitOps.JIT_UNBOX2VALUETYPE);
        					} else {
        						PushOp(JitOps.JIT_UNBOX2OBJECT);
        					}
        					PushStackType(pTypeDef);
        				}
        				break;

        			case OpCodes.LDTOKEN:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        				pMem = MetaData.GetTypeMethodField(pMethodDef->pMetaData, u32Value, &u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        				PushOp(JitOps.JIT_LOADTOKEN_BASE + u32Value);
        				PushPTR(pMem);
        				PushStackType(Type.types[
        					(u32Value==0)?Type.TYPE_SYSTEM_RUNTIMETYPEHANDLE:
        						((u32Value==1)?Type.TYPE_SYSTEM_RUNTIMEFIELDHANDLE:Type.TYPE_SYSTEM_RUNTIMEMETHODHANDLE)
        				]);
        				break;

        			case OpCodes.THROW:
        				PopStackTypeDontCare(); // Don't care what it is
        				PushOp(JitOps.JIT_THROW);
        				RestoreTypeStack(ref typeStack, ppTypeStacks[cilOfs]);
        				break;

        			case OpCodes.LEAVE_S:
                        u32Value = (uint)((sbyte)pCIL[cilOfs++]);
        				goto cilLeave;

        			case OpCodes.LEAVE:
        				u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        cilLeave:
        				// Put a temporary CIL offset value into the JITted code. This will be updated later
                        u32Value = (uint)(cilOfs + (int)u32Value);
        				MayCopyTypeStack(u32Value);
        				RestoreTypeStack(ref typeStack, ppTypeStacks[cilOfs]);
        				PushOp(JitOps.JIT_LEAVE);
        				PushBranch();
        				PushU32(u32Value);
        				break;

        			case OpCodes.ENDFINALLY:
        				PushOp(JitOps.JIT_END_FINALLY);
        				RestoreTypeStack(ref typeStack, ppTypeStacks[cilOfs]);
        				break;

        			case OpCodes.EXTENDED:
        				op = pCIL[cilOfs++];

        				switch (op)
        				{
        				case OpCodes.X_INITOBJ:
        					{
        						tMD_TypeDef *pTypeDef;

        						PopStackTypeDontCare(); // Don't care what it is
        						u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        						pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        						if (pTypeDef->isValueType != 0) {
        							PushOp(JitOps.JIT_INIT_VALUETYPE);
        							PushPTR(pTypeDef);
        						} else {
        							PushOp(JitOps.JIT_INIT_OBJECT);
        						}
        					}
        					break;

        				case OpCodes.X_LOADFUNCTION:
        					{
        						tMD_MethodDef *pFuncMethodDef;

        						u32Value = GetUnalignedU32(pCIL, ref cilOfs);
        						pFuncMethodDef = MetaData.GetMethodDefFromDefRefOrSpec(pMethodDef->pMetaData, u32Value, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        						PushOp(JitOps.JIT_LOADFUNCTION);
        						PushPTR(pFuncMethodDef);
        						PushStackType(Type.types[Type.TYPE_SYSTEM_INTPTR]);
        					}
        					break;

        				case OpCodes.X_CEQ:
        				case OpCodes.X_CGT:
        				case OpCodes.X_CGT_UN:
        				case OpCodes.X_CLT:
        				case OpCodes.X_CLT_UN:
        					pTypeB = PopStackType();
        					pTypeA = PopStackType();
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        					if ((pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) ||
        						(pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O) ||
        						(pTypeA->stackType == EvalStack.EVALSTACK_PTR && pTypeB->stackType == EvalStack.EVALSTACK_PTR)) {
        #else
                                if (pTypeA->stackType == EvalStack.EVALSTACK_INT32 && pTypeB->stackType == EvalStack.EVALSTACK_INT32) {
        #endif
                                PushOp((uint)(JitOps.JIT_CEQ_I32I32 + (op - OpCodes.X_CEQ)));
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
                            } else if (pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) {
        #else
                            } else if ((pTypeA->stackType == EvalStack.EVALSTACK_INT64 && pTypeB->stackType == EvalStack.EVALSTACK_INT64) ||
                                (pTypeA->stackType == EvalStack.EVALSTACK_O && pTypeB->stackType == EvalStack.EVALSTACK_O) ||
                                (pTypeA->stackType == EvalStack.EVALSTACK_PTR && pTypeB->stackType == EvalStack.EVALSTACK_PTR)) {
        #endif
                                PushOp((uint)(JitOps.JIT_CEQ_I64I64 + (op - OpCodes.X_CEQ)));
        					} else if (pTypeA->stackType == EvalStack.EVALSTACK_F32 && pTypeB->stackType == EvalStack.EVALSTACK_F32) {
                                PushOp((uint)(JitOps.JIT_CEQ_F32F32 + (op - OpCodes.X_CEQ)));
        					} else if (pTypeA->stackType == EvalStack.EVALSTACK_F64 && pTypeB->stackType == EvalStack.EVALSTACK_F64) {
                                PushOp((uint)(JitOps.JIT_CEQ_F64F64 + (op - OpCodes.X_CEQ)));
        					} else {
                                Sys.Crash("JITit(): Cannot perform comparison operand on stack Type.types: %s and %s", (PTR)pTypeA->name, (PTR)pTypeB->name);
        					}
        					PushStackType(Type.types[Type.TYPE_SYSTEM_INT32]);
        					break;
        					
        				case OpCodes.X_RETHROW:
        					PushOp(JitOps.JIT_RETHROW);
        					break;

        				case OpCodes.X_CONSTRAINED:
        					u32Value2 = GetUnalignedU32(pCIL, ref cilOfs);
        					cilOfs++;
        					goto cilCallVirtConstrained;

        				case OpCodes.X_READONLY:
        					// Do nothing
        					break;

        				default:
        					Sys.Crash("JITit(): JITter cannot handle extended op-code:0x%02x", op);
                            break;

        				}
        				break;

        			default:
        				Sys.Crash("JITit(): JITter cannot handle op-code: 0x%02x", op);
                        break;
        		}

        	} while (cilOfs < codeSize);

        	// Apply branch offset fixes
        	for (i=0; i<branchOffsets.ofs; i++) {
        		uint ofs2, jumpTarget;

        		ofs2 = branchOffsets.p[i];
        		jumpTarget = ops.p[ofs2];
        		// Rewrite the branch offset
        		jumpTarget = pJITOffsets[jumpTarget];
        		ops.p[ofs2] = jumpTarget;
        #if GEN_COMBINED_OPCODES
        		isDynamic.p[jumpTarget] |= DYNAMIC_JUMP_TARGET;
        #endif
        	}

        	// Apply expection handler offset fixes
        	for (i=0; i<pJITted->numExceptionHandlers; i++) {
        		tExceptionHeader *pEx;

                pEx = (tExceptionHeader*)&pJITted->pExceptionHeaders[i];
        		pEx->tryEnd = pJITOffsets[pEx->tryStart + pEx->tryEnd];
        		pEx->tryStart = pJITOffsets[pEx->tryStart];
        		pEx->handlerEnd = pJITOffsets[pEx->handlerStart + pEx->handlerEnd];
        		pEx->handlerStart = pJITOffsets[pEx->handlerStart];
        	}

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

        	DeleteOps(ref branchOffsets);
        	Mem.free(pJITOffsets);

        	// Copy ops to some memory of exactly the correct size. To not waste memory.
        	u32Value = ops.ofs * sizeof(uint);
            pFinalOps = genCombinedOpcodes != 0 ? (uint*)Mem.malloc(u32Value) : (uint*)Mem.mallocForever(u32Value);
        	Mem.memcpy(pFinalOps, ops.p, u32Value);
        	DeleteOps(ref ops);

        	return pFinalOps;
        }

        // Prepare a method for execution
        // This makes sure that the method has been JITed.
        public static void Prepare(tMD_MethodDef *pMethodDef, uint genCombinedOpcodes) 
        {
        	//tMetaData* pMetaData;
        	byte* pMethodHeader;
        	tJITted* pJITted;
        	/*FLAGS16*/ushort flags;
        	uint codeSize;
        	/*IDX_TABLE*/uint localsToken;
        	byte* pCIL;
        	/*SIG*/byte* sig;
        	uint i, sigLength, numLocals;
        	tParameter* pLocals;

            Sys.log_f(2, "JIT:   %s\n", (PTR)Sys.GetMethodDesc(pMethodDef));

        	//pMetaData = pMethodDef->pMetaData;
            pJITted = (genCombinedOpcodes != 0)?((tJITted*)Mem.malloc((SIZE_T)sizeof(tJITted))) : ((tJITted*)Mem.mallocForever((SIZE_T)sizeof(tJITted)));
        	pMethodDef->pJITted = pJITted;

            if (((pMethodDef->implFlags & MetaData.METHODIMPLATTRIBUTES_INTERNALCALL) != 0) ||
        		((pMethodDef->implFlags & MetaData.METHODIMPLATTRIBUTES_CODETYPE_MASK) == MetaData.METHODIMPLATTRIBUTES_CODETYPE_RUNTIME)) {
                tJITCallNative* pCallNative;

        		// Internal call
                if (S.strcmp(pMethodDef->name, ".ctor") == 0) {
        			// Internal constructor needs enough evaluation stack space to return itself
        			pJITted->maxStack = pMethodDef->pParentType->stackSize;
        		} else {
        			pJITted->maxStack = (pMethodDef->pReturnType == null)?0:pMethodDef->pReturnType->stackSize; // For return value
        		}
                pCallNative = ((tJITCallNative*)Mem.mallocForever((SIZE_T)sizeof(tJITCallNative)));
        		pCallNative->opCode = JitOps.JIT_CALL_NATIVE;
        		pCallNative->pMethodDef = pMethodDef;
        		pCallNative->fn = InternalCall.Map(pMethodDef);
        		pCallNative->retOpCode = JitOps.JIT_RETURN;

        		pJITted->localsStackSize = 0;
        		pJITted->pOps = (uint*)pCallNative;

        		return;
        	}
            if ((pMethodDef->flags & MetaData.METHODATTRIBUTES_PINVOKEIMPL) != 0) {
                throw new System.NotSupportedException();

                #if NO
        		tJITCallPInvoke *pCallPInvoke;

        		// PInvoke call
        		tMD_ImplMap *pImplMap = MetaData.GetImplMap(pMetaData, pMethodDef->tableIndex);
        		fnPInvoke fn = PInvoke.GetFunction(pMetaData, pImplMap);
        		if (fn == null) {
                    Sys.Crash("PInvoke library or function not found: %s()", (PTR)pImplMap->importName);
        		}

        		pCallPInvoke = ((tJITCallPInvoke*)Mem.mallocForever(sizeof(tJITCallPInvoke)));
        		pCallPInvoke->opCode = JitOps.JIT_CALL_PINVOKE;
        		pCallPInvoke->fn = fn;
        		pCallPInvoke->pMethod = pMethodDef;
        		pCallPInvoke->pImplMap = pImplMap;

        		pJITted->localsStackSize = 0;
        		pJITted->maxStack = (pMethodDef->pReturnType == null)?0:pMethodDef->pReturnType->stackSize; // For return value
        		pJITted->pOps = (uint*)pCallPInvoke;

        		return;
                #endif
        	}

        	pMethodHeader = (byte*)pMethodDef->pCIL;
        	if ((*pMethodHeader & 0x3) == CorILMethod_TinyFormat) {
        		// Tiny header
                flags = (ushort)(*pMethodHeader & 0x3);
        		pJITted->maxStack = 8;
                codeSize = (uint)((*pMethodHeader & 0xfc) >> 2);
        		localsToken = 0;
        		pCIL = pMethodHeader + 1;
        	} else {
        		// Fat header
                flags = (ushort)(*(ushort*)pMethodHeader & 0x0fff);
        		pJITted->maxStack = *(ushort*)&pMethodHeader[2];
        		codeSize = *(uint*)&pMethodHeader[4];
        		localsToken = *(/*IDX_TABLE*/uint*)&pMethodHeader[8];
        		pCIL = pMethodHeader + ((pMethodHeader[1] & 0xf0) >> 2);
        	}
            if ((flags & CorILMethod_MoreSects) != 0) {
        		uint numClauses;

        		pMethodHeader = pCIL + ((codeSize + 3) & (~0x3));
                if ((*pMethodHeader & CorILMethod_Sect_FatFormat) != 0) {
                    tExceptionHeader* pOrigExHeaders;
        			uint exSize;

        			// Fat header
        			numClauses = ((*(uint*)pMethodHeader >> 8) - 4) / 24;
        			//pJITted->pExceptionHeaders = (tExceptionHeader*)(pMethodHeader + 4);
                    exSize = (uint)(numClauses * sizeof(tJITExceptionHeader));
                    
                    // Copy ex header into slightly larger JIT ex header which has typedef ptr at end
        			pJITted->pExceptionHeaders =
                        (tJITExceptionHeader*)(genCombinedOpcodes!=0?Mem.malloc((SIZE_T)exSize):Mem.mallocForever((SIZE_T)exSize));
                    Mem.memset(pJITted->pExceptionHeaders, 0, exSize);
                    pOrigExHeaders = (tExceptionHeader*)(pMethodHeader + 4);
                    for (i=0; i<numClauses; i++) {
                        Mem.memcpy(pJITted->pExceptionHeaders + i, pOrigExHeaders + i, (SIZE_T)sizeof(tExceptionHeader));
                    }
        		} else {
        			// Thin header
        			tJITExceptionHeader *pExHeaders;
        			uint exSize;

                    numClauses = (uint)((((byte*)pMethodHeader)[1] - 4) / 12);
                    exSize = (uint)(numClauses * sizeof(tJITExceptionHeader));
        			pMethodHeader += 4;
        			//pExHeaders = pJITted->pExceptionHeaders = (tExceptionHeader*)Mem.mallocForever(numClauses * sizeof(tExceptionHeader));
        			pExHeaders = pJITted->pExceptionHeaders =
                        (tJITExceptionHeader*)(genCombinedOpcodes!=0?Mem.malloc((SIZE_T)exSize):Mem.mallocForever((SIZE_T)exSize));
                    Mem.memset(pJITted->pExceptionHeaders, 0, exSize);
        			for (i=0; i<numClauses; i++) {
        				pExHeaders[i].flags = ((ushort*)pMethodHeader)[0];
        				pExHeaders[i].tryStart = ((ushort*)pMethodHeader)[1];
        				pExHeaders[i].tryEnd = ((byte*)pMethodHeader)[4];
                        pExHeaders[i].handlerStart = (uint)(((byte*)pMethodHeader)[5] | (((byte*)pMethodHeader)[6] << 8));
        				pExHeaders[i].handlerEnd = ((byte*)pMethodHeader)[7];
                        pExHeaders[i].classTokenOrFilterOffset = ((uint*)pMethodHeader)[2];

        				pMethodHeader += 12;
        			}
        		}
        		pJITted->numExceptionHandlers = numClauses;
        		// replace all classToken's with the actual tMD_TypeDef*
        		for (i=0; i<numClauses; i++) {
        			if (pJITted->pExceptionHeaders[i].flags == COR_ILEXCEPTION_CLAUSE_EXCEPTION) {
        				pJITted->pExceptionHeaders[i].pCatchTypeDef =
                            MetaData.GetTypeDefFromDefRefOrSpec(pMethodDef->pMetaData, pJITted->pExceptionHeaders[i].classTokenOrFilterOffset, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
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
                uint i2, totalSize;

        		pStandAloneSig = (tMD_StandAloneSig*)MetaData.GetTableRow(pMethodDef->pMetaData, localsToken);
        		sig = MetaData.GetBlob(pStandAloneSig->signature, &sigLength);
        		MetaData.DecodeSigEntry(&sig); // Always 0x07
        		numLocals = MetaData.DecodeSigEntry(&sig);
                pLocals = (tParameter*)Mem.malloc((SIZE_T)(numLocals * sizeof(tParameter)));
        		totalSize = 0;
        		for (i2=0; i2<numLocals; i2++) {
        			tMD_TypeDef *pTypeDef;

        			pTypeDef = Type.GetTypeFromSig(pMethodDef->pMetaData, &sig, pMethodDef->pParentType->ppClassTypeArgs, pMethodDef->ppMethodTypeArgs);
        			MetaData.Fill_TypeDef(pTypeDef, null, null);
        			pLocals[i2].pTypeDef = pTypeDef;
        			pLocals[i2].offset = totalSize;
        			pLocals[i2].size = pTypeDef->stackSize;
        			totalSize += pTypeDef->stackSize;
        		}
        		pJITted->localsStackSize = totalSize;
        	}

        	// JIT the CIL code
        	pJITted->pOps = JITit(pMethodDef, pCIL, codeSize, pLocals, pJITted, genCombinedOpcodes);
            
            pJITted->maxStack += 64;
            
        	Mem.free(pLocals);
        }
          
    }
}
