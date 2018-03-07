using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class Test01_Expressions
{
 #if NO
    public static void Assert(bool b)
    {
        if (!b)
            throw new Exception("Assertion failure");
    }


    public static void TestShort()
    {
        short _1 = 1;
        short _10 = 10;
        short _5 = 5;
        short _20 = 20;
        short _19 = 19;
        short _FFF = 0xFFF;
        short _F00 = 0xF00;
        short _555 = 0x555;
        short _CCC = 0xCCC;

        short i1 = (short)-((_10 * _5 + _10) / _20 - _1);
        Assert(i1 == -2);

        short i2 = (short)(_FFF & _F00);
        Assert(i2 == 0xF00);

        short i3 = (short)(_FFF ^ _555);
        Assert(i3 == 0xCCC);

        short i4 = (short)(_CCC | _555);
        Assert(i4 == 0xFFF);

        short i5 = (short)(_19 % _10);
        Assert(i5 == 9);
    }

    public static void TestInt()
    {
        int _1 = 1;
        int _10 = 10;
        int _5 = 5;
        int _20 = 20;
        int _19 = 19;
        int _FFFFFFF = 0xFFFFFFF;
        int _FFFFF00 = 0xFFFFF00;
        int _5555555 = 0x5555555;
        int _CCCCCCC = 0xCCCCCCC;

        int i1 = -((_10 * _5 + _10) / _20 - _1);
        Assert(i1 == -2);

        int i2 = _FFFFFFF & _FFFFF00;
        Assert(i2 == 0xFFFFF00);

        int i3 = _FFFFFFF ^ _5555555;
        Assert(i3 == 0xCCCCCCC);

        int i4 = _CCCCCCC | _5555555;
        Assert(i4 == 0xFFFFFFF);

        int i5 = _19 % _10;
        Assert(i5 == 9);
    }

    public static void TestLong()
    {
        long _1 = 1;
        long _10 = 10;
        long _5 = 5;
        long _20 = 20;
        long _19 = 19;
        long _FFFFFFFFFFFFFFF = 0xFFFFFFFFFFFFFFFL;
        long _FFF00FFFFFFFF00 = 0xFFF00FFFFFFFF00L;
        long _555555555555555 = 0x555555555555555;
        long _CCCCCCCCCCCCCCC = 0xCCCCCCCCCCCCCCC;

        long i1 = -((_10 * _5 + _10) / _20 - _1);
        Assert(i1 == -2);

        long i2 = _FFFFFFFFFFFFFFF & 0xFFF00FFFFFFFF00L;
        Assert(i2 == 0xFFF00FFFFFFFF00L);

        long i3 = _FFFFFFFFFFFFFFF ^ _555555555555555;
        Assert(i3 == 0xCCCCCCCCCCCCCCCL);

        long i4 = _CCCCCCCCCCCCCCC | _555555555555555;
        Assert(i4 == 0xFFFFFFFFFFFFFFFL);

        long i5 = _19 % _10;
        Assert(i5 == 9);
    }


    public static void TestFloat()
    {
    }

    public static void TestDouble()
    {
    }

    public static void Test()
    {
        TestShort();
        TestInt();
        TestLong();
        TestFloat();
        TestDouble();
    }
#endif

    public static void Test()
    {
        Debug.Log("This is a test!");
    }

}
