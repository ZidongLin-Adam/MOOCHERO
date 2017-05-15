using UnityEngine;
using System.Collections;

public class IKController : MonoBehaviour {

	Animator animator;						
	public bool isActive = true;		
	public Transform lookObj = null;	
	public Transform leftHandObj = null;	
	public Transform rightHandObj = null;	
	public Transform bodyObj = null;		

	void Start(){
		animator = GetComponent<Animator> ();
	}

	void OnAnimatorIK()
	{
		if (animator) {
			if (isActive) {
				if (lookObj != null) {
					animator.SetLookAtWeight (1.0f);
					animator.SetLookAtPosition (lookObj.position);
				}
				if (bodyObj != null) {
					animator.bodyRotation = bodyObj.rotation;
				}
				if (leftHandObj != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1.0f);
					animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 1.0f);
					animator.SetIKPosition (AvatarIKGoal.LeftHand, leftHandObj.position);
					animator.SetIKRotation (AvatarIKGoal.LeftHand, leftHandObj.rotation);
				}
				if (rightHandObj != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 1.0f);
					animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 1.0f);
					animator.SetIKPosition (AvatarIKGoal.RightHand, rightHandObj.position);
					animator.SetIKRotation (AvatarIKGoal.RightHand, rightHandObj.rotation);
				}
			} else {
				animator.SetLookAtWeight (0);
				animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 0);
				animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 0);
			}
		}
	}
}
