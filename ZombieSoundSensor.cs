using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSoundSensor : MonoBehaviour {

	public float Range = 15.0f;			
	public float sensorInterval = 1.0f;		

	private float trigerTime = 0.0f;

	private Transform sensorTransform;
	private Transform nearestPlayer;

	void Start()
	{
		sensorTransform = transform;
	}
	void FixedUpdate()
	{
		if (trigerTime >= sensorInterval) {
			trigerTime = 0;
			UpdatePlayerList ();
		}
		trigerTime += Time.deltaTime;

	}
	void UpdatePlayerList()
	{
		nearestPlayer = null;
		GameObject[] playerObjList = GameObject.FindGameObjectsWithTag ("Player");
		float min = float.MaxValue;
		foreach (GameObject p in playerObjList) 
		{
			PlayerHealth ph = p.GetComponent<PlayerHealth> ();
			if (ph != null && ph.isAlive)
			{
				float dist = Vector3.Distance (p.transform.position, sensorTransform.position);
				if (dist < Range && dist < min) {
					min = dist;
					nearestPlayer = p.transform;
				}
					
			}
		}
	}

	public Transform getNearestPlayer()
	{
		return nearestPlayer;
	}
}
