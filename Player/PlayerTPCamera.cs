using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTPCamera : MonoBehaviour {

    public Camera mainCamera;

    private Transform selfTransform;
    private Vector2 mouseDown;
    private Vector2 mouseHold;

    void RotateCamera()
    {
        if (Input.GetMouseButtonDown(1)){
            //Debug.Log("MouseButtonDown:" + Input.mousePosition);
            mouseDown = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            //Debug.Log("MouseButtonUp:" + Input.mousePosition);
            mouseHold = Input.mousePosition;
            if (mouseDown != mouseHold)
            {
                mainCamera.transform.RotateAround(selfTransform.position, Vector3.up, (mouseHold.x - mouseDown.x) * Time.deltaTime);
            }
        }
    }

    void Start () {
        selfTransform = transform;
    }
	
	void LateUpdate () {
        RotateCamera();

    }
}
