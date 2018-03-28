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

#if !LOCALTEST

using System.Globalization;

namespace System {
	public unsafe struct UIntPtr {

        unsafe private void* m_value;

        public static readonly UIntPtr Zero;

        public unsafe UIntPtr(uint value)
        {
            m_value = (void*)value;
        }

        public unsafe UIntPtr(ulong value)
        {
#if WIN32
            m_value = (void*)checked((uint)value);
#else
            m_value = (void*)value;
#endif
        }

        public unsafe UIntPtr(void* value)
        {
            m_value = value;
        }

//        private unsafe UIntPtr(SerializationInfo info, StreamingContext context)
//        {
//            ulong l = info.GetUInt64("value");
//
//            if (Size == 4 && l > UInt32.MaxValue) {
//                throw new ArgumentException(Environment.GetResourceString("Serialization_InvalidPtrValue"));
//            }
//
//            m_value = (void*)l;
//        }

#if FEATURE_SERIALIZATION
        [System.Security.SecurityCritical]
        unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();
            info.AddValue("value", (ulong)m_value);
        }
#endif

        public unsafe override bool Equals(Object obj)
        {
            if (obj is UIntPtr) {
                return (m_value == ((UIntPtr)obj).m_value);
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return unchecked((int)((long)m_value)) & 0x7fffffff;
        }

        public unsafe uint ToUInt32()
        {
#if WIN32
            return (uint)m_value;
#else
            return checked((uint)m_value);
#endif
        }

        public unsafe ulong ToUInt64()
        {
            return (ulong)m_value;
        }

        public unsafe override String ToString()
        {
#if WIN32
            return ((uint)m_value).ToString(CultureInfo.InvariantCulture);
#else
            return ((ulong)m_value).ToString(CultureInfo.InvariantCulture);
#endif
        }

        public static explicit operator UIntPtr(uint value)
        {
            return new UIntPtr(value);
        }

        public static explicit operator UIntPtr(ulong value)
        {
            return new UIntPtr(value);
        }

        public unsafe static explicit operator uint(UIntPtr value)
        {
#if WIN32
            return (uint)value.m_value;
#else
            return checked((uint)value.m_value);
#endif
        }

        public unsafe static explicit operator ulong(UIntPtr value)
        {
            return (ulong)value.m_value;
        }

        public static unsafe explicit operator UIntPtr(void* value)
        {
            return new UIntPtr(value);
        }

        public static unsafe explicit operator void* (UIntPtr value)
        {
            return value.m_value;
        }

        public unsafe static bool operator ==(UIntPtr value1, UIntPtr value2)
        {
            return value1.m_value == value2.m_value;
        }

        public unsafe static bool operator !=(UIntPtr value1, UIntPtr value2)
        {
            return value1.m_value != value2.m_value;
        }

        public static UIntPtr Add(UIntPtr pointer, int offset)
        {
            return pointer + offset;
        }

        public static UIntPtr operator +(UIntPtr pointer, int offset)
        {
#if WIN32
                return new UIntPtr(pointer.ToUInt32() + (uint)offset);
#else
            return new UIntPtr(pointer.ToUInt64() + (ulong)offset);
#endif
        }

        public static UIntPtr Subtract(UIntPtr pointer, int offset)
        {
            return pointer - offset;
        }

        public static UIntPtr operator -(UIntPtr pointer, int offset)
        {
#if WIN32
                return new UIntPtr(pointer.ToUInt32() - (uint)offset);
#else
            return new UIntPtr(pointer.ToUInt64() - (ulong)offset);
#endif
        }

        public static int Size {
            get {
#if WIN32
                return 4;
#else
                return 8;
#endif
            }
        }

        public unsafe void* ToPointer()
        {
            return m_value;
        }


    }
}

#endif