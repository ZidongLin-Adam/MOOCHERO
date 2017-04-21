using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMove : MonoBehaviour
{

    public int MoveSpeed = 2;
    public Animator PlayerAnimator;

    private float horizonCrossInput;
    private float vertiCrossInput;

    void CrossMove(float h_cInput,float v_cInput)
    {
        if (h_cInput != 0.0f || v_cInput != 0.0f)
        {
            Debug.Log("horizonInput:" + h_cInput+"====="+"vertical"+ v_cInput);

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

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        horizonCrossInput = CrossPlatformInputManager.GetAxis("Horizontal");
        vertiCrossInput = CrossPlatformInputManager.GetAxis("Vertical");
        CrossMove(horizonCrossInput, vertiCrossInput);
    }
}