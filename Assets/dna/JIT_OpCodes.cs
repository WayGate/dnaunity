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

// JIT opcodes may be up to 9 bits long
const int JIT_OPCODE_MAXBITS 9
const int JIT_OPCODE_MAXNUM (1 << JIT_OPCODE_MAXBITS)
//const int JIT_OPCODE_MASK ((1 << JIT_OPCODE_MAXBITS) - 1)
//const int JIT_OPCODE(opCode) ((opCode) & JIT_OPCODE_MASK)
//const int JIT_PARAM(opCode) ((opCode) >> JIT_OPCODE_MAXBITS)

// Note that the exact order of some of these op-codes matters.
// This is due to optimisations where groups of opcodes can be handled together

const int JIT_NOP                       0x0
const int JIT_RETURN                    0x1
const int JIT_LOAD_I32              0x2
const int JIT_BRANCH                    0x3
const int JIT_LOAD_STRING               0x4
const int JIT_CALLVIRT_O                0x5
const int JIT_CALL_NATIVE               0x6
const int JIT_CALL_O                    0x7
const int JIT_NEWOBJECT             0x8
const int JIT_LOAD_PARAMLOCAL_ADDR  0x9
const int JIT_CALL_PTR              0xa
const int JIT_BOX_CALLVIRT          0xb
const int JIT_INIT_VALUETYPE            0xc
const int JIT_NEW_VECTOR                0xd
const int JIT_NEWOBJECT_VALUETYPE       0xe
const int JIT_IS_INSTANCE               0xf
const int JIT_LOAD_NULL             0x10
const int JIT_UNBOX2VALUETYPE           0x11
const int JIT_UNBOX2OBJECT          0x12
const int JIT_LOAD_FIELD_ADDR           0x13
const int JIT_DUP_GENERAL               0x14
const int JIT_POP                       0x15
const int JIT_STORE_OBJECT_VALUETYPE    0x16
const int JIT_DEREF_CALLVIRT            0x17
const int JIT_STORE_ELEMENT         0x18
const int JIT_LEAVE                 0x19
const int JIT_END_FINALLY               0x1a
const int JIT_THROW                 0x1b
const int JIT_RETHROW                   0x1c
const int JIT_LOADOBJECT                0x1d
const int JIT_LOAD_VECTOR_LEN           0x1e
const int JIT_SWITCH                    0x1f
const int JIT_LOAD_ELEMENT_ADDR     0x20
const int JIT_CALL_INTERFACE            0x21
const int JIT_CAST_CLASS                0x22
const int JIT_LOAD_ELEMENT          0x23
const int JIT_LOADFIELD_VALUETYPE       0x24
const int   JIT_LOADFIELD               0x25
const int JIT_LOADFUNCTION          0x26
const int JIT_INVOKE_DELEGATE           0x27
const int JIT_CALL_PINVOKE          0x28
const int JIT_LOAD_I64              0x29
const int JIT_INIT_OBJECT               0x2a
const int JIT_DUP_4                 0x2b
const int JIT_DUP_8                 0x2c
const int JIT_LOADSTATICFIELDADDRESS_CHECKTYPEINIT  0x2d
const int JIT_POP_4                 0x2e
const int JIT_LOAD_F32              0x2f

const int JIT_LOADPARAMLOCAL_TYPEID     0x30
const int JIT_LOADPARAMLOCAL_INT64      (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_LOADPARAMLOCAL_INT32      (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_LOADPARAMLOCAL_INTNATIVE  (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_LOADPARAMLOCAL_F            (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_LOADPARAMLOCAL_F32            (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_LOADPARAMLOCAL_PTR            (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_LOADPARAMLOCAL_O          (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_LOADPARAMLOCAL_TRANSPTR     (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_LOADPARAMLOCAL_F64            (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_LOADPARAMLOCAL_VALUETYPE  (JIT_LOADPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_STOREPARAMLOCAL_TYPEID        0x38
const int JIT_STOREPARAMLOCAL_INT64     (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_STOREPARAMLOCAL_INT32     (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_STOREPARAMLOCAL_INTNATIVE (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_STOREPARAMLOCAL_F           (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_STOREPARAMLOCAL_F32           (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_STOREPARAMLOCAL_PTR           (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_STOREPARAMLOCAL_O         (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_STOREPARAMLOCAL_TRANSPTR    (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_STOREPARAMLOCAL_F64           (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_STOREPARAMLOCAL_VALUETYPE (JIT_STOREPARAMLOCAL_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_STOREFIELD_TYPEID     0x48
const int JIT_STOREFIELD_INT64      (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_STOREFIELD_INT32      (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_STOREFIELD_INTNATIVE  (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_STOREFIELD_F            (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_STOREFIELD_F32            (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_STOREFIELD_PTR            (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_STOREFIELD_O          (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_STOREFIELD_TRANSPTR     (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_STOREFIELD_F64            (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_STOREFIELD_VALUETYPE  (JIT_STOREFIELD_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID  0x50
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT64       (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_INT32       (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_INTNATIVE   (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_F         (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_F32         (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_PTR     (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_O           (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_TRANSPTR  (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_F64         (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_LOADSTATICFIELD_CHECKTYPEINIT_VALUETYPE   (JIT_LOADSTATICFIELD_CHECKTYPEINIT_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_LOADSTATICFIELD_TYPEID        0x58
const int JIT_LOADSTATICFIELD_INT64     (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_LOADSTATICFIELD_INT32     (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_LOADSTATICFIELD_INTNATIVE (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_LOADSTATICFIELD_F           (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_LOADSTATICFIELD_F32           (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_LOADSTATICFIELD_PTR           (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_LOADSTATICFIELD_O         (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_LOADSTATICFIELD_TRANSPTR    (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_LOADSTATICFIELD_F64           (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_LOADSTATICFIELD_VALUEPTYE (JIT_LOADSTATICFIELD_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_STORESTATICFIELD_TYPEID       0x60
const int JIT_STORESTATICFIELD_INT64        (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_STORESTATICFIELD_INT32        (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_STORESTATICFIELD_INTNATIVE    (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_STORESTATICFIELD_F          (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_STORESTATICFIELD_F32      (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_STORESTATICFIELD_F64      (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_STORESTATICFIELD_PTR      (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_STORESTATICFIELD_O            (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_STORESTATICFIELD_TRANSPTR   (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_STORESTATICFIELD_VALUETYPE    (JIT_STORESTATICFIELD_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_BOX_TYPEID        0x68
const int JIT_BOX_INT64     (JIT_BOX_TYPEID + EvalStack.EVALSTACK_INT64)
const int JIT_BOX_INT32     (JIT_BOX_TYPEID + EvalStack.EVALSTACK_INT32)
const int JIT_BOX_INTNATIVE (JIT_BOX_TYPEID + EvalStack.EVALSTACK_INTNATIVE)
//const int JIT_BOX_F           (JIT_BOX_TYPEID + EvalStack.EVALSTACK_F)
const int JIT_BOX_F32           (JIT_BOX_TYPEID + EvalStack.EVALSTACK_F32)
const int JIT_BOX_PTR           (JIT_BOX_TYPEID + EvalStack.EVALSTACK_PTR)
const int JIT_BOX_O         (JIT_BOX_TYPEID + EvalStack.EVALSTACK_O)
//const int JIT_BOX_TRANSPTR    (JIT_BOX_TYPEID + EvalStack.EVALSTACK_TRANSPTR)
const int JIT_BOX_F64           (JIT_BOX_TYPEID + EvalStack.EVALSTACK_F64)
const int JIT_BOX_VALUETYPE (JIT_BOX_TYPEID + EvalStack.EVALSTACK_VALUETYPE)

const int JIT_CEQ_I32I32            0x70
const int JIT_CGT_I32I32            0x71
const int JIT_CGT_UN_I32I32     0x72
const int JIT_CLT_I32I32            0x73
const int JIT_CLT_UN_I32I32     0x74

const int JIT_CEQ_I64I64            0x75
const int JIT_CGT_I64I64            0x76
const int JIT_CGT_UN_I64I64     0x77
const int JIT_CLT_I64I64            0x78
const int JIT_CLT_UN_I64I64     0x79

const int JIT_ADD_OVF_I32I32        0x7a
const int JIT_ADD_OVF_UN_I32I32 0x7b
const int JIT_MUL_OVF_I32I32        0x7c
const int JIT_MUL_OVF_UN_I32I32 0x7d
const int JIT_SUB_OVF_I32I32        0x7e
const int JIT_SUB_OVF_UN_I32I32 0x7f
const int JIT_ADD_I32I32            0x80
const int JIT_SUB_I32I32            0x81
const int JIT_MUL_I32I32            0x82
const int JIT_DIV_I32I32            0x83
const int JIT_DIV_UN_I32I32     0x84
const int JIT_REM_I32I32            0x85
const int JIT_REM_UN_I32I32     0x86
const int JIT_AND_I32I32            0x87
const int JIT_OR_I32I32         0x88
const int JIT_XOR_I32I32            0x89

const int JIT_NEG_I32               0x8a
const int JIT_NOT_I32               0x8b
const int JIT_NEG_I64               0x8c
const int JIT_NOT_I64               0x8d

const int JIT_BOX_NULLABLE      0x8e
const int JIT_LOAD_F64          0x8f

const int JIT_BEQ_I32I32            0x90
const int JIT_BGE_I32I32            0x91
const int JIT_BGT_I32I32            0x92
const int JIT_BLE_I32I32            0x93
const int JIT_BLT_I32I32            0x94
const int JIT_BNE_UN_I32I32     0x95
const int JIT_BGE_UN_I32I32     0x96
const int JIT_BGT_UN_I32I32     0x97
const int JIT_BLE_UN_I32I32     0x98
const int JIT_BLT_UN_I32I32     0x99

const int JIT_BEQ_I64I64            0x9a
const int JIT_BGE_I64I64            0x9b
const int JIT_BGT_I64I64            0x9c
const int JIT_BLE_I64I64            0x9d
const int JIT_BLT_I64I64            0x9e
const int JIT_BNE_UN_I64I64     0x9f
const int JIT_BGE_UN_I64I64     0xa0
const int JIT_BGT_UN_I64I64     0xa1
const int JIT_BLE_UN_I64I64     0xa2
const int JIT_BLT_UN_I64I64     0xa3

const int JIT_SHL_I32               0xa8
const int JIT_SHR_I32               0xa9
const int JIT_SHR_UN_I32            0xaa
const int JIT_SHL_I64               0xab
const int JIT_SHR_I64               0xac
const int JIT_SHR_UN_I64            0xad

const int JIT_LOADTOKEN_BASE        0xb0
const int JIT_LOADTOKEN_TYPE        (JIT_LOADTOKEN_BASE + 0)
const int JIT_LOADTOKEN_METHOD  (JIT_LOADTOKEN_BASE + 1)
const int JIT_LOADTOKEN_FIELD       (JIT_LOADTOKEN_BASE + 2)

const int JIT_LOADINDIRECT_I8       0xb3
const int JIT_LOADINDIRECT_U8       0xb4
const int JIT_LOADINDIRECT_I16  0xb5
const int JIT_LOADINDIRECT_U16  0xb6
const int JIT_LOADINDIRECT_I32  0xb7
const int JIT_LOADINDIRECT_U32  0xb8
const int JIT_LOADINDIRECT_I64  0xb9
const int JIT_LOADINDIRECT_I        0xba
const int JIT_LOADINDIRECT_R32  0xbb
const int JIT_LOADINDIRECT_R64  0xbc
const int JIT_LOADINDIRECT_REF  0xbd

const int JIT_STOREINDIRECT_REF 0xbe
const int JIT_STOREINDIRECT_U8  0xbf
const int JIT_STOREINDIRECT_U16 0xc0
const int JIT_STOREINDIRECT_U32 0xc1
const int JIT_STOREINDIRECT_U64 0xc2
const int JIT_STOREINDIRECT_R32 0xc3
const int JIT_STOREINDIRECT_R64 0xc4

//const int JIT_CONV_SIGNED32       0xc5
//const int JIT_CONV_UNSIGNED32     0xc6
//const int JIT_CONV_INT_I64        0xc7

//const int JIT_CONV_I1             0xc5
//const int JIT_CONV_I2             0xc6
//const int JIT_CONV_I4             0xc7
//const int JIT_CONV_I8             0xc8
//const int JIT_CONV_R4             0xc9
//const int JIT_CONV_R8             0xca
//const int JIT_CONV_U4             0xcb
//const int JIT_CONV_U8             0xcc
//const int JIT_CONV_U2             0xcd
//const int JIT_CONV_U1             0xce
//const int JIT_CONV_I_NATIVE       0xcf
//const int JIT_CONV_U_NATIVE       0xd0

//const int JIT_CONV_OVF_I1         0xd1
//const int JIT_CONV_OVF_U1         0xd2
//const int JIT_CONV_OVF_I2         0xd3
//const int JIT_CONV_OVF_U2         0xd4
//const int JIT_CONV_OVF_I4         0xd5
//const int JIT_CONV_OVF_U4         0xd6
//const int JIT_CONV_OVF_I8         0xd7
//const int JIT_CONV_OVF_U8         0xd8

const int JIT_UNBOX_NULLABLE        0xda

const int JIT_STORE_ELEMENT_32  0xde
const int JIT_STORE_ELEMENT_64  0xdf

const int JIT_LOAD_ELEMENT_I8       0xe0
const int JIT_LOAD_ELEMENT_U8       0xe1
const int JIT_LOAD_ELEMENT_I16  0xe2
const int JIT_LOAD_ELEMENT_U16  0xe3
const int JIT_LOAD_ELEMENT_I32  0xe4
const int JIT_LOAD_ELEMENT_U32  0xe5
const int JIT_LOAD_ELEMENT_I64  0xe6
const int JIT_LOAD_ELEMENT_R32  0xe7
const int JIT_LOAD_ELEMENT_R64  0xe8

const int JIT_ADD_OVF_I64I64        0xea
const int JIT_ADD_OVF_UN_I64I64 0xeb
const int JIT_MUL_OVF_I64I64        0xec
const int JIT_MUL_OVF_UN_I64I64 0xed
const int JIT_SUB_OVF_I64I64        0xee
const int JIT_SUB_OVF_UN_I64I64 0xef
const int JIT_ADD_I64I64            0xf0
const int JIT_SUB_I64I64            0xf1
const int JIT_MUL_I64I64            0xf2
const int JIT_DIV_I64I64            0xf3
const int JIT_DIV_UN_I64I64     0xf4
const int JIT_REM_I64I64            0xf5
const int JIT_REM_UN_I64I64     0xf6
const int JIT_AND_I64I64            0xf7
const int JIT_OR_I64I64         0xf8
const int JIT_XOR_I64I64            0xf9

const int JIT_CEQ_F32F32            0xfa
const int JIT_CGT_F32F32            0xfb
const int JIT_CGT_UN_F32F32     0xfc
const int JIT_CLT_F32F32            0xfd
const int JIT_CLT_UN_F32F32     0xfe

const int JIT_BEQ_F32F32            0xff
const int JIT_BGE_F32F32            0x100
const int JIT_BGT_F32F32            0x101
const int JIT_BLE_F32F32            0x102
const int JIT_BLT_F32F32            0x103
const int JIT_BNE_UN_F32F32     0x104
const int JIT_BGE_UN_F32F32     0x105
const int JIT_BGT_UN_F32F32     0x106
const int JIT_BLE_UN_F32F32     0x107
const int JIT_BLT_UN_F32F32     0x108

const int JIT_ADD_F32F32            0x109
const int JIT_SUB_F32F32            0x10a
const int JIT_MUL_F32F32            0x10b
const int JIT_DIV_F32F32            0x10c
const int JIT_DIV_UN_F32F32     0x10d // Never used
const int JIT_REM_F32F32            0x10e
const int JIT_REM_UN_F32F32     0x10f // Never used

const int JIT_CEQ_F64F64            0x110
const int JIT_CGT_F64F64            0x111
const int JIT_CGT_UN_F64F64     0x112
const int JIT_CLT_F64F64            0x113
const int JIT_CLT_UN_F64F64     0x114

const int JIT_BEQ_F64F64            0x115
const int JIT_BGE_F64F64            0x116
const int JIT_BGT_F64F64            0x117
const int JIT_BLE_F64F64            0x118
const int JIT_BLT_F64F64            0x119
const int JIT_BNE_UN_F64F64     0x11a
const int JIT_BGE_UN_F64F64     0x11b
const int JIT_BGT_UN_F64F64     0x11c
const int JIT_BLE_UN_F64F64     0x11d
const int JIT_BLT_UN_F64F64     0x11e

const int JIT_ADD_F64F64            0x11f
const int JIT_SUB_F64F64            0x120
const int JIT_MUL_F64F64            0x121
const int JIT_DIV_F64F64            0x122
const int JIT_DIV_UN_F64F64     0x123 // Never used
const int JIT_REM_F64F64            0x124
const int JIT_REM_UN_F64F64     0x125 // Never used

const int JIT_LOADPARAMLOCAL_0  0x127 // Load 4-byte param/local at offset 0
const int JIT_LOADPARAMLOCAL_1  0x128 // Load 4-byte param/local at offset 4
const int JIT_LOADPARAMLOCAL_2  0x129 // Load 4-byte param/local at offset 8
const int JIT_LOADPARAMLOCAL_3  0x12a // Load 4-byte param/local at offset 12
const int JIT_LOADPARAMLOCAL_4  0x12b // Load 4-byte param/local at offset 16
const int JIT_LOADPARAMLOCAL_5  0x12c // Load 4-byte param/local at offset 20
const int JIT_LOADPARAMLOCAL_6  0x12d // Load 4-byte param/local at offset 24
const int JIT_LOADPARAMLOCAL_7  0x12e // Load 4-byte param/local at offset 28

const int JIT_STOREPARAMLOCAL_0 0x12f // Store 4-byte param/local at offset 0
const int JIT_STOREPARAMLOCAL_1 0x130 // Store 4-byte param/local at offset 4
const int JIT_STOREPARAMLOCAL_2 0x131 // Store 4-byte param/local at offset 8
const int JIT_STOREPARAMLOCAL_3 0x132 // Store 4-byte param/local at offset 12
const int JIT_STOREPARAMLOCAL_4 0x133 // Store 4-byte param/local at offset 16
const int JIT_STOREPARAMLOCAL_5 0x134 // Store 4-byte param/local at offset 20
const int JIT_STOREPARAMLOCAL_6 0x135 // Store 4-byte param/local at offset 24
const int JIT_STOREPARAMLOCAL_7 0x136 // Store 4-byte param/local at offset 28

const int JIT_LOAD_I4_M1            0x137
const int JIT_LOAD_I4_0         0x138
const int JIT_LOAD_I4_1         0x139
const int JIT_LOAD_I4_2         0x13a

const int JIT_LOADFIELD_4           0x13b
const int JIT_LOADFIELD_8           0x13c

const int JIT_CONV_OFFSET_I32 0
const int JIT_CONV_OFFSET_U32 1
const int JIT_CONV_OFFSET_I64 2
const int JIT_CONV_OFFSET_U64 3
const int JIT_CONV_OFFSET_R32 4
const int JIT_CONV_OFFSET_R64 5

const int JIT_CONV_FROM_I32     0x140
const int JIT_CONV_I32_I32      (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_I32)
const int JIT_CONV_I32_U32      (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_U32)
const int JIT_CONV_I32_I64      (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_I64)
const int JIT_CONV_I32_U64      (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_U64)
const int JIT_CONV_I32_R32      (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_R32)
const int JIT_CONV_I32_R64      (JIT_CONV_FROM_I32 + JIT_CONV_OFFSET_R64)

const int JIT_CONV_FROM_U32     0x146
const int JIT_CONV_U32_I32      (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_I32)
const int JIT_CONV_U32_U32      (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_U32)
const int JIT_CONV_U32_I64      (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_I64)
const int JIT_CONV_U32_U64      (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_U64)
const int JIT_CONV_U32_R32      (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_R32)
const int JIT_CONV_U32_R64      (JIT_CONV_FROM_U32 + JIT_CONV_OFFSET_R64)

const int JIT_CONV_FROM_I64     0x14c
const int JIT_CONV_I64_I32      (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_I32)
const int JIT_CONV_I64_U32      (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_U32)
const int JIT_CONV_I64_I64      (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_I64) // Not used
const int JIT_CONV_I64_U64      (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_U64) // Not used
const int JIT_CONV_I64_R32      (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_R32)
const int JIT_CONV_I64_R64      (JIT_CONV_FROM_I64 + JIT_CONV_OFFSET_R64)

const int JIT_CONV_FROM_U64     0x152
const int JIT_CONV_U64_I32      (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_I32)
const int JIT_CONV_U64_U32      (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_U32)
const int JIT_CONV_U64_I64      (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_I64) // Not used
const int JIT_CONV_U64_U64      (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_U64) // Not used
const int JIT_CONV_U64_R32      (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_R32)
const int JIT_CONV_U64_R64      (JIT_CONV_FROM_U64 + JIT_CONV_OFFSET_R64)

const int JIT_CONV_FROM_R32     0x158
const int JIT_CONV_R32_I32      (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_I32)
const int JIT_CONV_R32_U32      (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_U32)
const int JIT_CONV_R32_I64      (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_I64)
const int JIT_CONV_R32_U64      (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_U64)
const int JIT_CONV_R32_R32      (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_R32)
const int JIT_CONV_R32_R64      (JIT_CONV_FROM_R32 + JIT_CONV_OFFSET_R64)

const int JIT_CONV_FROM_R64     0x15e
const int JIT_CONV_R64_I32      (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_I32)
const int JIT_CONV_R64_U32      (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_U32)
const int JIT_CONV_R64_I64      (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_I64)
const int JIT_CONV_R64_U64      (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_U64)
const int JIT_CONV_R64_R32      (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_R32)
const int JIT_CONV_R64_R64      (JIT_CONV_FROM_R64 + JIT_CONV_OFFSET_R64)

const int JIT_INVOKE_SYSTEM_REFLECTION_METHODBASE          0x164
const int JIT_REFLECTION_DYNAMICALLY_BOX_RETURN_VALUE      0x165

const int JIT_BRANCH_FALSE_U32  0x166
const int JIT_BRANCH_TRUE_U32       0x167
const int JIT_BRANCH_FALSE_U64  0x168
const int JIT_BRANCH_TRUE_U64       0x169

#endif
