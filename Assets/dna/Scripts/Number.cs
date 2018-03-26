// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// Used under the MIT License - See https://github.com/mono/mono/blob/master/LICENSE
#if NO
namespace DnaUnity
{
    
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Text;

    // The Number class implements methods for formatting and parsing
    // numeric values. To format and parse numeric values, applications should
    // use the Format and Parse methods provided by the numeric
    // classes (byte, Int16, int, Int64,
    // Single, double, Currency, and Decimal). Those
    // Format and Parse methods share a common implementation
    // provided by this class, and are thus documented in detail here.
    //
    // Formatting
    //
    // The Format methods provided by the numeric classes are all of the
    // form
    //
    //  public static string Format(XXX value, string format);
    //  public static string Format(XXX value, string format, NumberFormatInfo info);
    //
    // where XXX is the name of the particular numeric class. The methods convert
    // the numeric value to a string using the format string given by the
    // format parameter. If the format parameter is null or
    // an empty string, the number is formatted as if the string "G" (general
    // format) was specified. The info parameter specifies the
    // NumberFormatInfo instance to use when formatting the number. If the
    // info parameter is null or omitted, the numeric formatting information
    // is obtained from the current culture. The NumberFormatInfo supplies
    // such information as the characters to use for decimal and thousand
    // separators, and the spelling and placement of currency symbols in monetary
    // values.
    //
    // Format strings fall into two categories: Standard format strings and
    // user-defined format strings. A format string consisting of a single
    // alphabetic character (A-Z or a-z), optionally followed by a sequence of
    // digits (0-9), is a standard format string. All other format strings are
    // used-defined format strings.
    //
    // A standard format string takes the form Axx, where A is an
    // alphabetic character called the format specifier and xx is a
    // sequence of digits called the precision specifier. The format
    // specifier controls the type of formatting applied to the number and the
    // precision specifier controls the number of significant digits or decimal
    // places of the formatting operation. The following table describes the
    // supported standard formats.
    //
    // C c - Currency format. The number is
    // converted to a string that represents a currency amount. The conversion is
    // controlled by the currency format information of the NumberFormatInfo
    // used to format the number. The precision specifier indicates the desired
    // number of decimal places. If the precision specifier is omitted, the default
    // currency precision given by the NumberFormatInfo is used.
    //
    // D d - Decimal format. This format is
    // supported for integral types only. The number is converted to a string of
    // decimal digits, prefixed by a minus sign if the number is negative. The
    // precision specifier indicates the minimum number of digits desired in the
    // resulting string. If required, the number will be left-padded with zeros to
    // produce the number of digits given by the precision specifier.
    //
    // E e Engineering (scientific) format.
    // The number is converted to a string of the form
    // "-d.ddd...E+ddd" or "-d.ddd...e+ddd", where each
    // 'd' indicates a digit (0-9). The string starts with a minus sign if the
    // number is negative, and one digit always precedes the decimal point. The
    // precision specifier indicates the desired number of digits after the decimal
    // point. If the precision specifier is omitted, a default of 6 digits after
    // the decimal point is used. The format specifier indicates whether to prefix
    // the exponent with an 'E' or an 'e'. The exponent is always consists of a
    // plus or minus sign and three digits.
    //
    // F f Fixed point format. The number is
    // converted to a string of the form "-ddd.ddd....", where each
    // 'd' indicates a digit (0-9). The string starts with a minus sign if the
    // number is negative. The precision specifier indicates the desired number of
    // decimal places. If the precision specifier is omitted, the default numeric
    // precision given by the NumberFormatInfo is used.
    //
    // G g - General format. The number is
    // converted to the shortest possible decimal representation using fixed point
    // or scientific format. The precision specifier determines the number of
    // significant digits in the resulting string. If the precision specifier is
    // omitted, the number of significant digits is determined by the type of the
    // number being converted (10 for int, 19 for long, 7 for
    // float, 15 for double, 19 for Currency, and 29 for
    // Decimal). Trailing zeros after the decimal point are removed, and the
    // resulting string contains a decimal point only if required. The resulting
    // string uses fixed point format if the exponent of the number is less than
    // the number of significant digits and greater than or equal to -4. Otherwise,
    // the resulting string uses scientific format, and the case of the format
    // specifier controls whether the exponent is prefixed with an 'E' or an
    // 'e'.
    //
    // N n Number format. The number is
    // converted to a string of the form "-d,ddd,ddd.ddd....", where
    // each 'd' indicates a digit (0-9). The string starts with a minus sign if the
    // number is negative. Thousand separators are inserted between each group of
    // three digits to the left of the decimal point. The precision specifier
    // indicates the desired number of decimal places. If the precision specifier
    // is omitted, the default numeric precision given by the
    // NumberFormatInfo is used.
    //
    // X x - Hexadecimal format. This format is
    // supported for integral types only. The number is converted to a string of
    // hexadecimal digits. The format specifier indicates whether to use upper or
    // lower case characters for the hexadecimal digits above 9 ('X' for 'ABCDEF',
    // and 'x' for 'abcdef'). The precision specifier indicates the minimum number
    // of digits desired in the resulting string. If required, the number will be
    // left-padded with zeros to produce the number of digits given by the
    // precision specifier.
    //
    // Some examples of standard format strings and their results are shown in the
    // table below. (The examples all assume a default NumberFormatInfo.)
    //
    // Value        Format  Result
    // 12345.6789   C       $12,345.68
    // -12345.6789  C       ($12,345.68)
    // 12345        D       12345
    // 12345        D8      00012345
    // 12345.6789   E       1.234568E+004
    // 12345.6789   E10     1.2345678900E+004
    // 12345.6789   e4      1.2346e+004
    // 12345.6789   F       12345.68
    // 12345.6789   F0      12346
    // 12345.6789   F6      12345.678900
    // 12345.6789   G       12345.6789
    // 12345.6789   G7      12345.68
    // 123456789    G7      1.234568E8
    // 12345.6789   N       12,345.68
    // 123456789    N4      123,456,789.0000
    // 0x2c45e      x       2c45e
    // 0x2c45e      X       2C45E
    // 0x2c45e      X8      0002C45E
    //
    // Format strings that do not start with an alphabetic character, or that start
    // with an alphabetic character followed by a non-digit, are called
    // user-defined format strings. The following table describes the formatting
    // characters that are supported in user defined format strings.
    //
    // 
    // 0 - Digit placeholder. If the value being
    // formatted has a digit in the position where the '0' appears in the format
    // string, then that digit is copied to the output string. Otherwise, a '0' is
    // stored in that position in the output string. The position of the leftmost
    // '0' before the decimal point and the rightmost '0' after the decimal point
    // determines the range of digits that are always present in the output
    // string.
    //
    // # - Digit placeholder. If the value being
    // formatted has a digit in the position where the '#' appears in the format
    // string, then that digit is copied to the output string. Otherwise, nothing
    // is stored in that position in the output string.
    //
    // . - Decimal point. The first '.' character
    // in the format string determines the location of the decimal separator in the
    // formatted value; any additional '.' characters are ignored. The actual
    // character used as a the decimal separator in the output string is given by
    // the NumberFormatInfo used to format the number.
    //
    // , - Thousand separator and number scaling.
    // The ',' character serves two purposes. First, if the format string contains
    // a ',' character between two digit placeholders (0 or #) and to the left of
    // the decimal point if one is present, then the output will have thousand
    // separators inserted between each group of three digits to the left of the
    // decimal separator. The actual character used as a the decimal separator in
    // the output string is given by the NumberFormatInfo used to format the
    // number. Second, if the format string contains one or more ',' characters
    // immediately to the left of the decimal point, or after the last digit
    // placeholder if there is no decimal point, then the number will be divided by
    // 1000 times the number of ',' characters before it is formatted. For example,
    // the format string '0,,' will represent 100 million as just 100. Use of the
    // ',' character to indicate scaling does not also cause the formatted number
    // to have thousand separators. Thus, to scale a number by 1 million and insert
    // thousand separators you would use the format string '#,##0,,'.
    //
    // % - Percentage placeholder. The presence of
    // a '%' character in the format string causes the number to be multiplied by
    // 100 before it is formatted. The '%' character itself is inserted in the
    // output string where it appears in the format string.
    //
    // E+ E- e+ e-   - Scientific notation.
    // If any of the strings 'E+', 'E-', 'e+', or 'e-' are present in the format
    // string and are immediately followed by at least one '0' character, then the
    // number is formatted using scientific notation with an 'E' or 'e' inserted
    // between the number and the exponent. The number of '0' characters following
    // the scientific notation indicator determines the minimum number of digits to
    // output for the exponent. The 'E+' and 'e+' formats indicate that a sign
    // character (plus or minus) should always precede the exponent. The 'E-' and
    // 'e-' formats indicate that a sign character should only precede negative
    // exponents.
    //
    // \ - Literal character. A backslash character
    // causes the next character in the format string to be copied to the output
    // string as-is. The backslash itself isn't copied, so to place a backslash
    // character in the output string, use two backslashes (\\) in the format
    // string.
    //
    // 'ABC' "ABC" - Literal string. Characters
    // enclosed in single or double quotation marks are copied to the output string
    // as-is and do not affect formatting.
    //
    // ; - Section separator. The ';' character is
    // used to separate sections for positive, negative, and zero numbers in the
    // format string.
    //
    // Other - All other characters are copied to
    // the output string in the position they appear.
    //
    // For fixed point formats (formats not containing an 'E+', 'E-', 'e+', or
    // 'e-'), the number is rounded to as many decimal places as there are digit
    // placeholders to the right of the decimal point. If the format string does
    // not contain a decimal point, the number is rounded to the nearest
    // integer. If the number has more digits than there are digit placeholders to
    // the left of the decimal point, the extra digits are copied to the output
    // string immediately before the first digit placeholder.
    //
    // For scientific formats, the number is rounded to as many significant digits
    // as there are digit placeholders in the format string.
    //
    // To allow for different formatting of positive, negative, and zero values, a
    // user-defined format string may contain up to three sections separated by
    // semicolons. The results of having one, two, or three sections in the format
    // string are described in the table below.
    //
    // Sections:
    //
    // One - The format string applies to all values.
    //
    // Two - The first section applies to positive values
    // and zeros, and the second section applies to negative values. If the number
    // to be formatted is negative, but becomes zero after rounding according to
    // the format in the second section, then the resulting zero is formatted
    // according to the first section.
    //
    // Three - The first section applies to positive
    // values, the second section applies to negative values, and the third section
    // applies to zeros. The second section may be left empty (by having no
    // characters between the semicolons), in which case the first section applies
    // to all non-zero values. If the number to be formatted is non-zero, but
    // becomes zero after rounding according to the format in the first or second
    // section, then the resulting zero is formatted according to the third
    // section.
    //
    // For both standard and user-defined formatting operations on values of type
    // float and double, if the value being formatted is a NaN (Not
    // a Number) or a positive or negative infinity, then regardless of the format
    // string, the resulting string is given by the NaNSymbol,
    // PositiveInfinitySymbol, or NegativeInfinitySymbol property of
    // the NumberFormatInfo used to format the number.
    //
    // Parsing
    //
    // The Parse methods provided by the numeric classes are all of the form
    //
    //  public static XXX Parse(string s);
    //  public static XXX Parse(string s, int style);
    //  public static XXX Parse(string s, int style, NumberFormatInfo info);
    //
    // where XXX is the name of the particular numeric class. The methods convert a
    // string to a numeric value. The optional style parameter specifies the
    // permitted style of the numeric string. It must be a combination of bit flags
    // from the NumberStyles enumeration. The optional info parameter
    // specifies the NumberFormatInfo instance to use when parsing the
    // string. If the info parameter is null or omitted, the numeric
    // formatting information is obtained from the current culture.
    //
    // Numeric strings produced by the Format methods using the Currency,
    // Decimal, Engineering, Fixed point, General, or Number standard formats
    // (the C, D, E, F, G, and N format specifiers) are guaranteed to be parseable
    // by the Parse methods if the NumberStyles.Any style is
    // specified. Note, however, that the Parse methods do not accept
    // NaNs or Infinities.
    //
    //This class contains only static members and does not need to be serializable 
    public static unsafe class System_Number
    {
        private unsafe static bool NumberBufferToDouble(ref NumberBuffer number, ref double value)
        {
            uint64 val;
            uint16* src;
            int exp, remaining, total, count, scale, absscale, index;

            total = 0;
            src = number.digits;
            while (*src++) total++;

            remaining = total;

            src = number.digits;
            while (*src == '0') {
                remaining--;
                src++;
            }

            if (remaining == 0) {
                *value = 0;
                goto done;
            }

            count = MIN(remaining, 9);
            remaining -= count;
            val = digits_to_int(src, count);

            if (remaining > 0) {
                count = MIN(remaining, 9);
                remaining -= count;

                /* get the denormalized power of 10 */
                guint32 mult = (guint32)(rgval64Power10[count - 1] >> (64 - rgexp64Power10[count - 1]));
                val = ((guint64)(guint32)val) * ((guint64)mult) + digits_to_int(src + 9, count);
            }

            scale = number.scale - (total - remaining);
            absscale = abs(scale);

            if (absscale >= 22 * 16) {
                /* overflow / underflow */
                *(guint64*)value = (scale > 0) ? 0x7FF0000000000000UL : 0;
                goto done;
            }

            exp = 64;

            /* normalize the mantiss */
            if ((val & 0xFFFFFFFF00000000UL) == 0) { val <<= 32; exp -= 32; }
            if ((val & 0xFFFF000000000000UL) == 0) { val <<= 16; exp -= 16; }
            if ((val & 0xFF00000000000000UL) == 0) { val <<= 8; exp -= 8; }
            if ((val & 0xF000000000000000UL) == 0) { val <<= 4; exp -= 4; }
            if ((val & 0xC000000000000000UL) == 0) { val <<= 2; exp -= 2; }
            if ((val & 0x8000000000000000UL) == 0) { val <<= 1; exp -= 1; }

            index = absscale & 15;
            if (index) {
                gint multexp = rgexp64Power10[index - 1];
                /* the exponents are shared between the inverted and regular table */
                exp += (scale < 0) ? (-multexp + 1) : multexp;

                guint64 multval = rgval64Power10[index + ((scale < 0) ? 15 : 0) - 1];
                val = mul_64_lossy(val, multval, &exp);
            }

            index = absscale >> 4;
            if (index) {
                int multexp = rgexp64Power10By16[index - 1];
                /* the exponents are shared between the inverted and regular table */
                exp += (scale < 0) ? (-multexp + 1) : multexp;

                uint64 multval = rgval64Power10By16[index + ((scale < 0) ? 21 : 0) - 1];
                val = mul_64_lossy(val, multval, &exp);
            }

            if ((uint32)val & (1 << 10)) {
                /* IEEE round to even */
                uint64 tmp = val + ((1 << 10) - 1) + (((uint32)val >> 11) & 1);
                if (tmp < val) {
                    /* overflow */
                    tmp = (tmp >> 1) | 0x8000000000000000UL;
                    exp += 1;
                }
                val = tmp;
            }

            /* return the exponent to a biased state */
            exp += 0x3FE;

            /* handle overflow, underflow, "Epsilon - 1/2 Epsilon", denormalized, and the normal case */
            if (exp <= 0) {
                if (exp == -52 && (val >= 0x8000000000000058UL)) {
                    /* round X where {Epsilon > X >= 2.470328229206232730000000E-324} up to Epsilon (instead of down to zero) */
                    val = 0x0000000000000001UL;
                } else if (exp <= -52) {
                    /* underflow */
                    val = 0;
                }
                else {
                    /* denormalized */
                    val >>= (-exp + 11 + 1);
                }
            }
            else if (exp >= 0x7FF) {
                /* overflow */
                val = 0x7FF0000000000000UL;
            }
            else {
                /* normal postive exponent case */
                val = ((uint64)exp << 52) + ((val >> 11) & 0x000FFFFFFFFFFFFFUL);
            }

            *(uint64*)value = val;

            done:
            if (number.sign)
                *(uint64*)value |= 0x8000000000000000UL;
        }

        // Constants used by number parsing
        private const int NumberMaxDigits = 50;

        private const int Int32Precision = 10;
        private const int UInt32Precision = Int32Precision;
        private const int Int64Precision = 19;
        private const int UInt64Precision = 20;

        // NumberBuffer is a partial wrapper around a stack pointer that maps on to
        // the native NUMBER struct so that it can be passed to native directly. It 
        // must be initialized with a stack byte * of size NumberBufferBytes.
        // For performance, this structure should attempt to be completely inlined.
        // 
        // It should always be initialized like so:
        //
        // byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
        // NumberBuffer number = new NumberBuffer(numberBufferBytes);
        //
        // For performance, when working on the buffer in managed we use the values in this
        // structure, except for the digits, and pack those values into the byte buffer
        // if called out to managed.
        internal unsafe struct NumberBuffer 
        {

            // Enough space for NumberMaxDigit characters plus null and 3 32 bit integers and a pointer
            public static readonly int NumberBufferBytes = 12 + ((NumberMaxDigits + 1) * 2) + IntPtr.Size;

            private byte * baseAddress;
            public char * digits;
            public int precision;
            public int scale;
            public bool sign;

            public NumberBuffer(byte* stackBuffer) 
            {
                this.baseAddress = stackBuffer;
                this.digits = (((char*) stackBuffer) + 6);
                this.precision = 0;
                this.scale = 0;
                this.sign = false;
            }

            public byte* PackForNative() 
            {
                int* baseInteger = (int*) baseAddress;
                baseInteger[0] = precision;
                baseInteger[1] = scale;
                baseInteger[2] = sign ? 1 : 0;
                return baseAddress;
            }
        }

        // Just return the current culture format (ignore the one passed into us)
        private static NumberFormatInfo MarshalNumFormat(byte* numfmt)
        {
            return CultureInfo.CurrentCulture.NumberFormat;
        }

        private static bool HexNumberToInt32(ref NumberBuffer number, ref int value) 
        {
            UInt32 passedValue = 0;
            bool returnValue = HexNumberToUInt32(ref number, ref passedValue);
            value = (int)passedValue;
            return returnValue;
        }

        private static bool HexNumberToInt64(ref NumberBuffer number, ref Int64 value) 
        {
            UInt64 passedValue = 0;
            bool returnValue = HexNumberToUInt64(ref number, ref passedValue);
            value = (Int64)passedValue;
            return returnValue;
        }

        private unsafe static bool HexNumberToUInt32(ref NumberBuffer number, ref UInt32 value) 
        {
            int i = number.scale;
            if (i > UInt32Precision || i < number.precision) { 
                return false;
            }
            char* p = number.digits;
            Contract.Assert(p != null, "");

            UInt32 n = 0;
            while (--i >= 0) {
                if (n > ((UInt32)0xFFFFFFFF / 16)) {
                    return false;
                }
                n *= 16;
                if (*p != '\0') {
                    UInt32 newN = n;
                    if (*p != '\0') {
                        if (*p >= '0' && *p <= '9') {
                            newN += (UInt32)(*p - '0');
                        }
                        else {
                            if (*p >= 'A' && *p <= 'F') {
                                newN += (UInt32)((*p - 'A') + 10);
                            }
                            else {
                                Contract.Assert(*p >= 'a' && *p <= 'f', "");
                                newN += (UInt32)((*p - 'a') + 10);
                            }
                        }
                        p++;
                    }
                
                    // Detect an overflow here...
                    if (newN < n) { 
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool HexNumberToUInt64(ref NumberBuffer number, ref UInt64 value) 
        {

            int i = number.scale;
            if (i > UInt64Precision || i < number.precision) {
                return false;
            }
            char* p = number.digits;
            Contract.Assert(p != null, "");
            
            UInt64 n = 0;
            while (--i >= 0) {
                if (n > (0xFFFFFFFFFFFFFFFF / 16)) {
                    return false;
                }
                n *= 16;
                if (*p != '\0') {
                     UInt64 newN = n;
                    if (*p != '\0') {
                        if (*p >= '0' && *p <= '9') {
                            newN += (UInt64)(*p - '0');
                        }
                        else {
                            if (*p >= 'A' && *p <= 'F') {
                                newN += (UInt64)((*p - 'A') + 10);
                            }
                            else {
                                Contract.Assert(*p >= 'a' && *p <= 'f', "");
                                newN += (UInt64)((*p - 'a') + 10);
                            }
                        }
                        p++;
                    }
                    
                    // Detect an overflow here...
                    if (newN < n) { 
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private static bool IsWhite(char ch) 
        {
            return (((ch) == 0x20) || ((ch) >= 0x09 && (ch) <= 0x0D));
        }

        private unsafe static bool NumberToInt32(ref NumberBuffer number, ref int value) 
        {

            int i = number.scale;
            if (i > Int32Precision || i < number.precision) {
                return false;
            }
            char * p = number.digits;
            Contract.Assert(p != null, "");
            int n = 0;
            while (--i >= 0) {
                if ((UInt32)n > (0x7FFFFFFF / 10)) {
                    return false;
                }
                n *= 10;
                if (*p != '\0') {
                    n += (int)(*p++ - '0');
                }
            }
            if (number.sign) {
                n = -n;
                if (n > 0) {
                    return false;
                }
            }
            else {
                if (n < 0) {
                    return false;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool NumberToInt64(ref NumberBuffer number, ref Int64 value) {

            int i = number.scale;
            if (i > Int64Precision || i < number.precision) {
                return false;
            }
            char* p = number.digits;
            Contract.Assert(p != null, "");
            Int64 n = 0;
            while (--i >= 0) {
                if ((UInt64)n > (0x7FFFFFFFFFFFFFFF / 10)) {
                    return false;
                }
                n *= 10;
                if (*p != '\0') {
                    n += (int)(*p++ - '0');
                }
            }
            if (number.sign) {
                n = -n;
                if (n > 0) {
                    return false;
                }
            }
            else {
                if (n < 0) {
                    return false;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool NumberToUInt32(ref NumberBuffer number, ref UInt32 value) {

            int i = number.scale;
            if (i > UInt32Precision || i < number.precision || number.sign ) {
                return false;
            }
            char* p = number.digits;
            Contract.Assert(p != null, "");
            UInt32 n = 0;
            while (--i >= 0) {
                if (n > (0xFFFFFFFF / 10)) {
                    return false;
                }
                n *= 10;
                if (*p != '\0') {
                    UInt32 newN = n + (UInt32)(*p++ - '0');
                    // Detect an overflow here...
                    if (newN < n) {
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private unsafe static bool NumberToUInt64(ref NumberBuffer number, ref UInt64 value) {

            int i = number.scale;
            if (i > UInt64Precision || i < number.precision || number.sign) {
                return false;
            }
            char * p = number.digits;
            Contract.Assert(p != null, "");
            UInt64 n = 0;
            while (--i >= 0) {
                if (n > (0xFFFFFFFFFFFFFFFF / 10)) {
                    return false;
                }
                n *= 10;
                if (*p != '\0') {
                    UInt64 newN = n + (UInt64)(*p++ - '0');
                    // Detect an overflow here...
                    if (newN < n) {
                        return false;
                    }
                    n = newN;
                }
            }
            value = n;
            return true;
        }

        private unsafe static char * MatchChars(char* p, string str) {
            fixed (char* stringPointer = str) {
                return MatchChars(p, stringPointer);
            }
        }

        private unsafe static char * MatchChars(char* p, char* str) {
            Contract.Assert(p != null && str != null, "");

            if (*str == '\0') {
                return null;
            }
            for (; (*str != '\0'); p++, str++) {
                if (*p != *str) { //We only hurt the failure case
                    if ((*str == '\u00A0') && (*p == '\u0020')) {// This fix is for French or Kazakh cultures. Since a user cannot type 0xA0 as a 
                        // space character we use 0x20 space character instead to mean the same.
                        continue;
                    }
                    return null;
                }
            }
            return p;
        }

        public static double ParseDouble(string value, NumberStyles options, NumberFormatInfo numfmt) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            double d = 0;
            
            if (!TryStringToNumber(value, options, ref number, numfmt, false)) {
                //If we failed TryStringToNumber, it may be from one of our special strings.
                //Check the three with which we're concerned and rethrow if it's not one of
                //those strings.
                string sTrim = value.Trim();
                if (sTrim.Equals(numfmt.PositiveInfinitySymbol)) {
                    return double.PositiveInfinity;
                }
                if (sTrim.Equals(numfmt.NegativeInfinitySymbol)) {
                    return double.NegativeInfinity;
                }
                if (sTrim.Equals(numfmt.NaNSymbol)) {
                    return double.NaN;
                }
                throw new FormatException();
            }

            if (!NumberBufferToDouble(number.PackForNative(), ref d)) {
                throw new OverflowException();
            }

            return d;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        internal unsafe static int ParseInt32(string s, NumberStyles style, NumberFormatInfo info) {

            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            int i = 0;
    
            StringToNumber(s, style, ref number, info, false);

            if ((style & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToInt32(ref number, ref i)) { 
                    throw new OverflowException();
                }
            }
            else {
                if (!NumberToInt32(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
            return i;           
        }

        public static tAsyncCall* ParseInt64(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        { 
            tSystemString* pValue = *(tSystemString**)(pParams + 0);
            NumberStyles options = *(NumberStyles*)(pParms + Sys.S_PTR);
            numfmt = MarshalNumFormat(*(byte**)pParams + Sys.S_PTR + Sys.S_INT);

            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            Int64 i = 0;

            StringToNumber(value, options, ref number, numfmt, false);

            if ((options & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToInt64(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
            else {
                if (!NumberToInt64(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
            return i;
        }

        private unsafe static bool ParseNumber(ref char * str, NumberStyles options, ref NumberBuffer number, StringBuilder sb, NumberFormatInfo numfmt, bool parseDecimal) {

            const int StateSign = 0x0001;
            const int StateParens = 0x0002;
            const int StateDigits = 0x0004;
            const int StateNonZero = 0x0008;
            const int StateDecimal = 0x0010;
            const int StateCurrency = 0x0020;

            number.scale = 0;
            number.sign = false;
            string decSep;                  // decimal separator from NumberFormatInfo.
            string groupSep;                // group separator from NumberFormatInfo.
            string currSymbol = null;       // currency symbol from NumberFormatInfo.

            // The alternative currency symbol used in ANSI codepage, that can not roundtrip between ANSI and Unicode.
            // Currently, only ja-JP and ko-KR has non-null values (which is U+005c, backslash)
            string ansicurrSymbol = null;   // currency symbol from NumberFormatInfo.
            string altdecSep = null;        // decimal separator from NumberFormatInfo as a decimal
            string altgroupSep = null;      // group separator from NumberFormatInfo as a decimal

            bool parsingCurrency = false; 
            if ((options & NumberStyles.AllowCurrencySymbol) != 0) {
                currSymbol = numfmt.CurrencySymbol;
                if (numfmt.AnsiCurrencySymbol != null) {
                    ansicurrSymbol = numfmt.AnsiCurrencySymbol;
                }

                // The idea here is to match the currency separators and on failure match the number separators to keep the perf of VB's IsNumeric fast.
                // The values of decSep are setup to use the correct relevant separator (currency in the if part and decimal in the else part).
                altdecSep = numfmt.NumberDecimalSeparator;
                altgroupSep = numfmt.NumberGroupSeparator;
                decSep = numfmt.CurrencyDecimalSeparator;
                groupSep = numfmt.CurrencyGroupSeparator;
                parsingCurrency = true;
            }
            else {
                decSep = numfmt.NumberDecimalSeparator;
                groupSep = numfmt.NumberGroupSeparator;
            }

            int state = 0;
            bool signflag = false; // Cache the results of "options & PARSE_LEADINGSIGN && !(state & STATE_SIGN)" to avoid doing this twice
            bool bigNumber = (sb != null); // When a StringBuilder is provided then we use it in place of the number.digits char[50]
            bool bigNumberHex = (bigNumber && ((options & NumberStyles.AllowHexSpecifier) != 0));
            int maxParseDigits = bigNumber ? int.MaxValue : NumberMaxDigits;
              
            char* p = str;
            char ch = *p;
            char* next;

            while (true) {
                // Eat whitespace unless we've found a sign which isn't followed by a currency symbol.
                // "-Kr 1231.47" is legal but "- 1231.47" is not.
                if (IsWhite(ch) && ((options & NumberStyles.AllowLeadingWhite) != 0) && (((state & StateSign) == 0) || (((state & StateSign) != 0) && (((state & StateCurrency) != 0) || numfmt.NumberNegativePattern == 2)))) {
                    // Do nothing here. We will increase p at the end of the loop.
                }
                else if ((signflag = (((options & NumberStyles.AllowLeadingSign) != 0) && ((state & StateSign) == 0))) && ((next = MatchChars(p, numfmt.PositiveSign)) != null)) {
                    state |= StateSign;
                    p = next - 1;
                } else if (signflag && (next = MatchChars(p, numfmt.NegativeSign)) != null) {
                    state |= StateSign;
                    number.sign = true;
                    p = next - 1;
                }
                else if (ch == '(' && ((options & NumberStyles.AllowParentheses) != 0) && ((state & StateSign) == 0)) {
                    state |= StateSign | StateParens;
                    number.sign = true;
                }
                else if ((currSymbol != null && (next = MatchChars(p, currSymbol)) != null) || (ansicurrSymbol != null && (next = MatchChars(p, ansicurrSymbol)) != null)) {
                    state |= StateCurrency;
                    currSymbol = null;  
                    ansicurrSymbol = null;  
                    // We already found the currency symbol. There should not be more currency symbols. Set
                    // currSymbol to NULL so that we won't search it again in the later code path.
                    p = next - 1;
                }
                else {
                    break;
                }
                ch = *++p;
            }
            int digCount = 0;
            int digEnd = 0;
            while (true) {
                if ((ch >= '0' && ch <= '9') || (((options & NumberStyles.AllowHexSpecifier) != 0) && ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')))) {
                    state |= StateDigits;

                    if (ch != '0' || (state & StateNonZero) != 0 || bigNumberHex) {
                        if (digCount < maxParseDigits) {
                            if (bigNumber)
                                sb.Append(ch);
                            else
                                number.digits[digCount++] = ch;
                            if (ch != '0' || parseDecimal) {
                                digEnd = digCount;
                            }
                        }
                        if ((state & StateDecimal) == 0) { 
                            number.scale++;
                        }
                        state |= StateNonZero;
                    }
                    else if ((state & StateDecimal) != 0) {
                        number.scale--;
                    }
                }
                else if (((options & NumberStyles.AllowDecimalPoint) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars(p, decSep)) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars(p, altdecSep)) != null)) {
                    state |= StateDecimal;
                    p = next - 1;
                }
                else if (((options & NumberStyles.AllowThousands) != 0) && ((state & StateDigits) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars(p, groupSep)) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars(p, altgroupSep)) != null)) {
                    p = next - 1;
                }
                else {
                    break;
                }
                ch = *++p;
            }

            bool negExp = false;
            number.precision = digEnd;
            if (bigNumber)
                sb.Append('\0');
            else
                number.digits[digEnd] = '\0';
            if ((state & StateDigits) != 0) {
                if ((ch == 'E' || ch == 'e') && ((options & NumberStyles.AllowExponent) != 0)) {
                    char* temp = p;
                    ch = *++p;
                    if ((next = MatchChars(p, numfmt.PositiveSign)) != null) {
                        ch = *(p = next);
                    }
                    else if ((next = MatchChars(p, numfmt.NegativeSign)) != null) {
                        ch = *(p = next);
                        negExp = true;
                    }
                    if (ch >= '0' && ch <= '9') {
                        int exp = 0;
                        do {
                            exp = exp * 10 + (ch - '0');
                            ch = *++p;
                            if (exp > 1000) {
                                exp = 9999;
                                while (ch >= '0' && ch <= '9') {
                                    ch = *++p;
                                }
                            }
                        } while (ch >= '0' && ch <= '9');
                        if (negExp) {
                            exp = -exp;
                        }
                        number.scale += exp;
                    }
                    else {
                        p = temp;
                        ch = *p;
                    }
                }
                while (true) {
                    if (IsWhite(ch) && ((options & NumberStyles.AllowTrailingWhite) != 0)) {
                    }
                    else if ((signflag = (((options & NumberStyles.AllowTrailingSign) != 0) && ((state & StateSign) == 0))) && (next = MatchChars(p, numfmt.PositiveSign)) != null) {
                        state |= StateSign;
                        p = next - 1;
                    } else if (signflag && (next = MatchChars(p, numfmt.NegativeSign)) != null) {
                        state |= StateSign;
                        number.sign = true;
                        p = next - 1;
                    }
                    else if (ch == ')' && ((state & StateParens) != 0)) {
                        state &= ~StateParens;
                    }
                    else if ((currSymbol != null && (next = MatchChars(p, currSymbol)) != null) || (ansicurrSymbol != null && (next = MatchChars(p, ansicurrSymbol)) != null)) {
                        currSymbol = null;
                        ansicurrSymbol = null;
                        p = next - 1;
                    }
                    else {
                        break;
                    }
                    ch = *++p;
                }
                if ((state & StateParens) == 0) {
                    if ((state & StateNonZero) == 0) {
                        if (!parseDecimal) {
                            number.scale = 0;
                        }
                        if ((state & StateDecimal) == 0) {
                            number.sign = false;
                        }
                    }
                    str = p;
                    return true;
                }
            }
            str = p;
            return false;
        }

        internal unsafe static Single ParseSingle(string value, NumberStyles options, NumberFormatInfo numfmt) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            double d = 0;

            if (!TryStringToNumber(value, options, ref number, numfmt, false)) {
                //If we failed TryStringToNumber, it may be from one of our special strings.
                //Check the three with which we're concerned and rethrow if it's not one of
                //those strings.
                string sTrim = value.Trim();
                if (sTrim.Equals(numfmt.PositiveInfinitySymbol)) {
                    return Single.PositiveInfinity;
                }
                if (sTrim.Equals(numfmt.NegativeInfinitySymbol)) {
                    return Single.NegativeInfinity;
                }
                if (sTrim.Equals(numfmt.NaNSymbol)) {
                    return Single.NaN;
                }
                throw new FormatException();
            }

            if (!NumberBufferToDouble(number.PackForNative(), ref d)) {
                throw new OverflowException();
            }
            Single castSingle = (Single)d;
            if (Single.IsInfinity(castSingle)) {
                throw new OverflowException();
            }
            return castSingle;
        }

        internal unsafe static UInt32 ParseUInt32(string value, NumberStyles options, NumberFormatInfo numfmt) {

            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            UInt32 i = 0;
        
            StringToNumber(value, options, ref number, numfmt, false);

            if ((options & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToUInt32(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
            else {
                if (!NumberToUInt32(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
    
            return i;
        }

        internal unsafe static UInt64 ParseUInt64(string value, NumberStyles options, NumberFormatInfo numfmt) {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            UInt64 i = 0;

            StringToNumber(value, options, ref number, numfmt, false);
            if ((options & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToUInt64(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
            else {
                if (!NumberToUInt64(ref number, ref i)) {
                    throw new OverflowException();
                }
            }
            return i;
        }

        private unsafe static void StringToNumber(string str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo info, bool parseDecimal) 
        {
    
            if (str == null) {
                throw new ArgumentNullException("string");
            }
            Contract.EndContractBlock();
            Contract.Assert(info != null, "");
            fixed (char* stringPointer = str) {
                char * p = stringPointer;
                if (!ParseNumber(ref p, options, ref number, null, info , parseDecimal) 
                    || (p - stringPointer < str.Length && !TrailingZeros(str, (int)(p - stringPointer)))) {
                    throw new FormatException();
                }
            }
        }
        
        private static bool TrailingZeros(string s, int index) 
        {
            // For compatability, we need to allow trailing zeros at the end of a number string
            for (int i = index; i < s.Length; i++) {
                if (s[i] != '\0') {
                    return false;
                }
            }
            return true;
        }

        internal unsafe static bool TryParseDecimal(string value, NumberStyles options, NumberFormatInfo numfmt, out Decimal result) 
        {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;
                        
            if (!TryStringToNumber(value, options, ref number, numfmt, true)) {
                return false;
            }

            if (!NumberBufferToDecimal(number.PackForNative(), ref result)) {
                return false;
            }
            return true;
        }

        internal unsafe static bool TryParseDouble(string value, NumberStyles options, NumberFormatInfo numfmt, out double result) 
        {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;


            if (!TryStringToNumber(value, options, ref number, numfmt, false)) {
                return false;
            }
            if (!NumberBufferToDouble(number.PackForNative(), ref result)) {
                return false;
            }
            return true;
        }

        internal unsafe static bool TryParseInt32(string s, NumberStyles style, NumberFormatInfo info, out int result) 
        {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;
    
            if (!TryStringToNumber(s, style, ref number, info, false)) {
                return false;
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToInt32(ref number, ref result)) { 
                    return false;
                }
            }
            else {
                if (!NumberToInt32(ref number, ref result)) {
                    return false;
                }
            }
            return true;           
        }

        internal unsafe static bool TryParseInt64(string s, NumberStyles style, NumberFormatInfo info, out Int64 result) 
        {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;
    
            if (!TryStringToNumber(s, style, ref number, info, false)) {
                return false;
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToInt64(ref number, ref result)) { 
                    return false;
                }
            }
            else {
                if (!NumberToInt64(ref number, ref result)) {
                    return false;
                }
            }
            return true;           
        }

        internal unsafe static bool TryParseSingle(string value, NumberStyles options, NumberFormatInfo numfmt, out Single result) 
        {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;
            double d = 0;

            if (!TryStringToNumber(value, options, ref number, numfmt, false)) {
                return false;
            }
            if (!NumberBufferToDouble(number.PackForNative(), ref d)) {
                return false;
            }
            Single castSingle = (Single)d;
            if (Single.IsInfinity(castSingle)) {
                return false;
            }

            result = castSingle;
            return true;
        }

        internal unsafe static bool TryParseUInt32(string s, NumberStyles style, NumberFormatInfo info, out UInt32 result) 
        {
            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;
    
            if (!TryStringToNumber(s, style, ref number, info, false)) {
                return false;
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToUInt32(ref number, ref result)) { 
                    return false;
                }
            }
            else {
                if (!NumberToUInt32(ref number, ref result)) {
                    return false;
                }
            }
            return true;           
        }

        internal unsafe static bool TryParseUInt64(string s, NumberStyles style, NumberFormatInfo info, out UInt64 result) 
        {

            byte * numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
            NumberBuffer number = new NumberBuffer(numberBufferBytes);
            result = 0;
    
            if (!TryStringToNumber(s, style, ref number, info, false)) {
                return false;
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0) {
                if (!HexNumberToUInt64(ref number, ref result)) { 
                    return false;
                }
            }
            else {
                if (!NumberToUInt64(ref number, ref result)) {
                    return false;
                }
            }
            return true;           
        }

        private static bool TryStringToNumber(char* str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numfmt, bool parseDecimal) 
        {   
            return TryStringToNumber(str, options, ref number, null, numfmt, parseDecimal);
        }

        private unsafe static bool TryStringToNumber(string str, NumberStyles options, ref NumberBuffer number, StringBuilder sb, NumberFormatInfo numfmt, bool parseDecimal) 
        {   
            if (str == null) {
                return false;
            }

            fixed (char* stringPointer = str) {
                char * p = stringPointer;
                if (!ParseNumber(ref p, options, ref number, sb, numfmt, parseDecimal) 
                    || (p - stringPointer < str.Length && !TrailingZeros(str, (int)(p - stringPointer)))) {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif