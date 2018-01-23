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
    #if UNITY_WEBGL || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif   

    // This structure must tie up with string.cs
    public unsafe struct tSystemString
    {
    	// Length in characters (not bytes)
    	public uint length;
    	// The characters
    	// ushort chars[0];

        public static char* GetChars(tSystemString* str)
        {
            return (char*)str + 4;
        }
    };


    public unsafe static class System_String
    {
        // length in characters, not bytes
        static tSystemString* CreateStringHeapObj(uint len)
        {
        	tSystemString* pSystemString;
        	uint totalSize;
        	
            totalSize = (uint)(sizeof(tSystemString) + (len << 1));
        	pSystemString = (tSystemString*)Heap.Alloc(Type.types[Type.TYPE_SYSTEM_STRING], totalSize);
        	pSystemString->length = len;
        	return pSystemString;
        }

        public static tAsyncCall* ctor_CharInt32(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pSystemString;
        	char c;
        	uint i, len;
            char* pChars;

            c = (char)(*((uint*)(pParams + 0)));
            len = (*((uint*)(pParams + Sys.S_INT)));
        	pSystemString = CreateStringHeapObj(len);
            pChars = tSystemString.GetChars(pSystemString);
        	for (i=0; i<len; i++) {
                pChars[i] = c;
        	}
            Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pSystemString);

        	return null;
        }

        public static tAsyncCall* ctor_CharAIntInt(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pSystemString;
        	/*HEAP_PTR*/byte* charArray;
        	byte* charElements;
        	uint startIndex, length;
            char* pChars;

            charArray = (*((byte**)(pParams + 0)));
            startIndex = (*((uint*)(pParams + Sys.S_PTR)));
            length = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT)));

        	charElements = System_Array.GetElements(charArray);
        	pSystemString = CreateStringHeapObj(length);
            pChars = tSystemString.GetChars(pSystemString);
            Mem.memcpy(pChars, charElements + (startIndex << 1), (SIZE_T)(length << 1));
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pSystemString);

        	return null;
        }

        public static tAsyncCall* ctor_StringIntInt(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pThis, pStr;
        	uint startIndex, length;
            char* pChars;

        	pStr = (*((tSystemString**)(pParams + 0)));
        	startIndex = (*((uint*)(pParams + Sys.S_PTR)));
        	length = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT)));

        	pThis = CreateStringHeapObj(length);
            pChars = tSystemString.GetChars(pThis);
            Mem.memcpy(pChars, &pChars[startIndex], (SIZE_T)(length << 1));
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pThis);

        	return null;
        }

        public static tAsyncCall* get_Chars(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pThis = (tSystemString*)pThis_;
        	uint index;
            char* pChars;

        	index = (*((uint*)(pParams + 0)));
            pChars = tSystemString.GetChars(pThis);
            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (uint)(pChars[index]));

        	return null;
        }

        public static tAsyncCall* InternalConcat(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* s0, s1, ret;
            char* pS0Chars, pS1Chars, pRetChars;

        	s0 = (*((tSystemString**)(pParams + 0)));
            pS0Chars = tSystemString.GetChars(s0);
        	s1 = (*((tSystemString**)(pParams + Sys.S_PTR)));
            pS1Chars = tSystemString.GetChars(s1);
        	ret = CreateStringHeapObj(s0->length + s1->length);
            pRetChars = tSystemString.GetChars(ret);
            Mem.memcpy(pRetChars, pS0Chars, (SIZE_T)(s0->length << 1));
            Mem.memcpy(&pRetChars[s0->length], pS1Chars, (SIZE_T)(s1->length << 1));
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, ret);

        	return null;
        }

        public static tAsyncCall* InternalTrim(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pThis = (tSystemString*)pThis_;
        	/*HEAP_PTR*/byte* pWhiteChars;
        	uint trimType, i, j, checkCharsLen;
        	uint ofsStart, ofsEnd;
        	ushort* pCheckChars;
        	uint isWhiteSpace;
        	tSystemString* pRet;
        	ushort c;
            char* pChars, pRetChars;

        	pWhiteChars = (*((/*HEAP_PTR*/byte**)(pParams + 0)));
        	trimType = (*((uint*)(pParams + Sys.S_PTR)));
        	pCheckChars = (ushort*)System_Array.GetElements(pWhiteChars);
        	checkCharsLen = System_Array.GetLength(pWhiteChars);

        	ofsStart = 0;
        	ofsEnd = pThis->length;
            pChars = tSystemString.GetChars(pThis);
            if ((trimType & 1) != 0) {
        		// Trim start
        		for (i=ofsStart; i<ofsEnd; i++) {
        			// Check if each char is in the array
        			isWhiteSpace = 0;
        			c = pChars[i];
        			for (j=0; j<checkCharsLen; j++) {
        				if (c == pCheckChars[j]) {
        					isWhiteSpace = 1;
        					break;
        				}
        			}
        			if (isWhiteSpace == 0) {
        				ofsStart = i;
        				break;
        			}
        		}
        	}
            if ((trimType & 2) != 0) {
        		// Trim end
        		for (i=ofsEnd-1; i>=ofsStart; i--) {
        			// Check if each char is in the array
        			isWhiteSpace = 0;
        			c = pChars[i];
        			for (j=0; j<checkCharsLen; j++) {
        				if (c == pCheckChars[j]) {
        					isWhiteSpace = 1;
        					break;
        				}
        			}
        			if (isWhiteSpace == 0) {
        				ofsEnd = i + 1;
        				break;
        			}
        		}
        	}

        	pRet = CreateStringHeapObj(ofsEnd - ofsStart);
            pRetChars = tSystemString.GetChars(pRet);
            Mem.memcpy(pRetChars, &pChars[ofsStart], (SIZE_T)((ofsEnd - ofsStart) << 1));
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pRet);

        	return null;
        }

        public static tAsyncCall* Equals(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* a, b;
        	uint ret;
            char* pAChars, pBChars;

            a = (*((tSystemString**)(pParams + 0)));
            b = (*((tSystemString**)(pParams + Sys.S_PTR)));

        	if (a == b) {
        		ret = 1;
        	} else if (a == null || b == null || a->length != b->length) {
        		ret = 0;
        	} else {
                pAChars = tSystemString.GetChars(a);
                pBChars = tSystemString.GetChars(b);
                ret = (uint)((Mem.memcmp(pAChars, pBChars, (SIZE_T)(a->length<<1)) == 0)?1:0);
        	}
        	Sys.INTERNALCALL_RESULT_U32(pReturnValue, ret);

        	return null;
        }

        public static tAsyncCall* GetHashCode(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pThis = (tSystemString*)pThis_;
        	char* pChar, pEnd;
        	int hash;
        	
        	hash = 0;
            pChar = tSystemString.GetChars(pThis);
        	pEnd = pChar + pThis->length - 1;
        	for (; pChar < pEnd; pChar += 2) {
        		hash = (hash << 5) - hash + pChar[0];
        		hash = (hash << 5) - hash + pChar[1];
        	}
        	if (pChar <= pEnd) {
        		hash = (hash << 5) - hash + pChar[0];
        	}
            Sys.INTERNALCALL_RESULT_U32(pReturnValue, (uint)hash);

        	return null;
        }

        public static tAsyncCall* InternalReplace(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pThis = (tSystemString*)pThis_;
        	tSystemString* pOld = (*((tSystemString**)(pParams + 0)));
        	tSystemString* pNew = (*((tSystemString**)(pParams + Sys.S_PTR)));
        	tSystemString* pResult;
        	uint thisLen, oldLen, newLen;
        	char* pThisChar0, pOldChar0, pNewChar0, pResultChar0;
        	uint i, j, replacements, dstIndex;
        	uint resultLen;

        	// This function (correctly) assumes that the old string is not empty
        	thisLen = pThis->length;
        	oldLen = pOld->length;
        	newLen = pNew->length;
            pThisChar0 = tSystemString.GetChars(pThis);
            pOldChar0 = tSystemString.GetChars(pOld);
            pNewChar0 = tSystemString.GetChars(pNew);

        	replacements = 0;
        	for (i=0; i<thisLen-oldLen+1; i++) {
        		uint match = 1;
        		for (j=0; j<oldLen; j++) {
        			if (pThisChar0[i+j] != pOldChar0[j]) {
        				match = 0;
        				break;
        			}
        		}
        		if (match != 0) {
        			i += oldLen - 1;
        			replacements++;
        		}
        	}
        	resultLen = thisLen - (oldLen - newLen) * replacements;
        	pResult = CreateStringHeapObj(resultLen);
            pResultChar0 = tSystemString.GetChars(pResult);
        	dstIndex = 0;
        	for (i=0; i<thisLen; i++) {
        		uint match = 1;
        		if (i<thisLen-oldLen+1) {
        			for (j=0; j<oldLen; j++) {
        				match = 1;
        				if (pThisChar0[i+j] != pOldChar0[j]) {
        					match = 0;
        					break;
        				}
        			}
        		} else {
        			match = 0;
        		}
        		if (match != 0) {
        			Mem.memcpy(&pResultChar0[dstIndex], pNewChar0, newLen << 1);
        			dstIndex += newLen;
        			i += oldLen - 1;
        		} else {
        			pResultChar0[dstIndex++] = pThisChar0[i];
        		}
        	}
        	Sys.INTERNALCALL_RESULT_PTR(pReturnValue, pResult);

        	return null;
        }

        public static tAsyncCall* InternalIndexOf(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString* pThis = (tSystemString*)pThis_;
        	ushort value = (*((ushort*)(pParams + 0)));
        	int startIndex = (*((int*)(pParams + Sys.S_INT)));
        	int count = (*((int*)(pParams + Sys.S_INT + Sys.S_INT)));
        	uint forwards = (*((uint*)(pParams + Sys.S_INT + Sys.S_INT + Sys.S_INT)));
            char* pChars = tSystemString.GetChars(pThis);

        	int lastIndex;
        	int inc;
        	int i;

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
        		if (pChars[i] == value) {
        			Sys.INTERNALCALL_RESULT_I32(pReturnValue, i);
        			return null;
        		}
        	}
            Sys.INTERNALCALL_RESULT_I32(pReturnValue, -1);
        	return null;
        }

        public static tAsyncCall* InternalIndexOfAny(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tSystemString *pThis = (tSystemString*)pThis_;
        	/*HEAP_PTR*/byte* valueArray = (*((/*HEAP_PTR*/byte**)(pParams + 0)));
        	int startIndex = (*((int*)(pParams + Sys.S_PTR)));
        	int count = (*((int*)(pParams + Sys.S_PTR + Sys.S_INT)));
        	uint forwards = (*((uint*)(pParams + Sys.S_PTR + Sys.S_INT + Sys.S_INT)));
            char* pChars = tSystemString.GetChars(pThis);

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
        		char thisChar = pChars[i];
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

        public static /*HEAP_PTR*/byte* FromUserStrings(tMetaData *pMetaData, /*IDX_USERSTRINGS*/uint index) 
        {
        	uint stringLen;
        	/*STRING2*/ushort* str;
        	tSystemString *pSystemString;
            char* pSystemStringChars;
        	
        	str = MetaData.GetUserString(pMetaData, index, &stringLen);
        	// Note: stringLen is in bytes
        	pSystemString = (tSystemString*)CreateStringHeapObj(stringLen >> 1);
            pSystemStringChars = tSystemString.GetChars(pSystemString);
            Mem.memcpy(pSystemStringChars, str, (SIZE_T)stringLen);
        	return (/*HEAP_PTR*/byte*)pSystemString;
        }

        public static /*HEAP_PTR*/byte* FromCharPtrASCII(byte *pStr) 
        {
        	int stringLen, i;
        	tSystemString *pSystemString;
            char* pSystemStringChars;

        	stringLen = (int)S.strlen(pStr);
            pSystemString = CreateStringHeapObj((uint)stringLen);
            pSystemStringChars = tSystemString.GetChars(pSystemString);
        	for (i=0; i<stringLen; i++) {
                pSystemStringChars[i] = (char)pStr[i];
        	}
        	return (/*HEAP_PTR*/byte*)pSystemString;
        }

        public static /*HEAP_PTR*/byte* FromCharPtrUTF16(ushort *pStr) 
        {
        	tSystemString *pSystemString;
        	int strLen = 0;
            char* pSystemStringChars;

        	while (pStr[strLen] != 0) {
        		strLen++;
        	}
            pSystemString = CreateStringHeapObj((uint)strLen);
            pSystemStringChars = tSystemString.GetChars(pSystemString);
            Mem.memcpy(pSystemStringChars, pStr, (SIZE_T)(strLen << 1));
        	return (/*HEAP_PTR*/byte*)pSystemString;
        }

        public static /*STRING2*/char* GetString(/*HEAP_PTR*/byte* pThis_, uint *pLength) 
        {
        	tSystemString *pThis = (tSystemString*)pThis_;

        	if (pLength != null) {
        		*pLength = pThis->length;
        	}
            return tSystemString.GetChars(pThis);
        }

        public static uint GetNumBytes(/*HEAP_PTR*/byte* pThis_) 
        {
            return (uint)((((tSystemString*)pThis_)->length << 1) + sizeof(tSystemString));
        }
    }
}
