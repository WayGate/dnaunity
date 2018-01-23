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
    public static class JitOps
    {

        // JIT opcodes may be up to 9 bits long
        public const uint JIT_OPCODE_MAXBITS =                  9;
        public const uint JIT_OPCODE_MAXNUM =                   (uint)(1 << (int)JIT_OPCODE_MAXBITS);
        //public const uint JIT_OPCODE_MASK = ((1 << JIT_OPCODE_MAXBITS) - 1);
        //public const uint JIT_OPCODE(opCode) ((opCode) & JIT_OPCODE_MASK)
        //public const uint JIT_PARAM(opCode) ((opCode) >> JIT_OPCODE_MAXBITS)

        // Note that the exact order of some of these op-codes matters.
        // This is due to optimisations where groups of opcodes can be handled together

        public const uint JIT_NOP =                             0x0;
        public const uint JIT_RETURN =                          0x1;
        public const uint JIT_LOAD_I32 =                        0x2;
        public const uint JIT_BRANCH =                          0x3;
        public const uint JIT_LOAD_STRING =                     0x4;
        public const uint JIT_CALLVIRT_O =                      0x5;
        public const uint JIT_CALL_NATIVE =                     0x6;
        public const uint JIT_CALL_O =                          0x7;
        public const uint JIT_NEWOBJECT =                       0x8;
        public const uint JIT_LOAD_PARAMLOCAL_ADDR =            0x9;
        public const uint JIT_CALL_PTR =                        0xa;
        public const uint JIT_BOX_CALLVIRT =                    0xb;
        public const uint JIT_INIT_VALUETYPE =                  0xc;
        public const uint JIT_NEW_VECTOR =                      0xd;
        public const uint JIT_NEWOBJECT_VALUETYPE =             0xe;
        public const uint JIT_IS_INSTANCE =                     0xf;
        public const uint JIT_LOAD_NULL =                       0x10;
        public const uint JIT_UNBOX2VALUETYPE =                 0x11;
        public const uint JIT_UNBOX2OBJECT =                    0x12;
        public const uint JIT_LOAD_FIELD_ADDR =                 0x13;
        public const uint JIT_DUP_GENERAL =                     0x14;
        public const uint JIT_POP =                             0x15;
        public const uint JIT_STORE_OBJECT_VALUETYPE =          0x16;
        public const uint JIT_DEREF_CALLVIRT =                  0x17;
        public const uint JIT_STORE_ELEMENT =                   0x18;
        public const uint JIT_LEAVE =                           0x19;
        public const uint JIT_END_FINALLY =                     0x1a;
        public const uint JIT_THROW =                           0x1b;
        public const uint JIT_RETHROW =                         0x1c;
        public const uint JIT_LOADOBJECT =                      0x1d;
        public const uint JIT_LOAD_VECTOR_LEN =                 0x1e;
        public const uint JIT_SWITCH =                          0x1f;
        public const uint JIT_LOAD_ELEMENT_ADDR =               0x20;
        public const uint JIT_CALL_INTERFACE =                  0x21;
        public const uint JIT_CAST_CLASS =                      0x22;
        public const uint JIT_LOAD_ELEMENT =                    0x23;
        public const uint JIT_LOADFIELD_VALUETYPE =             0x24;
        public const uint   JIT_LOADFIELD =                     0x25;
        public const uint JIT_LOADFUNCTION =                    0x26;
        public const uint JIT_INVOKE_DELEGATE =                 0x27;
        public const uint JIT_CALL_PINVOKE =                    0x28;
        public const uint JIT_LOAD_I64 =                        0x29;
        public const uint JIT_INIT_OBJECT =                     0x2a;
        public const uint JIT_DUP_4 =                           0x2b;
        public const uint JIT_DUP_8 =                           0x2c;
        public const uint JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT =  0x2d;
        public const uint JIT_POP_4 =                           0x2e;
        public const uint JIT_LOAD_F32 =                        0x2f;

        public const uint JIT_LOADPARAMLOCAL_TYPEID =           0x30;
        public const uint JIT_LOADPARAMLOCAL_INT64 =            (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_LOADPARAMLOCAL_INT32 =            (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_LOADPARAMLOCAL_INTNATIVE =        (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_LOADPARAMLOCAL_F =              (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_LOADPARAMLOCAL_F32 =              (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_LOADPARAMLOCAL_PTR =              (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_LOADPARAMLOCAL_O =                (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_LOADPARAMLOCAL_TRANSPTR =       (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_LOADPARAMLOCAL_F64 =              (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_LOADPARAMLOCAL_VALUETYPE =        (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_STOREPARAMLOCAL_TYPEID =          0x38;
        public const uint JIT_STOREPARAMLOCAL_INT64 =           (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_STOREPARAMLOCAL_INT32 =           (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_STOREPARAMLOCAL_INTNATIVE =       (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_STOREPARAMLOCAL_F =             (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_STOREPARAMLOCAL_F32 =             (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_STOREPARAMLOCAL_PTR =             (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_STOREPARAMLOCAL_O =               (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_STOREPARAMLOCAL_TRANSPTR =      (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_STOREPARAMLOCAL_F64 =             (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_STOREPARAMLOCAL_VALUETYPE =       (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_STOREFIELD_TYPEID =               0x48;
        public const uint JIT_STOREFIELD_INT64 =                (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_STOREFIELD_INT32 =                (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_STOREFIELD_INTNATIVE =            (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_STOREFIELD_F =                  (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_STOREFIELD_F32 =                  (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_STOREFIELD_PTR =                  (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_STOREFIELD_O =                    (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_STOREFIELD_TRANSPTR =           (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_STOREFIELD_F64 =                  (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_STOREFIELD_VALUETYPE =            (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID =    0x50;
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT64 =     (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32 =     (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_INTNATIVE = (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_F =       (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_F32 =       (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR =       (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_O =         (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_TRANSPTR = (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64 =       (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE = (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_LOADSTATICFIELD_TYPEID =          0x58;
        public const uint JIT_LOADSTATICFIELD_INT64 =           (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_LOADSTATICFIELD_INT32 =           (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_LOADSTATICFIELD_INTNATIVE =       (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_LOADSTATICFIELD_F =             (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_LOADSTATICFIELD_F32 =             (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_LOADSTATICFIELD_PTR =             (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_LOADSTATICFIELD_O =               (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_LOADSTATICFIELD_TRANSPTR =      (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_LOADSTATICFIELD_F64 =             (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_LOADSTATICFIELD_VALUEPTYE =       (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_STORESTATICFIELD_TYPEID =         0x60;
        public const uint JIT_STORESTATICFIELD_INT64 =          (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_STORESTATICFIELD_INT32 =          (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_STORESTATICFIELD_INTNATIVE =      (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_STORESTATICFIELD_F =            (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_STORESTATICFIELD_F32 =            (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_STORESTATICFIELD_F64 =            (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_STORESTATICFIELD_PTR =            (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_STORESTATICFIELD_O =              (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_STORESTATICFIELD_TRANSPTR =     (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_STORESTATICFIELD_VALUETYPE =      (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_BOX_TYPEID =                      0x68;
        public const uint JIT_BOX_INT64 =                       (JIT_BOX_TYPEID + EvalStack.EVALSTACK_INT64);
        public const uint JIT_BOX_INT32 =                       (JIT_BOX_TYPEID + EvalStack.EVALSTACK_INT32);
        public const uint JIT_BOX_INTNATIVE =                   (JIT_BOX_TYPEID + EvalStack.EVALSTACK_INTNATIVE);
        //public const uint JIT_BOX_F =                         (JIT_BOX_TYPEID + EvalStack.EVALSTACK_F);
        public const uint JIT_BOX_F32 =                         (JIT_BOX_TYPEID + EvalStack.EVALSTACK_F32);
        public const uint JIT_BOX_PTR =                         (JIT_BOX_TYPEID + EvalStack.EVALSTACK_PTR);
        public const uint JIT_BOX_O =                           (JIT_BOX_TYPEID + EvalStack.EVALSTACK_O);
        //public const uint JIT_BOX_TRANSPTR =                  (JIT_BOX_TYPEID + EvalStack.EVALSTACK_TRANSPTR);
        public const uint JIT_BOX_F64 =                         (JIT_BOX_TYPEID + EvalStack.EVALSTACK_F64);
        public const uint JIT_BOX_VALUETYPE =                   (JIT_BOX_TYPEID + EvalStack.EVALSTACK_VALUETYPE);

        public const uint JIT_CEQ_I32I32 =                      0x70;
        public const uint JIT_CGT_I32I32 =                      0x71;
        public const uint JIT_CGT_UN_I32I32 =                   0x72;
        public const uint JIT_CLT_I32I32 =                      0x73;
        public const uint JIT_CLT_UN_I32I32 =                   0x74;

        public const uint JIT_CEQ_I64I64 =                      0x75;
        public const uint JIT_CGT_I64I64 =                      0x76;
        public const uint JIT_CGT_UN_I64I64 =                   0x77;
        public const uint JIT_CLT_I64I64 =                      0x78;
        public const uint JIT_CLT_UN_I64I64 =                   0x79;

        public const uint JIT_ADD_OVF_I32I32 =                  0x7a;
        public const uint JIT_ADD_OVF_UN_I32I32 =               0x7b;
        public const uint JIT_MUL_OVF_I32I32 =                  0x7c;
        public const uint JIT_MUL_OVF_UN_I32I32 =               0x7d;
        public const uint JIT_SUB_OVF_I32I32 =                  0x7e;
        public const uint JIT_SUB_OVF_UN_I32I32 =               0x7f;
        public const uint JIT_ADD_I32I32 =                      0x80;
        public const uint JIT_SUB_I32I32 =                      0x81;
        public const uint JIT_MUL_I32I32 =                      0x82;
        public const uint JIT_DIV_I32I32 =                      0x83;
        public const uint JIT_DIV_UN_I32I32 =                   0x84;
        public const uint JIT_REM_I32I32 =                      0x85;
        public const uint JIT_REM_UN_I32I32 =                   0x86;
        public const uint JIT_AND_I32I32 =                      0x87;
        public const uint JIT_OR_I32I32 =                       0x88;
        public const uint JIT_XOR_I32I32 =                      0x89;

        public const uint JIT_NEG_I32 =                         0x8a;
        public const uint JIT_NOT_I32 =                         0x8b;
        public const uint JIT_NEG_I64 =                         0x8c;
        public const uint JIT_NOT_I64 =                         0x8d;

        public const uint JIT_BOX_NULLABLE =                    0x8e;
        public const uint JIT_LOAD_F64 =                        0x8f;

        public const uint JIT_BEQ_I32I32 =                      0x90;
        public const uint JIT_BGE_I32I32 =                      0x91;
        public const uint JIT_BGT_I32I32 =                      0x92;
        public const uint JIT_BLE_I32I32 =                      0x93;
        public const uint JIT_BLT_I32I32 =                      0x94;
        public const uint JIT_BNE_UN_I32I32 =                   0x95;
        public const uint JIT_BGE_UN_I32I32 =                   0x96;
        public const uint JIT_BGT_UN_I32I32 =                   0x97;
        public const uint JIT_BLE_UN_I32I32 =                   0x98;
        public const uint JIT_BLT_UN_I32I32 =                   0x99;

        public const uint JIT_BEQ_I64I64 =                      0x9a;
        public const uint JIT_BGE_I64I64 =                      0x9b;
        public const uint JIT_BGT_I64I64 =                      0x9c;
        public const uint JIT_BLE_I64I64 =                      0x9d;
        public const uint JIT_BLT_I64I64 =                      0x9e;
        public const uint JIT_BNE_UN_I64I64 =                   0x9f;
        public const uint JIT_BGE_UN_I64I64 =                   0xa0;
        public const uint JIT_BGT_UN_I64I64 =                   0xa1;
        public const uint JIT_BLE_UN_I64I64 =                   0xa2;
        public const uint JIT_BLT_UN_I64I64 =                   0xa3;

        public const uint JIT_SHL_I32 =                         0xa8;
        public const uint JIT_SHR_I32 =                         0xa9;
        public const uint JIT_SHR_UN_I32 =                      0xaa;
        public const uint JIT_SHL_I64 =                         0xab;
        public const uint JIT_SHR_I64 =                         0xac;
        public const uint JIT_SHR_UN_I64 =                      0xad;

        public const uint JIT_LOADTOKEN_BASE =                  0xb0;
        public const uint JIT_LOADTOKEN_TYPE =                  (JIT_LOADTOKEN_BASE + 0);
        public const uint JIT_LOADTOKEN_METHOD =                (JIT_LOADTOKEN_BASE + 1);
        public const uint JIT_LOADTOKEN_FIELD =                 (JIT_LOADTOKEN_BASE + 2);

        public const uint JIT_LOADINDIRECT_I8 =                 0xb3;
        public const uint JIT_LOADINDIRECT_U8 =                 0xb4;
        public const uint JIT_LOADINDIRECT_I16 =                0xb5;
        public const uint JIT_LOADINDIRECT_U16 =                0xb6;
        public const uint JIT_LOADINDIRECT_I32 =                0xb7;
        public const uint JIT_LOADINDIRECT_U32 =                0xb8;
        public const uint JIT_LOADINDIRECT_I64 =                0xb9;
        public const uint JIT_LOADINDIRECT_I =                  0xba;
        public const uint JIT_LOADINDIRECT_R32 =                0xbb;
        public const uint JIT_LOADINDIRECT_R64 =                0xbc;
        public const uint JIT_LOADINDIRECT_REF =                0xbd;

        public const uint JIT_STOREINDIRECT_REF =               0xbe;
        public const uint JIT_STOREINDIRECT_U8 =                0xbf;
        public const uint JIT_STOREINDIRECT_U16 =               0xc0;
        public const uint JIT_STOREINDIRECT_U32 =               0xc1;
        public const uint JIT_STOREINDIRECT_U64 =               0xc2;
        public const uint JIT_STOREINDIRECT_R32 =               0xc3;
        public const uint JIT_STOREINDIRECT_R64 =               0xc4;

        //public const uint JIT_CONV_SIGNED32 =                 0xc5;
        //public const uint JIT_CONV_UNSIGNED32 =               0xc6;
        //public const uint JIT_CONV_INT_I64 =                  0xc7;

        //public const uint JIT_CONV_I1 =                       0xc5;
        //public const uint JIT_CONV_I2 =                       0xc6;
        //public const uint JIT_CONV_I4 =                       0xc7;
        //public const uint JIT_CONV_I8 =                       0xc8;
        //public const uint JIT_CONV_R4 =                       0xc9;
        //public const uint JIT_CONV_R8 =                       0xca;
        //public const uint JIT_CONV_U4 =                       0xcb;
        //public const uint JIT_CONV_U8 =                       0xcc;
        //public const uint JIT_CONV_U2 =                       0xcd;
        //public const uint JIT_CONV_U1 =                       0xce;
        //public const uint JIT_CONV_I_NATIVE =                 0xcf;
        //public const uint JIT_CONV_U_NATIVE =                 0xd0;

        //public const uint JIT_CONV_OVF_I1 =                   0xd1;
        //public const uint JIT_CONV_OVF_U1 =                   0xd2;
        //public const uint JIT_CONV_OVF_I2 =                   0xd3;
        //public const uint JIT_CONV_OVF_U2 =                   0xd4;
        //public const uint JIT_CONV_OVF_I4 =                   0xd5;
        //public const uint JIT_CONV_OVF_U4 =                   0xd6;
        //public const uint JIT_CONV_OVF_I8 =                   0xd7;
        //public const uint JIT_CONV_OVF_U8 =                   0xd8;

        public const uint JIT_UNBOX_NULLABLE =                  0xda;

        public const uint JIT_STORE_ELEMENT_32 =                0xde;
        public const uint JIT_STORE_ELEMENT_64 =                0xdf;

        public const uint JIT_LOAD_ELEMENT_I8 =                 0xe0;
        public const uint JIT_LOAD_ELEMENT_U8 =                 0xe1;
        public const uint JIT_LOAD_ELEMENT_I16 =                0xe2;
        public const uint JIT_LOAD_ELEMENT_U16 =                0xe3;
        public const uint JIT_LOAD_ELEMENT_I32 =                0xe4;
        public const uint JIT_LOAD_ELEMENT_U32 =                0xe5;
        public const uint JIT_LOAD_ELEMENT_I64 =                0xe6;
        public const uint JIT_LOAD_ELEMENT_R32 =                0xe7;
        public const uint JIT_LOAD_ELEMENT_R64 =                0xe8;

        public const uint JIT_ADD_OVF_I64I64 =                  0xea;
        public const uint JIT_ADD_OVF_UN_I64I64 =               0xeb;
        public const uint JIT_MUL_OVF_I64I64 =                  0xec;
        public const uint JIT_MUL_OVF_UN_I64I64 =               0xed;
        public const uint JIT_SUB_OVF_I64I64 =                  0xee;
        public const uint JIT_SUB_OVF_UN_I64I64 =               0xef;
        public const uint JIT_ADD_I64I64 =                      0xf0;
        public const uint JIT_SUB_I64I64 =                      0xf1;
        public const uint JIT_MUL_I64I64 =                      0xf2;
        public const uint JIT_DIV_I64I64 =                      0xf3;
        public const uint JIT_DIV_UN_I64I64 =                   0xf4;
        public const uint JIT_REM_I64I64 =                      0xf5;
        public const uint JIT_REM_UN_I64I64 =                   0xf6;
        public const uint JIT_AND_I64I64 =                      0xf7;
        public const uint JIT_OR_I64I64 =                       0xf8;
        public const uint JIT_XOR_I64I64 =                      0xf9;

        public const uint JIT_CEQ_F32F32 =                      0xfa;
        public const uint JIT_CGT_F32F32 =                      0xfb;
        public const uint JIT_CGT_UN_F32F32 =                   0xfc;
        public const uint JIT_CLT_F32F32 =                      0xfd;
        public const uint JIT_CLT_UN_F32F32 =                   0xfe;

        public const uint JIT_BEQ_F32F32 =                      0xff;
        public const uint JIT_BGE_F32F32 =                      0x100;
        public const uint JIT_BGT_F32F32 =                      0x101;
        public const uint JIT_BLE_F32F32 =                      0x102;
        public const uint JIT_BLT_F32F32 =                      0x103;
        public const uint JIT_BNE_UN_F32F32 =                   0x104;
        public const uint JIT_BGE_UN_F32F32 =                   0x105;
        public const uint JIT_BGT_UN_F32F32 =                   0x106;
        public const uint JIT_BLE_UN_F32F32 =                   0x107;
        public const uint JIT_BLT_UN_F32F32 =                   0x108;

        public const uint JIT_ADD_F32F32 =                      0x109;
        public const uint JIT_SUB_F32F32 =                      0x10a;
        public const uint JIT_MUL_F32F32 =                      0x10b;
        public const uint JIT_DIV_F32F32 =                      0x10c;
        public const uint JIT_DIV_UN_F32F32 =                   0x10d; // Never used
        public const uint JIT_REM_F32F32 =                      0x10e;
        public const uint JIT_REM_UN_F32F32 =                   0x10f; // Never used

        public const uint JIT_CEQ_F64F64 =                      0x110;
        public const uint JIT_CGT_F64F64 =                      0x111;
        public const uint JIT_CGT_UN_F64F64 =                   0x112;
        public const uint JIT_CLT_F64F64 =                      0x113;
        public const uint JIT_CLT_UN_F64F64 =                   0x114;

        public const uint JIT_BEQ_F64F64 =                      0x115;
        public const uint JIT_BGE_F64F64 =                      0x116;
        public const uint JIT_BGT_F64F64 =                      0x117;
        public const uint JIT_BLE_F64F64 =                      0x118;
        public const uint JIT_BLT_F64F64 =                      0x119;
        public const uint JIT_BNE_UN_F64F64 =                   0x11a;
        public const uint JIT_BGE_UN_F64F64 =                   0x11b;
        public const uint JIT_BGT_UN_F64F64 =                   0x11c;
        public const uint JIT_BLE_UN_F64F64 =                   0x11d;
        public const uint JIT_BLT_UN_F64F64 =                   0x11e;

        public const uint JIT_ADD_F64F64 =                      0x11f;
        public const uint JIT_SUB_F64F64 =                      0x120;
        public const uint JIT_MUL_F64F64 =                      0x121;
        public const uint JIT_DIV_F64F64 =                      0x122;
        public const uint JIT_DIV_UN_F64F64 =                   0x123; // Never used
        public const uint JIT_REM_F64F64 =                      0x124;
        public const uint JIT_REM_UN_F64F64 =                   0x125; // Never used

        public const uint JIT_LOADPARAMLOCAL_0 =                0x127; // Load 4-byte param/local at offset 0
        public const uint JIT_LOADPARAMLOCAL_1 =                0x128; // Load 4-byte param/local at offset 4
        public const uint JIT_LOADPARAMLOCAL_2 =                0x129; // Load 4-byte param/local at offset 8
        public const uint JIT_LOADPARAMLOCAL_3 =                0x12a; // Load 4-byte param/local at offset 12
        public const uint JIT_LOADPARAMLOCAL_4 =                0x12b; // Load 4-byte param/local at offset 16
        public const uint JIT_LOADPARAMLOCAL_5 =                0x12c; // Load 4-byte param/local at offset 20
        public const uint JIT_LOADPARAMLOCAL_6 =                0x12d; // Load 4-byte param/local at offset 24
        public const uint JIT_LOADPARAMLOCAL_7 =                0x12e; // Load 4-byte param/local at offset 28

        public const uint JIT_STOREPARAMLOCAL_0 =               0x12f; // Store 4-byte param/local at offset 0
        public const uint JIT_STOREPARAMLOCAL_1 =               0x130; // Store 4-byte param/local at offset 4
        public const uint JIT_STOREPARAMLOCAL_2 =               0x131; // Store 4-byte param/local at offset 8
        public const uint JIT_STOREPARAMLOCAL_3 =               0x132; // Store 4-byte param/local at offset 12
        public const uint JIT_STOREPARAMLOCAL_4 =               0x133; // Store 4-byte param/local at offset 16
        public const uint JIT_STOREPARAMLOCAL_5 =               0x134; // Store 4-byte param/local at offset 20
        public const uint JIT_STOREPARAMLOCAL_6 =               0x135; // Store 4-byte param/local at offset 24
        public const uint JIT_STOREPARAMLOCAL_7 =               0x136; // Store 4-byte param/local at offset 28

        public const uint JIT_LOAD_I4_M1 =                      0x137;
        public const uint JIT_LOAD_I4_0 =                       0x138;
        public const uint JIT_LOAD_I4_1 =                       0x139;
        public const uint JIT_LOAD_I4_2 =                       0x13a;

        public const uint JIT_LOADFIELD_4 =                     0x13b;
        public const uint JIT_LOADFIELD_8 =                     0x13c;

        public const uint JIT_CONV_OFFSET_I32 =                 0;
        public const uint JIT_CONV_OFFSET_U32 =                 1;
        public const uint JIT_CONV_OFFSET_I64 =                 2;
        public const uint JIT_CONV_OFFSET_U64 =                 3;
        public const uint JIT_CONV_OFFSET_R32 =                 4;
        public const uint JIT_CONV_OFFSET_R64 =                 5;

        public const uint JIT_CONV_FROM_I32 =                   0x140;
        public const uint JIT_CONV_I32_I32 =                    (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_I32);
        public const uint JIT_CONV_I32_U32 =                    (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_U32);
        public const uint JIT_CONV_I32_I64 =                    (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_I64);
        public const uint JIT_CONV_I32_U64 =                    (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_U64);
        public const uint JIT_CONV_I32_R32 =                    (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_R32);
        public const uint JIT_CONV_I32_R64 =                    (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_R64);

        public const uint JIT_CONV_FROM_U32 =                   0x146;
        public const uint JIT_CONV_U32_I32 =                    (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_I32);
        public const uint JIT_CONV_U32_U32 =                    (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_U32);
        public const uint JIT_CONV_U32_I64 =                    (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_I64);
        public const uint JIT_CONV_U32_U64 =                    (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_U64);
        public const uint JIT_CONV_U32_R32 =                    (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_R32);
        public const uint JIT_CONV_U32_R64 =                    (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_R64);

        public const uint JIT_CONV_FROM_I64 =                   0x14c;
        public const uint JIT_CONV_I64_I32 =                    (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_I32);
        public const uint JIT_CONV_I64_U32 =                    (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_U32);
        public const uint JIT_CONV_I64_I64 =                    (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_I64); // Not used
        public const uint JIT_CONV_I64_U64 =                    (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_U64); // Not used
        public const uint JIT_CONV_I64_R32 =                    (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_R32);
        public const uint JIT_CONV_I64_R64 =                    (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_R64);

        public const uint JIT_CONV_FROM_U64 =                   0x152;
        public const uint JIT_CONV_U64_I32 =                    (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_I32);
        public const uint JIT_CONV_U64_U32 =                    (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_U32);
        public const uint JIT_CONV_U64_I64 =                    (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_I64); // Not used
        public const uint JIT_CONV_U64_U64 =                    (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_U64); // Not used
        public const uint JIT_CONV_U64_R32 =                    (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_R32);
        public const uint JIT_CONV_U64_R64 =                    (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_R64);

        public const uint JIT_CONV_FROM_R32 =                   0x158;
        public const uint JIT_CONV_R32_I32 =                    (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_I32);
        public const uint JIT_CONV_R32_U32 =                    (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_U32);
        public const uint JIT_CONV_R32_I64 =                    (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_I64);
        public const uint JIT_CONV_R32_U64 =                    (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_U64);
        public const uint JIT_CONV_R32_R32 =                    (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_R32);
        public const uint JIT_CONV_R32_R64 =                    (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_R64);

        public const uint JIT_CONV_FROM_R64 =                   0x15e;
        public const uint JIT_CONV_R64_I32 =                    (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_I32);
        public const uint JIT_CONV_R64_U32 =                    (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_U32);
        public const uint JIT_CONV_R64_I64 =                    (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_I64);
        public const uint JIT_CONV_R64_U64 =                    (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_U64);
        public const uint JIT_CONV_R64_R32 =                    (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_R32);
        public const uint JIT_CONV_R64_R64 =                    (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_R64);

        public const uint JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE =     0x164;
        public const uint JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE = 0x165;

        public const uint JIT_BRANCH_FALSE_U32 =                0x166;
        public const uint JIT_BRANCH_TRUE_U32 =                 0x167;
        public const uint JIT_BRANCH_FALSE_U64 =                0x168;
        public const uint JIT_BRANCH_TRUE_U64 =                 0x169;
    }
}
