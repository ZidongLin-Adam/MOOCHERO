using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine.UI;
public class GameManager : PunBehaviour {
	public static GameManager gm;		

	public enum GameState{		
		PreStart,			
		Playing,			
		GameWin,			
		GameLose,			
		Tie};			
	public GameState state = GameState.PreStart;

	public float checkplayerTime = 5.0f;		
	public float gamePlayingTime = 600.0f;	
	public float gameOverTime = 10.0f;		
	public float spawnTime = 5.0f;				
	public int targetScore = 50;				

	void Start () {
		gm = GetComponent<GameManager> ();	
		mainCamera = Camera.main;			
		photonView.RPC ("ConfirmLoad", PhotonTargets.All);			
		playerCustomProperties = new ExitGames.Client.Photon.Hashtable{ { "Score",0 } };
		PhotonNetwork.player.SetCustomProperties (playerCustomProperties);	
		targetScoreLabel.text = "目标得分：" + targetScore.ToString ();

		currentScoreOfTeam1 = 0;										
		currentScoreOfTeam2 = 0;
		UpdateScores (currentScoreOfTeam1, currentScoreOfTeam2);	
		if (PhotonNetwork.isMasterClient)							
			photonView.RPC ("SetTime", PhotonTargets.All, PhotonNetwork.time, checkplayerTime);
		gameResult.text = "";				
		scorePanel.SetActive (false);		
	}


	[PunRPC]
	void ConfirmLoad(){
		loadedPlayerNum++;
	}


	void Update(){
		countDown = endTimer - PhotonNetwork.time;
		if (countDown >= photonCircleTime)			
			countDown -= photonCircleTime;
		UpdateTimeLabel ();							


		switch (state) {

		case GameState.PreStart:					
			if (PhotonNetwork.isMasterClient) {	
				CheckPlayerConnected ();
			}
			break;

		case GameState.Playing:				
			hpSlider.value = playerHealth.currentHP;		
			#if(!UNITY_ANDROID)
			scorePanel.SetActive (Input.GetKey (KeyCode.Tab));
			#endif
			if (PhotonNetwork.isMasterClient) {				
				if (currentScoreOfTeam1 >= targetScore)		
					photonView.RPC ("EndGame", PhotonTargets.All, "Team1",PhotonNetwork.time);
				else if (currentScoreOfTeam2 >= targetScore)	
					photonView.RPC ("EndGame", PhotonTargets.All, "Team2",PhotonNetwork.time);
				else if (countDown <= 0.0f) {					
					if (currentScoreOfTeam1 > currentScoreOfTeam2)		
						photonView.RPC ("EndGame", PhotonTargets.All, 
							"Team1", PhotonNetwork.time);
					else if (currentScoreOfTeam1 < currentScoreOfTeam2)
						photonView.RPC ("EndGame", PhotonTargets.All, 
							"Team2",PhotonNetwork.time);
					else                                       
						photonView.RPC ("EndGame", PhotonTargets.All, 
							"Tie",PhotonNetwork.time);
				}
			}
			break;
		case GameState.GameWin:	
			if (countDown <= 0)
				LeaveRoom ();
			break;
		case GameState.GameLose:	
			if (countDown <= 0)
				LeaveRoom ();
			break;
		case GameState.Tie:
			if (countDown <= 0)	
				LeaveRoom ();
			break;
		}
	}
	void CheckTeamNumber(){
		PhotonPlayer[] players = PhotonNetwork.playerList;	
		int teamOneNum = 0, teamTwoNum = 0;						
		foreach (PhotonPlayer p in players) {				
			if (p.customProperties ["Team"].ToString () == "Team1")
				teamOneNum++;
			else
				teamTwoNum++;
		}
		if (teamOneNum == 0)
			photonView.RPC ("EndGame", PhotonTargets.All, "Team2",PhotonNetwork.time);
		else if (teamTwoNum == 0)
			photonView.RPC ("EndGame", PhotonTargets.All, "Team1",PhotonNetwork.time);
	}

	void UpdateRealTimeScorePanel()
	{
		string team1Title = string.Empty;
		string team2Title = string.Empty;

		team1Title = string.Format("得分：{0}",currentScoreOfTeam1);
		team2Title = string.Format("得分：{0}",currentScoreOfTeam2);

		Team1RealTimeScorePanelScore.text = team1Title;
		Team2RealTimeScorePanelScore.text = team2Title;
	}

	void UpdateTimeLabel(){
		int minute = (int)countDown / 60;
		int second = (int)countDown % 60;
		timeLabel.text = minute.ToString ("00") + ":" + second.ToString ("00");
	}

	void CheckPlayerConnected(){
		if (countDown <=0.0f || loadedPlayerNum == PhotonNetwork.playerList.Length) {
			startTimer = PhotonNetwork.time;								
			photonView.RPC ("StartGame",PhotonTargets.All,startTimer);		
		}
	}

	[PunRPC]
	void StartGame(double timer){
		SetTime(timer,gamePlayingTime);
		gm.state = GameState.Playing;
		InstantiatePlayer ();		
		AudioSource.PlayClipAtPoint(gameStartAudio, localPlayer.transform.position);
	}


	

	public void localPlayerAddHealth(int points){
		PlayerHealth ph = localPlayer.GetComponent<PlayerHealth> ();
		ph.requestAddHP (points);
	}
	
}
