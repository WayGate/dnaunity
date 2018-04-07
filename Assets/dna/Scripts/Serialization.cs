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

using System.Collections.Generic;
using System.Text;

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
#endif

    #if UINTY_5 || UNITY_2017 || UNITY_2018
    using OBJ_TYPE = UnityEngine.Object;
    using OBJ_LIST = List<UnityEngine.Object>;
#else
    using OBJ_TYPE = System.Object;
    using OBJ_LIST = List<System.Object>;
#endif

    // List<T> struct
    public unsafe struct tGenericList
    {
        public tSystemArray* pItems;
        public int size;
    }

    /// <summary>
    /// Serialization information for 
    /// </summary>
    [System.Serializable]
    public class DnaSerializedTypeInfo
    {
        // Name of class who's strings this table records.
        public string name;

        // Index in type array
        public int typeCode;

        // The hash for this type (if two hashes match, type is assumed to be the same)
        public uint hash;

        // The strings (property names) for this class
        public DnaSerializedFieldInfo[] fields;

        // PTR to DNA typedef (when serializing from DNA)
        [System.NonSerialized]
        public PTR typeDef;

        // Ref to Mono System.Type (when serializing from Mono)
        [System.NonSerialized]
        public System.Type type;

        // The target type info if we're reading
        [System.NonSerialized]
        public DnaSerializedTypeInfo targetTypeInfo;

        private static uint HashString(uint hash, string s)
        {
            for (int i = 0; i < s.Length; i++) {
                hash = hash * 131 + s[i];
            }

            return hash;
        }

        private uint HashField(uint hash, DnaSerializedFieldInfo field)
        {
            hash = DnaSerializedTypeInfo.HashString(hash, name);
            hash = hash * 131 + (uint)field.typeCode;
            hash = hash * 131 + (uint)field.elementTypeCode;
            return hash;
        }

        public void GenerateHash()
        {
            uint hash = 0;

            hash = HashString(hash, name);

            for (int i = 0; i < fields.Length; i++) {
                hash = HashField(hash, fields[i]);
            }

            this.hash = hash;
        }

        public DnaSerializedFieldInfo FindField(string name)
        {
            int i;

            for (i = 0; i < fields.Length; i++) {
                if (string.CompareOrdinal(fields[i].name, name) == 0) {
                    return fields[i];
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Field info for serialized fields.
    /// </summary>
    [System.Serializable]
    public class DnaSerializedFieldInfo
    {
        // Name of the field
        public string name;
        
        // Type code for the field (<100 is .NET type code, 97 = UnityEngine.Object, 98 = Array, 99 = List<T>, >=100 is index of user defined type + 100)
        public int typeCode;

        // Type of element if array
        public int elementTypeCode;

        // This field should be skipped during reading (it was deleted, type was changed, etc.)
        [System.NonSerialized]
        public bool skip;

        // Pointer to field def (when serializing from DNA)
        [System.NonSerialized]
        public PTR fieldDef;

        // Ref to field info (when serializing from Mono)
        [System.NonSerialized]
        public System.Reflection.FieldInfo fieldInfo;

        public bool MatchesField(DnaSerializedFieldInfo field)
        {
            return string.CompareOrdinal(name, field.name) == 0 && typeCode == field.typeCode && elementTypeCode == field.elementTypeCode;
        }
    }

    public unsafe static class Serialization
    {
        const uint DEFAULT_WRITE_BUF_SIZE = 2048;

        // Serialize static fields
        static byte* pWriteBuf = null;       // A growable buffer for writing
        static byte* pWriteBufPos = null;
        static byte* pWriteBufEnd = null;
        static uint writeBufSize = 0;
        static OBJ_LIST writeObjsList;
        static Dictionary<PTR, DnaSerializedTypeInfo> dnaWriteTypeMap;
        static Dictionary<System.Type, DnaSerializedTypeInfo> monoWriteTypeMap;

        // Deserialize static fields
        static byte* pReadBuf = null;
        static byte* pReadBufPos = null;
        static Dictionary<int, DnaSerializedTypeInfo> readTypeMap;
        static OBJ_TYPE[] readObjList;

        // Other fields
        static uint[] memSizes;
        static tMD_TypeDef* pListTypeDef;


        public static void Init()
        {
            pWriteBuf = (byte*)Mem.malloc(DEFAULT_WRITE_BUF_SIZE);
            pWriteBufPos = pWriteBuf;
            pWriteBufEnd = pWriteBuf + DEFAULT_WRITE_BUF_SIZE;
            writeBufSize = 2048;
            writeObjsList = new OBJ_LIST();
            pListTypeDef = null;
        }

        public static void Clear()
        {
            pWriteBuf = pWriteBufPos = pReadBuf = pReadBufPos = null;
            dnaWriteTypeMap = null;
            monoWriteTypeMap = null;
            pReadBuf = pReadBufPos = null;
            readTypeMap = null;
            readObjList = null;
            memSizes = null;
            pListTypeDef = null;
        }

        private static void ExpandWriteBuf(uint size)
        {
            uint minSize = writeBufSize + size;
            uint newSize = writeBufSize;
            while (newSize < minSize) {
                newSize = newSize * 2;
            }
            uint pos = (uint)(pWriteBufPos - pWriteBuf);
            pWriteBuf = (byte*)Mem.realloc(pWriteBuf, newSize);
            pWriteBufPos = pWriteBuf + pos;
            writeBufSize = newSize;
            pWriteBufEnd = pWriteBuf + writeBufSize;
        }

        private static void WriteVarInt(uint i)
        {
            while (i != 0) {
                *(pWriteBufPos) = (byte)((i & 0x7fU) | (i > 0x7fU ? 0x80U : 0U));
                pWriteBufPos += 1;
                i = i >> 7;
            }
        }

        private static uint ReadVarInt()
        {
            byte b;
            uint i = 0;
            do {
                b = *pReadBufPos;
                pReadBuf += 1;
                i = (i << 7) | (b & 0x7FU);
            } while ((b & 0x80) != 0);
            return i;
        }

        private static void WriteString(string s)
        {
            int i;
            uint len;
            bool isAnsi;
            ushort u16;

            if (s == null) {
                *pWriteBufPos = 0; // Null string
                pWriteBufPos += 1;
            } else {
                len = (uint)s.Length;
                // Check if string is 7-bit ascii (is ansi)
                isAnsi = true;
                for (i = 0; i < len; i++) {
                    if ((ushort)s[i] > 0x7F) {
                        isAnsi = false;
                        break;
                    }
                }
                if (isAnsi) {
                    while (pWriteBufPos + (len + 8) >= pWriteBufEnd) {
                        ExpandWriteBuf(len + 8);
                    }
                    *pWriteBufPos = 1; // Is Ansi (8 bit)
                    pWriteBufPos++;
                    WriteVarInt(len);
                    for (i = 0; i < len; i++) {
                        *(pWriteBufPos) = (byte)s[i];
                        pWriteBufPos += 1;
                    }
                } else {
                    if (pWriteBufPos + ((len << 1) + 8) >= pWriteBufEnd) {
                        ExpandWriteBuf((len << 1) + 8);
                    }
                    *pWriteBufPos = 2; // Is UTF16 (16 bit)
                    pWriteBufPos++;
                    WriteVarInt(len);
                    for (i = 0; i < len; i++) {
                        u16 = (ushort)s[i];
                        *(pWriteBufPos) = (byte)u16;
                        *(pWriteBufPos + 1) = (byte)(u16 >> 8);
                        pWriteBufPos += 2;
                    }

                }
            }
        }

        private static string ReadString()
        {
            byte b;
            int len;
            int i;
            char* pCharBuf;

            b = *pReadBuf;
            pReadBuf += 1;

            if (b == 0) {
                return null;
            } else if (b == 1) {
                len = (int)ReadVarInt();
                StringBuilder sb = new StringBuilder(len);
                for (i = 0; i < len; i++) {
                    sb.Append((char)pReadBuf[i]);
                }
                pReadBuf += len;
                return sb.ToString();
            } else {
                len = (int)ReadVarInt();
                StringBuilder sb = new StringBuilder(len);
                pCharBuf = (char*)pReadBuf;
                for (i = 0; i < len; i++) {
                    sb.Append(pCharBuf[i]);
                }
                pReadBuf += len + len;
                return sb.ToString();
            }
        }

        private static uint GetMemSize(int typeCode)
        {
            if (memSizes == null) {
                memSizes = new uint[] {
                    0,                  // Empty = 0,          // Null reference
                    (uint)sizeof(void*), // Object = 1,         // Instance that isn't a value
                    0,                  // DBNull = 2,         // Database null value
                    1,                  // Boolean = 3,        // Boolean
                    2,                  // Char = 4,           // Unicode character
                    1,                  // SByte = 5,          // Signed 8-bit integer
                    1,                  // Byte = 6,           // Unsigned 8-bit integer
                    2,                  // Int16 = 7,          // Signed 16-bit integer
                    2,                  // UInt16 = 8,         // Unsigned 16-bit integer
                    4,                  // Int32 = 9,          // Signed 32-bit integer
                    4,                  // UInt32 = 10,        // Unsigned 32-bit integer
                    8,                  // Int64 = 11,         // Signed 64-bit integer
                    8,                  // UInt64 = 12,        // Unsigned 64-bit integer
                    4,                  // Single = 13,        // IEEE 32-bit float
                    8,                  // Double = 14,        // IEEE 64-bit double
                    (uint)sizeof(decimal),          // Decimal = 15,       // Decimal
                    (uint)sizeof(System.DateTime),  // DateTime = 16,      // DateTime
                    (uint)sizeof(void*)             // String = 18,        // Unicode character string
                };
            }
            return typeCode <= 19 ? memSizes[typeCode] : 0;
        }

        // *****************
        // DNA SERIALIZATION
        // *****************

        private static DnaSerializedTypeInfo BuildDnaTypeInfo(tMD_TypeDef* pTypeDef)
        {
            int i;
            tMD_FieldDef* pFieldDef;
            ushort flags;
            DnaSerializedTypeInfo typeInfo, fieldTypeInfo;
            List<DnaSerializedFieldInfo> fieldList;
            DnaSerializedFieldInfo fieldInfo;

            if (pTypeDef == null) {
                return null;
            }

            typeInfo = new DnaSerializedTypeInfo();
            typeInfo.name = pTypeDef->fullNameS;
            typeInfo.typeCode = dnaWriteTypeMap.Count + 100;
            typeInfo.typeDef = (PTR)pTypeDef;
            dnaWriteTypeMap.Add((PTR)pTypeDef, typeInfo);
            fieldList = new List<DnaSerializedFieldInfo>();

            for (i = 0; i < pTypeDef->numFields; i++) {

                pFieldDef = pTypeDef->ppFields[i];
                flags = pFieldDef->flags;

                if ((flags & (MetaData.FIELD_ACCESS_MASK | MetaData.FIELDATTRIBUTES_STATIC | MetaData.FIELDATTRIBUTES_NOTSERIALIZED))
                    != MetaData.FIELDATTRIBUTES_PUBLIC)
                    continue;

                fieldInfo = new DnaSerializedFieldInfo();
                fieldInfo.name = pFieldDef->nameS;
                fieldInfo.fieldDef = (PTR)pFieldDef;

                System.Type monoType = pFieldDef->pType->monoType != null ? H.ToObj(pFieldDef->pType->monoType) as System.Type : null;

                System.TypeCode typeCode = Type.GetTypeCode(pFieldDef->pType);
                if (typeCode != System.TypeCode.Object) {
                    fieldInfo.typeCode = (int)typeCode;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                } else if (monoType != null && typeof(OBJ_TYPE).IsAssignableFrom(monoType)) {
                    fieldInfo.typeCode = 97; // UnityEngine.Object type (add to object ref array, and store varint index)
#endif
                } else if (pFieldDef->pType->pArrayElementType != null) {
                    fieldInfo.typeCode = 98; // Array type
                    typeCode = Type.GetTypeCode(pFieldDef->pType->pArrayElementType);
                    if (typeCode != System.TypeCode.Object) {
                        fieldInfo.elementTypeCode = (int)typeCode;
                    } else {
                        if (!dnaWriteTypeMap.TryGetValue((PTR)pFieldDef->pType->pArrayElementType, out fieldTypeInfo)) {
                            fieldTypeInfo = BuildDnaTypeInfo(pFieldDef->pType->pArrayElementType);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else if (pFieldDef->pType->pGenericDefinition != null &&
                    S.strcmp(pFieldDef->pType->pGenericDefinition->nameSpace, "System.Collections.Generic") == 0 &&
                    S.strcmp(pFieldDef->pType->pGenericDefinition->name, "List`1") == 0) {
                    fieldInfo.typeCode = 99; // List<T> type
                    typeCode = Type.GetTypeCode(pFieldDef->pType->ppClassTypeArgs[0]);
                    if (typeCode != System.TypeCode.Object) {
                        fieldInfo.elementTypeCode = (int)typeCode;
                    } else {
                        if (!dnaWriteTypeMap.TryGetValue((PTR)pFieldDef->pType->ppClassTypeArgs[0], out fieldTypeInfo)) {
                            fieldTypeInfo = BuildDnaTypeInfo(pFieldDef->pType->ppClassTypeArgs[0]);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else { 
                    if (!dnaWriteTypeMap.TryGetValue((PTR)pFieldDef->pType, out fieldTypeInfo)) {
                        fieldTypeInfo = BuildDnaTypeInfo(pFieldDef->pType);
                    }
                    fieldInfo.typeCode = fieldTypeInfo.typeCode + 100;
                }

                fieldList.Add(fieldInfo);
            }

            typeInfo.fields = fieldList.ToArray();
            typeInfo.GenerateHash();

            return typeInfo;
        }

        private static void SerializeDnaInst(tMD_TypeDef* pTypeDef, byte* pInst)
        {
            int i, j, len, typeCode;
            tMD_FieldDef* pFieldDef;
            tMD_TypeDef* pElemType;
            uint memOffset, memSize, elemSize;
            string s;
            ushort u16;
            uint u32;
            ulong u64;
            OBJ_TYPE obj;
            byte* pPtr, pElem;
            tSystemArray* pArray;
            tGenericList* pList;
            DnaSerializedTypeInfo typeInfo;
            DnaSerializedFieldInfo fieldInfo;

            if (!dnaWriteTypeMap.TryGetValue((PTR)pTypeDef, out typeInfo)) {
                typeInfo = BuildDnaTypeInfo(pTypeDef);
            }

            for (i = 0; i < typeInfo.fields.Length; i++) {

                fieldInfo = typeInfo.fields[i];
                pFieldDef = (tMD_FieldDef*)fieldInfo.fieldDef;
                memOffset = pFieldDef->memOffset;
                memSize = pFieldDef->memSize;
                typeCode = fieldInfo.typeCode;

                // Check to see if we need to expand serialization buffer
                if (pWriteBufPos + (memSize + 8) >= pWriteBufEnd) {
                    ExpandWriteBuf(memSize + 8);
                }

                if (pFieldDef->pType->isValueType == 0) {

                    if (typeCode == (int)System.TypeCode.String) {

                        // String (special case)

                        s = System_String.ToMonoString(*(tSystemString**)(pInst + memOffset));
                        WriteString(s);

#if UNITY_5 || UNITY_2017 || UNITY_2018
                    } else if (typeCode == 97) { // UnityEngine.Object derived types

                        pPtr = *(byte**)(pInst + memOffset);
                        obj = pPtr != null ? Heap.GetMonoObject(pPtr) as OBJ_TYPE : null;
                        if (pPtr == null) {
                            *pWriteBufPos = 0;
                            pWriteBufPos += 1;
                        } else {
                            writeObjsList.Add(obj);
                            u32 = (uint)writeObjsList.Count;
                            WriteVarInt(u32);
                        }
#endif
                    } else if (typeCode == 98 || typeCode == 99) {  // Array or List<T> type

                        if (typeCode == 98) {   // Array
                            pArray = *(tSystemArray**)(pInst + memOffset);
                            if (pArray != null) {
                                len = (int)System_Array.GetLength(pArray);
                            } else {
                                len = 0;
                            }
                        } else {                // List<T>
                            pList = *(tGenericList**)(pInst + memOffset);
                            if (pList != null) {
                                pArray = pList->pItems;
                                len = pList->size;
                            } else {
                                pArray = null;
                                len = 0;
                            }
                        }

                        if (pArray != null) {
                            pPtr = tSystemArray.GetElements(pArray);
                            pElemType = pFieldDef->pType->pArrayElementType;
                            elemSize = pElemType->arrayElementSize;
                        } else {
                            pPtr = null;
                            pElemType = null;
                            elemSize = 0;
                        }

                        if (pArray == null) {

                            // 0 len is null
                            WriteVarInt(0);

                        } else {

                            // len + 1 is array
                            WriteVarInt((uint)(len + 1));

                            if (pElemType->fixedBlittable != 0) {

                                // Check to see if we need to expand serialization buffer
                                if (pWriteBufPos + (elemSize * len + 16) >= pWriteBufEnd) {
                                    ExpandWriteBuf((uint)(elemSize * len + 16));
                                }

                                Mem.memcpy(pWriteBufPos, pPtr, (SIZE_T)(elemSize * len));
                                pWriteBufPos += (elemSize * len);

                            } else if (fieldInfo.elementTypeCode == (int)System.TypeCode.String) {

                                // Check to see if we need to expand serialization buffer
                                if (pWriteBufPos + (len + 16) >= pWriteBufEnd) {
                                    ExpandWriteBuf((uint)(len + 16));
                                }

                                for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                    s = System_String.ToMonoString(*(tSystemString**)pPtr);
                                    WriteString(s);
                                }

#if UNITY_5 || UNITY_2017 || UNITY_2018

                            } else if (fieldInfo.elementTypeCode == 97) {  // UnityEngine.Object derived type

                                // Check to see if we need to expand serialization buffer
                                if (pWriteBufPos + (len + 16) >= pWriteBufEnd) {
                                    ExpandWriteBuf((uint)(len + 16));
                                }

                                for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                    pInst = *(byte**)(pPtr);
                                    obj = (pInst != null ? Heap.GetMonoObject(pInst) as OBJ_TYPE : null);
                                    if (obj == null) {
                                        *pWriteBufPos = 0;
                                        pWriteBufPos += 1;
                                    } else {
                                        writeObjsList.Add(obj);
                                        u32 = (uint)writeObjsList.Count;
                                        WriteVarInt(u32);
                                    }
                                }

#endif
                            } else {

                                if (pElemType->isValueType != 0) {
                                    for (j = 0; j < len; j++, pPtr += elemSize) {
                                        SerializeDnaInst(pElemType, pPtr);
                                    }
                                } else {
                                    for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                        pInst = *(byte**)pPtr;
                                        if (pInst == null) {
                                            *pWriteBufPos = 0;
                                            pWriteBufPos += 1;
                                        } else {
                                            *pWriteBufPos = 1;
                                            pWriteBufPos += 1;
                                            SerializeDnaInst(pElemType, pInst);
                                        }
                                    }
                                }

                            }
                        }

                    } else { 

                        // Other DNA Reference types (we assume they're serializable)

                        pPtr = *(byte**)(pInst + memOffset);
                        if (pPtr == null) {
                            *pWriteBufPos = 0;
                            pWriteBufPos += 1;
                        } else {
                            *pWriteBufPos = 1;
                            pWriteBufPos += 1;
                            SerializeDnaInst(pFieldDef->pType, (byte*)pPtr);
                        }

                    }

                } else {

                    // Value types

                    // NOTE: We can't assume alignment so we have to serialize byte by byte

                    if (fieldInfo.typeCode <= 18) { // Standard System.TypeCode - this is a basic type

                        switch (pFieldDef->memSize) {
                            case 1:
                                *pWriteBufPos = *(pInst + memOffset);
                                pWriteBufPos += 1;
                                break;
                            case 2:
                                u16 = *(ushort*)(pInst + memOffset);
                                *(byte*)(pWriteBufPos) = (byte)u16;
                                *(byte*)(pWriteBufPos + 1) = (byte)(u16 >> 8);
                                pWriteBufPos += 2;
                                break;
                            case 4:
                                u32 = *(uint*)(pInst + memOffset);
                                *(byte*)(pWriteBufPos) = (byte)u32;
                                *(byte*)(pWriteBufPos + 1) = (byte)(u32 >> 8);
                                *(byte*)(pWriteBufPos + 2) = (byte)(u32 >> 16);
                                *(byte*)(pWriteBufPos + 3) = (byte)(u32 >> 24);
                                pWriteBufPos += 4;
                                break;
                            case 8:
                                u64 = *(ulong*)(pInst + memOffset);
                                *(byte*)(pWriteBufPos) = (byte)u64;
                                *(byte*)(pWriteBufPos + 1) = (byte)(u64 >> 8);
                                *(byte*)(pWriteBufPos + 2) = (byte)(u64 >> 16);
                                *(byte*)(pWriteBufPos + 3) = (byte)(u64 >> 24);
                                *(byte*)(pWriteBufPos + 4) = (byte)(u64 >> 32);
                                *(byte*)(pWriteBufPos + 5) = (byte)(u64 >> 40);
                                *(byte*)(pWriteBufPos + 6) = (byte)(u64 >> 48);
                                *(byte*)(pWriteBufPos + 7) = (byte)(u64 >> 56);
                                pWriteBufPos += 8;
                                break;
                            default:
                                Sys.Crash("Invalid value type to serialize");
                                break;
                        }

                    } else {

                        SerializeDnaInst(pFieldDef->pType, pInst + memOffset);

                    }

                }
            }

        }

        // Serializes a dna object tree to a dna byte buffer, obj array pair.
        public static void SerializeDna(tMD_TypeDef* pTypeDef, byte* pInst, out byte[] buf, out OBJ_TYPE[] objs, 
            Dictionary<PTR, DnaSerializedTypeInfo> typeMap)
        {
            DnaSerializedTypeInfo typeInfo;

            writeObjsList.Clear();
            dnaWriteTypeMap = typeMap;

            pWriteBufPos = pWriteBuf;

            if (pInst == null) {
                *pWriteBufPos = 0;
                pWriteBufPos += 1;
            } else {
                if (!dnaWriteTypeMap.TryGetValue((PTR)pTypeDef, out typeInfo)) {
                    typeInfo = BuildDnaTypeInfo(pTypeDef);
                }
                WriteVarInt((uint)typeInfo.typeCode);
            }

            buf = new byte[(int)(pWriteBufPos - pWriteBuf)];
            System.Runtime.InteropServices.Marshal.Copy((System.IntPtr)pWriteBuf, buf, 0, buf.Length);
            objs = writeObjsList.ToArray();
        }

        // ******************
        // MONO SERIALIZATION
        // ******************

        private static DnaSerializedTypeInfo BuildMonoTypeInfo(System.Type type)
        {
            int i;
            DnaSerializedTypeInfo typeInfo, fieldTypeInfo;
            List<DnaSerializedFieldInfo> fieldList;
            DnaSerializedFieldInfo fieldInfo;
            System.Reflection.FieldInfo monoFieldInfo;
            System.Type[] typeArgs;

            if (type == null) {
                return null;
            }

            typeInfo = new DnaSerializedTypeInfo();
            typeInfo.name = type.FullName;
            typeInfo.typeCode = monoWriteTypeMap.Count + 100;
            typeInfo.type = type;
            monoWriteTypeMap.Add(type, typeInfo);
            fieldList = new List<DnaSerializedFieldInfo>();

            System.Reflection.FieldInfo[] fieldInfos = type.GetFields(System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            for (i = 0; i < fieldInfos.Length; i++) {

                monoFieldInfo = fieldInfos[i];

                if (monoFieldInfo.IsNotSerialized)
                    continue;

                fieldInfo = new DnaSerializedFieldInfo();
                fieldInfo.name = monoFieldInfo.Name;
                fieldInfo.fieldInfo = monoFieldInfo;

                System.TypeCode typeCode = System.Type.GetTypeCode(monoFieldInfo.FieldType);
                if (typeCode != System.TypeCode.Object) {
                    fieldInfo.typeCode = (int)typeCode;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                } else if (typeof(OBJ_TYPE).IsAssignableFrom(monoFieldInfo.FieldType)) {
                    fieldInfo.typeCode = 97; // UnityEngine.Object type (add to object ref array, and store varint index)
#endif

                } else if (monoFieldInfo.FieldType.IsArray && monoFieldInfo.FieldType.GetElementType() != null) {
                    fieldInfo.typeCode = 98; // Array type
                    typeCode = System.Type.GetTypeCode(monoFieldInfo.FieldType.GetElementType());
                    if (typeCode != System.TypeCode.Object) {
                        fieldInfo.elementTypeCode = (int)typeCode;
                    } else {
                        if (!monoWriteTypeMap.TryGetValue(monoFieldInfo.FieldType.GetElementType(), out fieldTypeInfo)) {
                            fieldTypeInfo = BuildMonoTypeInfo(monoFieldInfo.FieldType.GetElementType());
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else if (monoFieldInfo.FieldType.IsGenericType && monoFieldInfo.FieldType.GetGenericTypeDefinition() != null &&
                    monoFieldInfo.FieldType.GetGenericTypeDefinition().FullName == "System.Collections.Generic.List`1") {
                    fieldInfo.typeCode = 99; // List<T> type
                    typeArgs = monoFieldInfo.FieldType.GetGenericArguments();
                    typeCode = System.Type.GetTypeCode(typeArgs[0]);
                    if (typeCode != System.TypeCode.Object) {
                        fieldInfo.elementTypeCode = (int)typeCode;
                    } else {
                        if (!monoWriteTypeMap.TryGetValue(typeArgs[0], out fieldTypeInfo)) {
                            fieldTypeInfo = BuildMonoTypeInfo(typeArgs[0]);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else {
                    if (!monoWriteTypeMap.TryGetValue(monoFieldInfo.FieldType, out fieldTypeInfo)) {
                        fieldTypeInfo = BuildMonoTypeInfo(monoFieldInfo.FieldType);
                    }
                    fieldInfo.typeCode = fieldTypeInfo.typeCode + 100;
                }

                fieldList.Add(fieldInfo);
            }

            typeInfo.fields = fieldList.ToArray();
            typeInfo.GenerateHash();

            return typeInfo;
        }

        private static void SerializeMonoInst(object inst)
        {
            int i, j, len, elemSize;
            System.Reflection.FieldInfo monoFieldInfo, listItemsFieldInfo;
            uint memSize;
            string s;
            ushort u16;
            uint u32;
            ulong u64;
            OBJ_TYPE obj;
            object childInst;
            DnaSerializedTypeInfo typeInfo;
            DnaSerializedFieldInfo fieldInfo;
            System.Array array;
            System.Collections.IList list;
            System.Type type, elemType;
            byte* pPtr, pInst;
            int typeCode;
            byte* pTemp = stackalloc byte[16];

            type = inst.GetType();

            if (!monoWriteTypeMap.TryGetValue(type, out typeInfo)) {
                typeInfo = BuildMonoTypeInfo(type);
            }

            for (i = 0; i < typeInfo.fields.Length; i++) {

                fieldInfo = typeInfo.fields[i];
                monoFieldInfo = fieldInfo.fieldInfo;
                typeCode = fieldInfo.typeCode;

                memSize = GetMemSize(fieldInfo.typeCode);

                // Check to see if we need to expand serialization buffer
                if (pWriteBufPos + (memSize + 8) >= pWriteBufEnd) {
                    ExpandWriteBuf(memSize + 8);
                }

                if (!monoFieldInfo.FieldType.IsValueType) {

                    if (typeCode == (int)System.TypeCode.String) {

                        // String (special case)

                        s = monoFieldInfo.GetValue(inst) as string;
                        WriteString(s);

#if UNITY_5 || UNITY_2017 || UNITY_2018

                    } else if (typeCode == 97) { // UnityEngine.Object derived type

                        // UnityEngine.Object

                        obj = monoFieldInfo.GetValue(inst) as OBJ_TYPE;
                        if (obj == null) {
                            *pWriteBufPos = 0;
                            pWriteBufPos += 1;
                        } else {
                            writeObjsList.Add(obj);
                            u32 = (uint)writeObjsList.Count;
                            WriteVarInt(u32);
                        }

#endif
                    } else if (typeCode == 98 || typeCode == 99) {  // Array or List<T> type

                        if (typeCode == 98) {   // Array
                            array = monoFieldInfo.GetValue(inst) as System.Array;
                            if (array != null) {
                                len = array.Length;
                            } else {
                                len = 0;
                            }
                        } else {                // List<T>
                            list = monoFieldInfo.GetValue(inst) as System.Collections.IList;
                            if (list != null) {
                                listItemsFieldInfo = list.GetType().GetField("items",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                array = listItemsFieldInfo.GetValue(list) as System.Array;
                                len = list.Count;
                            } else {
                                array = null;
                                len = 0;
                            }
                        }

                        if (array == null) {

                            // 0 len is null
                            WriteVarInt(0);

                        } else {

                            elemType = array.GetType().GetElementType();
                            elemSize = System.Runtime.InteropServices.Marshal.SizeOf(elemType);

                            pPtr = null;
                            if (elemType.IsValueType) { 
                                try {
                                    pPtr = (byte*)System.Runtime.InteropServices.GCHandle.Alloc(array, 
                                        System.Runtime.InteropServices.GCHandleType.Pinned).AddrOfPinnedObject();
                                } catch {
                                }
                            }

                            // len + 1 is array
                            WriteVarInt((uint)(len + 1));

                            if (pPtr != null) {

                                // Check to see if we need to expand serialization buffer
                                if (pWriteBufPos + (elemSize * len + 16) >= pWriteBufEnd) {
                                    ExpandWriteBuf((uint)(elemSize * len + 16));
                                }

                                Mem.memcpy(pWriteBufPos, pPtr, (SIZE_T)(elemSize * len));
                                pWriteBufPos += (elemSize * len);

                                System.Runtime.InteropServices.GCHandle.FromIntPtr((System.IntPtr)pPtr).Free();

                            } else if (fieldInfo.elementTypeCode == (int)System.TypeCode.String) {

                                // Check to see if we need to expand serialization buffer
                                if (pWriteBufPos + (len + 16) >= pWriteBufEnd) {
                                    ExpandWriteBuf((uint)(len + 16));
                                }

                                for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                    s = array.GetValue(j) as string;
                                    WriteString(s);
                                }

#if UNITY_5 || UNITY_2017 || UNITY_2018

                            } else if (fieldInfo.elementTypeCode == 97) {  // UnityEngine.Object derived type

                                // Check to see if we need to expand serialization buffer
                                if (pWriteBufPos + (len + 16) >= pWriteBufEnd) {
                                    ExpandWriteBuf((uint)(len + 16));
                                }

                                for (j = 0; j < len; j++) {
                                    obj = array.GetValue(j) as OBJ_TYPE;
                                    if (pPtr == null) {
                                        *pWriteBufPos = 0;
                                        pWriteBufPos += 1;
                                    } else {
                                        writeObjsList.Add(obj);
                                        u32 = (uint)writeObjsList.Count;
                                        WriteVarInt(u32);
                                    }
                                }

#endif
                            } else {

                                if (elemType.IsValueType) {
                                    for (j = 0; j < len; j++) {
                                        childInst = array.GetValue(j);
                                        SerializeMonoInst(childInst);
                                    }
                                } else {
                                    for (j = 0; j < len; j++) {
                                        childInst = array.GetValue(j);
                                        if (childInst == null) {
                                            *pWriteBufPos = 0;
                                            pWriteBufPos += 1;
                                        } else {
                                            *pWriteBufPos = 1;
                                            pWriteBufPos += 1;
                                            SerializeMonoInst(childInst);
                                        }
                                    }
                                }

                            }
                        }

                    } else {

                        // Mono obj serializable type (we assume it's serializable)

                        childInst = monoFieldInfo.GetValue(inst);

                        if (childInst == null) {
                            *pWriteBufPos = 0;
                            pWriteBufPos += 1;
                        } else {
                            *pWriteBufPos = 1;
                            pWriteBufPos += 1;
                            SerializeMonoInst(childInst);
                        }

                    }

                } else {

                    // Value types

                    // NOTE: We can't assume alignment so we have to serialize byte by byte

                    if (fieldInfo.typeCode <= 18) { // Standard System.TypeCode - this is a basic type

                        switch ((System.TypeCode)fieldInfo.typeCode) {
                            case System.TypeCode.Empty:          // Null reference
                            case System.TypeCode.Object:         // Instance that isn't a value
                            case System.TypeCode.DBNull:         // Database null value
                                Sys.Crash("Invalid type code");
                                break;
                            case System.TypeCode.Boolean:        // Boolean
                                monoFieldInfo.GetValueDirect(__makeref(*(bool*)pTemp));
                                break;
                            case System.TypeCode.Char:           // Unicode character
                                monoFieldInfo.GetValueDirect(__makeref(*(char*)pTemp));
                                break;
                            case System.TypeCode.SByte:          // Signed 8-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(sbyte*)pTemp));
                                break;
                            case System.TypeCode.Byte:           // Unsigned 8-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(byte*)pTemp));
                                break;
                            case System.TypeCode.Int16:          // Signed 16-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(short*)pTemp));
                                break;
                            case System.TypeCode.UInt16:         // Unsigned 16-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(ushort*)pTemp));
                                break;
                            case System.TypeCode.Int32:          // Signed 32-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(int*)pTemp));
                                break;
                            case System.TypeCode.UInt32:         // Unsigned 32-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(uint*)pTemp));
                                break;
                            case System.TypeCode.Int64:          // Signed 64-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(long*)pTemp));
                                break;
                            case System.TypeCode.UInt64:         // Unsigned 64-bit integer
                                monoFieldInfo.GetValueDirect(__makeref(*(ulong*)pTemp));
                                break;
                            case System.TypeCode.Single:         // IEEE 32-bit float
                                monoFieldInfo.GetValueDirect(__makeref(*(float*)pTemp));
                                break;
                            case System.TypeCode.Double:         // IEEE 64-bit double
                                monoFieldInfo.GetValueDirect(__makeref(*(double*)pTemp));
                                break;
                            case System.TypeCode.Decimal:        // Decimal
                                Sys.Crash("Invalid type code");
                                break;
                            case System.TypeCode.DateTime:       // DateTime
                                monoFieldInfo.GetValueDirect(__makeref(*(System.DateTime*)pTemp));
                                break;
                            case System.TypeCode.String:         // Unicode character string
                                Sys.Crash("Invalid type code");
                                break;
                        }

                        switch (memSize) { 
                            case 1:
                                *pWriteBufPos = *(pTemp);
                                pWriteBufPos += 1;
                                break;
                            case 2:
                                u16 = *(ushort*)(pTemp);
                                *(byte*)(pWriteBufPos) = (byte)u16;
                                *(byte*)(pWriteBufPos + 1) = (byte)(u16 >> 8);
                                pWriteBufPos += 2;
                                break;
                            case 4:
                                u32 = *(uint*)(pTemp);
                                *(byte*)(pWriteBufPos) = (byte)u32;
                                *(byte*)(pWriteBufPos + 1) = (byte)(u32 >> 8);
                                *(byte*)(pWriteBufPos + 2) = (byte)(u32 >> 16);
                                *(byte*)(pWriteBufPos + 3) = (byte)(u32 >> 24);
                                pWriteBufPos += 4;
                                break;
                            case 8:
                                u64 = *(ulong*)(pTemp);
                                *(byte*)(pWriteBufPos) = (byte)u64;
                                *(byte*)(pWriteBufPos + 1) = (byte)(u64 >> 8);
                                *(byte*)(pWriteBufPos + 2) = (byte)(u64 >> 16);
                                *(byte*)(pWriteBufPos + 3) = (byte)(u64 >> 24);
                                *(byte*)(pWriteBufPos + 4) = (byte)(u64 >> 32);
                                *(byte*)(pWriteBufPos + 5) = (byte)(u64 >> 40);
                                *(byte*)(pWriteBufPos + 6) = (byte)(u64 >> 48);
                                *(byte*)(pWriteBufPos + 7) = (byte)(u64 >> 56);
                                pWriteBufPos += 8;
                                break;
                            default:
                                Sys.Crash("Invalid value type to serialize");
                                break;
                        }

                    } else {

                        childInst = monoFieldInfo.GetValue(inst);
                        SerializeMonoInst(childInst);

                    }

                }
            }

        }

        // Serializes a standard mono/unity object tree to a dna byte buffer, obj array pair.
        public static void SerializeMono(object inst, out byte[] buf, out OBJ_TYPE[] objs,
            Dictionary<System.Type, DnaSerializedTypeInfo> typeMap)
        {
            DnaSerializedTypeInfo typeInfo;

            writeObjsList.Clear();
            monoWriteTypeMap = typeMap;

            pWriteBufPos = pWriteBuf;

            if (inst == null) {
                *pWriteBufPos = 0;
                pWriteBufPos += 1;
            } else {
                if (!monoWriteTypeMap.TryGetValue(inst.GetType(), out typeInfo)) {
                    typeInfo = BuildMonoTypeInfo(inst.GetType());
                }
                WriteVarInt((uint)typeInfo.typeCode);
                SerializeMonoInst(inst);
            }

            buf = new byte[(int)(pWriteBufPos - pWriteBuf)];
            System.Runtime.InteropServices.Marshal.Copy((System.IntPtr)pWriteBuf, buf, 0, buf.Length);
            objs = writeObjsList.ToArray();
        }

        // ***************
        // DESERIALIZE DNA
        // ***************

        private static void DeserializeDnaInst(byte* pInst, DnaSerializedTypeInfo typeInfo)
        {
            int i, j, len;
            bool skip;
            tMD_TypeDef* pArrayType, pElemType; 
            tMD_FieldDef* pFieldDef;
            uint memOffset, memSize, elemSize;
            string s;
            byte b;
            ushort u16;
            uint u32;
            ulong u64;
            OBJ_TYPE obj;
            DnaSerializedFieldInfo fieldInfo;
            DnaSerializedTypeInfo childTypeInfo;
            int typeCode;
            byte* pChildInst, pPtr;
            tSystemArray* pArray;
            tGenericList* pList;
            int objId;

            if (pListTypeDef == null) {
                pListTypeDef = CLIFile.FindTypeInAllLoadedAssemblies(new S("System.Collections.Generic"), new S("List`1"));
                if (pListTypeDef == null) {
                    Sys.Crash("Unable to find List<T>");
                }
            }

            for (i = 0; i < typeInfo.fields.Length; i++) {

                fieldInfo = typeInfo.fields[i];
                skip = (pInst == null) || fieldInfo.skip;
                pFieldDef = (tMD_FieldDef*)fieldInfo.fieldDef;
                memOffset = pFieldDef->memOffset;
                memSize = pFieldDef->memSize;
                typeCode = fieldInfo.typeCode;

                if (pFieldDef->pType->isValueType == 0) {

                    if (typeCode == (int)System.TypeCode.String) {

                        // String (special case)

                        s = ReadString();
                        if (!skip) {
                            *(tSystemString**)(pInst + memOffset) = (s != null ? System_String.FromMonoString(s) : null);
                        }

#if UNITY_5 || UNITY_2017 || UNITY_2018
                    } else if (typeCode == 97) { // UnityEngine.Object derived types

                        objId = (int)ReadVarInt();
                        if (!skip) {
                            if (objId == 0) {
                                *(void**)(pInst + memOffset) = null;
                            } else {
                                obj = readObjList[objId - 1];
                                *(void**)(pInst + memOffset) = Heap.AllocMonoObject(MonoType.GetTypeForMonoObject(obj, null, null), obj);
                            }
                        }
#endif
                    } else if (typeCode == 98 || typeCode == 99) {  // Array or List<T> type

                        len = (int)ReadVarInt();

                        if (len == 0) {

                            *(void**)(pInst + memOffset) = null;

                        } else {

                            len--; // 0 = null, so we have to -1 to get actual array len

                            if (typeCode == 98) {
                                pArrayType = pFieldDef->pType;
                                pArray = (tSystemArray*)System_Array.NewVector(pArrayType, (uint)len);
                                *(void**)(pInst + memOffset) = pArray;
                            } else {
                                if (pFieldDef->pType->ppClassTypeArgs == null) {
                                    Sys.Crash("No generic type arg for List<T>");
                                }
                                pArrayType = Type.GetArrayTypeDef(pFieldDef->pType->ppClassTypeArgs[0], null, null);
                                pArray = (tSystemArray*)System_Array.NewVector(pArrayType, (uint)len);
                                pList = (tGenericList*)Heap.AllocType(pListTypeDef);
                                pList->pItems = pArray;
                                pList->size = len;
                                *(void**)(pInst + memOffset) = pList;
                            }

                            pElemType = pArrayType->pArrayElementType;
                            elemSize = pElemType->arrayElementSize;
                            pPtr = System_Array.GetElements(pArray);

                            if (pElemType->fixedBlittable != 0) {

                                Mem.memcpy(pPtr, pReadBufPos, (SIZE_T)(elemSize * len));
                                pReadBufPos += (elemSize * len);

                            } else if (fieldInfo.elementTypeCode == (int)System.TypeCode.String) {

                                for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                    s = ReadString();
                                    *(tSystemString**)pPtr = s != null ? System_String.FromMonoString(s) : null;
                                }

#if UNITY_5 || UNITY_2017 || UNITY_2018

                            } else if (fieldInfo.elementTypeCode == 97) {  // UnityEngine.Object derived type

                                for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                    u32 = ReadVarInt();
                                    if (u32 == 0) {
                                        *(byte**)(pPtr) = null;
                                    } else {
                                        *(byte**)(pPtr) = Heap.AllocMonoObject(pElemType, readObjList[u32]);
                                    }
                                }

#endif
                            } else {

                                childTypeInfo = readTypeMap[fieldInfo.elementTypeCode];

                                if (pElemType->isValueType != 0) {
                                    for (j = 0; j < len; j++, pPtr += elemSize) {
                                        DeserializeDnaInst(pPtr, childTypeInfo);
                                    }
                                } else {
                                    for (j = 0; j < len; j++, pPtr += sizeof(void*)) {
                                        b = *pReadBufPos;
                                        pReadBufPos++;
                                        if (b == 0) {
                                            *(byte**)pPtr = null;
                                        } else {
                                            DeserializeDnaInst(pPtr, childTypeInfo);
                                        }
                                    }
                                }

                            }
                        }

                    } else {

                        // Other DNA Reference types (we assume they're serializable)

                        b = *(byte*)pReadBuf;
                        pReadBufPos += 1;
                        if (b == 0) {
                            if (!skip) {
                                *(void**)(pInst + memOffset) = null;
                            }
                        } else {
                            childTypeInfo = readTypeMap[fieldInfo.typeCode];
                            pChildInst = skip ? null : Heap.AllocType((tMD_TypeDef*)childTypeInfo.typeDef);
                            DeserializeDnaInst(pChildInst, childTypeInfo);
                            if (!skip) {
                                *(void**)(pInst + memOffset) = pChildInst;
                            }
                        }

                    }

                } else {

                    // Value types

                    // NOTE: We can't assume alignment so we have to serialize byte by byte

                    if (fieldInfo.typeCode <= 18) { // Standard System.TypeCode - this is a basic type

                        switch (pFieldDef->memSize) {
                            case 1:
                                if (!skip) {
                                    *(pInst + memOffset) = *pReadBufPos;
                                }
                                pReadBufPos += 1;
                                break;
                            case 2:
                                if (!skip) {
                                    u16 = (ushort)((ushort)*(byte*)(pReadBufPos) |
                                                   (ushort)*(byte*)(pReadBufPos + 1) << 8);
                                    *(ushort*)(pInst + memOffset) = u16;
                                }
                                pReadBufPos += 2;
                                break;
                            case 4:
                                if (!skip) {
                                    u32 = (uint)((uint)*(byte*)(pReadBufPos) |
                                                 (uint)*(byte*)(pReadBufPos + 1) << 8 |
                                                 (uint)*(byte*)(pReadBufPos + 2) << 16 |
                                                 (uint)*(byte*)(pReadBufPos + 3) << 24);
                                    *(uint*)(pInst + memOffset) = u32;
                                }
                                pReadBufPos += 4;
                                break;
                            case 8:
                                if (!skip) {
                                    u64 = (ulong)((ulong)*(byte*)(pReadBufPos) |
                                                 (ulong)*(byte*)(pReadBufPos + 1) << 8 |
                                                 (ulong)*(byte*)(pReadBufPos + 2) << 16 |
                                                 (ulong)*(byte*)(pReadBufPos + 3) << 24 |
                                                 (ulong)*(byte*)(pReadBufPos + 4) << 32 |
                                                 (ulong)*(byte*)(pReadBufPos + 5) << 40 |
                                                 (ulong)*(byte*)(pReadBufPos + 6) << 48 |
                                                 (ulong)*(byte*)(pReadBufPos + 7) << 56);
                                    *(ulong*)(pInst + memOffset) = u64;
                                }
                                pReadBufPos += 8;
                                break;
                            default:
                                Sys.Crash("Invalid value type to serialize");
                                break;
                        }

                    } else {

                        childTypeInfo = readTypeMap[fieldInfo.typeCode];
                        pChildInst = skip ? null : (pInst + memOffset);
                        DeserializeDnaInst(pChildInst, childTypeInfo);

                    }

                }
            }
        }

        public static byte* DeserializeDna(object wrapInst, byte[] buf, OBJ_TYPE[] objs, 
            DnaSerializedTypeInfo[] typeList)
        {
            int i, j;

            readObjList = objs;
            readTypeMap = new Dictionary<int, DnaSerializedTypeInfo>();

            // Get type map for reading.  Take into account any types that may have been changed, fields that may have been 
            // added or deleted, etc.
            for (i = 0; i < typeList.Length; i++) {
                DnaSerializedTypeInfo sourceTypeInfo = typeList[i];
                DnaSerializedTypeInfo targetTypeInfo = BuildDnaTypeInfo((tMD_TypeDef*)Dna.FindType(sourceTypeInfo.name));
                sourceTypeInfo.targetTypeInfo = targetTypeInfo;
                if (targetTypeInfo != null) {
                    sourceTypeInfo.typeDef = targetTypeInfo.typeDef;
                    if (sourceTypeInfo.hash == targetTypeInfo.hash) {
                        sourceTypeInfo.fields = targetTypeInfo.fields;
                    } else {
                        if (sourceTypeInfo.targetTypeInfo != null) {
                            for (j = 0; j < sourceTypeInfo.fields.Length; j++) {
                                DnaSerializedFieldInfo sourceFieldInfo = sourceTypeInfo.fields[j];
                                DnaSerializedFieldInfo targetFieldInfo = targetTypeInfo.FindField(sourceFieldInfo.name);
                                if (targetFieldInfo == null || !sourceFieldInfo.MatchesField(targetFieldInfo)) {
                                    sourceFieldInfo.skip = true;
                                } else {
                                    sourceFieldInfo.fieldDef = targetFieldInfo.fieldDef;
                                }
                            }
                        }
                    }
                    readTypeMap[sourceTypeInfo.typeCode] = sourceTypeInfo;
                }
            }

            // Read the top type
            byte* pRet = null;
            fixed (byte* pBuf = buf) {
                pReadBuf = pReadBufPos = pBuf;
                int typeCode = (int)ReadVarInt();
                DnaSerializedTypeInfo typeInfo = readTypeMap[typeCode];
                if (wrapInst != null) {
                    pRet = Heap.AllocMonoObject((tMD_TypeDef*)typeInfo.typeDef, wrapInst);
                } else {
                    pRet = Heap.AllocType((tMD_TypeDef*)typeInfo.typeDef);
                }
                DeserializeDnaInst(pRet, typeInfo);
            }

            return pRet;
        }


    }
}