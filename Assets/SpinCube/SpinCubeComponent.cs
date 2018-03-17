using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinCubeComponent : MonoBehaviour {

    float speedOffset = 0.0f;

	// Update is called once per frame
	void Update () {
        speedOffset.ToString();
//        Debug.Log("Left " + speedOffset);
//        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
//            speedOffset -= 1;
//        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
//            Debug.Log("Right " + speedOffset);
//            speedOffset += 1;
//        }
//        transform.eulerAngles += new Vector3(0f, 30.0f + speedOffset, 0f) * Time.deltaTime;
	}
}
