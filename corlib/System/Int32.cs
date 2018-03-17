#if !LOCALTEST

using System.Runtime.CompilerServices;
using System.Globalization;
using System.Threading;

namespace System {
	public struct Int32 : IFormattable, IComparable, IConvertible, IComparable<int>,IEquatable<int> {
		public const int MaxValue = 0x7fffffff;
		public const int MinValue = -2147483648;

		internal int m_value;

		public override bool Equals(object obj) {
			return (obj is int && ((int)obj).m_value == this.m_value);
		}

		public override int GetHashCode() {
			return this.m_value;
		}

		#region ToString methods

		public override string ToString() {
			return NumberFormatter.FormatGeneral(new NumberFormatter.NumberStore(this.m_value));
		}

		public string ToString(IFormatProvider formatProvider) {
			return NumberFormatter.FormatGeneral(new NumberFormatter.NumberStore(this.m_value), formatProvider);
		}

		public string ToString(string format) {
			return this.ToString(format, null);
		}

		public string ToString(string format, IFormatProvider formatProvider) {
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance(formatProvider);
			return NumberFormatter.NumberToString(format, this.m_value, nfi);
		}

        #endregion

        #region Parse methods

        public static int Parse(string s)
        {
            return Parse(s, NumberStyles.Integer, null);
        }

        public static int Parse(string s, NumberStyles style)
        {
            return Parse(s, style, null);
        }

        public static int Parse(string s, IFormatProvider formatProvider)
        {
            return Parse(s, NumberStyles.Integer, formatProvider);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static int Parse(string s, NumberStyles style, IFormatProvider formatProvider);

        public static bool TryParse(string s, out int result)
        {
            return TryParse(s, NumberStyles.Integer, null, out result);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static bool TryParse(string s, NumberStyles style, IFormatProvider format, out int result);

        #endregion

        #region IComparable Members

        public int CompareTo(object obj) {
			if (obj == null) {
				return 1;
			}
			if (!(obj is int)){
				throw new ArgumentException();
			}
			return this.CompareTo((int)obj);
		}

		#endregion

		#region IComparable<int> Members

		public int CompareTo(int x) {
			return (this.m_value > x) ? 1 : ((this.m_value < x) ? -1 : 0);
		}

		#endregion

		#region IEquatable<int> Members

		public bool Equals(int x) {
			return this.m_value == x;
		}

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
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
            return Convert.ToSByte(this);
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
            return this;
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
