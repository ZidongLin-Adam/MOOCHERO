using UnityEngine;
using System.Collections;
using Photon;

public class PlayerHealth : PunBehaviour{

	public int killScore = 10;
	public int maxHP = 100;
	public GameObject gun;
	public float invincibleTime = 3.0f;


	float timer;
	Animator anim;
	Rigidbody rigid;
	Collider colli;

	
	void Start () {
		init ();	
		anim = GetComponent<Animator> ();		
		rigid = GetComponent<Rigidbody> ();		
		colli = GetComponent<CapsuleCollider> ();
		if (!photonView.isMine) return;				
		photonView.RPC ("UpdateHP", PhotonTargets.Others, currentHP);				
		photonView.RPC ("SetTeam", PhotonTargets.Others, team);	
	}

	
	void init(){
		currentHP = maxHP;
		isAlive = true;		
		timer = 0.0f;
		invincible = true;
	}

	
	void Update () {
		timer += Time.deltaTime;	
		if (timer > invincibleTime && invincible == true)				
			photonView.RPC ("SetInvincible", PhotonTargets.All, false);
		else if (timer <= invincibleTime && invincible == false)
			photonView.RPC ("SetInvincible", PhotonTargets.All, true);
	}

	
	public void TakeDamage(int damage,PhotonPlayer attacker){
		if (!isAlive || invincible)				
			return;
			photonView.RPC ("UpdateHP", PhotonTargets.All, currentHP);
			if (currentHP <= 0 && attacker!=null) {					
				GameManager.gm.AddScore (killScore, attacker);	
			}
		}
	}

	
	public void requestAddHP(int value)
	{
		photonView.RPC ("AddHP", PhotonTargets.MasterClient, value);	//使用RPC,向MasterClient发起加血请求
	}

	




}
