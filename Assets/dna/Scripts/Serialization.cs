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

        // PTR to DNA typedef (when serializing from DNA)
        [System.NonSerialized]
        public PTR typeDef;

        // Ref to Mono System.Type (when serializing from Mono)
        [System.NonSerialized]
        public System.Type type;

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
        
        // Type code for the field (<100 is .NET type code, 97 = UnityEngine.Object, 98 = Array, 99 = List<T>, >=100 is index of user defined type + 100)
        public int typeCode;

        // Type of element if array
        public int elementTypeCode;

        // Pointer to field def (when serializing from DNA)
        [System.NonSerialized]
        public PTR fieldDef;

        // Ref to field info (when serializing from Mono)
        [System.NonSerialized]
        public System.Reflection.FieldInfo fieldInfo;
    }

    public unsafe static class Serialization
    {

        static byte* pBuf = null;
        static byte* pBufPos = null;
        static uint bufSize = 0;
        static OBJ_ARRAY objsList;
        static Dictionary<PTR, DnaSerializedTypeInfo> dnaTypeMap;
        static Dictionary<System.Type, DnaSerializedTypeInfo> monoTypeMap;
        static uint[] memSizes;

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

        private static void WriteString(string s)
        {
            int i;
            uint len;
            bool isAnsi;
            ushort u16;

            if (s == null) {
                *pBufPos = 0; // Null string
                pBufPos += 1;
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
                    while ((pBufPos - pBuf) + len + 8 >= bufSize) {
                        bufSize = bufSize * 2;
                        pBuf = (byte*)Mem.realloc(pBuf, (SIZE_T)(bufSize));
                    }
                    *pBufPos = 1; // Is Ansi (8 bit)
                    pBufPos++;
                    WriteVarInt(len);
                    for (i = 0; i < len; i++) {
                        *(pBufPos) = (byte)s[i];
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
                    for (i = 0; i < len; i++) {
                        u16 = (ushort)s[i];
                        *(pBufPos) = (byte)u16;
                        *(pBufPos + 1) = (byte)(u16 >> 8);
                        pBufPos += 2;
                    }

                }
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

            typeInfo = new DnaSerializedTypeInfo();
            typeInfo.name = pTypeDef->fullNameS;
            typeInfo.typeCode = dnaTypeMap.Count + 100;
            typeInfo.typeDef = (PTR)pTypeDef;
            dnaTypeMap.Add((PTR)pTypeDef, typeInfo);
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
                        if (!dnaTypeMap.TryGetValue((PTR)pFieldDef->pType->pArrayElementType, out fieldTypeInfo)) {
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
                        if (!dnaTypeMap.TryGetValue((PTR)pFieldDef->pType->ppClassTypeArgs[0], out fieldTypeInfo)) {
                            fieldTypeInfo = BuildDnaTypeInfo(pFieldDef->pType->ppClassTypeArgs[0]);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else { 
                    if (!dnaTypeMap.TryGetValue((PTR)pFieldDef->pType, out fieldTypeInfo)) {
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
            int i;
            tMD_FieldDef* pFieldDef;
            uint memOffset;
            uint memSize;
            string s;
            ushort u16;
            uint u32;
            ulong u64;
            OBJ_TYPE obj;
            void* pPtr;
            DnaSerializedTypeInfo typeInfo;
            DnaSerializedFieldInfo fieldInfo;
            int typeCode;

            if (!dnaTypeMap.TryGetValue((PTR)pTypeDef, out typeInfo)) {
                typeInfo = BuildDnaTypeInfo(pTypeDef);
            }

            for (i = 0; i < typeInfo.fields.Length; i++) {

                fieldInfo = typeInfo.fields[i];
                pFieldDef = (tMD_FieldDef*)fieldInfo.fieldDef;
                memOffset = pFieldDef->memOffset;
                memSize = pFieldDef->memSize;
                typeCode = fieldInfo.typeCode;

                // Check to see if we need to expand serialization buffer
                if ((pBufPos - pBuf) + memSize + 8 >= bufSize) {
                    bufSize = bufSize * 2;
                    pBuf = (byte*)Mem.realloc(pBuf, (SIZE_T)(bufSize * 2));
                }

                if (pFieldDef->pType->isValueType == 0) {

                    if (typeCode == (int)System.TypeCode.String) {

                        // String (special case)

                        s = System_String.ToMonoString(*(tSystemString**)(pInst + memOffset));
                        WriteString(s);

#if UNITY_5 || UNITY_2017 || UNITY_2018
                    } else if (typeCode == 97) { // UnityEngine.Object derived types

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
#endif
                    } else if (typeCode == 98) {  // Array type

                        Sys.Crash("Array serialization not implemented yet!");

                    } else if (typeCode == 99) {  // List<T> type

                        Sys.Crash("List<T> serialization not implemented yet!");

                    } else { 

                        // Other DNA Reference types (we assume they're serializable)

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

            objsList.Clear();
            dnaTypeMap = typeMap;

            pBufPos = pBuf;

            if (pInst == null) {
                *pBufPos = 0;
                pBufPos += 1;
            } else {
                if (!dnaTypeMap.TryGetValue((PTR)pTypeDef, out typeInfo)) {
                    typeInfo = BuildDnaTypeInfo(pTypeDef);
                }
                WriteVarInt((uint)typeInfo.typeCode);
            }

            buf = new byte[(int)(pBufPos - pBuf)];
            System.Runtime.InteropServices.Marshal.Copy((System.IntPtr)pBuf, buf, 0, buf.Length);
            objs = objsList.ToArray();
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

            typeInfo = new DnaSerializedTypeInfo();
            typeInfo.name = type.FullName;
            typeInfo.typeCode = monoTypeMap.Count + 100;
            typeInfo.type = type;
            monoTypeMap.Add(type, typeInfo);
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
                        if (!monoTypeMap.TryGetValue(monoFieldInfo.FieldType.GetElementType(), out fieldTypeInfo)) {
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
                        if (!monoTypeMap.TryGetValue(typeArgs[0], out fieldTypeInfo)) {
                            fieldTypeInfo = BuildMonoTypeInfo(typeArgs[0]);
                        }
                        fieldInfo.elementTypeCode = fieldTypeInfo.typeCode + 100;
                    }
                } else {
                    if (!monoTypeMap.TryGetValue(monoFieldInfo.FieldType, out fieldTypeInfo)) {
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
            int i;
            System.Reflection.FieldInfo monoFieldInfo;
            uint memSize;
            string s;
            ushort u16;
            uint u32;
            ulong u64;
            OBJ_TYPE obj;
            object childInst;
            DnaSerializedTypeInfo typeInfo;
            DnaSerializedFieldInfo fieldInfo;
            System.Type type;
            int typeCode;
            byte* pTemp = stackalloc byte[16];

            type = inst.GetType();

            if (!monoTypeMap.TryGetValue(type, out typeInfo)) {
                typeInfo = BuildMonoTypeInfo(type);
            }

            for (i = 0; i < typeInfo.fields.Length; i++) {

                fieldInfo = typeInfo.fields[i];
                monoFieldInfo = fieldInfo.fieldInfo;
                typeCode = fieldInfo.typeCode;

                memSize = GetMemSize(fieldInfo.typeCode);

                // Check to see if we need to expand serialization buffer
                if ((pBufPos - pBuf) + memSize + 8 >= bufSize) {
                    bufSize = bufSize * 2;
                    pBuf = (byte*)Mem.realloc(pBuf, (SIZE_T)(bufSize * 2));
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
                            *pBufPos = 0;
                            pBufPos += 1;
                        } else {
                            objsList.Add(obj);
                            u32 = (uint)objsList.Count;
                            WriteVarInt(u32);
                        }

#endif
                    } else if (typeCode == 98) {  // Array type

                        Sys.Crash("Array serialization not implemented yet!");

                    } else if (typeCode == 99) {  // List<T> type

                        Sys.Crash("List<T> serialization not implemented yet!");

                    } else {

                        // Mono obj serializable type (we assume it's serializable)

                        childInst = monoFieldInfo.GetValue(inst);

                        if (childInst == null) {
                            *pBufPos = 0;
                            pBufPos += 1;
                        } else {
                            SerializeMonoInst(childInst);
                        }

                    }

                } else {

                    // Value types

                    // NOTE: We can't assume alignment so we have to serialize byte by byte

                    if (fieldInfo.typeCode < 98) { // Standard System.TypeCode - this is a basic type

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
                                *pBufPos = *(pTemp);
                                pBufPos += 1;
                                break;
                            case 2:
                                u16 = *(ushort*)(pTemp);
                                *(byte*)(pBufPos) = (byte)u16;
                                *(byte*)(pBufPos + 1) = (byte)(u16 >> 8);
                                pBufPos += 2;
                                break;
                            case 4:
                                u32 = *(uint*)(pTemp);
                                *(byte*)(pBufPos) = (byte)u32;
                                *(byte*)(pBufPos + 1) = (byte)(u32 >> 8);
                                *(byte*)(pBufPos + 2) = (byte)(u32 >> 16);
                                *(byte*)(pBufPos + 3) = (byte)(u32 >> 24);
                                pBufPos += 4;
                                break;
                            case 8:
                                u64 = *(ulong*)(pTemp);
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

            objsList.Clear();
            monoTypeMap = typeMap;

            pBufPos = pBuf;

            if (inst == null) {
                *pBufPos = 0;
                pBufPos += 1;
            } else {
                if (!monoTypeMap.TryGetValue(inst.GetType(), out typeInfo)) {
                    typeInfo = BuildMonoTypeInfo(inst.GetType());
                }
                WriteVarInt((uint)typeInfo.typeCode);
                SerializeMonoInst(inst);
            }

            buf = new byte[(int)(pBufPos - pBuf)];
            System.Runtime.InteropServices.Marshal.Copy((System.IntPtr)pBuf, buf, 0, buf.Length);
            objs = objsList.ToArray();
        }

        // ***************
        // DESERIALIZE DNA
        // ***************

        public static void DeserializeDna(OBJ_TYPE wrapInst, byte[] buf, OBJ_TYPE[] objs, 
            Dictionary<PTR, DnaSerializedTypeInfo> typeMap)
        {

        }


    }
}