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

}
