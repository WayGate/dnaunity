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

namespace System
{
    public interface IConvertible
    {
        TypeCode GetTypeCode();
        bool ToBoolean(IFormatProvider provider);
        char ToChar(IFormatProvider provider);
        sbyte ToSByte(IFormatProvider provider);
        byte ToByte(IFormatProvider provider);
        short ToInt16(IFormatProvider provider);
        ushort ToUInt16(IFormatProvider provider);
        int ToInt32(IFormatProvider provider);
        uint ToUInt32(IFormatProvider provider);
        long ToInt64(IFormatProvider provider);
        ulong ToUInt64(IFormatProvider provider);
        float ToSingle(IFormatProvider provider);
        double ToDouble(IFormatProvider provider);
        Decimal ToDecimal(IFormatProvider provider);
        DateTime ToDateTime(IFormatProvider provider);
        String ToString(IFormatProvider provider);
        Object ToType(Type conversionType, IFormatProvider provider);
    }
    
}

#endif