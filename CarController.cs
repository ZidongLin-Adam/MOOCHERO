using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;
public class CarController : PunBehaviour {

    public SplineTrailRenderer trailReference;      //宝物车路径  
    public float speed = 2.0f;                      //宝物车基础速度
    private float moveSpeed = 1.0f;                 //宝物车实际移动速度
    const float rotateRatio = 120.0f;               //宝物车轮子滚动速度
    private NavMeshAgent agent;			            //导航代理组件
    public CarFSMState curState;

    public Transform[] wheelMeshes;

    //private bool isMove = false;
    [HideInInspector]
    public float distance;

    private HashSet<int> nearbyAttackerPlayerSet = new HashSet<int>(); //宝物车附近进攻队伍玩家集合
    private HashSet<int> nearbyDefenderPlayerSet = new HashSet<int>();//宝物车附近防守队伍玩家集合

    private List<int> waitForDelete = new List<int>();      //检测无效玩家时的待删除列表
    
    private float checkInvalidPlayerTimer = 0.0f;           //检测无效玩家计时器
    private float checkInvalidPlayerInterval = 0.1f;        //检测无效玩家时间间隔
    private Vector3 offset = new Vector3(0.0f, -0.15f, 0.0f);
    
    /// <summary>
    /// 宝物车状态机
    /// Diasable：宝物车未启动
    /// Prepare：宝物车从墓穴运动到指定位置
    /// Stop：宝物车在推送过程中处于停滞状态
    /// Running：宝物车在推送过程中处于移动状态
    /// Finish：宝物车到达目的地
    /// </summary>
    public enum CarFSMState
    {
        Disable, Prepare, Stop, Running, Finish
    }

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        curState = CarFSMState.Disable;
        if (agent != null) agent.enabled = false;
        distance = 0.0f;
    }

    void Update()
    {
        //MasterClient控制宝物车状态
        if (PhotonNetwork.isMasterClient)
        {
            FSMUpdate();
            checkInvalidPlayerTimer += Time.deltaTime;
            //每隔一段时间检测宝物车附近的玩家是否在线
            if (checkInvalidPlayerTimer > checkInvalidPlayerInterval)
            {
                validateNearbyPlayer();
                checkInvalidPlayerTimer = 0.0f;
            }
        }
        //宝物车运作时，使宝物车轮子滚动起来
        if (curState == CarFSMState.Prepare || curState == CarFSMState.Running)
        {
            UpdateWheelsRotations();
        }

    }

    //宝物车状态机更新
    void FSMUpdate()
    {
        switch (curState)
        {
            case CarFSMState.Prepare:
                UpdatePrepareState();
                break;
            case CarFSMState.Running:
                UpdateRunningState();
                break;
            case CarFSMState.Stop:
                UpdateStopState();
                break;
            case CarFSMState.Finish:
                break;
        }
    }

    //使CarController开始工作
    public void StartWorking()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        curState = CarFSMState.Prepare;
    }

    //更新起始状态
    void UpdatePrepareState()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, trailReference.spline.FindPositionFromDistance(0)+offset, step);
        if (Vector3.Distance(transform.position, trailReference.spline.FindPositionFromDistance(0) + offset) <= 0.1f)
            curState = CarFSMState.Running;
    }
    //更新停止状态
    void UpdateStopState()
    {
        if (nearbyAttackerPlayerSet.Count > 0 && nearbyDefenderPlayerSet.Count <= 0)
        {
            curState = CarFSMState.Running;
        }
    }
    //更新运行状态
    void UpdateRunningState()
    {
        if (nearbyDefenderPlayerSet.Count > 0 || nearbyAttackerPlayerSet.Count <= 0)
        {
            curState = CarFSMState.Stop;
            moveSpeed = 0.0f;
        }
        else
        {
            moveSpeed = speed * nearbyAttackerPlayerSet.Count; //人越多 推的越快

            //计算宝物车已经走过的路程
            float length = trailReference.spline.Length();
            distance = Mathf.Clamp(distance + moveSpeed * Time.deltaTime, 0, length - 0.1f);

            //计算宝物车的位置和朝向
            Vector3 forward = trailReference.spline.FindTangentFromDistance(distance);
            Vector3 position = trailReference.spline.FindPositionFromDistance(distance);

            if (forward != Vector3.zero)
            {
                transform.forward = forward;
                transform.position = new Vector3(position.x,transform.position.y,position.z);
            }
            if (length - distance <= 0.1f)
            {
                curState = CarFSMState.Finish;
            }
        }
    }

    //车轮旋转
    void UpdateWheelsRotations()
    {
        for(int i = 0; i < wheelMeshes.Length; i++)
        {
            wheelMeshes[i].Rotate(0.0f, rotateRatio * Time.deltaTime * moveSpeed, 0.0f);
        }
    }

    //检测是否有玩家进入宝物车范围
    void OnTriggerEnter(Collider Other)
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        PlayerHealth playerHealth = Other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (playerHealth.team == "AttackerTeam")
                this.photonView.RPC("changeNearByPlayerInfo", PhotonTargets.All, true, true, Other.gameObject.GetPhotonView().viewID);
            else
                this.photonView.RPC("changeNearByPlayerInfo", PhotonTargets.All, false, true, Other.gameObject.GetPhotonView().viewID);
        }
    }

    //检测是否有玩家离开宝物车范围
    void OnTriggerExit(Collider Other)
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        PlayerHealth playerHealth = Other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (playerHealth.team == "AttackerTeam")
                this.photonView.RPC("changeNearByPlayerInfo", PhotonTargets.All, true, false, Other.gameObject.GetPhotonView().viewID);
            else
                this.photonView.RPC("changeNearByPlayerInfo", PhotonTargets.All, false, false, Other.gameObject.GetPhotonView().viewID);
        }
    }

    //远程更新集合状态
    //para1:是否为机器人队伍 para2 是否为插入操作 para3玩家的photonViewID
    [PunRPC]
    void changeNearByPlayerInfo(bool isAttackerTeam,bool isInsert ,int id)
    {
        if (isAttackerTeam)
        {
            if (isInsert)
                nearbyAttackerPlayerSet.Add(id);
            else
                nearbyAttackerPlayerSet.Remove(id);
        }
        else
        {
            if (isInsert)
                nearbyDefenderPlayerSet.Add(id);
            else
                nearbyDefenderPlayerSet.Remove(id);
        }
    }

    //检测周围玩家列表中是否仍然有效 例如由于死亡和掉线而产生的无效数据
    void validateNearbyPlayer()
    {
        waitForDelete.Clear();
        PhotonView view;

        foreach (var id in nearbyDefenderPlayerSet)
        {
            view = PhotonView.Find(id);
            if (view == null || !view.GetComponent<PlayerHealth>().isAlive)
                waitForDelete.Add(id);
        }
        foreach (var id in waitForDelete)
        {
            nearbyDefenderPlayerSet.Remove(id);
            this.photonView.RPC("changeNearByPlayerInfo", PhotonTargets.Others, true, false, id);
        }
        waitForDelete.Clear();
        foreach (var id in nearbyAttackerPlayerSet)
        {
            view = PhotonView.Find(id);
            if (view == null || !view.GetComponent<PlayerHealth>().isAlive)
                waitForDelete.Add(id);
        }
        foreach (var id in waitForDelete)
        {
            nearbyAttackerPlayerSet.Remove(id);
            this.photonView.RPC("changeNearByPlayerInfo", PhotonTargets.Others, false, false, id);
        }
    }

    //判断宝物车是否已经到达终点
    public bool carReachedDestination()
    {
        return curState == CarFSMState.Finish;
    }

    //同步更新宝物车的状态和数据
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting) 						//本地玩家发送数据
        {
            stream.SendNext(curState);
            stream.SendNext(moveSpeed);
            stream.SendNext(distance);
        }
        else 										//远程玩家接收数据
        {
            curState = (CarFSMState)stream.ReceiveNext();
            moveSpeed = (float)stream.ReceiveNext();
            distance = (float)stream.ReceiveNext();
        }
    }
}
