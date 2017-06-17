using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;

public class LobbyPanelController : PunBehaviour {

	public GameObject loginPanel;	
	public GameObject lobbyPanel;	
	public GameObject userMessage;		
	public Button backButton;			
	public GameObject lobbyLoadingLabel;
	public GameObject roomLoadingLabel;		
	public GameObject roomMessagePanel;		
	public Button randomJoinButton;		
	public GameObject previousButton;	
	public GameObject nextButton;		
	public Text pageMessage;			
	public GameObject createRoomPanel;		
	public GameObject roomPanel;		
	private RoomInfo[] roomInfo;			
	private int currentPageNumber;		
	private int maxPageNumber;			
	private int roomPerPage = 4;		
	private GameObject[] roomMessage;	

	void OnEnable(){
		currentPageNumber = 1;			
		maxPageNumber = 1;				
		lobbyLoadingLabel.SetActive (true);
		roomLoadingLabel.SetActive (false);	
		if(createRoomPanel!=null)
			createRoomPanel.SetActive (false);	

		RectTransform rectTransform = roomMessagePanel.GetComponent<RectTransform> ();
		roomPerPage = rectTransform.childCount;	

		
		roomMessage = new GameObject[roomPerPage];	
		for (int i = 0; i < roomPerPage; i++) {
			roomMessage [i] = rectTransform.GetChild (i).gameObject;
			roomMessage [i].SetActive (false);		
		}

		backButton.onClick.RemoveAllListeners ();		
		backButton.onClick.AddListener (delegate() {	
			PhotonNetwork.Disconnect();					
			loginPanel.SetActive(true);					
			lobbyPanel.SetActive(false);				
			userMessage.SetActive (false);				
			backButton.gameObject.SetActive (false);	
		});
		if(roomPanel!=null)
			roomPanel.SetActive (false);			

	public override void OnJoinedLobby(){
		lobbyLoadingLabel.SetActive (false);
	}
	public override void OnJoinedRoom(){
		lobbyPanel.SetActive (false);
		roomPanel.SetActive (true);
	}
	public override void OnReceivedRoomListUpdate(){
		roomInfo = PhotonNetwork.GetRoomList ();				
		maxPageNumber = (roomInfo.Length - 1) / roomPerPage + 1;
		if (currentPageNumber > maxPageNumber)	
			currentPageNumber = maxPageNumber;	
		pageMessage.text = currentPageNumber.ToString () + "/" + maxPageNumber.ToString ();	
		ButtonControl ();		
		ShowRoomMessage ();		

		if (roomInfo.Length == 0) {
			randomJoinButton.interactable = false;	
		} else
			randomJoinButton.interactable = true;
	}


	void ShowRoomMessage(){
		int start, end, i, j;
		start = (currentPageNumber - 1) * roomPerPage;			
		if (currentPageNumber * roomPerPage < roomInfo.Length)	
			end = currentPageNumber * roomPerPage;
		else
			end = roomInfo.Length;

	
		for (i = start,j = 0; i < end; i++,j++) {
			RectTransform rectTransform = roomMessage [j].GetComponent<RectTransform> ();
			string roomName = roomInfo [i].name;	
			rectTransform.GetChild (0).GetComponent<Text> ().text = (i + 1).ToString ();	
			rectTransform.GetChild (1).GetComponent<Text> ().text = roomName;				
			rectTransform.GetChild (2).GetComponent<Text> ().text 						
				= roomInfo [i].playerCount + "/" + roomInfo [i].maxPlayers;					
			Button button = rectTransform.GetChild (3).GetComponent<Button> ();	
			if (roomInfo [i].playerCount == roomInfo [i].maxPlayers || roomInfo [i].open == false)
				button.gameObject.SetActive (false);
			else {
				button.gameObject.SetActive (true);
				button.onClick.RemoveAllListeners ();
				button.onClick.AddListener (delegate() {
					ClickJoinRoomButton (roomName);
				});
			}	
		}
		
		while (j < 4) {
			roomMessage [j++].SetActive (false);
		}
	}

	
	void ButtonControl(){
		if (currentPageNumber == 1)
			previousButton.SetActive (false);
		else
			previousButton.SetActive (true);
		if (currentPageNumber == maxPageNumber)
			nextButton.SetActive (false);
		else
			nextButton.SetActive (true);
	}

	public void ClickCreateRoomButton(){
		createRoomPanel.SetActive (true);
	}
	public void ClickRandomJoinButton(){
		PhotonNetwork.JoinRandomRoom ();
		roomLoadingLabel.SetActive (true);
	}
	public void ClickPreviousButton(){
		currentPageNumber--;		
		pageMessage.text = currentPageNumber.ToString () + "/" + maxPageNumber.ToString ();
		ButtonControl ();		
		ShowRoomMessage ();		
	}
	
	public void ClickNextButton(){
		currentPageNumber++;		
		pageMessage.text = currentPageNumber.ToString () + "/" + maxPageNumber.ToString ();	
		ButtonControl ();			
		ShowRoomMessage ();		
	}

	public void ClickJoinRoomButton(string roomName){
		PhotonNetwork.JoinRoom(roomName);
		roomLoadingLabel.SetActive (true);	
	}
}
