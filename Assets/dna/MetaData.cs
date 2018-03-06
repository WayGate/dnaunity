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

#if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
using SIZE_T = System.UInt32;
using PTR = System.UInt32;
#else
using SIZE_T = System.UInt64;
using PTR = System.UInt64;
#endif 

namespace DnaUnity
{

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMetaDataStrings
    {
        // The start of the string heap
        public byte *pStart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMetaDataBlobs 
    {
        // The start of the blob heap
        public byte *pStart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMetaDataUserStrings 
    {
        // The start of the user string heap
        public byte *pStart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMetaDataGUIDs 
    {
        // The total number of GUIDs
        public uint numGUIDs;
        // Pointer to the first GUID
        public byte *pGUID1;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tTables
    {
        // The number of rows in each table
        public fixed uint numRows[MetaData.MAX_TABLES];
        // The table data itself. 64 pointers to table data
        // See MetaDataTables.h for each table structure
        public fixed /*void**/PTR data[MetaData.MAX_TABLES];

        // Should each coded index lookup type use 16 or 32 bit indexes?
        public fixed byte codedIndex32Bit[13];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMetaData
    {
        public tMetaDataStrings strings;
        public tMetaDataBlobs blobs;
        public tMetaDataUserStrings userStrings;
        public tMetaDataGUIDs GUIDs;
        public tTables tables;

        public void* monoAssembly;
        public tMetaData** ppChildMetaData;

        public System.Reflection.Assembly GetMonoAssembly()
        {
            return monoAssembly != null ?
                H.ToObj(monoAssembly) as System.Reflection.Assembly : null;
        }

        public byte index32BitString, index32BitBlob, index32BitGUID;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tParameter
    {
        // The type of the parameter on the stack (will be IntPtr if this is a ref, in, or out param)
        public tMD_TypeDef *pStackTypeDef;
        // The type of the parameter of a byref type (ref, in, or out param) or null if not a byref param
        public tMD_TypeDef* pByRefTypeDef;
        // The offset for this parameter into the paramater stack (in bytes)
        public uint offset;
        // The size of this value on the parameter stack (in bytes)
        public uint size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tInterfaceMap
    {
        // The interface this is implementing
        public tMD_TypeDef *pInterface;
        // The vTable for this interface implementation
        public uint *pVTableLookup;
        // The direct method table for this interface. This is only used for special auto-generated interfaces
        public tMD_MethodDef **ppMethodVLookup;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe static partial class MetaData
    {
        public const int MAX_TABLES = 48;

        public const uint TYPEATTRIBUTES_INTERFACE                 = 0x20;

        public const uint METHODATTRIBUTES_STATIC                  = 0x10;
        public const uint METHODATTRIBUTES_VIRTUAL                 = 0x40;
        public const uint METHODATTRIBUTES_NEWSLOT                 = 0x100;
        public const uint METHODATTRIBUTES_PINVOKEIMPL             = 0x2000;

        public const uint METHODIMPLATTRIBUTES_CODETYPE_MASK       = 0x3;
        public const uint METHODIMPLATTRIBUTES_CODETYPE_RUNTIME    = 0x3;
        public const uint METHODIMPLATTRIBUTES_INTERNALCALL        = 0x1000;

        public const uint FIELDATTRIBUTES_STATIC                   = 0x10;
        public const uint FIELDATTRIBUTES_LITERAL                  = 0x40; // compile-time constant
        public const uint FIELDATTRIBUTES_HASFIELDRVA              = 0x100;

        public const uint SIG_METHODDEF_DEFAULT                    = 0x0;
        public const uint SIG_METHODDEF_VARARG                     = 0x5;
        public const uint SIG_METHODDEF_GENERIC                    = 0x10;
        public const uint SIG_METHODDEF_HASTHIS                    = 0x20;
        public const uint SIG_METHODDEF_EXPLICITTHIS               = 0x40;
        public const uint SIG_METHODDEF_SENTINEL                   = 0x41;

        public const uint IMPLMAP_FLAGS_CHARSETMASK                = 0x0006;
        public const uint IMPLMAP_FLAGS_CHARSETNOTSPEC             = 0x0000;
        public const uint IMPLMAP_FLAGS_CHARSETANSI                = 0x0002;
        public const uint IMPLMAP_FLAGS_CHARSETUNICODE             = 0x0004;
        public const uint IMPLMAP_FLAGS_CHARSETAUTO                = 0x0006;

        public static bool TYPE_ISARRAY(tMD_TypeDef* pType) 
        { 
            return ((pType)->pArrayElementType != null); 
        }

        public static bool TYPE_ISINTERFACE(tMD_TypeDef* pType) 
        { 
            return ((pType)->flags & TYPEATTRIBUTES_INTERFACE) != 0; 
        }

        public static bool TYPE_ISGENERICINSTANCE(tMD_TypeDef* pType) 
        { 
            return ((pType)->pGenericDefinition != null); 
        }

        public static bool METHOD_ISVIRTUAL(tMD_MethodDef* pMethod) 
        { 
            return ((pMethod)->flags & METHODATTRIBUTES_VIRTUAL) != 0; 
        }

        public static bool METHOD_ISSTATIC(tMD_MethodDef* pMethod) 
        { 
            return ((pMethod)->flags & METHODATTRIBUTES_STATIC) != 0; 
        }

        public static bool METHOD_ISNEWSLOT(tMD_MethodDef* pMethod) 
        { 
            return ((pMethod)->flags & METHODATTRIBUTES_NEWSLOT) != 0; 
        }

        public static bool PARAM_ISBYREF(tParameter* pParam)
        {
            return (pParam->pByRefTypeDef != null);
        }

        public static bool FIELD_HASFIELDRVA(tMD_FieldDef* pField) 
        { 
            return ((pField)->flags & FIELDATTRIBUTES_HASFIELDRVA) != 0; 
        }

        public static bool FIELD_ISLITERAL(tMD_FieldDef* pField) 
        { 
            return ((pField)->flags & FIELDATTRIBUTES_LITERAL) != 0; 
        }

        public static bool FIELD_ISSTATIC(tMD_FieldDef* pField) 
        { 
            return ((pField)->flags & FIELDATTRIBUTES_STATIC) != 0; 
        }

        public static bool IMPLMAP_ISCHARSET_NOTSPEC(tMD_ImplMap* pImplMap) 
        { 
            return (((pImplMap)->mappingFlags & IMPLMAP_FLAGS_CHARSETMASK) == IMPLMAP_FLAGS_CHARSETNOTSPEC); 
        }

        public static bool IMPLMAP_ISCHARSET_ANSI(tMD_ImplMap* pImplMap) 
        { 
            return (((pImplMap)->mappingFlags & IMPLMAP_FLAGS_CHARSETMASK) == IMPLMAP_FLAGS_CHARSETANSI); 
        }

        public static bool IMPLMAP_ISCHARSET_UNICODE(tMD_ImplMap* pImplMap) 
        { 
            return (((pImplMap)->mappingFlags & IMPLMAP_FLAGS_CHARSETMASK) == IMPLMAP_FLAGS_CHARSETUNICODE); 
        }

        public static bool IMPLMAP_ISCHARSET_AUTO(tMD_ImplMap* pImplMap) 
        { 
            return (((pImplMap)->mappingFlags & IMPLMAP_FLAGS_CHARSETMASK) == IMPLMAP_FLAGS_CHARSETAUTO); 
        }

        public static uint TABLE_ID(uint index) 
        { 
            return ((index) >> 24); 
        }

        public static uint TABLE_OFS(uint index) 
        { 
            return ((index) & 0x00ffffff); 
        }

        public static uint MAKE_TABLE_INDEX(uint table, uint index) 
        { 
            return ((/*IDX_TABLE*/uint)(((table) << 24) | ((index) & 0x00ffffff))); 
        }

        // Definition strings
        static byte** tableDefs = null;

        public const int TABLEDEFS_LENGTH = 0x2D;

        // Coded indexes use this lookup table.
        static byte** codedTags = null;

        static byte[] tableRowSize = null;

        static byte[] codedTagBits = null;

        static byte[] BuildTableRowSize(params int[] args)
        {
            byte[] rowSize = new byte[args.Length];
            for (int tableID = 0; tableID < args.Length; tableID++) {
                byte* pDef = tableDefs[tableID];
                if (pDef != null) {
                    int defLen = (int)S.strlen(pDef);
                    int rowLen = 0;
                    for (int i = 0; i < defLen; i += 2) {
                        switch ((char)pDef[i + 1]) {
                            case '*':
                                rowLen += sizeof(SIZE_T);
                                break;
                            case 'i':
                                rowLen += 4;
                                break;
                            case 's':
                                rowLen += 2;
                                break;
                            case 'c':
                                rowLen++;
                                break;
                            case 'x':
                                // Do nothing
                                break;
                            default:
                                Sys.Crash("Cannot determine length of MetaData destination definition character '%c'\n", pDef[tableID + 1]);
                                break;
                        }
                    }
                    int structLen = args[tableID];
                    if (rowLen != structLen) {
                        Sys.Crash("Metadata decoder string row len does not match target struct size %d != %d", rowLen, structLen);
                    }
                }
                rowSize[tableID] = (byte)args[tableID];
            }
            return rowSize;
        }

        public static void Init() 
        {
            /*
        Format of definition strings:
        Always 2 characters to togther. 1st character defines source, 2nd defines destination.
        Sources:
            c: 8-bit value
            s: 16-bit short
            i: 32-bit int
            S: Index into string heap
            G: Index into GUID heap
            B: Index into BLOB heap
            0: Coded index: TypeDefOrRef
            1: Coded index: HasConstant
            2: Coded index: HasCustomAttribute
            3: Coded index: HasFieldMarshall
            4: Coded index: HasDeclSecurity
            5: Coded index: MemberRefParent
            6: Coded index: HasSemantics
            7: Coded index: MethodDefOrRef
            8: Coded index: MemberForwarded
            9: Coded index: Implementation
            :: Coded index: CustomAttributeType
            ;: Coded index: ResolutionScope
            <: Coded index: TypeOrMethodDef
            \x00 - \x2c: Simple indexes into the respective table
            ^: RVA: Convert to pointer
            x: Nothing, use 0
            m: This metadata pointer
            l: (lower case L) Boolean, is this the last entry in this table?
            I: The original table index for this table item
        Destination:
            x: nowhere, ignore
            *: Pointer (also RVA)
            i: 32-bit index into relevant heap;
                Or coded index - MSB = which table, other 3 bytes = table index
                Or 32-bit int
            s: 16-bit value
            c: 8-bit value
            */

         tableDefs = S.buildArray(
                // 0x00 - Module
                "sxS*G*GxGx",
                // 0x01 - TypeRef
                "x*;ixiS*S*",
                // 0x02 - TypeDef
                "x*m*iixiS*S*0i\x04i\x06ixclcxcxcxixix*x*xixix*xixix*xixixixix*Iixix*x*x*x*xixix*xixix*x*x*x*x*",
                // 0x03 - Nothing
                null,
                // 0x04 - FieldDef
                "x*m*ssxsxiS*B*x*x*xixiIixix*x*x*x*",
                // 0x05 - Nothing
                null,
                // 0x06 - MethodDef
                "x*m*^*ssssxiS*B*\x08ixix*xixix*xixix*x*Iixix*x*x*x*"
                #if DIAG_METHOD_CALLS
                "xixix*"
                #endif
                ,
                // 0x07 - Nothing
                null,
                // 0x08 - Param
                "ssssxiS*",
                // 0x09 - InterfaceImpl
                "\x02i0i",
                // 0x0A - MemberRef
                "x*5ixiS*B*",
                // 0x0B - Constant
                "ccccxs1iB*",
                // 0x0C - CustomAttribute
                "2i:iB*",
                // 0x0D - FieldMarshal
                "3ixiB*",
                // 0x0E - DeclSecurity
                "ssxs4iB*",
                // 0x0F - ClassLayout
                "ssxsii\x02i",
                // 0x10 - FieldLayout
                "ii\x04i",
                // 0x11 - StandAloneSig
                "B*",
                // 0x12 - EventMap
                "\x02i\x14i",
                // 0x13 - Nothing
                null,
                // 0x14 - Event
                "ssxsxiS*0ixi",
                // 0x15 - PropertyMap
                "\x02i\x17i",
                // 0x16 - Nothing
                null,
                // 0x17 - Property
                "ssxsxiS*B*",
                // 0x18 - MethodSemantics
                "ssxs\x06i6i",
                // 0x19 - MethodImpl
                "\x02i7i7i",
                // 0x1A - ModuleRef
                "S*",
                // 0x1B - TypeSpec
                "x*m*B*",
                // 0x1C - ImplMap
                "ssxs8iS*\x1aixi",
                // 0x1D - FieldRVA
                "^*\x04ixi",
                // 0x1E - Nothing
                null,
                // 0x1F - Nothing
                null,
                // 0x20 - Assembly
                "iissssssssiiB*S*S*",
                // 0x21 - Nothing
                null,
                // 0x22 - Nothing
                null,
                // 0x23 - AssemblyRef
                "ssssssssiixiB*S*S*B*",
                // 0x24 - Nothing
                null,
                // 0x25 - Nothing
                null,
                // 0x26 - Nothing
                null,
                // 0x27 - Nothing
                null,
                // 0x28 - ManifestResource
                "iiiiS*9ixi",
                // 0x29 - NestedClass
                "\x02i\x02i",
                // 0x2A - GenericParam
                "ssss<iS*",
                // 0x2B - MethodSpec
                "x*m*7ixiB*",
                // 0x2C - GenericParamConstraint
                "\x2a*0ixi"
            );

            tableRowSize = BuildTableRowSize(
                // 0x00 - Module
                sizeof(tMD_Module),
                // 0x01 - TypeRef
                sizeof(tMD_TypeRef),
                // 0x02 - TypeDef
                sizeof(tMD_TypeDef),
                // 0x03 - Nothing
                0,
                // 0x04 - FieldDef
                sizeof(tMD_FieldDef),
                // 0x05 - Nothing
                0,
                // 0x06 - MethodDef
                sizeof(tMD_MethodDef),
                // 0x07 - Nothing
                0,
                // 0x08 - Param
                sizeof(tMD_Param),
                // 0x09 - InterfaceImpl
                sizeof(tMD_InterfaceImpl),
                // 0x0A - MemberRef
                sizeof(tMD_MemberRef),
                // 0x0B - Constant
                sizeof(tMD_Constant),
                // 0x0C - CustomAttribute
                sizeof(tMD_CustomAttribute),
                // 0x0D - FieldMarshal
                sizeof(tMD_FieldMarshal),
                // 0x0E - DeclSecurity
                sizeof(tMD_DeclSecurity),
                // 0x0F - ClassLayout
                sizeof(tMD_ClassLayout),
                // 0x10 - FieldLayout
                sizeof(tMD_FieldLayout),
                // 0x11 - StandAloneSig
                sizeof(tMD_StandAloneSig),
                // 0x12 - EventMap
                sizeof(tMD_EventMap),
                // 0x13 - Nothing
                0,
                // 0x14 - Event
                sizeof(tMD_Event),
                // 0x15 - PropertyMap
                sizeof(tMD_PropertyMap),
                // 0x16 - Nothing
                0,
                // 0x17 - Property
                sizeof(tMD_Property),
                // 0x18 - MethodSemantics
                sizeof(tMD_MethodSemantics),
                // 0x19 - MethodImpl
                sizeof(tMD_MethodImpl),
                // 0x1A - ModuleRef
                sizeof(tMD_ModuleRef),
                // 0x1B - TypeSpec
                sizeof(tMD_TypeSpec),
                // 0x1C - ImplMap
                sizeof(tMD_ImplMap),
                // 0x1D - FieldRVA
                sizeof(tMD_FieldRVA),
                // 0x1E - Nothing
                0,
                // 0x1F - Nothing
                0,
                // 0x20 - Assembly
                sizeof(tMD_Assembly),
                // 0x21 - Nothing
                0,
                // 0x22 - Nothing
                0,
                // 0x23 - AssemblyRef
                sizeof(tMD_AssemblyRef),
                // 0x24 - Nothing
                0,
                // 0x25 - Nothing
                0,
                // 0x26 - Nothing
                0,
                // 0x27 - Nothing
                0,
                // 0x28 - ManifestResource
                sizeof(tMD_ManifestResource),
                // 0x29 - NestedClass
                sizeof(tMD_NestedClass),
                // 0x2A - GenericParam
                sizeof(tMD_GenericParam),
                // 0x2B - MethodSpec
                sizeof(tMD_MethodSpec),
                // 0x2C - GenericParamConstraint
                sizeof(tMD_GenericParamConstraint)
            );

            // Coded indexes use this lookup table.
            // Note that the extra 'z' characters are important!
            // (Because of how the lookup works each string must be a power of 2 in length)
            codedTags = S.buildArray(
                // TypeDefOrRef
                "\x02\x01\x1Bz",
                // HasConstant
                "\x04\x08\x17z",
                // HasCustomAttribute
                "\x06\x04\x01\x02\x08\x09\x0A\x00\x0E\x17\x14\x11\x1A\x1B\x20\x23\x26\x27\x28zzzzzzzzzzzzz",
                // HasFieldMarshall
                "\x04\x08",
                // HasDeclSecurity
                "\x02\x06\x20z",
                // MemberRefParent
                "\x02\x01\x1A\x06\x1Bzzz",
                // HasSemantics
                "\x14\x17",
                // MethodDefOrRef
                "\x06\x0A",
                // MemberForwarded
                "\x04\x06",
                // Implementation
                "\x26\x23\x27z",
                // CustomAttributeType
                "zz\x06\x0Azzzz",
                // ResolutionScope
                "\x00\x1A\x23\x01",
                // TypeOrMethodDef
                "\x02\x06"
            );

            codedTagBits = new byte[] {
                2, 2, 5, 1, 2, 3, 1, 1, 1, 2, 3, 2, 1
            };
        }

        public static void Clear()
        {
            tableDefs = null;
            codedTags = null;
            tableRowSize = null;
            codedTagBits = null;
        }

        public static uint DecodeSigEntry(/*SIG*/byte* *pSig) {
        	byte a,b,c,d;
        	a = *(*pSig)++;
        	if ((a & 0x80) == 0) {
        		// 1-byte entry
        		return a;
        	}
        	// Special case
        	if (a == 0xff) {
        		return 0;
        	}

        	b = *(*pSig)++;
        	if ((a & 0xc0) == 0x80) {
        		// 2-byte entry
                return (uint)(((int)(a & 0x3f)) << 8 | b);
        	}
        	// 4-byte entry
        	c = *(*pSig)++;
        	d = *(*pSig)++;
            return (uint)(((int)(a & 0x1f)) << 24 | ((int)b) << 16 | ((int)c) << 8 | d);
        }

        static byte[] tableID = { MetaDataTable.MD_TABLE_TYPEDEF, MetaDataTable.MD_TABLE_TYPEREF, MetaDataTable.MD_TABLE_TYPESPEC, 0 };

        public static /*IDX_TABLE*/uint DecodeSigEntryToken(/*SIG*/byte* *pSig) 
        {
        	uint entry = DecodeSigEntry(pSig);
        	return MetaData.MAKE_TABLE_INDEX(tableID[entry & 0x3], entry >> 2);
        }

        public static tMetaData* New() 
        {
            tMetaData *pRet = ((tMetaData*)Mem.malloc((SIZE_T)sizeof(tMetaData)));
            Mem.memset(pRet, 0, (SIZE_T)sizeof(tMetaData));
        	return pRet;
        }

        public static void LoadStrings(tMetaData *pThis, void *pStream, uint streamLen) 
        {
        	pThis->strings.pStart = (byte*)pStream;

        	Sys.log_f(1, "Loaded strings\n");
        }

        public static uint DecodeHeapEntryLength(byte **ppHeapEntry) 
        {
        	return DecodeSigEntry((/*SIG*/byte**)ppHeapEntry);
        }

        public static void LoadBlobs(tMetaData *pThis, void *pStream, uint streamLen) 
        {
        	pThis->blobs.pStart = (byte*)pStream;

            Sys.log_f(1, "Loaded blobs\n");

        }

        public static void LoadUserStrings(tMetaData *pThis, void *pStream, uint streamLen) 
        {
        	pThis->userStrings.pStart = (byte*)pStream;

            Sys.log_f(1, "Loaded User Strings\n");
        }

        public static void LoadGUIDs(tMetaData *pThis, void *pStream, uint streamLen) 
        {
        	pThis->GUIDs.numGUIDs = streamLen / 16;

        	// This is stored -16 because numbering starts from 1. This means that a simple indexing calculation
        	// can be used, as if it started from 0
        	pThis->GUIDs.pGUID1 = (byte*)pStream;

            Sys.log_f(1, "Read %d GUIDs\n", pThis->GUIDs.numGUIDs);
        }

        static uint GetU16(byte *pSource) 
        {
        	uint a,b;

        	a = pSource[0];
        	b = pSource[1];
        	return a | (b << 8);
        }

        static uint GetU32(byte *pSource) 
        {
        	uint a,b,c,d;

        	a = pSource[0];
        	b = pSource[1];
        	c = pSource[2];
        	d = pSource[3];
        	return a | (b << 8) | (c << 16) | (d << 24);
        }

        // Loads a single table, returns pointer to table in memory.
        public static void* LoadSingleTable(tMetaData *pThis, tRVA *pRVA, int tableID, void **ppTable) 
        {
            int numRows = (int)pThis->tables.numRows[tableID];
        	int rowLen = tableRowSize[tableID];
            int i, row;
        	/*char**/byte *pDef = tableDefs[tableID];
        	int defLen = (int)S.strlen(pDef);
        	void *pRet;
            byte *pSource = (byte*)*ppTable;
        	byte *pDest;
        	uint v = 0;
            SIZE_T p = 0;

        	// Allocate memory for destination table
            pRet = Mem.malloc((SIZE_T)(numRows * rowLen));
            pDest = (byte*)pRet;

        	// Load table
            int srcLen = 0;
        	for (row=0; row<numRows; row++) {
                byte* pSrcStart = pSource;
        		for (i=0; i<defLen; i += 2) {
        			byte d = pDef[i];
        			if (d < MAX_TABLES) {
        				if (pThis->tables.numRows[d] < 0x10000) {
        					// Use 16-bit offset
        					v = GetU16(pSource);
        					pSource += 2;
        				} else {
        					// Use 32-bit offset
        					v = GetU32(pSource);
        					pSource += 4;
        				}
                        v |= (uint)d << 24;
        			} else {
                        switch ((char)d) {
        					case 'c': // 8-bit value
        						v = *(byte*)pSource;
        						pSource++;
        						break;
        					case 's': // 16-bit short
        						v = GetU16(pSource);
        						pSource += 2;
        						break;
        					case 'i': // 32-bit int
        						v = GetU32(pSource);
        						pSource += 4;
        						break;
        					case '0':
        					case '1':
        					case '2':
        					case '3':
        					case '4':
        					case '5':
        					case '6':
        					case '7':
        					case '8':
        					case '9':
        					case ':':
        					case ';':
        					case '<':
        						{
        							int ofs = pDef[i] - '0';
        							/*char*/byte* pCoding = codedTags[ofs];
        							int tagBits = codedTagBits[ofs];
                                    byte tag = (byte)(*pSource & ((1 << tagBits) - 1));
        							int idxIntoTableID = pCoding[tag]; // The actual table index that we're looking for
        							if (idxIntoTableID < 0 || idxIntoTableID > MAX_TABLES) {
        								Sys.Crash("Error: Bad table index: 0x%02x\n", idxIntoTableID);
        							}
        							if (pThis->tables.codedIndex32Bit[ofs] != 0) {
        								// Use 32-bit number
        								v = GetU32(pSource) >> tagBits;
        								pSource += 4;
        							} else {
        								// Use 16-bit number
        								v = GetU16(pSource) >> tagBits;
        								pSource += 2;
        							}
                                    v |= (uint)idxIntoTableID << 24;
        						}
        						break;
        					case 'S': // index into string heap
        						if (pThis->index32BitString != 0) {
        							v = GetU32(pSource);
        							pSource += 4;
        						} else {
        							v = GetU16(pSource);
        							pSource += 2;
        						}
        						p = (PTR)(pThis->strings.pStart + v);
                                // NOTE: Quick way to validate metadata loading, check if all strings are valid!
                                if (S.isvalidstr((byte*)p) == 0) {
                                    Sys.Crash("Invalid string %s", (PTR)p);
                                }
        						break;
        					case 'G': // index into GUID heap
        						if (pThis->index32BitGUID != 0) {
        							v = GetU32(pSource);
        							pSource += 4;
        						} else {
        							v = GetU16(pSource);
        							pSource += 2;
        						}
        						p = (PTR)(pThis->GUIDs.pGUID1 + ((v-1) * 16));
        						break;
        					case 'B': // index into BLOB heap
        						if (pThis->index32BitBlob != 0) {
        							v = GetU32(pSource);
        							pSource += 4;
        						} else {
        							v = GetU16(pSource);
        							pSource += 2;
        						}
        						p = (PTR)(pThis->blobs.pStart + v);
        						break;
        					case '^': // RVA to convert to pointer
        						v = GetU32(pSource);
        						pSource += 4;
        						p = (PTR)RVA.FindData(pRVA, v);
        						break;
        					case 'm': // Pointer to this metadata
        						p = (PTR)pThis;
        						break;
        					case 'l': // Is this the last table entry?
                                v = (row == numRows - 1) ? (uint)1 : (uint)0;
        						break;
        					case 'I': // Original table index
                                v = MetaData.MAKE_TABLE_INDEX((uint)tableID, (uint)(row + 1));
        						break;
        					case 'x': // Nothing, use 0
        						v = 0;
                                p = 0;
        						break;
        					default:
        						Sys.Crash("Cannot handle MetaData source definition character '%c' (0x%02X)\n", d, d);
                                break;
        				}
        			}
                    switch ((char)pDef[i+1]) {
        				case '*':
        					*(SIZE_T*)pDest = p;
        					pDest += sizeof(SIZE_T);
        					break;
                        case 'i':
                            *(uint*)pDest = v;
                            pDest += 4;
                            break;
        				case 's':
        					*(ushort*)pDest = (ushort)v;
        					pDest += 2;
        					break;
        				case 'c':
        					*(byte*)pDest = (byte)v;
        					pDest++;
        					break;
        				case 'x':
        					// Do nothing
        					break;
        				default:
        					Sys.Crash("Cannot handle MetaData destination definition character '%c'\n", pDef[i+1]);
                            break;
        			}
        		}
                if (srcLen == 0) {
                    srcLen = (int)(pSource - pSrcStart);
                }
        	}

        	Sys.log_f(1, "Loaded MetaData table 0x%02X; %d rows %d len\n", tableID, numRows, srcLen);

        	// Update the parameter to the position after this table
        	*ppTable = pSource;
        	// Return new table information
        	return pRet;
        }

        public static void LoadTables(tMetaData *pThis, tRVA *pRVA, void *pStream, uint streamLen) {
        	ulong valid, j;
        	byte c;
        	int i, k, numTables;
        	void *pTable;

        	c = *(byte*)&((byte*)pStream)[6];
            pThis->index32BitString = (c & 1) > 0 ? (byte)1 : (byte)0;
            pThis->index32BitGUID = (c & 2) > 0 ? (byte)1 : (byte)0;
            pThis->index32BitBlob = (c & 4) > 0 ? (byte)1 : (byte)0;

        	valid = *(ulong*)&((byte*)pStream)[8];

        	// Count how many tables there are, and read in all the number of rows of each table.
        	numTables = 0;
        	for (i=0, j=1; i<MAX_TABLES; i++, j <<= 1) {
                if ((valid & j) != 0) {
        			pThis->tables.numRows[i] = *(uint*)&((byte*)pStream)[24 + numTables * 4];
        			numTables++;
        		} else {
        			pThis->tables.numRows[i] = 0;
        			pThis->tables.data[i] = /*null*/ 0;
        		}
        	}

        	// Determine if each coded index lookup type needs to use 16 or 32 bit indexes
        	for (i=0; i<13; i++) {
        		/*char*/byte* pCoding = codedTags[i];
        		int tagBits = codedTagBits[i];
        		// Discover max table size
        		uint maxTableLen = 0;
        		for (k=0; k < (1<<tagBits); k++) {
        			byte t = pCoding[k];
        			if (t != 'z') {
        				if (pThis->tables.numRows[t] > maxTableLen) {
        					maxTableLen = pThis->tables.numRows[t];
        				}
        			}
        		}
        		if (maxTableLen < (uint)(1 << (16 - tagBits))) {
        			// Use 16-bit number
        			pThis->tables.codedIndex32Bit[i] = 0;
        		} else {
        			// Use 32-bit number
        			pThis->tables.codedIndex32Bit[i] = 1;
        		}
        	}

        	pTable = &((byte*)pStream)[24 + numTables * 4];

        	for (i=0; i<MAX_TABLES; i++) {
        		if (pThis->tables.numRows[i] > 0) {
                    if (i >= TABLEDEFS_LENGTH || tableDefs[i] == null) {
        				Sys.Crash("No table definition for MetaData table 0x%02x\n", i);
        			}
                    pThis->tables.data[i] = (PTR)LoadSingleTable(pThis, pRVA, i, &pTable);
        		}
        	}
        }

        public static byte* GetBlob(/*BLOB_*/byte* blob, uint *pBlobLength) 
        {
        	uint len = MetaData.DecodeHeapEntryLength(&blob);
        	if (pBlobLength != null) {
        		*pBlobLength = len;
        	}
        	return blob;
        }

        // Returns length in bytes, not characters
        public static /*STRING2*/ushort* GetUserString(tMetaData *pThis, /*IDX_USERSTRINGS*/uint index, uint *pStringLength) 
        {
        	byte *pString = pThis->userStrings.pStart + (index & 0x00ffffff);
        	uint len = MetaData.DecodeHeapEntryLength(&pString);
        	if (pStringLength != null) {
        		// -1 because of extra terminating character in the heap
        		*pStringLength = len - 1;
        	}
        	return (/*STRING2*/ushort*)pString;
        }

        public static void* GetTableRow(tMetaData *pThis, /*IDX_TABLE*/uint index) 
        {
        	/*char**/byte *pData;
            uint tableId;
        	
        	if (MetaData.TABLE_OFS(index) == 0) {
        		return null;
        	}
            tableId = MetaData.TABLE_ID(index);
            pData = (byte*)pThis->tables.data[tableId];
        	// Table indexes start at one, hence the -1 here.
        	return pData + (MetaData.TABLE_OFS(index) - 1) * tableRowSize[tableId];
        }

        public static void GetConstant(tMetaData *pThis, /*IDX_TABLE*/uint idx, byte* pResultMem) 
        {
        	tMD_Constant *pConst = null;

        	switch (MetaData.TABLE_ID(idx)) {
        	case MetaDataTable.MD_TABLE_FIELDDEF:
        		{
        			tMD_FieldDef *pField = (tMD_FieldDef*)MetaData.GetTableRow(pThis, idx);
        			pConst = (tMD_Constant*)pField->pMemory;
        		}
        		break;
        	default:
        		Sys.Crash("MetaData.GetConstant() Cannot handle idx: 0x%08x", idx);
                break;
        	}

        	switch (pConst->type) {
        	case Type.ELEMENT_TYPE_I4:
        		//*(uint*)pReturnMem = MetaData.DecodeSigEntry(
        		Mem.memcpy(pResultMem, pConst->value+1, 4);
        		return;
        	default:
        		Sys.Crash("MetaData.GetConstant() Cannot handle value type: 0x%02x", pConst->type);
                break;
        	}

        }

        public static void GetHeapRoots(tHeapRoots *pHeapRoots, tMetaData *pMetaData) 
        {
        	uint i, top;
        	// Go through all Type.types, getting their static variables.

        	top = pMetaData->tables.numRows[MetaDataTable.MD_TABLE_TYPEDEF];
        	for (i=1; i<=top; i++) {
        		tMD_TypeDef *pTypeDef;

        		pTypeDef = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_TYPEDEF, i));
        		if (pTypeDef->isGenericDefinition != 0) {
        			Generics.GetHeapRoots(pHeapRoots, pTypeDef);
        		} else {
        			if (pTypeDef->staticFieldSize > 0) {
        				Heap.SetRoots(pHeapRoots, pTypeDef->pStaticFields, pTypeDef->staticFieldSize);
        			}
        		}
        	}
        }

        public static void WrapMonoAssembly(tMetaData* pMetaData, System.Reflection.Assembly assembly)
        {
            System.Type[] types = assembly.GetTypes();

            pMetaData->tables.numRows[MetaDataTable.MD_TABLE_TYPEDEF] = (uint)types.Length;

            tMD_TypeDef* pTypeDefs = (tMD_TypeDef*)Mem.malloc((SIZE_T)(sizeof(tMD_TypeDef) * types.Length));
            Mem.memset(pTypeDefs, 0, (SIZE_T)(sizeof(tMD_TypeDef) * types.Length));

            for (int i=0; i<types.Length; i++) {
                tMD_TypeDef* pTypeDef = &pTypeDefs[i];
                System.Type monoType = types[i];
                pTypeDef->pMetaData = pMetaData;
                pTypeDef->name = new S(monoType.Name);
                pTypeDef->nameSpace = new S(monoType.Namespace);
                pTypeDef->monoType = new H(monoType);
                pTypeDef->flags =
                    (monoType.IsInterface ? TYPEATTRIBUTES_INTERFACE : 0);
                pTypeDef->isValueType = (byte)(monoType.IsValueType ? 1 : 0);
                pTypeDef->isGenericDefinition = (byte)(types[i].IsGenericTypeDefinition ? 1 : 0);
            }

            pMetaData->tables.data[MetaDataTable.MD_TABLE_TYPEDEF] = (PTR)pTypeDefs;

        }

    }
}
