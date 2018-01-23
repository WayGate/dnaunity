﻿// Copyright (c) 2012 DotNetAnywhere
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

    public unsafe struct H
    {
        public System.IntPtr _p;

        public H(object o)
        {
            _p = System.Runtime.InteropServices.GCHandle.Alloc(o, System.Runtime.InteropServices.GCHandleType.Normal).AddrOfPinnedObject();
        }

        public H(fnInternalCall o)
        {
            _p = System.Runtime.InteropServices.GCHandle.Alloc(o, System.Runtime.InteropServices.GCHandleType.Normal).AddrOfPinnedObject();
        }

        public H(fnInternalCallCheck o)
        {
            _p = System.Runtime.InteropServices.GCHandle.Alloc(o, System.Runtime.InteropServices.GCHandleType.Normal).AddrOfPinnedObject();
        }

        public H(ref void* p, object o)
        {
            if (p != null)
            {
                _p = (System.IntPtr)p;
            }
            else
            {
                _p = System.Runtime.InteropServices.GCHandle.Alloc(o, System.Runtime.InteropServices.GCHandleType.Normal).AddrOfPinnedObject();
                p = (void*)_p;
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
            if (p != null)
                return System.Runtime.InteropServices.GCHandle.FromIntPtr((System.IntPtr)p).Target;
            else
                return null;
        }

    }
}

