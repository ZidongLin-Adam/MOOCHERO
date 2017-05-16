using UnityEngine;
using System.Collections;
using Photon;

public class GunController : PunBehaviour {

	Vector3 m_position;
	Quaternion m_rotation;
	float lerpSpeed = 10.0f;


	void Start(){
		m_position = transform.position;	
		m_rotation = transform.rotation;
	}


	void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info){
		if (stream.isWriting) 		
		{
			stream.SendNext (transform.position);
			stream.SendNext (transform.rotation);
		} 
		else 							
		{
			m_position = (Vector3)stream.ReceiveNext();
			m_rotation = (Quaternion)stream.ReceiveNext();
		}
	}

	void Update () {
		if (!photonView.isMine) 
		{
			transform.position = Vector3.Lerp 		
				(transform.position, m_position, Time.deltaTime * lerpSpeed);
			transform.rotation = Quaternion.Lerp 
				(transform.rotation, m_rotation, Time.deltaTime * lerpSpeed);
		}
	}

}