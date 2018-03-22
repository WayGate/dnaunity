using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinCubeComponent : MonoBehaviour
{
    public float rotXSpeed = 0.0f;
    public float rotYSpeed = 0.0f;
    public Quaternion rot = Quaternion.identity;

    // Update is called once per frame
    public void Update()
    {
        bool keyDown = false;
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            Debug.Log("Up " + rotXSpeed.ToString());
            rotXSpeed -= 1f;
            keyDown = true;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            Debug.Log("Down " + rotXSpeed.ToString());
            rotXSpeed += 1f;
            keyDown = true;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            Debug.Log("Left " + rotYSpeed.ToString());
            rotYSpeed -= 1f;
            keyDown = true;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Debug.Log("Right " + rotYSpeed.ToString());
            rotYSpeed += 1f;
            keyDown = true;
        }
        if (keyDown) {
            rot = Quaternion.Euler(rotXSpeed, rotYSpeed, 0);
        }
        transform.rotation *= Quaternion.Slerp(Quaternion.identity, rot, Time.deltaTime);
    }
}


public static class Testing
{
    public static float speedOffset = 1f;

	public static void Test()
    {
        for (int i = 1; i < 1000; i++) {
            System.Console.WriteLine("Value = " + i.ToString());
        }

        System.Console.WriteLine("Left " + speedOffset.ToString());

        bool _b;
        byte b = byte.Parse("1");
        _b = byte.TryParse("1", out b);
        System.Console.WriteLine(b.ToString());
        sbyte sb = sbyte.Parse("-2");
        _b = sbyte.TryParse("-2", out sb);
        System.Console.WriteLine(sb.ToString());
        ushort us = ushort.Parse("3");
        _b = ushort.TryParse("3", out us);
        System.Console.WriteLine(us.ToString());
        short ss = short.Parse("-4");
        _b = short.TryParse("-4", out ss);
        System.Console.WriteLine(ss.ToString());
        uint ui = uint.Parse("5");
        _b = uint.TryParse("5", out ui);
        System.Console.WriteLine(ui.ToString());
        int si = int.Parse("-6");
        _b = int.TryParse("-6", out si);
        System.Console.WriteLine(si.ToString());
        ulong ul = ulong.Parse("7");
        _b = ulong.TryParse("7", out ul);
        System.Console.WriteLine(ul.ToString());
        long sl = long.Parse("-8");
        _b = long.TryParse("-8", out sl);
        System.Console.WriteLine(sl.ToString());
        float f = float.Parse("9.99");
        _b = float.TryParse("9.99", out f);
        System.Console.WriteLine(f.ToString());
        double d = double.Parse("10.10");
        _b = double.TryParse("10.10", out d);
        System.Console.WriteLine(d.ToString());
    }
}
