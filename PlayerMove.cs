using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMove : MonoBehaviour {
	public float moveSpeed = 6.0f;		
	public float rotateSpeed = 10.0f;	
	public float jumpVelocity = 5.0f;	

	float minMouseRotateX = -45.0f;		
	float maxMouseRotateX = 45.0f;		
	float mouseRotateX;								

	Camera myCamera;					
	Animator anim;
	Rigidbody rigid;
	CapsuleCollider capsuleCollider;
	PlayerHealth playerHealth;

	void Start(){
		myCamera = Camera.main;											
		mouseRotateX = myCamera.transform.localEulerAngles.x;			
		anim = GetComponent<Animator> ();								
		rigid = GetComponent<Rigidbody> ();								
		capsuleCollider = GetComponent<CapsuleCollider> ();				
		playerHealth = GetComponent<PlayerHealth> ();					
	}

	void FixedUpdate(){
		if (!playerHealth.isAlive)	
			return;
		CheckGround();			
		if (isGrounded == false)				
			anim.SetBool ("isJump", false);		
	}

	void Update()
	{
		if (!playerHealth.isAlive) 
			return;
		float h = CrossPlatformInputManager.GetAxisRaw ("Horizontal");	
		float v = CrossPlatformInputManager.GetAxisRaw ("Vertical");	
		Move (h, v);		
		float rv = CrossPlatformInputManager.GetAxisRaw ("Mouse X");	
		float rh = CrossPlatformInputManager.GetAxisRaw ("Mouse Y");	
		Rotate (rh, rv);	
		Jump (isGrounded);
	}

	void Move(float h,float v){

		transform.Translate ((Vector3.forward * v + Vector3.right * h) * moveSpeed * Time.deltaTime);
		if (h != 0.0f || v != 0.0f) {
			anim.SetBool ("isMove", true);	
		} else
			anim.SetBool ("isMove", false);
	}

	void Rotate(float rh,float rv){
		transform.Rotate (0, rv * rotateSpeed, 0);	
		mouseRotateX -= rh * rotateSpeed;	
		mouseRotateX = Mathf.Clamp (mouseRotateX, minMouseRotateX, maxMouseRotateX);
		myCamera.transform.localEulerAngles = new Vector3 (mouseRotateX, 0.0f, 0.0f);
	}
}






















