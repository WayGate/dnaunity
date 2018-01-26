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
    #else
    using SIZE_T = System.UInt64;
    #endif

    public static unsafe class Mem
    {
        const int DEFAULT_SIZE = 128 * 1024; // 128K

        static byte* pMem;
        static int memSize;
        static int memUsed;

        public static void Init(int size)
        {
            pMem = (byte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            memSize = size;
            memUsed = 0;
        }

        public static void Clear()
        {
            if (pMem != null)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal((System.IntPtr)pMem);
                pMem = null;
            }
            memSize = memUsed = 0;
        }

        public static void* malloc(SIZE_T size)
        {
            if (size == 0)
                return null;
            if (memUsed + (int)size > memSize)
                throw new System.OutOfMemoryException();
            SIZE_T realSize = 8 + ((size + 7) & 0xFFFFFFF8);
            if (pMem == null)
                Init((int)DEFAULT_SIZE);
            byte* p = pMem + memUsed;
            memUsed += (int)realSize;
            *(uint*)p = (uint)size;
            *(uint*)(p + 4) = (uint)0;
            return p + 8;
        }

        public static void* realloc(void* p, SIZE_T size)
        {
            ulong* a = (ulong*)p;
            ulong* b = (ulong*)malloc(size);
            int oldWords = (int)(((*(int*)(a - 8)) + 7) & 0xFFFFFFF8) >> 3;
            int newWords = (int)((size + 7) & 0xFFFFFFF8) >> 3;
            int minWords = newWords < oldWords ? newWords : oldWords;
            for (int i = 0; i < minWords; i++)
                *b++ = *a++;
            return b;
        }

        public static void* mallocForever(SIZE_T size)
        {
            return malloc(size);
        }

        public static void free(void* p)
        {
            // DO NOTHING FOR NOW
        }

        public static void memcpy(void* p1, void* p2, SIZE_T size)
        {
            // For now.. slow but simple - accurate
            byte* a = (byte*)p1;
            byte* b = (byte*)p2;
            int len = (int)size;
            for (int i = 0; i < len; i++)
                *a++ = *b++;
        }

        public static void memmove(void* p1, void* p2, SIZE_T size)
        {
            // Handle overlapping regions correctly!
            if (p1 > p2)
            {
                memcpy(p1, p2, size);
            }
            else
            {
                // For now.. slow but simple - accurate
                byte* a = (byte*)p1 + size - 1;
                byte* b = (byte*)p2 + size - 1;
                int len = (int)size;
                for (int i = 0; i < len; i++)
                    *a-- = *b--;                
            }
        }

        public static void memset(void* p, int v, SIZE_T size)
        {
            // For now.. slow but simple - accurate
            byte* a = (byte*)p;
            byte b = (byte)v;
            int len = (int)size;
            for (int i = 0; i < len; i++)
                *a++ = b;
        }

        public static int memcmp(void* p1, void* p2, SIZE_T size)
        {
            // For now.. slow but simple - accurate
            byte* a = (byte*)p1;
            byte* b = (byte*)p2;
            int len = (int)size;
            for (int i = 0; i < len; i++)
            {
                if (*a < *b)
                    return -1;
                else if (*a > *b)
                    return 1;
                a++;
                b++;
            }
            return 0;
        }
    }

}
