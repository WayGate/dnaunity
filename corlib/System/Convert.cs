using System;
using System.Globalization;
using System.Threading;

namespace System
{
    [Flags]
    public enum Base64FormattingOptions
    {
        None = 0,
        InsertLineBreaks = 1
    }

    public static class Convert
    {

        internal static RuntimeType[] _typeCodeToTypeCache = {
            null, // Empty - not supported
            (RuntimeType)typeof(Object),
            null, // DBNull - not supported
            (RuntimeType)typeof(Boolean),
            (RuntimeType)typeof(Char),
            (RuntimeType)typeof(SByte),
            (RuntimeType)typeof(Byte),
            (RuntimeType)typeof(Int16),
            (RuntimeType)typeof(UInt16),
            (RuntimeType)typeof(Int32),
            (RuntimeType)typeof(UInt32),
            (RuntimeType)typeof(Int64),
            (RuntimeType)typeof(UInt64),
            (RuntimeType)typeof(Single),
            (RuntimeType)typeof(Double),
            (RuntimeType)typeof(Decimal),
            (RuntimeType)typeof(DateTime),
            (RuntimeType)typeof(Object),
            (RuntimeType)typeof(String)
        };

        // Need to special case Enum because typecode will be underlying type, e.g. Int32
        private static readonly RuntimeType EnumType = (RuntimeType)typeof(Enum);

        public static TypeCode GetTypeCode(object value)
        {
            if (value == null)
                return TypeCode.Empty;
            IConvertible temp = value as IConvertible;
            if (temp != null) {
                return temp.GetTypeCode();
            }
            return TypeCode.Object;
        }

        public static bool IsDBNull(object value)
        {
            throw new NotImplementedException();
        }

        public static object ChangeType(Object value, TypeCode typeCode)
        {
            return ChangeType(value, typeCode, Thread.CurrentThread.CurrentCulture);
        }

        public static Object ChangeType(Object value, TypeCode typeCode, IFormatProvider provider)
        {
            if (value == null && (typeCode == TypeCode.Empty || typeCode == TypeCode.String || typeCode == TypeCode.Object)) {
                return null;
            }

            IConvertible v = value as IConvertible;
            if (v == null) {
                throw new InvalidCastException();
            }

            switch (typeCode) {
                case TypeCode.Boolean:
                    return v.ToBoolean(provider);
                case TypeCode.Char:
                    return v.ToChar(provider);
                case TypeCode.SByte:
                    return v.ToSByte(provider);
                case TypeCode.Byte:
                    return v.ToByte(provider);
                case TypeCode.Int16:
                    return v.ToInt16(provider);
                case TypeCode.UInt16:
                    return v.ToUInt16(provider);
                case TypeCode.Int32:
                    return v.ToInt32(provider);
                case TypeCode.UInt32:
                    return v.ToUInt32(provider);
                case TypeCode.Int64:
                    return v.ToInt64(provider);
                case TypeCode.UInt64:
                    return v.ToUInt64(provider);
                case TypeCode.Single:
                    return v.ToSingle(provider);
                case TypeCode.Double:
                    return v.ToDouble(provider);
                case TypeCode.Decimal:
                    return v.ToDecimal(provider);
                case TypeCode.DateTime:
                    return v.ToDateTime(provider);
                case TypeCode.String:
                    return v.ToString(provider);
                case TypeCode.Object:
                    return value;
                case TypeCode.DBNull:
                    throw new InvalidCastException();
                case TypeCode.Empty:
                    throw new InvalidCastException();
                default:
                    throw new ArgumentException();
            }
        }

        internal static Object DefaultToType(IConvertible value, Type targetType, IFormatProvider provider)
        {
            if (targetType == null) {
                throw new ArgumentNullException("targetType");
            }

            RuntimeType runtimeTargetType = targetType as RuntimeType;

            if (runtimeTargetType != null) {
                if (value.GetType() == targetType) {
                    return value;
                }

                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Boolean])
                    return value.ToBoolean(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Char])
                    return value.ToChar(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.SByte])
                    return value.ToSByte(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Byte])
                    return value.ToByte(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Int16])
                    return value.ToInt16(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.UInt16])
                    return value.ToUInt16(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Int32])
                    return value.ToInt32(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.UInt32])
                    return value.ToUInt32(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Int64])
                    return value.ToInt64(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.UInt64])
                    return value.ToUInt64(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Single])
                    return value.ToSingle(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Double])
                    return value.ToDouble(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Decimal])
                    return value.ToDecimal(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.DateTime])
                    return value.ToDateTime(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.String])
                    return value.ToString(provider);
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Object])
                    return (Object)value;
                if (runtimeTargetType == EnumType)
                    return (Enum)value;
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.DBNull])
                    throw new InvalidCastException();
                if (runtimeTargetType == _typeCodeToTypeCache[(int)TypeCode.Empty])
                    throw new InvalidCastException();
            }

            throw new InvalidCastException();
        }

        public static object ChangeType(Object value, Type conversionType)
        {
            return ChangeType(value, conversionType, Thread.CurrentThread.CurrentCulture);
        }

        public static object ChangeType(Object value, Type conversionType, IFormatProvider provider)
        {
            if (conversionType == null) {
                throw new ArgumentNullException("conversionType");
            }

            if (value == null) {
                if (conversionType.IsValueType) {
                    throw new InvalidCastException();
                }
                return null;
            }

            IConvertible ic = value as IConvertible;
            if (ic == null) {
                if (value.GetType() == conversionType) {
                    return value;
                }
                throw new InvalidCastException();
            }

            RuntimeType rtConversionType = conversionType as RuntimeType;

            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Boolean])
                return ic.ToBoolean(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Char])
                return ic.ToChar(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.SByte])
                return ic.ToSByte(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Byte])
                return ic.ToByte(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Int16])
                return ic.ToInt16(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.UInt16])
                return ic.ToUInt16(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Int32])
                return ic.ToInt32(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.UInt32])
                return ic.ToUInt32(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Int64])
                return ic.ToInt64(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.UInt64])
                return ic.ToUInt64(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Single])
                return ic.ToSingle(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Double])
                return ic.ToDouble(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Decimal])
                return ic.ToDecimal(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.DateTime])
                return ic.ToDateTime(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.String])
                return ic.ToString(provider);
            if (rtConversionType == _typeCodeToTypeCache[(int)TypeCode.Object])
                return (Object)value;

            return ic.ToType(conversionType, provider);
        }

        public static bool ToBoolean(Object value)
        {
            return value == null ? false : ((IConvertible)value).ToBoolean(null);
        }

        public static bool ToBoolean(Object value, IFormatProvider provider)
        {
            return value == null ? false : ((IConvertible)value).ToBoolean(provider);
        }

        public static bool ToBoolean(bool value)
        {
            return value;
        }

        public static bool ToBoolean(sbyte value)
        {
            return value != 0;
        }

        public static bool ToBoolean(char value)
        {
            return ((IConvertible)value).ToBoolean(null);
        }

        public static bool ToBoolean(byte value)
        {
            return value != 0;
        }

        public static bool ToBoolean(short value)
        {
            return value != 0;
        }

        public static bool ToBoolean(ushort value)
        {
            return value != 0;
        }

        public static bool ToBoolean(int value)
        {
            return value != 0;
        }

        public static bool ToBoolean(uint value)
        {
            return value != 0;
        }

        public static bool ToBoolean(long value)
        {
            return value != 0;
        }

        public static bool ToBoolean(ulong value)
        {
            return value != 0;
        }

        public static bool ToBoolean(String value)
        {
            if (value == null)
                return false;
            return Boolean.Parse(value);
        }

        public static bool ToBoolean(String value, IFormatProvider provider)
        {
            if (value == null)
                return false;
            return Boolean.Parse(value);
        }

        public static bool ToBoolean(float value)
        {
            return value != 0;
        }

        public static bool ToBoolean(double value)
        {
            return value != 0;
        }

        public static bool ToBoolean(DateTime value)
        {
            return ((IConvertible)value).ToBoolean(null);
        }

        public static char ToChar(object value)
        {
            return value == null ? (char)0 : ((IConvertible)value).ToChar(null);
        }

        public static char ToChar(object value, IFormatProvider provider)
        {
            return value == null ? (char)0 : ((IConvertible)value).ToChar(provider);
        }

        public static char ToChar(bool value)
        {
            return ((IConvertible)value).ToChar(null);
        }

        public static char ToChar(char value)
        {
            return value;
        }

        public static char ToChar(sbyte value)
        {
            if (value < 0) throw new OverflowException();
            
            return (char)value;
        }

        public static char ToChar(byte value)
        {
            return (char)value;
        }

        public static char ToChar(short value)
        {
            if (value < 0) throw new OverflowException();
            
            return (char)value;
        }
        
        public static char ToChar(ushort value)
        {
            return (char)value;
        }

        public static char ToChar(int value)
        {
            if (value < 0 || value > Char.MaxValue) throw new OverflowException();
            
            return (char)value;
        }
        
        public static char ToChar(uint value)
        {
            if (value > Char.MaxValue) throw new OverflowException();
            
            return (char)value;
        }

        public static char ToChar(long value)
        {
            if (value < 0 || value > Char.MaxValue) throw new OverflowException();
            
            return (char)value;
        }

        public static char ToChar(ulong value)
        {
            if (value > Char.MaxValue) throw new OverflowException();
            
            return (char)value;
        }

        public static char ToChar(String value)
        {
            return ToChar(value, null);
        }

        public static char ToChar(String value, IFormatProvider provider)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length != 1)
                throw new FormatException();
            return value[0];
        }

        public static char ToChar(float value)
        {
            return ((IConvertible)value).ToChar(null);
        }

        public static char ToChar(double value)
        {
            return ((IConvertible)value).ToChar(null);
        }

        public static char ToChar(DateTime value)
        {
            return ((IConvertible)value).ToChar(null);
        }

        public static sbyte ToSByte(object value)
        {
            return value == null ? (sbyte)0 : ((IConvertible)value).ToSByte(null);
        }
        
        public static sbyte ToSByte(object value, IFormatProvider provider)
        {
            return value == null ? (sbyte)0 : ((IConvertible)value).ToSByte(provider);
        }

        public static sbyte ToSByte(bool value)
        {
            return value ? (sbyte)1 : (sbyte)0;
        }

        public static sbyte ToSByte(sbyte value)
        {
            return value;
        }
        
        public static sbyte ToSByte(char value)
        {
            if (value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }
        
        public static sbyte ToSByte(byte value)
        {
            if (value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }
        
        public static sbyte ToSByte(short value)
        {
            if (value < SByte.MinValue || value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }
        
        public static sbyte ToSByte(ushort value)
        {
            if (value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }
        
        public static sbyte ToSByte(int value)
        {
            if (value < SByte.MinValue || value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }
        
        public static sbyte ToSByte(uint value)
        {
            if (value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }

        public static sbyte ToSByte(long value)
        {
            if (value < SByte.MinValue || value > SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }

        public static sbyte ToSByte(ulong value)
        {
            if (value > (ulong)SByte.MaxValue) throw new OverflowException();
            
            return (sbyte)value;
        }

        public static sbyte ToSByte(float value)
        {
            return ToSByte((double)value);
        }

        public static sbyte ToSByte(double value)
        {
            return ToSByte(ToInt32(value));
        }

        public static sbyte ToSByte(String value)
        {
            if (value == null)
                return 0;
            return SByte.Parse(value, CultureInfo.CurrentCulture);
        }
        
        public static sbyte ToSByte(String value, IFormatProvider provider)
        {
            return SByte.Parse(value, NumberStyles.Integer, provider);
        }

        public static sbyte ToSByte(DateTime value)
        {
            return ((IConvertible)value).ToSByte(null);
        }

        public static byte ToByte(object value)
        {
            return value == null ? (byte)0 : ((IConvertible)value).ToByte(null);
        }

        public static byte ToByte(object value, IFormatProvider provider)
        {
            return value == null ? (byte)0 : ((IConvertible)value).ToByte(provider);
        }

        public static byte ToByte(bool value)
        {
            return value ? (byte)1 : (byte)0;
        }

        public static byte ToByte(byte value)
        {
            return value;
        }

        public static byte ToByte(char value)
        {
            if (value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }
        
        public static byte ToByte(sbyte value)
        {
            if (value < Byte.MinValue) throw new OverflowException();
            
            return (byte)value;
        }

        public static byte ToByte(short value)
        {
            if (value < Byte.MinValue || value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }
        
        public static byte ToByte(ushort value)
        {
            if (value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }

        public static byte ToByte(int value)
        {
            if (value < Byte.MinValue || value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }
        
        public static byte ToByte(uint value)
        {
            if (value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }

        public static byte ToByte(long value)
        {
            if (value < Byte.MinValue || value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }
        
        public static byte ToByte(ulong value)
        {
            if (value > Byte.MaxValue) throw new OverflowException();
            
            return (byte)value;
        }

        public static byte ToByte(float value)
        {
            return ToByte((double)value);
        }

        public static byte ToByte(double value)
        {
            return ToByte(ToInt32(value));
        }

        public static byte ToByte(String value)
        {
            if (value == null)
                return 0;
            return Byte.Parse(value, CultureInfo.CurrentCulture);
        }

        public static byte ToByte(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return Byte.Parse(value, NumberStyles.Integer, provider);
        }

        public static byte ToByte(DateTime value)
        {
            return ((IConvertible)value).ToByte(null);
        }

        public static short ToInt16(object value)
        {
            return value == null ? (short)0 : ((IConvertible)value).ToInt16(null);
        }

        public static short ToInt16(object value, IFormatProvider provider)
        {
            return value == null ? (short)0 : ((IConvertible)value).ToInt16(provider);
        }

        public static short ToInt16(bool value)
        {
            return value ? (short)1 : (short)0;
        }

        public static short ToInt16(char value)
        {
            if (value > Int16.MaxValue) throw new OverflowException();
            
            return (short)value;
        }
        
        public static short ToInt16(sbyte value)
        {
            return value;
        }

        public static short ToInt16(byte value)
        {
            return value;
        }
        
        public static short ToInt16(ushort value)
        {
            if (value > Int16.MaxValue) throw new OverflowException();
            
            return (short)value;
        }

        public static short ToInt16(int value)
        {
            if (value < Int16.MinValue || value > Int16.MaxValue) throw new OverflowException();
            
            return (short)value;
        }
        
        public static short ToInt16(uint value)
        {
            if (value > Int16.MaxValue) throw new OverflowException();
            
            return (short)value;
        }

        public static short ToInt16(short value)
        {
            return value;
        }

        public static short ToInt16(long value)
        {
            if (value < Int16.MinValue || value > Int16.MaxValue) throw new OverflowException();
            
            return (short)value;
        }
        
        public static short ToInt16(ulong value)
        {
            if (value > (ulong)Int16.MaxValue) throw new OverflowException();
            
            return (short)value;
        }

        public static short ToInt16(float value)
        {
            return ToInt16((double)value);
        }

        public static short ToInt16(double value)
        {
            return ToInt16(ToInt32(value));
        }

        public static short ToInt16(String value)
        {
            if (value == null)
                return 0;
            return Int16.Parse(value, CultureInfo.CurrentCulture);
        }

        public static short ToInt16(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return Int16.Parse(value, NumberStyles.Integer, provider);
        }

        public static short ToInt16(DateTime value)
        {
            return ((IConvertible)value).ToInt16(null);
        }

        public static ushort ToUInt16(object value)
        {
            return value == null ? (ushort)0 : ((IConvertible)value).ToUInt16(null);
        }
        
        public static ushort ToUInt16(object value, IFormatProvider provider)
        {
            return value == null ? (ushort)0 : ((IConvertible)value).ToUInt16(provider);
        }
        
        public static ushort ToUInt16(bool value)
        {
            return value ? (ushort)1 : (ushort)0;
        }
        
        public static ushort ToUInt16(char value)
        {
            return value;
        }

        public static ushort ToUInt16(sbyte value)
        {
            if (value < 0) throw new OverflowException();
            
            return (ushort)value;
        }

        public static ushort ToUInt16(byte value)
        {
            return value;
        }

        public static ushort ToUInt16(short value)
        {
            if (value < 0) throw new OverflowException();
            
            return (ushort)value;
        }

        public static ushort ToUInt16(int value)
        {
            if (value < 0 || value > UInt16.MaxValue) throw new OverflowException();
            
            return (ushort)value;
        }

        public static ushort ToUInt16(ushort value)
        {
            return value;
        }

        public static ushort ToUInt16(uint value)
        {
            if (value > UInt16.MaxValue) throw new OverflowException();
            
            return (ushort)value;
        }

        public static ushort ToUInt16(long value)
        {
            if (value < 0 || value > UInt16.MaxValue) throw new OverflowException();
            
            return (ushort)value;
        }

        public static ushort ToUInt16(ulong value)
        {
            if (value > UInt16.MaxValue) throw new OverflowException();
            
            return (ushort)value;
        }

        public static ushort ToUInt16(float value)
        {
            return ToUInt16((double)value);
        }

        public static ushort ToUInt16(double value)
        {
            return ToUInt16(ToInt32(value));
        }

        public static ushort ToUInt16(String value)
        {
            if (value == null)
                return 0;
            return UInt16.Parse(value, CultureInfo.CurrentCulture);
        }

        public static ushort ToUInt16(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return UInt16.Parse(value, NumberStyles.Integer, provider);
        }

        public static ushort ToUInt16(DateTime value)
        {
            return ((IConvertible)value).ToUInt16(null);
        }

        public static int ToInt32(object value)
        {
            return value == null ? 0 : ((IConvertible)value).ToInt32(null);
        }

        public static int ToInt32(object value, IFormatProvider provider)
        {
            return value == null ? 0 : ((IConvertible)value).ToInt32(provider);
        }

        public static int ToInt32(bool value)
        {
            return value ? 1 : 0;
        }

        public static int ToInt32(char value)
        {
            return value;
        }

        public static int ToInt32(sbyte value)
        {
            return value;
        }

        public static int ToInt32(byte value)
        {
            return value;
        }

        public static int ToInt32(short value)
        {
            return value;
        }

        public static int ToInt32(ushort value)
        {
            return value;
        }

        public static int ToInt32(uint value)
        {
            if (value > Int32.MaxValue) throw new OverflowException();
            
            return (int)value;
        }

        public static int ToInt32(int value)
        {
            return value;
        }

        public static int ToInt32(long value)
        {
            if (value < Int32.MinValue || value > Int32.MaxValue) throw new OverflowException();
            
            return (int)value;
        }

        public static int ToInt32(ulong value)
        {
            if (value > Int32.MaxValue) throw new OverflowException();
            
            return (int)value;
        }

        public static int ToInt32(float value)
        {
            return ToInt32((double)value);
        }

        public static int ToInt32(double value)
        {
            if (value >= 0) {
                if (value < 2147483647.5) {
                    int result = (int)value;
                    double dif = value - result;
                    if (dif > 0.5 || dif == 0.5 && (result & 1) != 0) result++;
                    return result;
                }
            }
            else {
                if (value >= -2147483648.5) {
                    int result = (int)value;
                    double dif = value - result;
                    if (dif < -0.5 || dif == -0.5 && (result & 1) != 0) result--;
                    return result;
                }
            }
            throw new OverflowException();
        }

        public static int ToInt32(String value)
        {
            if (value == null)
                return 0;
            return Int32.Parse(value, CultureInfo.CurrentCulture);
        }

        public static int ToInt32(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return Int32.Parse(value, NumberStyles.Integer, provider);
        }

        public static int ToInt32(DateTime value)
        {
            return ((IConvertible)value).ToInt32(null);
        }

        public static uint ToUInt32(object value)
        {
            return value == null ? 0 : ((IConvertible)value).ToUInt32(null);
        }

        public static uint ToUInt32(object value, IFormatProvider provider)
        {
            return value == null ? 0 : ((IConvertible)value).ToUInt32(provider);
        }

        public static uint ToUInt32(bool value)
        {
            return value ? (uint)1 : (uint)0;
        }

        public static uint ToUInt32(char value)
        {
            return value;
        }

        public static uint ToUInt32(sbyte value)
        {
            if (value < 0) throw new OverflowException();
            
            return (uint)value;
        }

        public static uint ToUInt32(byte value)
        {
            return value;
        }

        public static uint ToUInt32(short value)
        {
            if (value < 0) throw new OverflowException();
            
            return (uint)value;
        }

        public static uint ToUInt32(ushort value)
        {
            return value;
        }

        public static uint ToUInt32(int value)
        {
            if (value < 0) throw new OverflowException();
            
            return (uint)value;
        }

        public static uint ToUInt32(uint value)
        {
            return value;
        }

        public static uint ToUInt32(long value)
        {
            if (value < 0 || value > UInt32.MaxValue) throw new OverflowException();
            
            return (uint)value;
        }

        public static uint ToUInt32(ulong value)
        {
            if (value > UInt32.MaxValue) throw new OverflowException();
            
            return (uint)value;
        }

        public static uint ToUInt32(float value)
        {
            return ToUInt32((double)value);
        }

        public static uint ToUInt32(double value)
        {
            if (value >= -0.5 && value < 4294967295.5) {
                uint result = (uint)value;
                double dif = value - result;
                if (dif > 0.5 || dif == 0.5 && (result & 1) != 0) result++;
                return result;
            }
            throw new OverflowException();
        }

        public static uint ToUInt32(String value)
        {
            if (value == null)
                return 0;
            return UInt32.Parse(value, CultureInfo.CurrentCulture);
        }

        public static uint ToUInt32(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return UInt32.Parse(value, NumberStyles.Integer, provider);
        }

        public static uint ToUInt32(DateTime value)
        {
            return ((IConvertible)value).ToUInt32(null);
        }

        public static long ToInt64(object value)
        {
            return value == null ? 0 : ((IConvertible)value).ToInt64(null);
        }

        public static long ToInt64(object value, IFormatProvider provider)
        {
            return value == null ? 0 : ((IConvertible)value).ToInt64(provider);
        }

        public static long ToInt64(bool value)
        {
            return value ? 1 : 0;
        }

        public static long ToInt64(char value)
        {
            return value;
        }

        
        public static long ToInt64(sbyte value)
        {
            return value;
        }

        public static long ToInt64(byte value)
        {
            return value;
        }

        public static long ToInt64(short value)
        {
            return value;
        }

        public static long ToInt64(ushort value)
        {
            return value;
        }

        public static long ToInt64(int value)
        {
            return value;
        }
        
        public static long ToInt64(uint value)
        {
            return value;
        }

        public static long ToInt64(ulong value)
        {
            if (value > Int64.MaxValue) throw new OverflowException();
            
            return (long)value;
        }

        public static long ToInt64(long value)
        {
            return value;
        }

        public static long ToInt64(float value)
        {
            return ToInt64((double)value);
        }

        public static long ToInt64(double value)
        {
            return checked((long)Math.Round(value));
        }

        public static long ToInt64(string value)
        {
            if (value == null)
                return 0;
            return Int64.Parse(value, CultureInfo.CurrentCulture);
        }

        public static long ToInt64(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return Int64.Parse(value, NumberStyles.Integer, provider);
        }

        public static long ToInt64(DateTime value)
        {
            return ((IConvertible)value).ToInt64(null);
        }

        public static ulong ToUInt64(object value)
        {
            return value == null ? 0 : ((IConvertible)value).ToUInt64(null);
        }
        
        public static ulong ToUInt64(object value, IFormatProvider provider)
        {
            return value == null ? 0 : ((IConvertible)value).ToUInt64(provider);
        }
        
        public static ulong ToUInt64(bool value)
        {
            return value ? (ulong)1 : (ulong)0;
        }
        
        public static ulong ToUInt64(char value)
        {
            return value;
        }
        
        public static ulong ToUInt64(sbyte value)
        {
            if (value < 0) throw new OverflowException();
            
            return (ulong)value;
        }
        
        public static ulong ToUInt64(byte value)
        {
            return value;
        }
        
        public static ulong ToUInt64(short value)
        {
            if (value < 0) throw new OverflowException();
            
            return (ulong)value;
        }
        
        public static ulong ToUInt64(ushort value)
        {
            return value;
        }
        
        public static ulong ToUInt64(int value)
        {
            if (value < 0) throw new OverflowException();
            
            return (ulong)value;
        }
        
        public static ulong ToUInt64(uint value)
        {
            return value;
        }
        
        public static ulong ToUInt64(long value)
        {
            if (value < 0) throw new OverflowException();
            
            return (ulong)value;
        }
        
        public static ulong ToUInt64(UInt64 value)
        {
            return value;
        }
        
        public static ulong ToUInt64(float value)
        {
            return ToUInt64((double)value);
        }
        
        public static ulong ToUInt64(double value)
        {
            return checked((ulong)Math.Round(value));
        }
        
        public static ulong ToUInt64(String value)
        {
            if (value == null)
                return 0;
            return UInt64.Parse(value, CultureInfo.CurrentCulture);
        }
        
        public static ulong ToUInt64(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return UInt64.Parse(value, NumberStyles.Integer, provider);
        }

        public static ulong ToUInt64(DateTime value)
        {
            return ((IConvertible)value).ToUInt64(null);
        }

        public static float ToSingle(object value)
        {
            return value == null ? 0 : ((IConvertible)value).ToSingle(null);
        }

        public static float ToSingle(object value, IFormatProvider provider)
        {
            return value == null ? 0 : ((IConvertible)value).ToSingle(provider);
        }
        
        public static float ToSingle(sbyte value)
        {
            return value;
        }

        public static float ToSingle(byte value)
        {
            return value;
        }

        public static float ToSingle(char value)
        {
            return ((IConvertible)value).ToSingle(null);
        }

        public static float ToSingle(short value)
        {
            return value;
        }

        
        public static float ToSingle(ushort value)
        {
            return value;
        }

        public static float ToSingle(int value)
        {
            return value;
        }

        
        public static float ToSingle(uint value)
        {
            return value;
        }

        public static float ToSingle(long value)
        {
            return value;
        }

        
        public static float ToSingle(ulong value)
        {
            return value;
        }

        public static float ToSingle(float value)
        {
            return value;
        }

        public static float ToSingle(double value)
        {
            return (float)value;
        }

        public static float ToSingle(String value)
        {
            if (value == null)
                return 0;
            return Single.Parse(value, CultureInfo.CurrentCulture);
        }

        public static float ToSingle(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return Single.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
        }


        public static float ToSingle(bool value)
        {
            return value ? 1 : 0;
        }

        public static float ToSingle(DateTime value)
        {
            return ((IConvertible)value).ToSingle(null);
        }

        public static double ToDouble(object value)
        {
            return value == null ? 0 : ((IConvertible)value).ToDouble(null);
        }

        public static double ToDouble(object value, IFormatProvider provider)
        {
            return value == null ? 0 : ((IConvertible)value).ToDouble(provider);
        }


        
        public static double ToDouble(sbyte value)
        {
            return value;
        }

        public static double ToDouble(byte value)
        {
            return value;
        }

        public static double ToDouble(short value)
        {
            return value;
        }

        public static double ToDouble(char value)
        {
            return ((IConvertible)value).ToDouble(null);
        }

        public static double ToDouble(ushort value)
        {
            return value;
        }

        public static double ToDouble(int value)
        {
            return value;
        }

        
        public static double ToDouble(uint value)
        {
            return value;
        }

        public static double ToDouble(long value)
        {
            return value;
        }

        
        public static double ToDouble(ulong value)
        {
            return value;
        }

        public static double ToDouble(float value)
        {
            return value;
        }

        public static double ToDouble(double value)
        {
            return value;
        }

        public static double ToDouble(String value)
        {
            if (value == null)
                return 0;
            return Double.Parse(value, CultureInfo.CurrentCulture);
        }

        public static double ToDouble(String value, IFormatProvider provider)
        {
            if (value == null)
                return 0;
            return Double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
        }

        public static double ToDouble(bool value)
        {
            return value ? 1 : 0;
        }

        public static double ToDouble(DateTime value)
        {
            return ((IConvertible)value).ToDouble(null);
        }

        public static DateTime ToDateTime(DateTime value)
        {
            return value;
        }

        public static DateTime ToDateTime(object value)
        {
            return value == null ? DateTime.MinValue : ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(object value, IFormatProvider provider)
        {
            return value == null ? DateTime.MinValue : ((IConvertible)value).ToDateTime(provider);
        }

        public static DateTime ToDateTime(String value)
        {
            if (value == null)
                return new DateTime(0);
            return DateTime.Parse(value, CultureInfo.CurrentCulture);
        }

        public static DateTime ToDateTime(String value, IFormatProvider provider)
        {
            if (value == null)
                return new DateTime(0);
            return DateTime.Parse(value, provider);
        }

        public static DateTime ToDateTime(sbyte value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(byte value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(short value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(ushort value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(int value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(uint value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(long value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(ulong value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(bool value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(char value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(float value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime ToDateTime(double value)
        {
            return ((IConvertible)value).ToDateTime(null);
        }

        public static string ToString(Object value)
        {
            return ToString(value, null);
        }

        public static string ToString(Object value, IFormatProvider provider)
        {
            IConvertible ic = value as IConvertible;
            if (ic != null)
                return ic.ToString(provider);
            IFormattable formattable = value as IFormattable;
            if (formattable != null)
                return formattable.ToString(null, provider);
            return value == null ? String.Empty : value.ToString();
        }

        public static string ToString(bool value)
        {
            return value.ToString();
        }

        public static string ToString(bool value, IFormatProvider provider)
        {
            return value.ToString();
        }

        public static string ToString(char value)
        {
            return value.ToString();
        }

        public static string ToString(char value, IFormatProvider provider)
        {
            return value.ToString();
        }

        public static string ToString(sbyte value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(sbyte value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(byte value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(byte value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(short value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(short value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }
        
        public static string ToString(ushort value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(ushort value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(int value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(int value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(uint value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(uint value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(long value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(long value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(ulong value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(ulong value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(float value)
        {
            return value.ToString();
        }

        public static string ToString(float value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(double value)
        {
            return value.ToString();
        }

        public static string ToString(double value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(DateTime value)
        {
            return value.ToString();
        }

        public static string ToString(DateTime value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static String ToString(String value)
        {
            return value;
        }

        public static String ToString(String value, IFormatProvider provider)
        {
            return value; // avoid the null check
        }

        public static byte ToByte(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static sbyte ToSByte(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static short ToInt16(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static ushort ToUInt16(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static int ToInt32(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static uint ToUInt32(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static long ToInt64(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static ulong ToUInt64(String value, int fromBase)
        {
            throw new NotImplementedException();
        }

        public static String ToString(byte value, int toBase)
        {
            throw new NotImplementedException();
        }

        public static String ToString(short value, int toBase)
        {
            throw new NotImplementedException();
        }

        public static String ToString(int value, int toBase)
        {
            throw new NotImplementedException();
        }

        public static String ToString(long value, int toBase)
        {
            throw new NotImplementedException();
        }

        public static String ToBase64String(byte[] inArray)
        {
            throw new NotImplementedException();
        }

        public static String ToBase64String(byte[] inArray, Base64FormattingOptions options)
        {
            throw new NotImplementedException();
        }

        public static String ToBase64String(byte[] inArray, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public static unsafe String ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
        {
            throw new NotImplementedException();
        }

        public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
        {
            throw new NotImplementedException();
        }

        public static unsafe int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, Base64FormattingOptions options)
        {
            throw new NotImplementedException();
        }

        private static unsafe int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)
        {
            throw new NotImplementedException();
        }

        public static Byte[] FromBase64String(String s)
        {
            throw new NotImplementedException();
        }

        public static Byte[] FromBase64CharArray(Char[] inArray, Int32 offset, Int32 length)
        {
            throw new NotImplementedException();
        }

    }

}
