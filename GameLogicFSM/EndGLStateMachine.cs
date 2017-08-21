using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.Json;
using PlayFab.ClientModels;
using System.Collections.Generic;
public class EndGLStateMachine : BaseGLStateMachine
{
    GameResult result;
    const double finishDuration = 10;
    private bool countDownFinishFlag; 
    string [] resultText = {"得分榜","战 败","平 局","胜 利"};
    public List<AudioClip> audioList = new List<AudioClip>(); //声音列表
    
    public EndGLStateMachine()
    {
    }
    //进入状态机
    public override void Enter()
    {
        result = GMInstance.m_gameResult;
        GMInstance.UIController.scorePanelEnable(true);		//游戏结束后，显示玩家得分榜
        GMInstance.UIController.setGameResultText(resultText[(int)result]); //设置结局文本
        GMInstance.localPlayer.GetComponent<PlayerMove>().enabled = false;  //使控制失效
        GMInstance.localPlayer.GetComponent<PlayerShoot>().enabled = false;  //使控制失效
        AudioSource.PlayClipAtPoint(audioList[(int)result-1], GMInstance.localPlayer.transform.position);	//播放结束音效
        GMInstance.UIController.ReleaseCursorLock();
        this.startCountDown(finishDuration + PhotonNetwork.time);
        AddUserCurrency((int)result); //安排枚举值排列 使其和原权重相对应
        AddUserExp((int)result);
        SetPlayFabUserData(); //设置玩家相关数据
        SetPlayFabUserStats();
    }
    //状态机初始化
    public override void Init()
    {
        countDownFinishFlag = false;
        audioList.Add(GMInstance.gameLoseAudio);
        audioList.Add(GMInstance.gameTieAudio);
        audioList.Add(GMInstance.gameWinAudio);
    }
    //退出状态机
    public override void Exit()
    {
        GMInstance.LeaveRoom(); //退出房间
    }
    //每帧更新
    public override void Update()
    {
        base.Update();
        GMInstance.UIController.updateTimeLabel((int)m_countDown); //更新相关的UI
        GMInstance.UIController.updateTimeLabelForProcess((int)m_countDown);
        GMInstance.UIController.updatePlayerHPLabel(GMInstance.playerHealth.currentHP);
    }
    public override void onCountDownFinish()
    {
        countDownFinishFlag = true;
    }
    public override bool checkEndCondition()
    {
        return countDownFinishFlag;
    }
    //添加经验值
    void AddUserExp(int resultWeight)
    {
        var playerCustomProperties = PhotonNetwork.player.customProperties;
        int expAmount = 20 + 50 * resultWeight + 10 * (int)playerCustomProperties["Score"] - 1 * (int)playerCustomProperties["Death"];
        Dictionary<string, string> skillData = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(GameInfo.titleData["ExpAndMoneySkill"]);
        expAmount = (int)((float)expAmount * (1.0f + float.Parse(skillData["Level" + PlayFabUserData.expAndMoneySkillLV.ToString()]) / 100));
        GMInstance.UIController.showExpReward(expAmount);
        PlayFabUserData.exp += expAmount;
        if (PlayFabUserData.exp >= GameInfo.levelExps[PlayFabUserData.lv] && GameInfo.levelExps[PlayFabUserData.lv] != -1)
        {
            PlayFabUserData.exp -= GameInfo.levelExps[PlayFabUserData.lv];
            PlayFabUserData.lv++;
        }
    }
    //添加金币
    void AddUserCurrency(int resultWeight)
    {
        var playerCustomProperties = PhotonNetwork.player.customProperties;
        int currencyAmount = 20 + 200 * resultWeight + 10 * (int)playerCustomProperties["Score"] - 1 * (int)playerCustomProperties["Death"];
        Dictionary<string, string> skillData = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(GameInfo.titleData["ExpAndMoneySkill"]);
        currencyAmount = (int)((float)currencyAmount * (1.0f + float.Parse(skillData["Level" + PlayFabUserData.expAndMoneySkillLV.ToString()]) / 100));
        GMInstance.UIController.showCurrencyReward(currencyAmount);
        AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "GC",
            Amount = currencyAmount
        };
        PlayFabClientAPI.AddUserVirtualCurrency(request, (result) => {  }, (error) => { });
    }
    //结算玩家数据
    void SetPlayFabUserData()
    {
        UpdateUserDataRequest request = new UpdateUserDataRequest();
        request.Data = new Dictionary<string, string>();

        request.Data.Add("LV", PlayFabUserData.lv.ToString());
        request.Data.Add("Exp", PlayFabUserData.exp.ToString());
        int killCount = (int)PhotonNetwork.player.customProperties["Score"];
        PlayFabUserData.totalKill += killCount;
        request.Data.Add("TotalKill", PlayFabUserData.totalKill.ToString());
        int deathCount = (int)PhotonNetwork.player.customProperties["Death"];
        PlayFabUserData.totalDeath += deathCount;
        request.Data.Add("TotalDeath", PlayFabUserData.totalDeath.ToString());

        if (PlayFabUserData.totalDeath == 0)
            PlayFabUserData.killPerDeath = (float)PlayFabUserData.totalKill * 100.0f;
        else
            PlayFabUserData.killPerDeath = PlayFabUserData.totalKill * 100.0f / PlayFabUserData.totalDeath;
		
        if (result == GameResult.win)
        {
            PlayFabUserData.totalWin++;
            request.Data.Add("TotalWin", PlayFabUserData.totalDeath.ToString());
        }
        PlayFabUserData.totalGame++;
        request.Data.Add("TotalGame", PlayFabUserData.totalDeath.ToString());

        if (PlayFabUserData.totalGame == 0)
            PlayFabUserData.winPercentage = 0.0f;
        else
            PlayFabUserData.winPercentage = PlayFabUserData.totalWin * 100.0f / PlayFabUserData.totalGame;
        PlayFabClientAPI.UpdateUserData(request, (result1) => { }, (error) => { });
    }
    //更新玩家统计数据 例如杀敌总数 总胜场数
    void SetPlayFabUserStats()
    {
        List<StatisticUpdate> stats = new List<StatisticUpdate>();
        stats.Add(new StatisticUpdate() { StatisticName = "TotalKill", Value = PlayFabUserData.totalKill });
        stats.Add(new StatisticUpdate() { StatisticName = "KillPerDeath", Value = (int)(PlayFabUserData.killPerDeath * 100) });
        stats.Add(new StatisticUpdate() { StatisticName = "TotalWin", Value = PlayFabUserData.totalWin });
        stats.Add(new StatisticUpdate() { StatisticName = "WinPercentage", Value = (int)(PlayFabUserData.winPercentage * 100) });
        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest
        {
            Statistics = stats
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, (result) => { }, (error) => { });
    }

}
