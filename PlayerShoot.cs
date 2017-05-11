using UnityEngine;
using System.Collections;
using Photon;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerShoot : PunBehaviour {

	public int shootingDamage = 10;			
	public float shootingRange = 50.0f;		
	public float timeBetweenShooting = 0.2f;

	public Transform gunTransform;			
	public Transform gunBarrelEnd;		
	public GameObject gunShootingEffect;	
	public GameObject bulletEffect;		
	public AudioClip shootingAudio;		
	[HideInInspector]
	public Vector3 shootingPosition 	
		= new Vector3(-0.238f,1.49f,0.065f);

	PlayerHealth playerHealth;
	bool isShooting;
	Ray ray;
	RaycastHit hitInfo;
	float timer;

	void Start(){ 
		playerHealth = GetComponent<PlayerHealth> ();
		timer = 0.0f;								
	}

	void Update () {
		if (!photonView.isMine || !playerHealth.isAlive)	
			return;
		timer += Time.deltaTime;	
		if (CrossPlatformInputManager.GetButton("Fire1") && timer >= timeBetweenShooting) {
			timer = 0.0f;			
			if (GameManager.gm.state == GameManager.GameState.Playing)						
				photonView.RPC ("Shoot", PhotonTargets.MasterClient, PhotonNetwork.player);	
		}
	}


}
