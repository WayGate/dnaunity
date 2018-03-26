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

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif

    // Handles to mono objects (essentially what GCHandle does, but compatible with Unity)
    public unsafe struct H
    {
        public PTR _p;

        public static List<object> objects = null;
        public static List<int> freeList = null;

        public static void Init()
        {
            objects = new List<object>(1024);
            objects.Add(0);  // Add first item at 0 for "null"
            freeList = new List<int>(256);
        }

        public static void Clear()
        {
            objects = null;
            freeList = null;
        }

        public static void* Alloc(object o)
        {
            if (o != null) {
                int idx;
                int freeCount = freeList.Count;
                if (freeCount > 0) {
                    idx = freeList[freeCount - 1];
                    freeList.RemoveAt(freeCount - 1);
                    objects[idx] = o;
                } else {
                    idx = objects.Count;
                    objects.Add(o);
                }
                return (void*)idx;
            } else {
                return null;
            }
        }

        public static void Free(void* p)
        {
            if (p != null) {
                int idx = (int)p;
                objects[idx] = null;
                freeList.Add(idx);
            }
        }

        public H(object o)
        {
            _p = (PTR)Alloc(o);
        }

        public H(fnInternalCall o)
        {
            _p = (PTR)Alloc(o);
        }

        public H(fnInternalCallCheck o)
        {
            _p = (PTR)Alloc(o);
        }

        public H(fnFieldGetterSetter o)
        {
            _p = (PTR)Alloc(o);
        }

        public H(ref void* p, object o)
        {
            if (p != null) {
                _p = (PTR)p;
            } else if (o != null) {
                p = Alloc(o);
                _p = (PTR)p;
            } else {
                _p = 0;
            }
        }

        public static implicit operator byte*(H h)  // explicit byte to digit conversion operator
        {
            return (byte*)h._p;
        }

        public static implicit operator void*(H h)  // explicit byte to digit conversion operator
        {
            return (void*)h._p;
        }

        public static object ToObj(void* p) 
        {
            return objects[(int)p];
        }

    }
}

