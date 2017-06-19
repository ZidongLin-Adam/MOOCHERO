using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;

public class RoomPanelController : PunBehaviour {

	public GameObject lobbyPanel;		
	public GameObject roomPanel;	
	public Button backButton;		
	public Text roomName;			
	public GameObject[] Team1;		
	public GameObject[] Team2;		
	public Button readyButton;		
	public Text promptMessage;			

	PhotonView pView;
	int teamSize;
	Text[] texts;
	ExitGames.Client.Photon.Hashtable costomProperties;

	void OnEnable () {
		pView = GetComponent<PhotonView>();					
		if(!PhotonNetwork.connected)return;
		roomName.text = "房间：" + PhotonNetwork.room.name;	
		promptMessage.text = "";						

		backButton.onClick.RemoveAllListeners ();			
		backButton.onClick.AddListener (delegate() {	
			PhotonNetwork.LeaveRoom ();					
			lobbyPanel.SetActive (true);				
			roomPanel.SetActive (false);				
		});

		teamSize = PhotonNetwork.room.maxPlayers / 2;		
		DisableTeamPanel ();							
		UpdateTeamPanel (false);							

		for (int i = 0; i < teamSize; i++) {	
			if (!Team1 [i].activeSelf) {	
				Team1 [i].SetActive (true);	
				texts = Team1 [i].GetComponentsInChildren<Text> ();
				texts [0].text = PhotonNetwork.playerName;				
				if(PhotonNetwork.isMasterClient)texts[1].text="房主";	
				else texts [1].text = "未准备";							
				costomProperties = new ExitGames.Client.Photon.Hashtable () {	
					{ "Team","Team1" },	
					{ "TeamNum",i },		
					{ "isReady",false },	
					{ "Score",0 }			
				};
				PhotonNetwork.player.SetCustomProperties (costomProperties);	
				break;
			} else if (!Team2 [i].activeSelf) {	
				Team2 [i].SetActive (true);		
				texts = Team2 [i].GetComponentsInChildren<Text> ();		
				if(PhotonNetwork.isMasterClient)texts[1].text="房主";	
				else texts [1].text = "未准备";							
				costomProperties = new ExitGames.Client.Photon.Hashtable () {	
					{ "Team","Team2" },		
					{ "TeamNum",i },		
					{ "isReady",false },	
					{ "Score",0 }			
				};
				PhotonNetwork.player.SetCustomProperties (costomProperties);
				break;
			}
		}
		ReadyButtonControl ();	
	}


	public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps){
		DisableTeamPanel ();	
		UpdateTeamPanel (true);	
	}

	public override void OnMasterClientSwitched (PhotonPlayer newMasterClient) {
		ReadyButtonControl ();
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer){
		DisableTeamPanel ();	
		UpdateTeamPanel (true);	
	}

	void DisableTeamPanel(){
		for (int i = 0; i < Team1.Length; i++) {
			Team1 [i].SetActive (false);
		}
		for (int i = 0; i < Team2.Length; i++) {
			Team2 [i].SetActive (false);
		}
	}

	void UpdateTeamPanel(bool isUpdateSelf){
		GameObject go;
		foreach (PhotonPlayer p in PhotonNetwork.playerList) {
			if (!isUpdateSelf && p.isLocal)	continue;			
			costomProperties = p.customProperties;				
			if (costomProperties ["Team"].Equals ("Team1")) {	
				go = Team1 [(int)costomProperties ["TeamNum"]];	
				go.SetActive (true);							
				texts = go.GetComponentsInChildren<Text> ();	
			} else {											
				go = Team2 [(int)costomProperties ["TeamNum"]];	
				go.SetActive (true);
				texts = go.GetComponentsInChildren<Text> ();
			}
			texts [0].text = p.name;						
			if(p.isMasterClient)							
				texts[1].text="房主";						
			else if ((bool)costomProperties ["isReady"]) {	
				texts [1].text = "已准备";					
			} else
				texts [1].text = "未准备";					
		}
	}


	void ReadyButtonControl(){
		if (PhotonNetwork.isMasterClient) {									
			readyButton.GetComponentInChildren<Text> ().text = "开始游戏";	
			readyButton.onClick.RemoveAllListeners ();						
			readyButton.onClick.AddListener (delegate() {					
				ClickStartGameButton ();								
			});
		} else {															
			if((bool)PhotonNetwork.player.customProperties["isReady"])		
				readyButton.GetComponentInChildren<Text> ().text = "取消准备";		
			else 
				readyButton.GetComponentInChildren<Text> ().text = "准备";
			readyButton.onClick.RemoveAllListeners ();						
			readyButton.onClick.AddListener (delegate() {					
				ClickReadyButton ();										
			});
		}
	}

	public void ClickSwitchButton(){
		costomProperties = PhotonNetwork.player.customProperties;	
		if ((bool)costomProperties ["isReady"]) {					
			promptMessage.text="准备状态下不能切换队伍";				
			return;												
		}
		bool isSwitched = false;		
		if (costomProperties ["Team"].ToString ().Equals ("Team1")) {				
			for (int i = 0; i < teamSize; i++) {									
				if (!Team2 [i].activeSelf) {									
					isSwitched = true;												
					Team1 [(int)costomProperties ["TeamNum"]].SetActive (false);	
					texts = Team2 [i].GetComponentsInChildren<Text> ();				
					texts [0].text = PhotonNetwork.playerName;						
					if(PhotonNetwork.isMasterClient)texts[1].text="房主";			
					else texts [1].text = "未准备";									
					Team2 [i].SetActive (true);										
					costomProperties = new ExitGames.Client.Photon.Hashtable ()		
					{ { "Team","Team2" }, { "TeamNum",i } };
					PhotonNetwork.player.SetCustomProperties (costomProperties);	
					break;
				}
			}
		} else if (costomProperties ["Team"].ToString ().Equals ("Team2")) {		
			for (int i = 0; i < teamSize; i++) {								
				if (!Team1 [i].activeSelf) {										
					isSwitched = true;												
					Team2 [(int)(costomProperties ["TeamNum"])].SetActive (false);	
					texts = Team1 [i].GetComponentsInChildren<Text> ();				
					texts [0].text = PhotonNetwork.playerName;						
					if(PhotonNetwork.isMasterClient)texts[1].text="房主";			
					else texts [1].text = "未准备";								
					Team1 [i].SetActive (true);									
					costomProperties = new ExitGames.Client.Photon.Hashtable ()	
					{ { "Team","Team1" }, { "TeamNum",i } };
					PhotonNetwork.player.SetCustomProperties (costomProperties);
					break;
				}
			}
		}
		if (!isSwitched)
			promptMessage.text = "另一队伍已满,无法切换";	
		else
			promptMessage.text = "";
	}

	public void ClickReadyButton(){
		bool isReady = (bool)PhotonNetwork.player.customProperties ["isReady"];				
		costomProperties = new ExitGames.Client.Photon.Hashtable (){ { "isReady",!isReady } };
		PhotonNetwork.player.SetCustomProperties (costomProperties);
		Text readyButtonText = readyButton.GetComponentInChildren<Text> ();
		if(isReady)readyButtonText.text="准备";	
		else readyButtonText.text="取消准备";
	}


	public void ClickStartGameButton(){
		foreach (PhotonPlayer p in PhotonNetwork.playerList) {	
			if (p.isLocal) continue;								
			if ((bool)p.customProperties ["isReady"] == false) {	
				promptMessage.text = "有人未准备，游戏无法开始";	
				return;											
			}
		}
		promptMessage.text = "";									
		PhotonNetwork.room.open = false;							
		pView.RPC ("LoadGameScene", PhotonTargets.All, "GameScene");
	}


	[PunRPC]
	public void LoadGameScene(string sceneName){	
		PhotonNetwork.LoadLevel (sceneName);	
	}
}
