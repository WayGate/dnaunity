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
            Debug.Log("Up");
            rotXSpeed -= 1f;
            keyDown = true;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            Debug.Log("Down");
            rotXSpeed += 1f;
            keyDown = true;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            Debug.Log("Left");
            rotYSpeed -= 1f;
            keyDown = true;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Debug.Log("Right");
            rotYSpeed += 1f;
            keyDown = true;
        }
        if (keyDown) {
            rot = Quaternion.Euler(rotXSpeed, rotYSpeed, 0);
        }
        transform.rotation *= Quaternion.Slerp(Quaternion.identity, rot, Time.deltaTime);
    }
}


/*public static class Testing
{
    public static float speedOffset = 1f;

	public static void Test()
    {
        speedOffset -= 1f;
        speedOffset -= 1f;
        speedOffset -= 1f;
    }
}*/
