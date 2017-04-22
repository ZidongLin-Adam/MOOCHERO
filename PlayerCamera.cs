using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    public Camera mainCamera;

    private Vector2 mouseDown;
    private Vector2 mouseHold;

    public void rotateCamera()
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
                //mainCamera.transform.RotateAround(transform.position,new Vector3(0,Vector3.Distance(mouseHold,mouseDown),0),0.5f);
                mainCamera.transform.RotateAround(transform.position, Vector3.up, (mouseHold.x - mouseDown.x) * Time.deltaTime);
                //mainCamera.transform.RotateAround(transform.position, Vector3.left, (mouseHold.y - mouseDown.y) * Time.deltaTime/2);
                //transform.Rotate( Vector3.up, (mouseHold.x - mouseDown.x) * Time.deltaTime);
            }
        }
        
            
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(Vector3.up * 3 * rotateSpeed * Time.deltaTime);
        rotateCamera();

    }
}
