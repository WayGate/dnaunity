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
using System.IO;

namespace System
{

    [Serializable]
    public unsafe struct IntPtr
    {

        unsafe private void* m_value; // The compiler treats void* closest to uint hence explicit casts are required to preserve int behavior

        public static readonly IntPtr Zero;

        internal unsafe bool IsNull()
        {
            return (this.m_value == null);
        }

        public unsafe IntPtr(int value)
        {
#if WIN32
                m_value = (void *)value;
#else
            m_value = (void*)(long)value;
#endif
        }

        public unsafe IntPtr(long value)
        {
#if WIN32
                m_value = (void *)checked((int)value);
#else
            m_value = (void*)value;
#endif
        }

        public unsafe IntPtr(void* value)
        {
            m_value = value;
        }

        //        private unsafe IntPtr(SerializationInfo info, StreamingContext context)
        //        {
        //            long l = info.GetInt64("value");
        //
        //            if (Size == 4 && (l > Int32.MaxValue || l < Int32.MinValue)) {
        //                throw new ArgumentException("Serialization_InvalidPtrValue");
        //            }
        //
        //            m_value = (void*)l;
        //        }

#if FEATURE_SERIALIZATION
        [System.Security.SecurityCritical]
        unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();
#if WIN32
                info.AddValue("value", (long)((int)m_value));
#else
                info.AddValue("value", (long)(m_value));
#endif
        }
#endif

        public unsafe override bool Equals(Object obj)
        {
            if (obj is IntPtr) {
                return (m_value == ((IntPtr)obj).m_value);
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return unchecked((int)((long)m_value));
        }

        public unsafe int ToInt32()
        {
#if WIN32
                return (int)m_value;
#else
            long l = (long)m_value;
            return checked((int)l);
#endif
        }

        public unsafe long ToInt64()
        {
#if WIN32
                return (long)(int)m_value;
#else
            return (long)m_value;
#endif
        }

        public unsafe override String ToString()
        {
#if WIN32
                return ((int)m_value).ToString(CultureInfo.InvariantCulture);
#else
            return ((long)m_value).ToString(CultureInfo.InvariantCulture);
#endif
        }

        public unsafe String ToString(String format)
        {
#if WIN32
            return ((int)m_value).ToString(format, CultureInfo.InvariantCulture);
#else
            return ((long)m_value).ToString(format, CultureInfo.InvariantCulture);
#endif
        }

        public static explicit operator IntPtr(int value)
        {
            return new IntPtr(value);
        }

        public static explicit operator IntPtr(long value)
        {
            return new IntPtr(value);
        }

        public static unsafe explicit operator IntPtr(void* value)
        {
            return new IntPtr(value);
        }

        public static unsafe explicit operator void* (IntPtr value)
        {
            return value.m_value;
        }

        public unsafe static explicit operator int(IntPtr value)
        {
#if WIN32
                return (int)value.m_value;
#else
            long l = (long)value.m_value;
            return checked((int)l);
#endif
        }

        public unsafe static explicit operator long(IntPtr value)
        {
#if WIN32
                return (long)(int)value.m_value;
#else
            return (long)value.m_value;
#endif
        }

        public unsafe static bool operator ==(IntPtr value1, IntPtr value2)
        {
            return value1.m_value == value2.m_value;
        }

        public unsafe static bool operator !=(IntPtr value1, IntPtr value2)
        {
            return value1.m_value != value2.m_value;
        }

        public static IntPtr Add(IntPtr pointer, int offset)
        {
            return pointer + offset;
        }

        public static IntPtr operator +(IntPtr pointer, int offset)
        {
#if WIN32
                return new IntPtr(pointer.ToInt32() + offset);
#else
            return new IntPtr(pointer.ToInt64() + offset);
#endif
        }

        public static IntPtr Subtract(IntPtr pointer, int offset)
        {
            return pointer - offset;
        }

        public static IntPtr operator -(IntPtr pointer, int offset)
        {
#if WIN32
                return new IntPtr(pointer.ToInt32() - offset);
#else
            return new IntPtr(pointer.ToInt64() - offset);
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
