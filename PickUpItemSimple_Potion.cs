using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class PickUpItemSimple_Potion : Photon.MonoBehaviour
{
	public int HealthPoint = 20;		
	
	public float SecondsBeforeRespawn = 10;
	public bool PickupOnCollide;
	public bool SentPickup;

	
	public void OnTriggerEnter(Collider other)
	{
	
		if(other.tag !="Player")return;							
		PhotonView otherpv = other.GetComponent<PhotonView>();		
		if (this.PickupOnCollide && otherpv != null && otherpv.isMine)	
		{
			this.Pickup();
		}
	}
		
	public void Pickup()
	{
		if (this.SentPickup)
		{
			return;
		}

		this.SentPickup = true;
		this.photonView.RPC("PunPickupSimple", PhotonTargets.AllViaServer);	
	}

	[PunRPC]
	public void PunPickupSimple(PhotonMessageInfo msgInfo)
	{
		if (this.SentPickup && msgInfo.sender.isLocal)	
		{
			if (this.gameObject.GetActive())
			{
			
				GameManager.gm.localPlayerAddHealth(HealthPoint);
			}
			else
			{
				
			}
		}

		this.SentPickup = false;

		if (!this.gameObject.GetActive())
		{
			Debug.Log("Ignored PUN RPC, cause item is inactive. " + this.gameObject);
			return;
		}

		double timeSinceRpcCall = (PhotonNetwork.time - msgInfo.timestamp);
		float timeUntilRespawn = SecondsBeforeRespawn - (float)timeSinceRpcCall;

		if (timeUntilRespawn > 0)	
		{
			this.gameObject.SetActive(false);		
			Invoke("RespawnAfter", timeUntilRespawn);	
		}
	}

}
