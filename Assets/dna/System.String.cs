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
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif   

    // This structure must tie up with string.cs
    public unsafe struct tSystemString
    {
        public void* monoStr;
    };


    public unsafe static class System_String
    {

        public static tAsyncCall* ctor_CharInt32(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pSystemString;
        	char c;
        	int len;

            c = (char)(*((uint*)(pParams + 0)));
            len = (*((int*)(pParams + Sys.S_INT32)));
        	pSystemString = FromMonoString(new System.String(c, len));
            Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pSystemString);

        	return null;
        }

        public static tAsyncCall* ctor_CharAIntInt(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pSystemString;
        	/*HEAP_PTR*/byte* charArray;
        	char* charElements;
        	int startIndex, length;

            charArray = (*((byte**)(pParams + 0)));
            startIndex = (*((int*)(pParams + Sys.S_PTR)));
            length = (*((int*)(pParams + Sys.S_PTR + Sys.S_INT32)));

        	charElements = (char*)System_Array.GetElements(charArray);
            pSystemString = FromMonoString(new System.String(charElements, startIndex, length));
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pSystemString);

        	return null;
        }

        public static tAsyncCall* get_Length(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            string s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (uint)s.Length);

            return null;
        }

        public static tAsyncCall* get_Chars(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
        	int index;
            string s;

        	index = (*((int*)(pParams + 0)));
            s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (uint)(s[index]));

        	return null;
        }

        public static tAsyncCall* InternalConcat(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* s0, s1, ret;
            string str0, str1;

        	s0 = (*((tSystemString**)(pParams + 0)));
            str0 = s0 != null ? H.ToObj(s0->monoStr) as string : null;
        	s1 = (*((tSystemString**)(pParams + Sys.S_PTR)));
            str1 = s1 != null ? H.ToObj(s1->monoStr) as string : null;
        	ret = FromMonoString(str0 + str1);
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, ret);

        	return null;
        }

        public static tAsyncCall* InternalTrim(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* pWhiteChars;
        	uint trimType, i, checkCharsLen;
            string s, ret;
        	char* pCheckChars;
        	tSystemString* pRet;

        	pWhiteChars = (*((/*HEAP_PTR*/byte**)(pParams + 0)));
            char[] whiteChars = null;
            if (pWhiteChars != null) {
                pCheckChars = (char*)System_Array.GetElements(pWhiteChars);
                checkCharsLen = System_Array.GetLength(pWhiteChars);
                whiteChars = new char[checkCharsLen];
                for (i = 0; i < checkCharsLen; i++) {
                    whiteChars[i] = pCheckChars[i];
                }
            }
            trimType = (*((uint*)(pParams + Sys.S_PTR)));

            s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            ret = null;

            if (trimType == 1)
                ret = s.TrimStart(whiteChars);
            else if (trimType == 2)
                ret = s.TrimEnd(whiteChars);
            else if (trimType == 3)
                ret = s.Trim(whiteChars);

            pRet = FromMonoString(ret);

        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pRet);

        	return null;
        }

        public static tAsyncCall* Equals(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* a, b;
            string aStr, bStr;
        	uint ret;

            a = (*((tSystemString**)(pParams + 0)));
            aStr = a != null ? H.ToObj(a->monoStr) as string : null;
            b = (*((tSystemString**)(pParams + Sys.S_PTR)));
            bStr = b != null ? H.ToObj(b->monoStr) as string : null;

            ret = (a == b ? 1U : 0U);

        	Sys.INTERNALCALL_RESULT_U32(pReturnValue, ret);

        	return null;
        }

        public static tAsyncCall* GetHashCode(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
        	int hash;
            string s;

            s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            hash = s.GetHashCode();

            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (uint)hash);

        	return null;
        }

        public static tAsyncCall* InternalSubstring(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
            tSystemString* pStr, pResult;
            int startIndex, length;
            string str;

            pStr = (*((tSystemString**)(pParams + 0)));
            str = pStr != null ? H.ToObj(((tSystemString*)pStr)->monoStr) as string : null;
            startIndex = (*((int*)(pParams + Sys.S_PTR)));
            length = (*((int*)(pParams + Sys.S_PTR + Sys.S_INT32)));

            pResult = FromMonoString(str.Substring(startIndex, length));

            Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pResult);

            return null;
        }

        public static tAsyncCall* InternalReplace(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pOld, pNew, pResult;
            string s, oldStr, newStr;

            s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            pOld = (*((tSystemString**)(pParams + 0)));
            oldStr = pOld != null ? H.ToObj(((tSystemString*)pOld)->monoStr) as string : null;
            pNew = (*((tSystemString**)(pParams + Sys.S_PTR)));
            newStr = pNew != null ? H.ToObj(((tSystemString*)pNew)->monoStr) as string : null;

            pResult = FromMonoString(s.Replace(oldStr, newStr));

            Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pResult);

        	return null;
        }

        public static tAsyncCall* InternalIndexOf(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
            char value;
            uint forwards;
        	int startIndex, count, i;
            string s;

            s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            value = (*((char*)(pParams + 0)));
            startIndex = (*((int*)(pParams + Sys.S_INT32)));
            count = (*((int*)(pParams + Sys.S_INT32 + Sys.S_INT32)));
            forwards = (*((uint*)(pParams + Sys.S_INT32 + Sys.S_INT32 + Sys.S_INT32)));

            if (forwards != 0) {
                i = s.IndexOf(value, startIndex, count);
            } else {
                i = s.LastIndexOf(value, startIndex, count);
            }

            Sys.INTERNALCALL_RESULT_I32(pReturnValue, i);

        	return null;
        }

        public static tAsyncCall* InternalIndexOfAny(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue) 
        {
        	/*HEAP_PTR*/byte* valueArray;
        	int startIndex, count;
        	uint forwards;
            string s;

            s = pThis != null ? H.ToObj(((tSystemString*)pThis)->monoStr) as string : null;
            valueArray = (*((/*HEAP_PTR*/byte**)(pParams + 0)));
            startIndex = (*((int*)(pParams + Sys.S_PTR)));
            count = (*((int*)(pParams + Sys.S_PTR + Sys.S_INT32)));
            forwards = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT32 + Sys.S_INT32)));

            byte* valueChars = System_Array.GetElements(valueArray);
        	uint numValueChars = System_Array.GetLength(valueArray);

        	int lastIndex;
        	int inc;
        	int i, j;

        	if (forwards != 0) {
        		lastIndex = startIndex + count;
        		inc = 1;
        		i = startIndex;
        	} else {
        		lastIndex = startIndex - 1;
        		inc = -1;
        		i = startIndex + count - 1;
        	}
        	for (; i != lastIndex; i += inc) {
        		char thisChar = s[i];
                for (j=(int)numValueChars - 1; j>=0; j--) {
        			if (thisChar == ((ushort*)valueChars)[j]) {
        				Sys.INTERNALCALL_RESULT_I32(pReturnValue, i);
        				return null;
        			}
        		}
        	}
            Sys.INTERNALCALL_RESULT_I32(pReturnValue, -1);
        	return null;
        }

        public static tSystemString* FromMonoString(string s)
        {
            return (tSystemString*)Heap.AllocMonoObject(Type.types[Type.TYPE_SYSTEM_STRING], s);
        }

        public static string ToMonoString(tSystemString* pStr)
        {
            return (pStr != null ? H.ToObj(((tSystemString*)pStr)->monoStr) as string : null);
        }

        public static /*HEAP_PTR*/byte* FromUserStrings(tMetaData *pMetaData, /*IDX_USERSTRINGS*/uint index) 
        {
        	uint stringLen;
        	/*STRING2*/ushort* str;
        	tSystemString *pSystemString;
            string s;
        	
        	str = MetaData.GetUserString(pMetaData, index, &stringLen);
            s = System.Runtime.InteropServices.Marshal.PtrToStringUni((System.IntPtr)str, (int)stringLen);
        	pSystemString = (tSystemString*)FromMonoString(s);
        	return (/*HEAP_PTR*/byte*)pSystemString;
        }

        public static tSystemString* FromCharPtrASCII(byte *pStr) 
        {
            string s;

            if (pStr == null) {
                return null;
            } else {
                s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)pStr);
                return FromMonoString(s);
            }
        }

        public static tSystemString*  FromCharPtrUTF16(ushort *pStr) 
        {
            string s;

            if (pStr == null) {
                return null;
            } else {
                s = System.Runtime.InteropServices.Marshal.PtrToStringUni((System.IntPtr)pStr);
                return FromMonoString(s);
            }
        }

        public static tAsyncCall* ToString_Internal_Byte(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.Byte);
        }

        public static tAsyncCall* ToString_Internal_SByte(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.SByte);
        }

        public static tAsyncCall* ToString_Internal_UInt16(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt16);
        }

        public static tAsyncCall* ToString_Internal_Int16(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int16);
        }

        public static tAsyncCall* ToString_Internal_UInt32(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt32);
        }

        public static tAsyncCall* ToString_Internal_Int32(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int32);
        }

        public static tAsyncCall* ToString_Internal_UInt64(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt64);
        }

        public static tAsyncCall* ToString_Internal_Int64(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int64);
        }

        public static tAsyncCall* ToString_Internal_Single(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.Single);
        }

        public static tAsyncCall* ToString_Internal_Double(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return ToString_Internal(pThis, pParams, pReturnValue, System.TypeCode.Double);
        }

        private static tAsyncCall* ToString_Internal(byte* pThis, byte* pParams, byte* pReturnValue, System.TypeCode typecode)
        {
            tSystemString* pFormat;
//            byte* pFormatProvider;
            string format, s;
            tSystemString* pResult;

            pFormat = (*((tSystemString**)(pParams + 0)));
            format = pFormat != null ? H.ToObj(pFormat->monoStr) as string : null;

            // Ignore IFormatProvider for now!
            //pFormatProvider = (*((tSystemString**)(pParams + Sys.S_PTR)));

            switch (typecode) {
                case System.TypeCode.Byte:
                    s = (*(byte*)pThis).ToString(format);
                    break;
                case System.TypeCode.SByte:
                    s = (*(sbyte*)pThis).ToString(format);
                    break;
                case System.TypeCode.UInt16:
                    s = (*(ushort*)pThis).ToString(format);
                    break;
                case System.TypeCode.Int16:
                    s = (*(short*)pThis).ToString(format);
                    break;
                case System.TypeCode.UInt32:
                    s = (*(uint*)pThis).ToString(format);
                    break;
                case System.TypeCode.Int32:
                    s = (*(int*)pThis).ToString(format);
                    break;
                case System.TypeCode.UInt64:
                    s = (*(ulong*)pThis).ToString(format);
                    break;
                case System.TypeCode.Int64:
                    s = (*(long*)pThis).ToString(format);
                    break;
                case System.TypeCode.Single:
                    s = (*(float*)pThis).ToString(format);
                    break;
                case System.TypeCode.Double:
                    s = (*(double*)pThis).ToString(format);
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            pResult = System_String.FromMonoString(s);

            Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pResult);

            return null;
        }

        public static tAsyncCall* Parse_Internal_Byte(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Byte);
        }

        public static tAsyncCall* Parse_Internal_SByte(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.SByte);
        }

        public static tAsyncCall* Parse_Internal_UInt16(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt16);
        }

        public static tAsyncCall* Parse_Internal_Int16(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int16);
        }

        public static tAsyncCall* Parse_Internal_UInt32(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt32);
        }

        public static tAsyncCall* Parse_Internal_Int32(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int32);
        }

        public static tAsyncCall* Parse_Internal_UInt64(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt64);
        }

        public static tAsyncCall* Parse_Internal_Int64(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int64);
        }

        public static tAsyncCall* Parse_Internal_Single(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Single);
        }

        public static tAsyncCall* Parse_Internal_Double(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return Parse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Double);
        }

        private static tAsyncCall* Parse_Internal(byte* pThis, byte* pParams, byte* pReturnValue, System.TypeCode typecode)
        {
            System.Globalization.NumberStyles numberStyle;
            ;
            tSystemString* pStr;
//            byte* pFormatProvider;
            string s;

            pStr = (*((tSystemString**)(pParams + 0)));
            s = pStr != null ? H.ToObj(pStr->monoStr) as string : null;
            numberStyle = (System.Globalization.NumberStyles)(*((int*)(pParams + Sys.S_PTR)));

            // Ignore IFormatProvider (for now)
            //pFormatProvider = (*((tSystemString**)(pParams + Sys.S_PTR + Sys.S_INT32)));

            switch (typecode) {
                case System.TypeCode.Byte:
                    (*(uint*)pReturnValue) = byte.Parse(s, numberStyle);
                    break;
                case System.TypeCode.SByte:
                    (*(uint*)pReturnValue) = (uint)sbyte.Parse(s, numberStyle);
                    break;
                case System.TypeCode.UInt16:
                    (*(uint*)pReturnValue) = ushort.Parse(s, numberStyle);
                    break;
                case System.TypeCode.Int16:
                    (*(uint*)pReturnValue) = (uint)short.Parse(s, numberStyle);
                    break;
                case System.TypeCode.UInt32:
                    (*(uint*)pReturnValue) = uint.Parse(s, numberStyle);
                    break;
                case System.TypeCode.Int32:
                    (*(uint*)pReturnValue) = (uint)int.Parse(s, numberStyle);
                    break;
                case System.TypeCode.UInt64:
                    (*(ulong*)pReturnValue) = ulong.Parse(s, numberStyle);
                    break;
                case System.TypeCode.Int64:
                    (*(long*)pReturnValue) = long.Parse(s, numberStyle);
                    break;
                case System.TypeCode.Single:
                    (*(float*)pReturnValue) = float.Parse(s, numberStyle);
                    break;
                case System.TypeCode.Double:
                    (*(double*)pReturnValue) = double.Parse(s, numberStyle);
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            return null;
        }

        public static tAsyncCall* TryParse_Internal_Byte(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Byte);
        }

        public static tAsyncCall* TryParse_Internal_SByte(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.SByte);
        }

        public static tAsyncCall* TryParse_Internal_UInt16(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt16);
        }

        public static tAsyncCall* TryParse_Internal_Int16(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int16);
        }

        public static tAsyncCall* TryParse_Internal_UInt32(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt32);
        }

        public static tAsyncCall* TryParse_Internal_Int32(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int32);
        }

        public static tAsyncCall* TryParse_Internal_UInt64(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.UInt64);
        }

        public static tAsyncCall* TryParse_Internal_Int64(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Int64);
        }

        public static tAsyncCall* TryParse_Internal_Single(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Single);
        }

        public static tAsyncCall* TryParse_Internal_Double(tJITCallNative* pCallNative, byte* pThis, byte* pParams, byte* pReturnValue)
        {
            return TryParse_Internal(pThis, pParams, pReturnValue, System.TypeCode.Double);
        }

        public static tAsyncCall* TryParse_Internal(byte* pThis, byte* pParams, byte* pReturnValue, System.TypeCode typecode)
        {
            System.Globalization.NumberStyles numberStyle;
            tSystemString* pStr;
//            byte* pFormatProvider;
            string s;
            byte* pResult;

            pStr = (*((tSystemString**)(pParams + 0)));
            s = pStr != null ? H.ToObj(pStr->monoStr) as string : null;
            numberStyle = (System.Globalization.NumberStyles)(*((int*)(pParams + Sys.S_PTR)));
            //pFormatProvider = (*((tSystemString**)(pParams + Sys.S_PTR + Sys.S_INT32)));
            pResult = (*((byte**)(pParams + Sys.S_PTR + Sys.S_INT32)));

            // Ignore IFormatProvider

            switch (typecode) {
                case System.TypeCode.Byte:
                    byte b;
                    (*(uint*)pReturnValue) = byte.TryParse(s, numberStyle, null, out b) ? 1U : 0U;
                    break;
                case System.TypeCode.SByte:
                    sbyte sb;
                    (*(uint*)pReturnValue) = sbyte.TryParse(s, numberStyle, null, out sb) ? 1U : 0U;
                    break;
                case System.TypeCode.UInt16:
                    ushort us;
                    (*(uint*)pReturnValue) = ushort.TryParse(s, numberStyle, null, out us) ? 1U : 0U;
                    break;
                case System.TypeCode.Int16:
                    short ss;
                    (*(uint*)pReturnValue) = short.TryParse(s, numberStyle, null, out ss) ? 1U : 0U;
                    break;
                case System.TypeCode.UInt32:
                    uint ui;
                    (*(uint*)pReturnValue) = uint.TryParse(s, numberStyle, null, out ui) ? 1U : 0U;
                    break;
                case System.TypeCode.Int32:
                    int si;
                    (*(uint*)pReturnValue) = int.TryParse(s, numberStyle, null, out si) ? 1U : 0U;
                    break;
                case System.TypeCode.UInt64:
                    ulong ul;
                    (*(uint*)pReturnValue) = ulong.TryParse(s, numberStyle, null, out ul) ? 1U : 0U;
                    break;
                case System.TypeCode.Int64:
                    long sl;
                    (*(uint*)pReturnValue) = long.TryParse(s, numberStyle, null, out sl) ? 1U : 0U;
                    break;
                case System.TypeCode.Single:
                    float f;
                    (*(uint*)pReturnValue) = float.TryParse(s, numberStyle, null, out f) ? 1U : 0U;
                    break;
                case System.TypeCode.Double:
                    double d;
                    (*(uint*)pReturnValue) = double.TryParse(s, numberStyle, null, out d) ? 1U : 0U;
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            return null;
        }

    }
}
