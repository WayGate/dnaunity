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
using System.Runtime.CompilerServices;

namespace System {
	public struct SByte : IFormattable, IComparable, IConvertible, IComparable<sbyte>, IEquatable<sbyte> {
		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;

#pragma warning disable 0169, 0649
        internal sbyte m_value;
#pragma warning restore 0169, 0649

        public override bool Equals(object obj) {
			return (obj is sbyte) && ((sbyte)obj).m_value == this.m_value;
		}

		public override int GetHashCode() {
			return (int)this.m_value;
		}

		#region ToString methods

		public override string ToString() {
            return this.ToString(null, null);
		}

		public string ToString(IFormatProvider formatProvider) {
            return this.ToString(null, formatProvider);
		}

		public string ToString(string format) {
            return this.ToString(format, null);
		}

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern string ToString(string format, IFormatProvider formatProvider);

        #endregion

        #region Parse methods

        public static sbyte Parse(String s)
        {
            return Parse(s, NumberStyles.Integer, null);
        }

        public static sbyte Parse(String s, NumberStyles style)
        {
            return Parse(s, style, null);
        }

        public static sbyte Parse(String s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, null);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern sbyte Parse(String s, NumberStyles style, IFormatProvider provider);

        public static bool TryParse(String s, out SByte result)
        {
            return TryParse(s, NumberStyles.Integer, null, out result);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool TryParse(String s, NumberStyles style, IFormatProvider provider, out SByte result);

        #endregion

        #region IComparable Members

        public int CompareTo(object obj) {
			if (obj == null) {
				return 1;
			}
			if (!(obj is int)) {
				throw new ArgumentException();
			}
			return this.CompareTo((sbyte)obj);
		}

		#endregion

		#region IComparable<sbyte> Members

		public int CompareTo(sbyte x) {
			return (this.m_value > x) ? 1 : ((this.m_value < x) ? -1 : 0);
		}

		#endregion

		#region IEquatable<sbyte> Members

		public bool Equals(sbyte x) {
			return this.m_value == x;
		}

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.SByte;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return this;
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this);
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(string))
                return this.ToString(provider);
            else
                return Convert.ChangeType(this, conversionType);
        }

        #endregion


    }
}

#endif