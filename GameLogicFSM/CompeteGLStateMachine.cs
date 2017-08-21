using UnityEngine;
using System.Collections;

public class CompeteGLStateMachine : BaseGLStateMachine {
    
    const double competeDuration = 300; //状态机持续时间
    private string winTeam;//胜利队伍
    private bool endFlag = false;//结束标记
    private bool timeoutFlag = false;//倒计时结束标记
    
    public CompeteGLStateMachine()
    {
    }
    //进入状态机
    public override void Enter()
    {
        GMInstance.UIController.updateTeamScore(GMInstance.currentScoreOfAttackerTeam, GMInstance.currentScoreOfDefenderTeam);
        GMInstance.UIController.setTargetScore(GMInstance.targetScore);
        AudioSource.PlayClipAtPoint(GMInstance.gameStartAudio, GMInstance.localPlayer.transform.position);	//播放游戏开始音效
        if (PhotonNetwork.isMasterClient)
        {
            GMInstance.InitCountDown(PhotonNetwork.time + competeDuration);
        }
    }
    public override void Init()
    {
    }
    //退出状态机
    public override void Exit()
    {
        GMInstance.gameStateQueue.Enqueue("end");

    }
    //每帧更新
    public override void Update()
    {
        base.Update();
        GMInstance.UIController.updateTimeLabel((int)m_countDown);
        GMInstance.UIController.updatePlayerHPLabel(GMInstance.playerHealth.currentHP);
#if (!UNITY_ANDROID)
        GMInstance.UIController.scorePanelEnable(Input.GetKey(KeyCode.Tab));
        if (GMInstance.lockCursor)
            GMInstance.UIController.InternalLockUpdate();
#endif
        if (PhotonNetwork.isMasterClient) //只有主节点才执行逻辑
        {
            if (timeoutFlag)//时间结束时
            {
                if (GMInstance.currentScoreOfAttackerTeam > GMInstance.currentScoreOfDefenderTeam) //根据已获得的分数判定胜负
                    winTeam = "AttackerTeam";
                else if (GMInstance.currentScoreOfAttackerTeam < GMInstance.currentScoreOfDefenderTeam)
                    winTeam = "DefenderTeam";
                else
                    winTeam = "Tie";
                endFlag = true;
            }
            else
            {
                if (GMInstance.currentScoreOfDefenderTeam == GMInstance.targetScore)//任意一方达到规定分数
                {
                    endFlag = true;
                    winTeam = "DefenderTeam";
                }
                else if (GMInstance.currentScoreOfAttackerTeam == GMInstance.targetScore)
                {
                    endFlag = true;
                    winTeam = "AttackerTeam";
                }
            }
            if(winTeam != null)
                GMInstance.photonView.RPC("notifyGameResult", PhotonTargets.All, winTeam); //通知游戏结果
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
