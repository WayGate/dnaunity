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
    #else
    using SIZE_T = System.UInt64;
    #endif

    public static unsafe class Mem
    {
        public static void* malloc(SIZE_T size)
        {
            throw new System.NotImplementedException();
        }

        public static void* realloc(void* p, SIZE_T size)
        {
            throw new System.NotImplementedException();
        }

        public static void* mallocForever(SIZE_T size)
        {
            throw new System.NotImplementedException();
        }

        public static void free(void* p)
        {
            throw new System.NotImplementedException();
        }

        public static void memcpy(void* p1, void* p2, SIZE_T size)
        {
            throw new System.NotImplementedException();
        }

        public static void memset(void* p, int v, SIZE_T size)
        {
            throw new System.NotImplementedException();
        }

        public static int memcmp(void* a, void* b, SIZE_T size)
        {
            throw new System.NotImplementedException();
        }
    }

}
