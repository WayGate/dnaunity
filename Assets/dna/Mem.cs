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

        const ulong HEAP_DEAD_BEEF = 0xDEADBEEFDEADBEEFUL;

        public static void Init(int size)
        {
            pMem = (byte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            *(ulong*)pMem = HEAP_DEAD_BEEF;
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
            heapcheck();
            if (size == 0)
                return null;
            if (memUsed + (int)size > memSize - 8)
                throw new System.OutOfMemoryException();
            SIZE_T realSize = 16 + ((size + 7) & 0xFFFFFFF8);
            if (pMem == null)
                Init((int)DEFAULT_SIZE);
            byte* p = pMem + memUsed;
            memUsed += (int)realSize;
            *(ulong*)p = HEAP_DEAD_BEEF;
            *(uint*)(p + 8) = (uint)size;
            *(uint*)(p + 12) = (uint)0;
            *(ulong*)(p + realSize) = HEAP_DEAD_BEEF;
            return p + 16;
        }

        //[System.Diagnostics.Conditional("CHECK_HEAP")]
        public static void heapcheck()
        {
            if (Sys.isCrashed == 1)
                return;
            byte* p = pMem;
            byte* e = pMem + memUsed;
            for (;;) {
                if (*(ulong*)p != HEAP_DEAD_BEEF) {
                    Sys.Crash("ERROR: Heap corruption detected!");
                }
                if (p >= e)
                    break;
                uint size = *(uint*)(p + 8);
                SIZE_T realSize = 16 + ((size + 7) & 0xFFFFFFF8);
                p += realSize;
            }
        }

        public static void* realloc(void* p, SIZE_T size)
        {
            heapcheck();
            ulong* a = (ulong*)p;
            ulong* newP = (ulong*)malloc(size);
            ulong* b = newP;
            int oldsize = *(int*)((byte*)a - 8);
            int oldWords = (int)((oldsize + 7) & 0xFFFFFFF8) >> 3;
            int newWords = (int)((size + 7) & 0xFFFFFFF8) >> 3;
            int minWords = newWords < oldWords ? newWords : oldWords;
            for (int i = 0; i < minWords; i++)
                *b++ = *a++;
            return newP;
        }

        public static void* mallocForever(SIZE_T size)
        {
            return malloc(size);
        }

        public static void free(void* p)
        {
            heapcheck();
            // DO NOTHING FOR NOW
        }

        public static void memcpy(void* p1, void* p2, SIZE_T size)
        {
            heapcheck();
            // For now.. slow but simple - accurate
            byte* a = (byte*)p1;
            byte* b = (byte*)p2;
            int len = (int)size;
            for (int i = 0; i < len; i++)
                *a++ = *b++;
            heapcheck();
        }

        public static void memmove(void* p1, void* p2, SIZE_T size)
        {
            heapcheck();
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
            heapcheck();
        }

        public static void memset(void* p, int v, SIZE_T size)
        {
            heapcheck();
            // For now.. slow but simple - accurate
            byte* a = (byte*)p;
            byte b = (byte)v;
            int len = (int)size;
            for (int i = 0; i < len; i++)
                *a++ = b;
            heapcheck();
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
