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

    public static class CIL
    {
        public const int NOP           = 0x00;

        public const int LDARG_0       = 0x02;
        public const int LDARG_1       = 0x03;
        public const int LDARG_2       = 0x04;
        public const int LDARG_3       = 0x05;
        public const int LDLOC_0       = 0x06;
        public const int LDLOC_1       = 0x07;
        public const int LDLOC_2       = 0x08;
        public const int LDLOC_3       = 0x09;
        public const int STLOC_0       = 0x0a;
        public const int STLOC_1       = 0x0b;
        public const int STLOC_2       = 0x0c;
        public const int STLOC_3       = 0x0d;
        public const int LDARG_S       = 0x0e;
        public const int LDARGA_S      = 0x0f;
        public const int STARG_S       = 0x10;
        public const int LDLOC_S       = 0x11;
        public const int LDLOCA_S      = 0x12;
        public const int STLOC_S       = 0x13;
        public const int LDNULL        = 0x14;
        public const int LDC_I4_M1     = 0x15;
        public const int LDC_I4_0      = 0x16;
        public const int LDC_I4_1      = 0x17;
        public const int LDC_I4_2      = 0x18;
        public const int LDC_I4_3      = 0x19;
        public const int LDC_I4_4      = 0x1a;
        public const int LDC_I4_5      = 0x1b;
        public const int LDC_I4_6      = 0x1c;
        public const int LDC_I4_7      = 0x1d;
        public const int LDC_I4_8      = 0x1e;
        public const int LDC_I4_S      = 0x1f;
        public const int LDC_I4        = 0x20;
        public const int LDC_I8        = 0x21;
        public const int LDC_R4        = 0x22;
        public const int LDC_R8        = 0x23;

        public const int DUP           = 0x25;
        public const int POP           = 0x26;

        public const int CALL          = 0x28;

        public const int RET           = 0x2a;
        public const int BR_S          = 0x2b;
        public const int BRFALSE_S     = 0x2c;
        public const int BRTRUE_S      = 0x2d;
        public const int BEQ_S         = 0x2e;
        public const int BGE_S         = 0x2f;
        public const int BGT_S         = 0x30;
        public const int BLE_S         = 0x31;
        public const int BLT_S         = 0x32;
        public const int BNE_UN_S      = 0x33;
        public const int BGE_UN_S      = 0x34;
        public const int BGT_UN_S      = 0x35;
        public const int BLE_UN_S      = 0x36;
        public const int BLT_UN_S      = 0x37;
        public const int BR            = 0x38;
        public const int BRFALSE       = 0x39;
        public const int BRTRUE        = 0x3a;
        public const int BEQ           = 0x3b;
        public const int BGE           = 0x3c;
        public const int BGT           = 0x3d;
        public const int BLE           = 0x3e;
        public const int BLT           = 0x3f;
        public const int BNE_UN        = 0x40;
        public const int BGE_UN        = 0x41;
        public const int BGT_UN        = 0x42;
        public const int BLE_UN        = 0x43;
        public const int BLT_UN        = 0x44;
        public const int SWITCH        = 0x45;
        public const int LDIND_I1      = 0x46;
        public const int LDIND_U1      = 0x47;
        public const int LDIND_I2      = 0x48;
        public const int LDIND_U2      = 0x49;
        public const int LDIND_I4      = 0x4a;
        public const int LDIND_U4      = 0x4b;
        public const int LDIND_I8      = 0x4c;
        public const int LDIND_I       = 0x4d;
        public const int LDIND_R4      = 0x4e;
        public const int LDIND_R8      = 0x4f;
        public const int LDIND_REF     = 0x50;
        public const int STIND_REF     = 0x51;
        public const int STIND_I1      = 0x52;
        public const int STIND_I2      = 0x53;
        public const int STIND_I4      = 0x54;

        public const int ADD           = 0x58;
        public const int SUB           = 0x59;
        public const int MUL           = 0x5a;
        public const int DIV           = 0x5b;
        public const int DIV_UN        = 0x5c;
        public const int REM           = 0x5d;
        public const int REM_UN        = 0x5e;
        public const int AND           = 0x5f;
        public const int OR            = 0x60;
        public const int XOR           = 0x61;
        public const int SHL           = 0x62;
        public const int SHR           = 0x63;
        public const int SHR_UN        = 0x64;
        public const int NEG           = 0x65;
        public const int NOT           = 0x66;
        public const int CONV_I1       = 0x67;
        public const int CONV_I2       = 0x68;
        public const int CONV_I4       = 0x69;
        public const int CONV_I8       = 0x6a;
        public const int CONV_R4       = 0x6b;
        public const int CONV_R8       = 0x6c;
        public const int CONV_U4       = 0x6d;
        public const int CONV_U8       = 0x6e;
        public const int CALLVIRT      = 0x6f;

        public const int LDOBJ         = 0x71;
        public const int LDSTR         = 0x72;
        public const int NEWOBJ        = 0x73;
        public const int CASTCLASS     = 0x74;
        public const int ISINST        = 0x75;
        public const int CONV_R_UN     = 0x76;

        public const int THROW             = 0x7a;
        public const int LDFLD             = 0x7b;
        public const int LDFLDA            = 0x7c;
        public const int STFLD             = 0x7d;
        public const int LDSFLD            = 0x7e;
        public const int LDSFLDA           = 0x7f;
        public const int STSFLD            = 0x80;
        public const int STOBJ             = 0x81;
        public const int CONV_OVF_I1_UN    = 0x82;
        public const int CONV_OVF_I2_UN    = 0x83;
        public const int CONV_OVF_I4_UN    = 0x84;
        public const int CONV_OVF_I8_UN    = 0x85;
        public const int CONV_OVF_U1_UN    = 0x86;
        public const int CONV_OVF_U2_UN    = 0x87;
        public const int CONV_OVF_U4_UN    = 0x88;
        public const int CONV_OVF_U8_UN    = 0x89;
        public const int CONV_OVF_I_UN     = 0x8a;
        public const int CONV_OVF_U_UN     = 0x8b;
        public const int BOX               = 0x8c;
        public const int NEWARR            = 0x8d;
        public const int LDLEN             = 0x8e;
        public const int LDELEMA           = 0x8f;
        public const int LDELEM_I1         = 0x90;
        public const int LDELEM_U1         = 0x91;
        public const int LDELEM_I2         = 0x92;
        public const int LDELEM_U2         = 0x93;
        public const int LDELEM_I4         = 0x94;
        public const int LDELEM_U4         = 0x95;
        public const int LDELEM_I8         = 0x96;

        public const int LDELEM_R4     = 0x98;
        public const int LDELEM_R8     = 0x99;
        public const int LDELEM_REF    = 0x9a;

        public const int STELEM_I1     = 0x9c;
        public const int STELEM_I2     = 0x9d;
        public const int STELEM_I4     = 0x9e;
        public const int STELEM_I8     = 0x9f;
        public const int STELEM_R4     = 0xa0;
        public const int STELEM_R8     = 0xa1;
        public const int STELEM_REF    = 0xa2;
        public const int LDELEM_ANY    = 0xa3;
        public const int STELEM_ANY    = 0xa4;
        public const int UNBOX_ANY     = 0xa5;

        public const int CONV_OVF_I1   = 0xb3;
        public const int CONV_OVF_U1   = 0xb4;
        public const int CONV_OVF_I2   = 0xb5;
        public const int CONV_OVF_U2   = 0xb6;
        public const int CONV_OVF_I4   = 0xb7;
        public const int CONV_OVF_U4   = 0xb8;
        public const int CONV_OVF_I8   = 0xb9;
        public const int CONV_OVF_U8   = 0xba;

        public const int LDTOKEN       = 0xd0;
        public const int CONV_U2       = 0xd1;
        public const int CONV_U1       = 0xd2;
        public const int CONV_I        = 0xd3;

        public const int ADD_OVF       = 0xd6;
        public const int ADD_OVF_UN    = 0xd7;
        public const int MUL_OVF       = 0xd8;
        public const int MUL_OVF_UN    = 0xd9;
        public const int SUB_OVF       = 0xda;
        public const int SUB_OVF_UN    = 0xdb;
        public const int ENDFINALLY    = 0xdc;
        public const int LEAVE         = 0xdd;
        public const int LEAVE_S       = 0xde;

        public const int CONV_U        = 0xe0;

        public const int EXTENDED      = 0xfe;


        // Extended op-codes

        public const int X_CEQ          = 0x01;
        public const int X_CGT          = 0x02;
        public const int X_CGT_UN       = 0x03;
        public const int X_CLT          = 0x04;
        public const int X_CLT_UN       = 0x05;
        public const int X_LOADFUNCTION = 0x06;

        public const int X_INITOBJ      = 0x15;
        public const int X_CONSTRAINED  = 0x16;

        public const int X_RETHROW      = 0x1a;

        public const int X_READONLY     = 0x1e;
    }
}