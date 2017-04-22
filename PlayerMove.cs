using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMove : MonoBehaviour
{

    public int MoveSpeed = 2;
    public Animator PlayerAnimator;
    public float jumpVelocity = 20.0f;
    public Camera maincamera;
    public Transform virtualCamera;

    private float horizonCrossInput;
    private float vertiCrossInput;
    private Rigidbody playerRigidbody;
    private bool isGrounded;
    private float groundedRaycastDistance = 0.01f; 

    void CrossMove(float h_cInput,float v_cInput)
    {
        if (h_cInput != 0.0f || v_cInput != 0.0f)
        {
            //Debug.Log("horizonInput:" + h_cInput+"====="+"vertical"+ v_cInput);
            transform.rotation = maincamera.transform.rotation;
            maincamera.transform.position = virtualCamera.transform.position;
            PlayerAnimator.SetBool("isWalk", true);
            if (v_cInput > 0)
            {
                transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime * v_cInput);
            }
            else if (v_cInput < 0)
            {
                transform.Translate(Vector3.forward * MoveSpeed / 2 * Time.deltaTime * v_cInput);
            }

            if (h_cInput < 0)
            {
                transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime * h_cInput);
            }
            else if (h_cInput > 0)
            {
                transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime * h_cInput);
            }
        }
        else
        {
            PlayerAnimator.SetBool("isWalk", false);
        }
        
    }
    void PlayerJump(bool isGround)
    {
        if (Input.GetKey(KeyCode.Space)&& isGround)
        {
            PlayerAnimator.SetBool("isJump", true);
            playerRigidbody.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
        }
        else
        {
            PlayerAnimator.SetBool("isJump", false);
        }
    }
    // Use this for initialization
    void Start()
    {
        
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        horizonCrossInput = CrossPlatformInputManager.GetAxisRaw("Horizontal");
        vertiCrossInput = CrossPlatformInputManager.GetAxisRaw("Vertical");
        CrossMove(horizonCrossInput, vertiCrossInput);
        isGrounded = Physics.Raycast(transform.position, -Vector3.up, groundedRaycastDistance);
        PlayerJump(isGrounded);
        //playerRigidbody.AddForce(-Vector3.up*3, ForceMode.VelocityChange);
        
    }
}