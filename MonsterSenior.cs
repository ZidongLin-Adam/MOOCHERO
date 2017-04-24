using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSenior : MonoBehaviour {

    public GameObject targetPlayer;
    public float SoundRange = 10.0f;
    public float SightRange = 50.0f;
    public float SightAngle = 80.0f;

    public GameObject MonFeeling()
    {
        if (Vector3.Distance(targetPlayer.transform.position, transform.position) <= SoundRange)
        {
            //Debug.Log("听到了");
            return targetPlayer;
        }
        
        if (Vector3.Distance(targetPlayer.transform.position, transform.position) <= SightAngle && Vector3.Angle(targetPlayer.transform.position - transform.position, transform.forward) <= SightAngle / 2)
        {
            //Debug.Log("看到了");
            //Debug.Log("看"+ Vector3.Angle(targetPlayer.transform.position - transform.position, transform.forward));
            return targetPlayer;
        }
            
        return null;
    }

    void Update()
    {
        MonFeeling();
    }
}
