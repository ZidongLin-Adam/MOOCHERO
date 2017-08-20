using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
public class PrepareGLStateMachine : BaseGLStateMachine
{
    private bool countDownFinishFlag;//倒计时结束标记
    const double prepareDuration = 20;//准备状态机持续时间 超过这个时间 不等待尚未读取完毕的玩家 直接开始

    public PrepareGLStateMachine() { }
    //进入状态机
    public override void Enter()
    {
        //通知本机器已经读取场景完毕
        GMInstance.photonView.RPC("ConfirmLoad", PhotonTargets.All);
        this.startCountDown(prepareDuration + PhotonNetwork.time);

        GMInstance.UIController.initUIPanel();

        //请求玩家数据
        GetUserDataRequest request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnGetUserData, (error) => { Debug.Log("Error:" + error.ErrorMessage); });

    }
    //状态机初始化
    public override void Init()
    {
        countDownFinishFlag = false;
    }
    //状态机退出
    public override void Exit()
    {
        GMInstance.InstantiatePlayer(); //准备结束时初始化玩家实例
    }
    //每帧更新
    public override void Update()
    {
        base.Update();
        GMInstance.UIController.updateTimeLabel((int)m_countDown); //主要是更新UI
        GMInstance.UIController.updateTimeLabelForProcess((int)m_countDown);
    }
    //重写倒计时结束函数
    public override void onCountDownFinish()
    {
        countDownFinishFlag = true;
    }
    //返回状态机结束标记
    public override bool checkEndCondition()
    {
        return countDownFinishFlag || GMInstance.loadedPlayerNum == PhotonNetwork.playerList.Length;
    }
    //当客户端获得请求的数据
    void OnGetUserData(GetUserDataResult result)
    {

        if (result.Data.ContainsKey("TotalKill"))
            PlayFabUserData.totalKill = int.Parse(result.Data["TotalKill"].Value);
        else
            PlayFabUserData.totalKill = 0;
        if (result.Data.ContainsKey("TotalDeath"))
            PlayFabUserData.totalDeath = int.Parse(result.Data["TotalDeath"].Value);
        else
            PlayFabUserData.totalDeath = 0;
        if (PlayFabUserData.totalDeath == 0)
            PlayFabUserData.killPerDeath = (float)PlayFabUserData.totalKill * 100.0f;
        else
            PlayFabUserData.killPerDeath = PlayFabUserData.totalKill * 100.0f / PlayFabUserData.totalDeath;

        if (result.Data.ContainsKey("TotalWin"))
            PlayFabUserData.totalWin = int.Parse(result.Data["TotalWin"].Value);
        else
            PlayFabUserData.totalWin = 0;
        if (result.Data.ContainsKey("TotalGame"))
            PlayFabUserData.totalGame = int.Parse(result.Data["TotalGame"].Value);
        else
            PlayFabUserData.totalGame = 0;
        if (PlayFabUserData.totalGame == 0)
            PlayFabUserData.winPercentage = 0.0f;
        else
            PlayFabUserData.winPercentage = PlayFabUserData.totalWin * 100.0f / PlayFabUserData.totalGame;
    }

}
