using UnityEngine;
using System.Collections;
using Photon ;

public class ZombieHealth :  PunBehaviour{

	public int currentHP = 100;		
	public int maxHP = 100;			
	public int killScore = 5;		
	public AudioClip zombieHurtAudio;


	public bool IsAlive {
		get {
			return currentHP > 0;
		}
	}


	public void TakeDamage(int damage,PhotonPlayer attacker){
		if (!IsAlive)
			return;

		if (PhotonNetwork.isMasterClient) {
			currentHP -= damage;
			if (currentHP <= 0 && attacker!=null) {
				GameManager.gm.AddScore (killScore, attacker);
				currentHP = 0;
			}

			photonView.RPC ("UpdateHP", PhotonTargets.All, currentHP);

			if (zombieHurtAudio != null)
				photonView.RPC ("PlayZombieHurtAudio", PhotonTargets.All);
		}
	}


	[PunRPC]
	void UpdateHP(int newHP)
	{
		currentHP = newHP;
	}

	[PunRPC]
	void PlayZombieHurtAudio()
	{
		AudioSource.PlayClipAtPoint (zombieHurtAudio, transform.position);
	}
}
