#if !LOCALTEST

using System.Globalization;
namespace System {
	public struct Byte : IFormattable, IComparable, IConvertible, IComparable<byte>, IEquatable<byte> {
		public const byte MinValue = 0;
		public const byte MaxValue = 255;

#pragma warning disable 0169, 0649
        internal byte m_value;
#pragma warning restore 0169, 0649

        public override bool Equals(object obj) {
			return (obj is byte) && ((byte)obj).m_value == this.m_value;
		}

		public override int GetHashCode() {
			return (int)this.m_value;
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

        public static byte Parse(String s)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static byte Parse(String s, NumberStyles style)
        {
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static byte Parse(String s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static byte Parse(String s, NumberStyles style, IFormatProvider provider)
        {
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static byte Parse(String s, NumberStyles style, NumberFormatInfo info)
        {
            int i = 0;
            try {
                i = Int32.Parse(s, style, info);
            }
            catch (OverflowException e) {
                throw new OverflowException();
            }

            if (i < MinValue || i > MaxValue)
                throw new OverflowException();
            return (byte)i;
        }

        public static bool TryParse(String s, out Byte result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(String s, NumberStyles style, IFormatProvider provider, out Byte result)
        {
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(String s, NumberStyles style, NumberFormatInfo info, out Byte result)
        {
            result = 0;
            int i;
            if (!Int32.TryParse(s, style, info, out i)) {
                return false;
            }
            if (i < MinValue || i > MaxValue) {
                return false;
            }
            result = (byte)i;
            return true;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj) {
			if (obj == null) {
				return 1;
			}
			if (!(obj is byte)) {
				throw new ArgumentException();
			}
			return this.CompareTo((byte)obj);
		}

		#endregion

		#region IComparable<byte> Members

		public int CompareTo(byte x) {
			return (this.m_value > x) ? 1 : ((this.m_value < x) ? -1 : 0);
		}

		#endregion

		#region IEquatable<byte> Members

		public bool Equals(byte x) {
			return this.m_value == x;
		}

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Byte;
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
            return this;
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
