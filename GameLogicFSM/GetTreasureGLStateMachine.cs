using UnityEngine;
using System.Collections;

public class GetTreasureGLStateMachine : BaseGLStateMachine
{
    const double getTreasureDuration = 180.0; //夺宝持续时间
    private bool endFlag = false; //状态机结束标识
    private bool timeoutFlag = false; //倒计时结束标识
    private string winTeam; //胜利队伍
    private int guardHP;//守护者血量
    GameObject GuardZombie;//守护者实例

    public GetTreasureGLStateMachine(){ }
    //进入状态机
    public override void Enter()
    {
        //显示夺宝状态的相关UI
        GMInstance.UIController.enableGuardHPUI(true);  
        GMInstance.UIController.enableTimeAndProcessPanel(true);
        GMInstance.UIController.enableRemainDistance(false);
        AudioSource.PlayClipAtPoint(GMInstance.gameStartAudio, GMInstance.localPlayer.transform.position);  //播放游戏开始音效
        if (PhotonNetwork.isMasterClient){
            GMInstance.InitCountDown(PhotonNetwork.time + getTreasureDuration);
        }

        GuardZombie = GameObject.FindWithTag("Guard");
        //僵尸生成池开始工作
        GMInstance.zombieGenerator.generatorStartWorking();
        /* 学生作业：
         * 添加骷髅兵 完成并完成生成池对骷髅兵调度（模仿僵尸生成池）
         */
        //骷髅生成池开始工作
        GMInstance.skeletonGenerator.generatorStartWorking();

        GuardZombie.GetComponent<GuardAI>().Born(); //守护者开始工作
        var maxHp = GuardZombie.GetComponent<ZombieHealth>().maxHP;
        GMInstance.UIController.setGuardMaxHp(maxHp);
        var team = PhotonNetwork.player.customProperties["Team"].ToString();
        if (team == "AttackerTeam")
            GMInstance.UIController.updateProcessText("击败守护者，夺得宝物车!");
        else
            GMInstance.UIController.updateProcessText("保护守护者，阻止盗墓者入侵!");
    }
    //状态机初始化
    public override void Init()
    {
        GMInstance.initMonsterGenerator();
    }
    //状态机退出
    public override void Exit()
    {
        if (GMInstance.winTeam != null)
            GMInstance.gameStateQueue.Enqueue("end");
        else
            GMInstance.gameStateQueue.Enqueue("transport");
        //僵尸生成池停止工作
		GMInstance.zombieGenerator.generatorStopWorking();
        /* 学生作业：
         * 添加骷髅兵 完成并完成生成池对骷髅兵调度（模仿僵尸生成池）
         */
        //骷髅生成池停止工作
        GMInstance.skeletonGenerator.generatorStopWorking();

        GMInstance.UIController.enableGuardHPUI(false);

        GameManager.gm.remainTime = base.m_countDown;
    }
    //每帧更新
    public override void Update()
    {
        base.Update();
        //每帧更新守护者血量 和相关UI
        guardHP = GuardZombie.GetComponent<ZombieHealth>().currentHP;
        GMInstance.UIController.updateGuardHpUI(guardHP);
        GMInstance.UIController.updateTimeLabelForProcess((int)m_countDown);
        GMInstance.UIController.updatePlayerHPLabel(GMInstance.playerHealth.currentHP);
#if (!UNITY_ANDROID)
        GMInstance.UIController.scorePanelEnable(Input.GetKey(KeyCode.Tab));
        if (GMInstance.lockCursor)
            GMInstance.UIController.InternalLockUpdate();
#endif
        if (PhotonNetwork.isMasterClient)
        {
            if (guardHP <= 0) //guard死了 则进入下一阶段 推车
            {
                endFlag = true;
                return;
            }
            else if (timeoutFlag) //时间到了但是guard没有死 则防守方直接胜利 游戏结束
            {
                winTeam = "DefenderTeam";
                GMInstance.photonView.RPC("notifyGameResult", PhotonTargets.All, winTeam);
                endFlag = true;
                return;
            }
        }

    }
    //重写倒计时结束函数
    public override void onCountDownFinish()
    {
        timeoutFlag = true;
    }
    //返回状态机结束标记
    public override bool checkEndCondition()
    {
        return endFlag;
    }
}
