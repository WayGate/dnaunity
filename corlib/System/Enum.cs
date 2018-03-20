#if !LOCALTEST

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;

namespace System {
	public abstract class Enum : ValueType, IComparable, IFormattable, IConvertible {

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern static private void Internal_GetInfo(Type enumType, out string[] names, out int[] values);

        private static Dictionary<Type, EnumInfo> cache = new Dictionary<Type, EnumInfo>();

		public static string[] GetNames(Type enumType) {
			if (enumType == null) {
				throw new ArgumentNullException();
			}
			if (!enumType.IsEnum) {
				throw new ArgumentException();
			}
			EnumInfo info = EnumInfo.GetInfo(enumType);
			return info.GetNames();
		}

        public static Array GetValues(Type enumType)
        {
            if (enumType == null) {
                throw new ArgumentNullException();
            }
            if (!enumType.IsEnum) {
                throw new ArgumentException();
            }
            EnumInfo info = EnumInfo.GetInfo(enumType);
            return info.GetValues();
        }

        public static Type GetUnderlyingType(Type enumType)
        {
            return typeof(int);
        }

        private struct EnumInfo {
			private string[] names;
			private int[] values;
			
			public static EnumInfo GetInfo(Type enumType) {
				lock (cache) {
					EnumInfo info;
					if (!Enum.cache.TryGetValue(enumType, out info)) {
						info = new EnumInfo();
						Enum.Internal_GetInfo(enumType, out info.names, out info.values);
						Enum.cache.Add(enumType, info);
					}
					return info;
				}
			}

			public string GetName(int value) {
				int valuesLen = values.Length;
				for (int i = 0; i < valuesLen; i++) {
					if (this.values[i] == value) {
						return this.names[i];
					}
				}
				// Pretend it's got the [Flags] attribute, so look for bits set.
				// TODO Sort out Flags attribute properly
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < valuesLen; i++) {
					int thisValue = this.values[i];
					if ((value & thisValue) == thisValue) {
						sb.Append(this.names[i]);
						sb.Append(", ");
					}
				}
				if (sb.Length > 0) {
					return sb.ToString(0, sb.Length - 2);
				}
				return null;
			}

			public string[] GetNames() {
				List<string> names = new List<string>();
				for (int i = 0; i < this.values.Length; i++) {
					names.Add(this.GetName(this.values[i]));
				}
				return names.ToArray();
			}

            public Array GetValues()
            {
                return this.values;
            }

        }

		protected Enum() { }

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern private int Internal_GetValue();

		public static string GetName(Type enumType, object value) {
			if (enumType == null || value == null) {
				throw new ArgumentNullException();
			}
			if (!enumType.IsEnum) {
				throw new ArgumentException("enumType is not an Enum type.");
			}
			EnumInfo info = EnumInfo.GetInfo(enumType);
			return info.GetName((int)value);
		}

		public static string Format(Type enumType, object value, string format) {
			if (enumType == null || value == null || format == null) {
				throw new ArgumentNullException("enumType");
			}
			if (!enumType.IsEnum) {
				throw new ArgumentException("Type provided must be an Enum.");
			}
			string ret = GetName(enumType, value);
			if (ret == null) {
				return value.ToString();
			} else {
				return ret;
			}
		}

		public override string ToString() {
			return Format(this.GetType(), this.Internal_GetValue(), "G");
		}

        public string ToString(string format, IFormatProvider formatProvider)
        {
            // NOTE: This may not be compatible with real enum
            return ToString();
        }

        public string ToString(IFormatProvider formatProvider)
        {
            // NOTE: This may not be compatible with real enum
            return ToString();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj == null) {
                return 1;
            }
            if (!(obj is int)) {
                throw new ArgumentException();
            }
            return this.CompareTo((int)obj);
        }

        #endregion

        #region IComparable<int> Members

        public int CompareTo(int x)
        {
            int value = Internal_GetValue();
            return (value > x) ? 1 : ((value < x) ? -1 : 0);
        }

        #endregion

        #region IEquatable<int> Members

        public bool Equals(int x)
        {
            return Internal_GetValue() == x;
        }

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(Internal_GetValue());
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(Internal_GetValue());
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(Internal_GetValue());
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(Internal_GetValue());
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(Internal_GetValue());
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(Internal_GetValue());
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Internal_GetValue();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(Internal_GetValue());
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(Internal_GetValue());
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(Internal_GetValue());
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(Internal_GetValue());
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(Internal_GetValue());
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
                return Internal_GetValue().ToString(provider);
            else
                return Convert.ChangeType(this, conversionType);
        }

        #endregion

    }
}

#endif
