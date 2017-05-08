using UnityEngine;
using System.Collections;
using Photon;

public class PlayerHealth : PunBehaviour{

	public int killScore = 10;
	public int maxHP = 100;
	public GameObject gun;
	public float respawnTime = 5.0f;
	public float invincibleTime = 3.0f;

	[HideInInspector]public int team;	
	[HideInInspector]public bool isAlive;	
	[HideInInspector]public int currentHP;	
	[HideInInspector]public bool invincible;

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
		if (PhotonNetwork.player.customProperties ["Team"].ToString () == "Team1")	
			team = 1;
		else
			team = 2;
		photonView.RPC ("SetTeam", PhotonTargets.Others, team);	
	}

	
	void init(){
		currentHP = maxHP;
		isAlive = true;		
		timer = 0.0f;
		invincible = true;
	}

	
	void Update () {
		if (!photonView.isMine)		
			return;
		timer += Time.deltaTime;	
		if (timer > invincibleTime && invincible == true)				
			photonView.RPC ("SetInvincible", PhotonTargets.All, false);
		else if (timer <= invincibleTime && invincible == false)
			photonView.RPC ("SetInvincible", PhotonTargets.All, true);
	}

	
	[PunRPC]
	void SetInvincible(bool isInvincible){
		invincible = isInvincible;
	}


	public void TakeDamage(int damage,PhotonPlayer attacker){
		if (!isAlive || invincible)				
			return;
		if (PhotonNetwork.isMasterClient) {		
			currentHP -= damage;				
			photonView.RPC ("UpdateHP", PhotonTargets.All, currentHP);
			if (currentHP <= 0 && attacker!=null) {					
				GameManager.gm.AddScore (killScore, attacker);	
			}
		}
	}

	
	
	[PunRPC]
	public void AddHP(int value)
	{
		if (!PhotonNetwork.isMasterClient)	
			return;
		if (!isAlive || currentHP == maxHP)	
			return;
		currentHP += value;			
		if (currentHP > maxHP) {	
			currentHP = maxHP;
		}
		photonView.RPC ("UpdateHP", PhotonTargets.All, currentHP);	//使用RPC，更新所有客户端，该玩家对象的血量
	}


	
	[PunRPC]
	void UpdateHP(int newHP)
	{
		currentHP = newHP;		
		if (currentHP <= 0) {
			isAlive = false;
			if (photonView.isMine) {					
				anim.SetBool ("isDead", true);			
				Invoke ("PlayerSpawn", respawnTime);
			}
			rigid.useGravity = false;	
			colli.enabled = false;		
			gun.SetActive (false);		
			anim.applyRootMotion = true;	
			GetComponent<IKController> ().enabled = false;
		}
	}
	
	[PunRPC]
	void PlayerReset(){
		init ();	
		rigid.useGravity = true;		
		colli.enabled = true;			
		gun.SetActive (true);				
		anim.SetBool ("isDead", false);	
		anim.applyRootMotion = false;	
		GetComponent<IKController> ().enabled = true;
	}


	[PunRPC]
	void SetTeam(int newTeam){
		team = newTeam;
	}

}
