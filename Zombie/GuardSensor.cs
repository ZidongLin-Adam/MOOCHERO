using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;
public class GuardSensor : PunBehaviour
{
    public float SensorInterval = 0.5f;	//僵尸的感知时间间隔
    private float senseTimer = 0.0f;	//统计僵尸的感知时间
    private Transform nearbyPlayer;		//僵尸周围玩家对象的缓存
    private Dictionary<int, int> damageMap = new Dictionary<int,int>(); //玩家造成的伤害表
    private GuardAI guardAI; //AI组件
    static public GuardSensor gs;//静态成员 方便外部访问
    //启动时调用
    void Start()
    {
        gs = this;
        guardAI = GetComponent<GuardAI>();
    }
    //计算权重（仇恨值）
    float calculateWeight(Vector3 playerPosition, int damage)
    {
        var distance = Vector3.Distance(playerPosition, guardAI.SourcePointTransform.position);
        return  90 / distance + damage ;
    }
    //判断玩家位置是否在Guard活动范围内 范围外的一律不感知
    bool notInSensorRange(Vector3 playerPosition)
    {
        var sourcePos = guardAI.SourcePointTransform.position;
        return Vector3.Distance(playerPosition, sourcePos) > (GuardAI.guardActiveRange * 0.5f);
    }
    //定时更新
    void FixedUpdate()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        //以一定的时间间隔，进行感知
        if (senseTimer >= SensorInterval)
        {
            senseTimer = 0;
            SenseNearbyPlayer();
        }
        senseTimer += Time.deltaTime;

    }
    //感知一个可攻击的玩家
    void SenseNearbyPlayer()
    {
        nearbyPlayer = null;
        float maxWeight = float.MinValue;
        float curWeight = 0f;
        //得到所有玩家实例
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)//对每个玩家进行权重计算
        {
            var ph = player.GetComponent<PlayerHealth>();
            if ( !ph.isAlive || ph == null || notInSensorRange(player.transform.position))
                continue;
            curWeight = calculateWeight(player.transform.position, getPlayerDamage(player));
            if (curWeight > maxWeight) //权重最大的感知对象会被返回
            {
                maxWeight = curWeight;
                nearbyPlayer = player.transform;
            }
        }
    }
    //获得当前缓存的附近玩家对象，如果附近没有玩家则返回null
    public Transform getNearbyPlayer()
    {
        return nearbyPlayer;
    }
    //从缓存列表中获取玩家造成的伤害
    public int getPlayerDamage(GameObject player)
    {
        var id = player.GetPhotonView().ownerId;
        if (damageMap.ContainsKey(id))
            return damageMap[id];
        else
            return 0;
    }
    //由其他函数调用 当造成伤害时进行统计
    public void addPlayerDamageRecord(PhotonPlayer player, int damage)
    {
        if (damageMap.ContainsKey(player.ID))
            damageMap[player.ID] += damage;
        else
            damageMap.Add(player.ID,damage);
    }
}
