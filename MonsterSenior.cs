using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSenior : MonoBehaviour {

    public float SoundRange = 10.0f;
    public float SightRange = 50.0f;
    public float SightAngle = 80.0f;
    public float FeelingInterval = 1.0f;

    private Transform minPlayer;
    private Transform selfPlayer;
    private float min;
    //private string minName;
    private float FeelingIntervaltimer = 0.0f;

    private void Start()
    {
        selfPlayer = transform;
    }


    private void MonFeeling()
    {
        minPlayer = null;
        GameObject[] palyerList = GameObject.FindGameObjectsWithTag("Player");
        min = float.MaxValue;
        //minName = "还未看到player";
        foreach (GameObject p in palyerList)
        {
            float dis = Vector3.Distance(p.transform.position, selfPlayer.position);
            if((dis<min && dis< SoundRange) || (dis<= SightRange && Vector3.Angle(p.transform.position - selfPlayer.position, selfPlayer.forward) <= SightAngle / 2))
            {
                min = dis;
                minPlayer = p.transform;
                //Debug.Log("我看到" + p.name +"======" +Time.frameCount);
                //minName = p.name;
            }
        }
        //if (minPlayer != null)
        //{
            //Debug.Log("我看到离我最近的是" + minName + "======" + Time.frameCount);
        //}
    }

    public Transform GetMinPlayer()
    {
        return minPlayer;
    }

    void FixedUpdate()
    {
        if (FeelingIntervaltimer > FeelingInterval)
        {
            MonFeeling();
            FeelingIntervaltimer = 0.0f;
        }
        FeelingIntervaltimer += Time.deltaTime;
    }
}
