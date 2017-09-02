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

const int CIL_NOP           0x00

const int CIL_LDARG_0       0x02
const int CIL_LDARG_1       0x03
const int CIL_LDARG_2       0x04
const int CIL_LDARG_3       0x05
const int CIL_LDLOC_0       0x06
const int CIL_LDLOC_1       0x07
const int CIL_LDLOC_2       0x08
const int CIL_LDLOC_3       0x09
const int CIL_STLOC_0       0x0a
const int CIL_STLOC_1       0x0b
const int CIL_STLOC_2       0x0c
const int CIL_STLOC_3       0x0d
const int CIL_LDARG_S       0x0e
const int CIL_LDARGA_S  0x0f
const int CIL_STARG_S       0x10
const int CIL_LDLOC_S       0x11
const int CIL_LDLOCA_S  0x12
const int CIL_STLOC_S       0x13
const int CIL_LDNULL        0x14
const int CIL_LDC_I4_M1 0x15
const int CIL_LDC_I4_0  0x16
const int CIL_LDC_I4_1  0x17
const int CIL_LDC_I4_2  0x18
const int CIL_LDC_I4_3  0x19
const int CIL_LDC_I4_4  0x1a
const int CIL_LDC_I4_5  0x1b
const int CIL_LDC_I4_6  0x1c
const int CIL_LDC_I4_7  0x1d
const int CIL_LDC_I4_8  0x1e
const int CIL_LDC_I4_S  0x1f
const int CIL_LDC_I4        0x20
const int CIL_LDC_I8        0x21
const int CIL_LDC_R4        0x22
const int CIL_LDC_R8        0x23

const int CIL_DUP           0x25
const int CIL_POP           0x26

const int CIL_CALL      0x28

const int CIL_RET           0x2a
const int CIL_BR_S      0x2b
const int CIL_BRFALSE_S 0x2c
const int CIL_BRTRUE_S  0x2d
const int CIL_BEQ_S     0x2e
const int CIL_BGE_S     0x2f
const int CIL_BGT_S     0x30
const int CIL_BLE_S     0x31
const int CIL_BLT_S     0x32
const int CIL_BNE_UN_S  0x33
const int CIL_BGE_UN_S  0x34
const int CIL_BGT_UN_S  0x35
const int CIL_BLE_UN_S  0x36
const int CIL_BLT_UN_S  0x37
const int CIL_BR            0x38
const int CIL_BRFALSE       0x39
const int CIL_BRTRUE        0x3a
const int CIL_BEQ           0x3b
const int CIL_BGE           0x3c
const int CIL_BGT           0x3d
const int CIL_BLE           0x3e
const int CIL_BLT           0x3f
const int CIL_BNE_UN        0x40
const int CIL_BGE_UN        0x41
const int CIL_BGT_UN        0x42
const int CIL_BLE_UN        0x43
const int CIL_BLT_UN        0x44
const int CIL_SWITCH        0x45
const int CIL_LDIND_I1  0x46
const int CIL_LDIND_U1  0x47
const int CIL_LDIND_I2  0x48
const int CIL_LDIND_U2  0x49
const int CIL_LDIND_I4  0x4a
const int CIL_LDIND_U4  0x4b
const int CIL_LDIND_I8  0x4c
const int CIL_LDIND_I       0x4d
const int CIL_LDIND_R4  0x4e
const int CIL_LDIND_R8  0x4f
const int CIL_LDIND_REF 0x50
const int CIL_STIND_REF 0x51
const int CIL_STIND_I1  0x52
const int CIL_STIND_I2  0x53
const int CIL_STIND_I4  0x54

const int CIL_ADD           0x58
const int CIL_SUB           0x59
const int CIL_MUL           0x5a
const int CIL_DIV           0x5b
const int CIL_DIV_UN        0x5c
const int CIL_REM           0x5d
const int CIL_REM_UN        0x5e
const int CIL_AND           0x5f
const int CIL_OR            0x60
const int CIL_XOR           0x61
const int CIL_SHL           0x62
const int CIL_SHR           0x63
const int CIL_SHR_UN        0x64
const int CIL_NEG           0x65
const int CIL_NOT           0x66
const int CIL_CONV_I1       0x67
const int CIL_CONV_I2       0x68
const int CIL_CONV_I4       0x69
const int CIL_CONV_I8       0x6a
const int CIL_CONV_R4       0x6b
const int CIL_CONV_R8       0x6c
const int CIL_CONV_U4       0x6d
const int CIL_CONV_U8       0x6e
const int CIL_CALLVIRT  0x6f

const int CIL_LDOBJ     0x71
const int CIL_LDSTR     0x72
const int CIL_NEWOBJ        0x73
const int CIL_CASTCLASS 0x74
const int CIL_ISINST        0x75
const int CIL_CONV_R_UN 0x76

const int CIL_THROW     0x7a
const int CIL_LDFLD     0x7b
const int CIL_LDFLDA        0x7c
const int CIL_STFLD     0x7d
const int CIL_LDSFLD        0x7e
const int CIL_LDSFLDA       0x7f
const int CIL_STSFLD        0x80
const int CIL_STOBJ     0x81
const int CIL_CONV_OVF_I1_UN    0x82
const int CIL_CONV_OVF_I2_UN    0x83
const int CIL_CONV_OVF_I4_UN    0x84
const int CIL_CONV_OVF_I8_UN    0x85
const int CIL_CONV_OVF_U1_UN    0x86
const int CIL_CONV_OVF_U2_UN    0x87
const int CIL_CONV_OVF_U4_UN    0x88
const int CIL_CONV_OVF_U8_UN    0x89
const int CIL_CONV_OVF_I_UN 0x8a
const int CIL_CONV_OVF_U_UN 0x8b
const int CIL_BOX           0x8c
const int CIL_NEWARR        0x8d
const int CIL_LDLEN     0x8e
const int CIL_LDELEMA       0x8f
const int CIL_LDELEM_I1 0x90
const int CIL_LDELEM_U1 0x91
const int CIL_LDELEM_I2 0x92
const int CIL_LDELEM_U2 0x93
const int CIL_LDELEM_I4 0x94
const int CIL_LDELEM_U4 0x95
const int CIL_LDELEM_I8 0x96

const int CIL_LDELEM_R4 0x98
const int CIL_LDELEM_R8 0x99
const int CIL_LDELEM_REF    0x9a

const int CIL_STELEM_I1 0x9c
const int CIL_STELEM_I2 0x9d
const int CIL_STELEM_I4 0x9e
const int CIL_STELEM_I8 0x9f
const int CIL_STELEM_R4 0xa0
const int CIL_STELEM_R8 0xa1
const int CIL_STELEM_REF    0xa2
const int CIL_LDELEM_ANY    0xa3
const int CIL_STELEM_ANY    0xa4
const int CIL_UNBOX_ANY 0xa5

const int CIL_CONV_OVF_I1   0xb3
const int CIL_CONV_OVF_U1   0xb4
const int CIL_CONV_OVF_I2   0xb5
const int CIL_CONV_OVF_U2   0xb6
const int CIL_CONV_OVF_I4   0xb7
const int CIL_CONV_OVF_U4   0xb8
const int CIL_CONV_OVF_I8   0xb9
const int CIL_CONV_OVF_U8   0xba

const int CIL_LDTOKEN       0xd0
const int CIL_CONV_U2       0xd1
const int CIL_CONV_U1       0xd2
const int CIL_CONV_I        0xd3

const int CIL_ADD_OVF       0xd6
const int CIL_ADD_OVF_UN    0xd7
const int CIL_MUL_OVF       0xd8
const int CIL_MUL_OVF_UN    0xd9
const int CIL_SUB_OVF       0xda
const int CIL_SUB_OVF_UN    0xdb
const int CIL_ENDFINALLY    0xdc
const int CIL_LEAVE     0xdd
const int CIL_LEAVE_S       0xde

const int CIL_CONV_U        0xe0

const int CIL_EXTENDED  0xfe


// Extended op-codes

const int CILX_CEQ      0x01
const int CILX_CGT      0x02
const int CILX_CGT_UN       0x03
const int CILX_CLT      0x04
const int CILX_CLT_UN       0x05
const int CILX_LOADFUNCTION 0x06

const int CILX_INITOBJ  0x15
const int CILX_CONSTRAINED 0x16

const int CILX_RETHROW  0x1a

const int CILX_READONLY 0x1e

#endif
