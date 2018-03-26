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

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tRVA_Item 
    {
        public uint baseAddress;
        public uint size;
        public void *pData;

        public tRVA_Item *pNext;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tRVA 
    {
        public tRVA_Item *pFirstRVA;
    };

    public unsafe static class RVA
    {
        public static tRVA* New() {
        	tRVA *pRet;
            pRet = ((tRVA*)Mem.malloc((SIZE_T)sizeof(tRVA)));
        	return pRet;
        }

        public static tRVA_Item* Create(tRVA *pThis, void *pFile, void *pSectionHeader) {
        	tRVA_Item* pRet;
        	uint rawOfs;
        	uint rawSize;

            pRet = ((tRVA_Item*)Mem.malloc((SIZE_T)sizeof(tRVA_Item)));
        	pRet->baseAddress = *(uint*)&((byte*)pSectionHeader)[12];
        	pRet->size = *(uint*)&((byte*)pSectionHeader)[8];
        	pRet->pData = Mem.malloc(pRet->size);
        	Mem.memset(pRet->pData, 0, pRet->size);
        	pRet->pNext = pThis->pFirstRVA;
        	pThis->pFirstRVA = pRet;

        	rawOfs = *(uint*)&((byte*)pSectionHeader)[20];
        	rawSize = *(uint*)&((byte*)pSectionHeader)[16];
        	if (rawOfs > 0) {
        		if (rawSize > pRet->size) {
        			rawSize = pRet->size;
        		}
        		Mem.memcpy(pRet->pData, ((byte*)pFile)+rawOfs, rawSize);
        	}

        	return pRet;
        }

        public static void* FindData(tRVA *pThis, uint rva) {
        	tRVA_Item *pRVA;

        	if (rva == 0) {
        		return null;
        	}

        	pRVA = pThis->pFirstRVA;
        	while (pRVA != null) {
        		if (rva >= pRVA->baseAddress && rva < pRVA->baseAddress+pRVA->size) {
        			return (byte*)(pRVA->pData) + (rva - pRVA->baseAddress);
        		}
        		pRVA = pRVA->pNext;
        	}
        	return null;
        }

    }
}
