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
    using OBJ_ARRAY = List<UnityEngine.Object>;
#else
    using OBJ_TYPE = System.Object;
    using OBJ_ARRAY = List<System.Object>;
#endif

    /// <summary>
    /// Serialization information for 
    /// </summary>
    [System.Serializable]
    public class DnaSerializedTypeInfo
    {
        // Name of class who's strings this table records.
        public string name;

        // Namespace of class who's strings this table records.
        public string nameSpace;

        // Index in type array
        public int typeCode;

        // The hash for this type (if two hashes match, type is assumed to be the same)
        public uint hash;

        // The strings (property names) for this class
        public DnaSerializedFieldInfo[] fields;

        // PTR to DNA typedef
        [System.NonSerialized]
        public PTR typeDef;

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
    }

    /// <summary>
    /// Field info for serialized fields.
    /// </summary>
    [System.Serializable]
    public class DnaSerializedFieldInfo
    {
        // Name of the field
        public string name;
        
        // Type code for the field (<100 is .NET type code, 98 = Array, 99 = List<T>, >=100 is index of user defined type + 100)
        public int typeCode;

        // Type of element if array
        public int elementTypeCode;

        [System.NonSerialized]
        public PTR fieldDef;
    }

    public unsafe static class Serialization
    {

        static byte* pBuf = null;
        static byte* pBufPos = null;
        static uint bufSize = 0;
        static OBJ_ARRAY objsList;
        static Dictionary<PTR, DnaSerializedTypeInfo> typeTable;

        public static void Init()
        {
            pBuf = (byte*)Mem.malloc(2048);
            pBufPos = pBuf;
            bufSize = 2048;
            objsList = new OBJ_ARRAY();
        }

        public static void Clear()
        {

        }

        private static void WriteVarInt(uint i)
        {
            while (i != 0) {
                *(pBufPos) = (byte)((i & 0x7fU) | (i > 0x7fU ? 0x80U : 0U));
                pBufPos += 1;
                i = i >> 7;
            }
        }

        private static DnaSerializedTypeInfo BuildTypeInfo(tMD_TypeDef* pTypeDef)
        {
            int i, j;
            tMD_FieldDef* pFieldDef;
            ushort flags;
            DnaSerializedTypeInfo typeInfo, fieldTypeInfo;
            List<DnaSerializedFieldInfo> fieldList;
            DnaSerializedFieldInfo fieldInfo;

            typeInfo = new DnaSerializedTypeInfo();
            typeInfo.name = pTypeDef->nameSpaceS + '.' + pTypeDef->nameSpaceS;
            typeInfo.typeCode = typeTable.Count + 100;
            typeInfo.typeDef = (PTR)pTypeDef;
            typeTable.Add((PTR)pTypeDef, typeInfo);
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

                System.TypeCode typeCode = Type.GetTypeCode(pFieldDef->pType);
                if (typeCode != System.TypeCode.Object) {
                    fieldInfo.typeCode = (int)typeCode;
                } else if (pFieldDef->pType->pArrayElementType != null) {
                    fieldInfo.typeCode = 98; // Array type
                    typeCode = Type.GetTypeCode(pFieldDef->pType->pArrayElementType);
                    if (typeCode != System.TypeCode.Object) {
                        fieldInfo.elementTypeCode = (int)typeCode;
                    } else {
                        if (!typeTable.TryGetValue((PTR)pFieldDef->pType->pArrayElementType, out fieldTypeInfo)) {
                            fieldTypeInfo = BuildTypeInfo(pFieldDef->pType->pArrayElementType);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else if (pFieldDef->pType->pGenericDefinition != null &&
                    pFieldDef->pType->pGenericDefinition->nameSpaceS == "Ssytem.Collections.Generic" &&
                    pFieldDef->pType->pGenericDefinition->nameS == "List`1") {
                    fieldInfo.typeCode = 99; // List<T> type
                    typeCode = Type.GetTypeCode(pFieldDef->pType->ppClassTypeArgs[0]);
                    if (typeCode != System.TypeCode.Object) {
                        fieldInfo.elementTypeCode = (int)typeCode;
                    } else {
                        if (!typeTable.TryGetValue((PTR)pFieldDef->pType->ppClassTypeArgs[0], out fieldTypeInfo)) {
                            fieldTypeInfo = BuildTypeInfo(pFieldDef->pType->ppClassTypeArgs[0]);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else { 
                    if (!typeTable.TryGetValue((PTR)pFieldDef->pType, out fieldTypeInfo)) {
                        fieldTypeInfo = BuildTypeInfo(pFieldDef->pType);
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
            int i, j;
            tMD_FieldDef* pFieldDef;
            uint memOffset;
            uint memSize;
            string s;
            bool isAnsi;
            uint len;
            ushort u16;
            uint u32;
            ulong u64;
            OBJ_TYPE obj;
            void* pPtr;
            DnaSerializedTypeInfo typeInfo;
            DnaSerializedFieldInfo fieldInfo;

            if (!typeTable.TryGetValue((PTR)pTypeDef, out typeInfo)) {
                typeInfo = BuildTypeInfo(pTypeDef);
            }

            for (i = 0; i < typeInfo.fields.Length; i++) {

                fieldInfo = typeInfo.fields[i];
                pFieldDef = (tMD_FieldDef*)fieldInfo.fieldDef;
                memOffset = pFieldDef->memOffset;
                memSize = pFieldDef->memSize;

                // Check to see if we need to expand serialization buffer
                if ((pBufPos - pBuf) + memSize + 8 >= bufSize) {
                    bufSize = bufSize * 2;
                    pBuf = (byte*)Mem.realloc(pBuf, (SIZE_T)(bufSize * 2));
                }

                if (pFieldDef->pType->isValueType == 0) {

                    if (pFieldDef->pType->hasMonoBase != 0) {

                        // Mono (Unity) objects

                        if (pFieldDef->pType == Type.types[Type.TYPE_SYSTEM_STRING]) {

                            // String (special case)

                            s = System_String.ToMonoString(*(tSystemString**)(pInst + memOffset));
                            if (s == null) {
                                *pBufPos = 0; // Null string
                                pBufPos += 1;
                            } else {
                                len = (uint)s.Length;
                                // Check if string is 7-bit ascii (is ansi)
                                isAnsi = true;
                                for (j = 0; j < len; j++) {
                                    if ((ushort)s[j] > 0x7F) {
                                        isAnsi = false;
                                        break;
                                    }
                                }
                                if (isAnsi) {
                                    while ((pBufPos - pBuf) + len + 8 >= bufSize) {
                                        bufSize = bufSize * 2;
                                        pBuf = (byte*)Mem.realloc(pBuf, (SIZE_T)(bufSize));
                                    }
                                    *pBufPos = 1; // Is Ansi (8 bit)
                                    pBufPos++;
                                    WriteVarInt(len);
                                    for (j = 0; j < len; j++) {
                                        *(pBufPos) = (byte)s[j];
                                        pBufPos += 1;
                                    }
                                } else {
                                    while ((pBufPos - pBuf) + (len << 1) + 8 >= bufSize) {
                                        bufSize = bufSize * 2;
                                        pBuf = (byte*)Mem.realloc(pBuf, (SIZE_T)(bufSize));
                                    }
                                    *pBufPos = 2; // Is UTF16 (16 bit)
                                    pBufPos++;
                                    WriteVarInt(len);
                                    for (j = 0; j < len; j++) {
                                        u16 = (ushort)s[j];
                                        *(pBufPos) = (byte)u16;
                                        *(pBufPos + 1) = (byte)(u16 >> 8);
                                        pBufPos += 2;
                                    }

                                }
                            }

                        } else {

                            // Other mono objects (NOTE: in Unity only objects derived from UnityEngine.Object are supported)

                            pPtr = *(void**)(pInst + memOffset);
                            obj = pPtr != null ? Heap.GetMonoObject((byte*)pPtr) as OBJ_TYPE : null;
                            if (pPtr == null) {
                                *pBufPos = 0;
                                pBufPos += 1;
                            } else {
                                objsList.Add(obj);
                                u32 = (uint)objsList.Count;
                                WriteVarInt(u32);
                            }

                        }

                    } else {

                        // DNA Reference types

                        pPtr = *(void**)(pInst + memOffset);
                        if (pPtr == null) {
                            *pBufPos = 0;
                            pBufPos += 1;
                        } else {
                            SerializeDnaInst(pFieldDef->pType, (byte*)pPtr);
                        }

                    }

                } else {

                    // Value types

                    // NOTE: We can't assume alignment so we have to serialize byte by byte

                    if (fieldInfo.typeCode < 98) { // Standard System.TypeCode - this is a basic type

                        switch (pFieldDef->memSize) {
                            case 1:
                                *pBufPos = *(pInst + memOffset);
                                pBufPos += 1;
                                break;
                            case 2:
                                u16 = *(ushort*)(pInst + memOffset);
                                *(byte*)(pBufPos) = (byte)u16;
                                *(byte*)(pBufPos + 1) = (byte)(u16 >> 8);
                                pBufPos += 2;
                                break;
                            case 4:
                                u32 = *(uint*)(pInst + memOffset);
                                *(byte*)(pBufPos) = (byte)u32;
                                *(byte*)(pBufPos + 1) = (byte)(u32 >> 8);
                                *(byte*)(pBufPos + 2) = (byte)(u32 >> 16);
                                *(byte*)(pBufPos + 3) = (byte)(u32 >> 24);
                                pBufPos += 4;
                                break;
                            case 8:
                                u64 = *(ulong*)(pInst + memOffset);
                                *(byte*)(pBufPos) = (byte)u64;
                                *(byte*)(pBufPos + 1) = (byte)(u64 >> 8);
                                *(byte*)(pBufPos + 2) = (byte)(u64 >> 16);
                                *(byte*)(pBufPos + 3) = (byte)(u64 >> 24);
                                *(byte*)(pBufPos + 4) = (byte)(u64 >> 32);
                                *(byte*)(pBufPos + 5) = (byte)(u64 >> 40);
                                *(byte*)(pBufPos + 6) = (byte)(u64 >> 48);
                                *(byte*)(pBufPos + 7) = (byte)(u64 >> 56);
                                pBufPos += 8;
                                break;
                            default:
                                Mem.memcpy(pBufPos, pInst + memOffset, (SIZE_T)(memSize));
                                pBufPos += memSize;
                                break;
                        }

                    } else {

                        SerializeDnaInst(pFieldDef->pType, pInst + memOffset);

                    }

                }
            }

        }

        public static void SerializeDna(tMD_TypeDef* pTypeDef, byte* pInst, out byte[] buf, out OBJ_TYPE[] objs, 
            Dictionary<PTR, DnaSerializedTypeInfo> typeMap)
        {
            DnaSerializedTypeInfo typeInfo;

            objsList.Clear();

            pBufPos = pBuf;

            if (pInst == null) {
                *pBufPos = 0;
                pBufPos += 1;
            } else {
                if (!typeMap.TryGetValue((PTR)pTypeDef, out typeInfo)) {
                    typeInfo = BuildTypeInfo(pTypeDef);
                }
                WriteVarInt((uint)typeInfo.typeCode);
            }

            buf = new byte[(int)(pBufPos - pBuf)];
            System.Runtime.InteropServices.Marshal.Copy((System.IntPtr)pBuf, buf, 0, buf.Length);
            objs = objsList.ToArray();
        }

        public static void SerializeMono(OBJ_TYPE inst, out byte[] buf, out OBJ_TYPE[] objs,
            Dictionary<PTR, DnaSerializedTypeInfo> typeMap)
        {

            buf = new byte[(int)(pBufPos - pBuf)];
            System.Runtime.InteropServices.Marshal.Copy((System.IntPtr)pBuf, buf, 0, buf.Length);
            objs = objsList.ToArray();
        }

        public static void DeserializeDna(OBJ_TYPE wrapInst, byte[] buf, OBJ_TYPE[] objs, 
            Dictionary<PTR, DnaSerializedTypeInfo> typeMap)
        {

        }


    }
}