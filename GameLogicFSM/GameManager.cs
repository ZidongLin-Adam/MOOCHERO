/*
 * 
 * 描述：游戏管理器 管理游戏流程以及UI
 * 
 */
using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;
using UnityEngine.UI;
public enum GameResult { none, lose, tie, win }; //游戏结局枚举，分别代表无意义（初始化），失败，平手，胜利
public class GameManager : PunBehaviour
{
    //所有的状态机实例表 可以通过名字查找
    [HideInInspector]public Dictionary<string, BaseGLStateMachine> stateMachineMap;
    //游戏状态执行队列 队列内的状态实例会依次执行（在不清空的前提下）
    [HideInInspector]public Queue<string> gameStateQueue;

    public static GameManager gm;
    
    //下面两个属性可以通过在Inspector视图中指定状态机名
    //在inspector视图中添加所有状态机名
    public List<string> AllStateMachineName = new List<string>();
    //在inspector试图中添加初始化的状态机名
    public List<string> InitStateMachineList = new List<string>();

    public BaseGLStateMachine curGLStatetMachine; //当前执行的状态机


    public float spawnTime = 5.0f;					//重生时间

    public int loadedPlayerNum; //已读取完毕的玩家数量
    public GameResult m_gameResult; //游戏结果

    public int currentScoreOfAttackerTeam = 0;//进攻者阵营得分
    public int currentScoreOfDefenderTeam = 0;	//防守者阵营得分

    public Transform[] attackerTeamSpawnTransform;		//进攻阵营出生位置
    public Transform[] defenderTeamSpawnTransform;		//防守阵营出生位置

    public int targetScore = 2;	//目标分数

    public GameUIController UIController; //游戏场景UI控制器

    public AudioClip gameStartAudio;			//游戏开始音效
    public AudioClip gameLoseAudio;
    public AudioClip gameTieAudio;
    public AudioClip gameWinAudio;


    public bool lockCursor = true;      //是否在游戏进行时锁定鼠标
    Camera mainCamera; //主摄像机
    public GameObject localPlayer = null; //本地玩家
    ExitGames.Client.Photon.Hashtable playerCustomProperties;//玩家属性
    public PlayerHealth playerHealth;
    [HideInInspector]
    public double remainTime;
    [HideInInspector]
    public string winTeam;
    
    private ZombieGenerator m_zombieGenerator;//生成器实例
    public ZombieGenerator zombieGenerator  //生成器访问器
    {
        get {
                return m_zombieGenerator;
        }
    }
    /*
     *添加骷髅兵必要组件 完成并完成生成池对骷髅兵调度
     *模仿僵尸组件添加骷髅并组件  模仿僵尸生成池机制对骷髅兵进行调度
     */
    private ZombieGenerator m_skeletonGenerator;//生成器实例
    public ZombieGenerator skeletonGenerator  //生成器访问器
    {
        get
        {
            return m_skeletonGenerator;
        }
    }



    [PunRPC]
    void ConfirmLoad() //玩家读取完毕时调用
    {
        loadedPlayerNum++;
    }
    //初始化玩家实例
    public void InstantiatePlayer()
    {
        var playerCustomProperties = PhotonNetwork.player.customProperties;	//获取玩家自定义属性
        //如果玩家属于进攻阵营，生成AttackerPlayer对象
        if (playerCustomProperties["Team"].ToString().Equals("AttackerTeam"))
        {
            localPlayer = PhotonNetwork.Instantiate("AttackerPlayer",
                attackerTeamSpawnTransform[(int)playerCustomProperties["TeamNum"]].position, Quaternion.identity, 0);
        }
        //如果玩家属于防守阵营，生成DefenderPlayer对象
        else if (PhotonNetwork.player.customProperties["Team"].ToString().Equals("DefenderTeam"))
        {
            localPlayer = PhotonNetwork.Instantiate("DefenderPlayer",
                defenderTeamSpawnTransform[(int)playerCustomProperties["TeamNum"]].position, Quaternion.identity, 0);
        }
        localPlayer.GetComponent<PlayerMove>().enabled = true;					//启用PlayerMove脚本，使玩家对象可以被本地客户端操控
        PlayerShoot playerShoot = localPlayer.GetComponent<PlayerShoot>();		//获取玩家对象的PlayerShoot脚本
        playerHealth = localPlayer.GetComponent<PlayerHealth>();				//获取玩家对象的PlayerHealth脚本

        Transform tempTransform = localPlayer.transform;
        mainCamera.transform.parent = tempTransform;							//将场景中的摄像机设为玩家对象的子对象
        mainCamera.transform.localPosition = playerShoot.shootingPosition;		//设置摄像机的位置，为PlayerShoot脚本中的射击起始位置
        mainCamera.transform.localRotation = Quaternion.identity;				//设置摄像机的朝向

        UIController.initPlayerHpLabel(playerHealth.maxHP, 0);
        UIController.updatePlayerHPLabel(playerHealth.currentHP);
    }
    //状态机发生变化时调用
    [PunRPC]
    void stateMachineChange()
    {
        if (curGLStatetMachine != null){
            gameStateQueue.Dequeue();  //当前状态机移除队列 并执行exit函数
            curGLStatetMachine.Exit();
        }
        if (gameStateQueue.Count == 0)
            return;
        curGLStatetMachine = stateMachineMap[gameStateQueue.Peek()]; //从队列中找到新的状态机
        if (curGLStatetMachine != null)
            curGLStatetMachine.Enter();//执行enter函数
    }
    //新增 强制状态机转换 会清除之前的状态机队列
    [PunRPC]
    void stateMachineChangeForce(string stateMachineName)
    {
        if (stateMachineName == null || stateMachineName == "")
            return;
        gameStateQueue.Clear(); //清空队列
        if (curGLStatetMachine != null)
            curGLStatetMachine.Exit();
        curGLStatetMachine = stateMachineMap[stateMachineName];
        if (curGLStatetMachine != null)
            curGLStatetMachine.Enter();//强制执行新的状态机
    }
    //新增 在状态机内利用RPC调用 通知战斗结果
    [PunRPC]
    void notifyGameResult(string winsTeam)
    {
        winTeam = winsTeam;
        if (winsTeam == PhotonNetwork.player.customProperties["Team"].ToString())
            m_gameResult = GameResult.win;
        else if (winsTeam == "Tie")
            m_gameResult = GameResult.tie;
        else
            m_gameResult = GameResult.lose;
    }
    //启动时调用
	void Start () {
        UIController = GetComponent<GameUIController>();
        gm = GetComponent<GameManager>();		//初始化GameManager静态实例gm
        mainCamera = Camera.main;				//获取摄像机
        stateMachineMap = new Dictionary<string, BaseGLStateMachine>();
        gameStateQueue = new Queue<string>();

        playerCustomProperties = new ExitGames.Client.Photon.Hashtable { { "Score", 0 }, { "Death", 0 } };  //初始化玩家得分

        PhotonNetwork.player.SetCustomProperties(playerCustomProperties);

        //初始化队伍得分
        currentScoreOfAttackerTeam = 0;
        currentScoreOfDefenderTeam = 0;

        //初始化状态机映射和队列
        initStateMachineMapAndQueue();
        //将队列首的状态机设置为当前状态机
        curGLStatetMachine = stateMachineMap[gameStateQueue.Peek()];
        curGLStatetMachine.Enter();
        m_gameResult = GameResult.none;
        winTeam = null;
#if (!UNITY_ANDROID)
        UIController.setCursorState(lockCursor);
#endif
	}
    //根据Inspector视图中指定的全部状态机以及初始状态机列表生成实际游戏所需状态机
    private void initStateMachineMapAndQueue()
    {
        //将Inspector中设置的值写入GameManager中
        foreach (var name in AllStateMachineName)
        {
            var instance = GLStateMachineGenerator.getGLStateMachineByName(name, this);
            if (instance != null)
                stateMachineMap.Add(name, instance);
        }
        foreach (var name in InitStateMachineList)
        {
            gameStateQueue.Enqueue(name);
        }
        AllStateMachineName.Clear();
        InitStateMachineList.Clear();
    }
    //初始化怪物生成池
    public void initMonsterGenerator()
    {
        GameObject ob;
        if (PhotonNetwork.isMasterClient)
        {
            ob = PhotonNetwork.InstantiateSceneObject("ZombieGenerator", Vector3.zero, Quaternion.identity, 0, null);
            m_zombieGenerator = ob.GetComponent<ZombieGenerator>();
            m_zombieGenerator.initGenerator();
            photonView.RPC("getGeneratorId", PhotonTargets.All, ob.GetPhotonView().viewID, "Zombie");

            //添加骷髅兵 完成并完成生成池对骷髅兵调度（模仿僵尸生成池）
            ob = PhotonNetwork.InstantiateSceneObject("SkeletonGenerator", Vector3.zero, Quaternion.identity, 0, null);
            m_skeletonGenerator = ob.GetComponent<ZombieGenerator>();
            m_skeletonGenerator.initGenerator();
            photonView.RPC("getGeneratorId", PhotonTargets.All, ob.GetPhotonView().viewID, "Skeleton");
        }
    }
    [PunRPC]
    void getGeneratorId(int id,string type)//通过PhotonView的ID使得非主节点获取到生成池实例
    {
        if (PhotonNetwork.isMasterClient)
            return;
        var ob = PhotonView.Find(id);
        if (type == "Zombie")
            this.m_zombieGenerator = ob.GetComponent<ZombieGenerator>();
        else if (type == "Skeleton")
        {
            this.m_skeletonGenerator = ob.GetComponent<ZombieGenerator>();
        }
        //添加骷髅兵 完成并完成生成池对骷髅兵调度（模仿僵尸生成池）
    }
	//每帧执行 主要是执行状态机的update函数 并对状态机是否达到结束状态进行检测 结束的话则需要进行状态机变更
	void Update () {
		UIController.SetPintText (PhotonNetwork.GetPing().ToString());
        if (curGLStatetMachine != null)
        {
            curGLStatetMachine.Update();
            if (PhotonNetwork.isMasterClient && curGLStatetMachine.checkEndCondition())
            {
                this.photonView.RPC("stateMachineChange", PhotonTargets.All);
            }
        }
	}
    //检测队伍人数
    void CheckTeamNumber()
    {
        PhotonPlayer[] players = PhotonNetwork.playerList;		//获取房间内玩家列表
        int attackerNum = 0, defenderNum = 0;
        foreach (PhotonPlayer p in players)
        {					//遍历所有玩家，计算两队人数
            if (p.customProperties["Team"].ToString() == "AttackerTeam")
                attackerNum++;
            else
                defenderNum++;
        }
        if (attackerNum == 0 || defenderNum == 0)
        {
            if (attackerNum == 0)
                this.photonView.RPC("notifyGameResult", PhotonTargets.All, "DefenderTeam");
            else
                this.photonView.RPC("notifyGameResult", PhotonTargets.All, "AttackerTeam");

            this.photonView.RPC("stateMachineChangeForce", PhotonTargets.All, "end"); //强制使游戏进入结算状态
        }
    }
    //玩家失去连接执行函数
    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        if (PhotonNetwork.isMasterClient && getCurStateName() != "end")
        {	//MasterClient检查
            CheckTeamNumber();				//检查两队人数
        }
    }
    //更新死亡数
    public void UpdateDeath()
    {
        int death = (int)PhotonNetwork.player.customProperties["Death"];
        death += 1;
        var playerCustomProperties = new ExitGames.Client.Photon.Hashtable { { "Death", death } };
        PhotonNetwork.player.SetCustomProperties(playerCustomProperties);
    }
    /**玩家得分增加函数
     * 该函数只由MasterClient调用
     */
    public void AddScore(int killScore, PhotonPlayer p)
    {
        if (!PhotonNetwork.isMasterClient)		//如果函数不是由MasterClient调用，结束函数执行
            return;
        int score = (int)p.customProperties["Score"];		//获取击杀者玩家得分
        score += killScore;									//增加击杀者玩家得分
        var playerCustomProperties = new ExitGames.Client.Photon.Hashtable { { "Score", score } };
        p.SetCustomProperties(playerCustomProperties);
        if (p.customProperties["Team"].ToString() == "AttackerTeam")
        {
            currentScoreOfAttackerTeam += killScore;       //增加进攻阵营总分
        }
        else
        {
            currentScoreOfDefenderTeam += killScore;       //增加防守阵营总分
        }
        photonView.RPC("UpdateScores", PhotonTargets.All, currentScoreOfAttackerTeam, currentScoreOfDefenderTeam);
    }
    //更细分数
    [PunRPC]
    void UpdateScores(int attackerTeamScore, int defenderTeamScore)
    {
        UIController.updateTeamScore(attackerTeamScore, defenderTeamScore);
    }

    //玩家属性更新函数
    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        UIController.teamScoreListEnable("teamA", false);
        UIController.teamScoreListEnable("teamB", false);

        PhotonPlayer[] players = PhotonNetwork.playerList;  //获取房间内所有玩家的信息
        List<PlayerInfo> attackerTeam = new List<PlayerInfo>();
        List<PlayerInfo> defenderTeam = new List<PlayerInfo>();
        PlayerInfo tempPlayer;
        //遍历房间内所有玩家，将他们的得分根据他们的队伍放入对应的队伍列表中
        foreach (PhotonPlayer p in players)
        {
            tempPlayer = new PlayerInfo(p.name, (int)p.customProperties["Score"], (int)p.customProperties["Death"]);
            if (p.customProperties["Team"].ToString() == "AttackerTeam")
                attackerTeam.Add(tempPlayer);
            else
                defenderTeam.Add(tempPlayer);
        }
        //分别对两队队伍列表排序，按照分数从大到小排序
        attackerTeam.Sort();
        defenderTeam.Sort();
        UIController.updateTeamScoreListData(attackerTeam, defenderTeam);
    }
    //本地玩家加血
    public void localPlayerAddHealth(int points)
    {
        PlayerHealth ph = localPlayer.GetComponent<PlayerHealth>();
        ph.requestAddHP(points);
    }
    //如果玩家断开与Photon服务器的连接，加载场景GameLobby
    public override void OnConnectionFail(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("GameLobby");
    }
    //显示玩家得分榜，用于移动端的得分榜按钮，代替PC的Tab键
    public void ShowScorePanel()
    {
    }
    //离开房间函数
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();				//玩家离开游戏房间
        PhotonNetwork.LoadLevel("GameLobby");	//加载场景GameLobby
    }
    //获取当前状态机名字
    public string getCurStateName()
    {
        return GLStateMachineGenerator.getNameByGLStateMachine(curGLStatetMachine);
    }
    //获取本机器的玩家实例
    public GameObject getLocalPlayer()
    {
        return localPlayer;
    }
    //通过RPC设置倒计时
    public void InitCountDown(double endTime)
    {
        if (PhotonNetwork.isMasterClient)
        {
            photonView.RPC("StartCountDown", PhotonTargets.AllViaServer, endTime);
        }
    }
    //执行状态机中的开始倒计时函数
    [PunRPC]
    void StartCountDown(double endTime)
    {
        curGLStatetMachine.startCountDown(endTime);
    }
}
