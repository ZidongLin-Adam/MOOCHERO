using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

    public int MoveSpeed = 2;
    public Animator PlayerAnimator;

    private Transform playerTransform;

    void crossMove()
    {
        playerTransform = transform;

        if (Input.GetKey(KeyCode.W))
        {
            playerTransform.position += Vector3.forward * MoveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
            {
                playerTransform.position += Vector3.left * MoveSpeed * Time.deltaTime;
                //transform.Translate((Vector3.forward + Vector3.left) * MoveSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                playerTransform.position += Vector3.right * MoveSpeed * Time.deltaTime;
                //transform.Translate((Vector3.forward + Vector3.right) * MoveSpeed * Time.deltaTime);
            }
        } else if (Input.GetKey(KeyCode.S))
        {
            playerTransform.position += Vector3.back * MoveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
            {
                playerTransform.position += Vector3.left * MoveSpeed * Time.deltaTime;
                //transform.Translate((Vector3.forward + Vector3.left) * MoveSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                playerTransform.position += Vector3.right * MoveSpeed * Time.deltaTime;
                //transform.Translate((Vector3.forward + Vector3.right) * MoveSpeed * Time.deltaTime);
            }
        }
        else
        {
            transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
        }
        
     }

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		crossMove();
	}
}
