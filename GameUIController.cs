using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
public class GameUIController :MonoBehaviour {

    public Text timeLabel;						//倒计时时间显示
    public Text targetScoreLabel;				//目标分数显示

    public GameObject scorePanel;				//玩家得分榜
    public Text teamATotal;					    //盗墓阵营总分
    public Text teamBTotal;					    //保卫阵营总分
    public GameObject[] teamAScorePanelList;	//盗墓阵营成员得分面板
    public GameObject[] teamBScorePanelList;	//保卫阵营成员得分面板
    public Text gameResultInScorePanel;			//游戏结束信息
    public Text mapNameInScorePanel;            //游戏地图
    public GameObject currencyReward;           //金币奖励
    public GameObject expReward;                //经验值奖励
    public Slider hpSlider;						//玩家血条hp
    public Text hpValue;                        //玩家血量
    public Image hurtImage;                     //玩家受伤屏幕泛红
    
    public Slider enemyHpSlider;                //敌方血量相关
    public Text enemyHpValue;
    public GameObject enemyHpObject;

    public Slider guardHpSlider;                //Boss血量相关
    public GameObject guardHpObject;

    public GameObject remainDistanceObject;     //推车剩余距离UI相关
    public Text remainText;
    public Slider remainSlider;

    public GameObject timeAndProcessPanel;
    public Text processHintText;
    public Text timeLabelForProcess;

    public GameObject hintText;                 //任务提示信息
    

    public GameObject crossIcon;                //瞄准点
    private Image crossImage;                   

    private Coroutine coroutineForTextFlash;
    private Coroutine coroutineForDamageFlash;
    
    //Ping
	public Text pingText;

    private bool m_cursorIsLocked;
    public void setCursorState(bool locked)
    {
        this.m_cursorIsLocked = locked;
    }
    //初始化UI面板
    public void initUIPanel()
    {
        this.setGameResultText("得分榜");
        this.setGameMapName("游戏地图:"+PhotonNetwork.room.customProperties["MapName"].ToString());
        currencyReward.SetActive(false);
        expReward.SetActive(false);
        scorePanel.SetActive(false);
        enemyHpObject.SetActive(false);
        crossImage = crossIcon.GetComponent<Image>();
    }
    //初始化玩家hpUI
    public void initPlayerHpLabel(int maxHp, int minHp)
    {
        hpSlider.maxValue = maxHp;
        hpSlider.minValue = minHp;

        enemyHpSlider.maxValue = 0;
        enemyHpSlider.minValue = 0;
        if (guardHpSlider != null)
        {
            guardHpSlider.maxValue = 0;
            guardHpSlider.minValue = 0;
        }
    }
    //更新队伍分数
    public void updateTeamScore(int teamAScore, int teamBScore)
    {
        teamATotal.text = teamAScore.ToString();
        teamBTotal.text = teamBScore.ToString();
    }
    //设置游戏结算结果
    public void setGameResultText(string gameResult)
    {
        gameResultInScorePanel.text = gameResult;
    }
    //设置游戏地图名字
    public void setGameMapName(string mapName)
    {
        mapNameInScorePanel.text = mapName;
    }
    //设置目标分数
    public void setTargetScore(int score)
    {
        targetScoreLabel.text = score.ToString();
    }
    //更新倒计时面板
    public void updateTimeLabel(int timeLeft)
    {
        if (timeLeft < 0) timeLeft = 0;
        int minute = timeLeft / 60;
        int second = timeLeft % 60;
        timeLabel.text = minute.ToString("00") + ":" + second.ToString("00");
    }
    //更细玩家hpUI
    public void updatePlayerHPLabel(int hp)
    {
        hpSlider.value = hp;
        hpValue.text = hp.ToString();
    }
    //设置结算分数面板使能
    public void scorePanelEnable(bool enable)
    {
        scorePanel.SetActive(enable);
    }
    //设置分数结算面板中队伍列表使能
    public void teamScoreListEnable(string type, bool enable)
    {
        if (type == "teamA"){
            foreach(var go in teamAScorePanelList){
                go.SetActive(enable);
            }
        }
        else{
            foreach (var go in teamBScorePanelList){
                go.SetActive(enable);
            }
        }
    }
    //更新得分榜数据
    public void updateTeamScoreListData(List<PlayerInfo> teamAdata, List<PlayerInfo> teamBdata)
    {
        Text[] texts;
        int length = teamAdata.Count;
        //依次在玩家得分榜显示两队玩家得分，保证得分高的玩家在得分低的玩家之上
        for (int i = 0; i < length; i++)
        {
            texts = teamAScorePanelList[i].GetComponentsInChildren<Text>();
            texts[0].text = teamAdata[i].playerName;
            texts[1].text = teamAdata[i].playerScore.ToString() + "/" + teamAdata[i].playerDeath.ToString();
            teamAScorePanelList[i].SetActive(true);
        }
        length = teamBdata.Count;
        for (int i = 0; i < length; i++)
        {
            texts = teamBScorePanelList[i].GetComponentsInChildren<Text>();
            texts[0].text = teamBdata[i].playerName;
            texts[1].text = teamBdata[i].playerScore.ToString() + "/" + teamBdata[i].playerDeath.ToString();
            teamBScorePanelList[i].SetActive(true);
        }

    }
    //显示分数结算板
    public void ShowScorePanel()
    {
        scorePanel.SetActive (!scorePanel.activeSelf);
    }
    //显示经验值奖励
    public void showExpReward(int value)
    {
        expReward.GetComponentInChildren<Text>().text = "+" + value.ToString();
        expReward.SetActive(true);
    }
    //显示货币奖励
    public void showCurrencyReward(int value)
    {
        currencyReward.GetComponentInChildren<Text>().text = "+" + value.ToString();
        currencyReward.SetActive(true);
    }
    //更新鼠标锁定状态
    public void InternalLockUpdate()
    {

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_cursorIsLocked = true;
        }

        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    //显示鼠标
    public void ReleaseCursorLock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    //更新敌人血条
    public void updateEnemyHpUI(int curHp, int maxHp)
    {
        enemyHpSlider.maxValue = maxHp;
        enemyHpSlider.value = curHp;
        enemyHpValue.text = curHp.ToString();
    }
    //敌人血条使能
    public void EnableEnemyHpLabel(bool enable)
    {
        enemyHpObject.SetActive(enable);
    }
    //设置boos最大生命值
    public void setGuardMaxHp(int maxHp)
    {
        guardHpSlider.maxValue = maxHp;
    }
    //更新boss血条
    public void updateGuardHpUI(int curHp)
    {
        guardHpSlider.value = curHp;
    }
    //boos血条使能
    public void enableGuardHPUI(bool enable)
    {
        guardHpObject.SetActive(enable);
    }
    //剩余距离显示块使能
    public void enableRemainDistance(bool enable)
    {
        remainDistanceObject.SetActive(enable);
    }
    //设置推车的最大距离
    public void setMaxRemainDistance(float dis)
    {
        remainSlider.maxValue = dis;
        remainSlider.minValue = 0;
    }
    //设置剩余距离UI 保留两位小数
    public void updateRemainDistanceUI(float dis)
    {
        remainText.text = dis.ToString("f2") + "m";
        remainSlider.value = remainSlider.maxValue - dis;
    }
    public void enableTimeAndProcessPanel(bool enable)
    {
        timeAndProcessPanel.SetActive(enable);
    }
    public void updateTimeLabelForProcess(int timeLeft)
    {
        if (timeLabelForProcess == null)
            return;
        if (timeLeft < 0) timeLeft = 0;
        int minute = timeLeft / 60;
        int second = timeLeft % 60;
        timeLabelForProcess.text = minute.ToString("00") + ":" + second.ToString("00");
    }
    //更新游戏进度提示信息
    public void updateProcessText(string content)
    {
        processHintText.text = content;
    }
    //展示闪烁的提示信息
    public void showHintText(string content)
    {
        hintText.SetActive(true);
        hintText.GetComponent<Text>().text = content;
        if (coroutineForTextFlash != null)
        {
            StopCoroutine(coroutineForTextFlash);
            coroutineForTextFlash = null;
        }
        coroutineForTextFlash = StartCoroutine(hintTextFlash());
    }
    //展示玩家被伤害时的效果
    public void showDamageFlash()
    {
        if (coroutineForDamageFlash != null)
        {
            StopCoroutine(coroutineForDamageFlash);
            coroutineForDamageFlash = null;
        }
        coroutineForDamageFlash = StartCoroutine(damageHintFlash());
    }
    IEnumerator hintTextFlash()
    {
        hintText.SetActive(false);
        float timer = 0.0f;
        while(timer < 3.0f){
            timer += 0.5f;
            hintText.SetActive(!hintText.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
        hintText.SetActive(false);
        coroutineForTextFlash = null;
        yield break;
    }
    IEnumerator damageHintFlash()
    {
        var color0 = new Color(1, 0, 0, 0.2f);
        var color1 = new Color(1, 0, 0, 0);
        hurtImage.color = color0;
        float timer = 0.0f;
        while (timer < 0.5f)
        {
            timer += 0.1f;
            hurtImage.color = Color.Lerp(color0, color1, timer * 2);
            yield return new WaitForSeconds(0.1f);
        }
        hurtImage.color = color1;
        coroutineForDamageFlash = null;
        yield break;
    }
    //设置延迟显示UI
	public void SetPintText(string ping){
		pingText.text = "Ping:" + ping;
	}
    //设置准心颜色
    public void updateCrossIconColor(Color color)
    {
        if(crossImage)
            crossImage.color = color;
    }
}
