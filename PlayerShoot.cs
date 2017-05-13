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


	[PunRPC]
	void Shoot(PhotonPlayer attacker){
		if (!PhotonNetwork.isMasterClient)	
			return;
		ray.origin = shootingPosition+transform.position;	
		ray.direction = gunTransform.forward;			
		Vector3 bulletEffectPosition;					


		if (Physics.Raycast (ray, out hitInfo, shootingRange)) {	
			GameObject go = hitInfo.collider.gameObject;		
			if (go.tag == "Player") {								
				PlayerHealth playerHealth = go.GetComponent<PlayerHealth> ();
				if (playerHealth.team != GetComponent<PlayerHealth> ().team) {
					playerHealth.TakeDamage (shootingDamage, attacker);		
				}
			} else if (go.tag == "Zombie") {					
				ZombieHealth zh = go.GetComponent<ZombieHealth> ();
				if (zh != null) {
					zh.TakeDamage (shootingDamage, attacker);		
				}
			}
			bulletEffectPosition = hitInfo.point;					
		}else
			bulletEffectPosition = ray.origin + shootingRange * ray.direction;	
		photonView.RPC ("ShootEffect", PhotonTargets.All, bulletEffectPosition);
	}

	[PunRPC]
	void ShootEffect(Vector3 bulletEffectPosition){
		AudioSource.PlayClipAtPoint (shootingAudio, transform.position);	
		if (gunShootingEffect != null && gunBarrelEnd != null) {			
			(Instantiate (gunShootingEffect, 
				gunBarrelEnd.position, 
				gunBarrelEnd.rotation) as GameObject).transform.parent = gunBarrelEnd;
		}
		if (bulletEffect != null) {													
			Instantiate (bulletEffect, 
				bulletEffectPosition, 
				Quaternion.identity);
		}
	}
}
