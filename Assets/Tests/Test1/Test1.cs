using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public static class Test1
{

    public class A
    {
        public virtual void foo()
        {
            Console.WriteLine("A:foo");
        }
    }

    public class B : A
    {
        public override void foo()
        {
            Console.WriteLine("B:foo");
        }
    }

    public static void Test()
    {
        List<string> l = new List<string>();
        l.Add("Blah1");
        l.Add("Blah2");
        l.Add("Blah3");

        foreach (string s in l)
        {
            Console.WriteLine(s);
        }

        Console.WriteLine("Value is {0}", 100.2f * 34f / 20f);

        A a = new A();
        A b = new B();

        a.foo();
        b.foo();

/*        Console.WriteLine("i1 = 100 + 200");
        int i1 = 100 + 200;
        Debug.Assert(i1 == 300);

        Console.WriteLine("i2 = 100 * 10");
        int i2 = 100 + 200;
        Debug.Assert(i2 == 1000);

        Console.WriteLine("i3 = -((10 * 5 + 10) / 20 - 1)");
        int i3 = -((10 * 5 + 10) / 20 - 1);
        Debug.Assert(i3 == -2);

        Console.WriteLine("i4 = 0xFFFF &  0x5555");
        int i4 = 0xFFFF & 0x5555;
        Debug.Assert(i4 == 0x5555);

        Console.WriteLine("i5 = 0xFFFF ^ 0xFF00");
        int i5 = 0xFFFF ^ 0xFF00;
        Debug.Assert(i4 == 0x5555); */

    }
}
