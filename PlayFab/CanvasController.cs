using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;
using PlayFab;
using PlayFab.ClientModels;
//GameLobby场景的StartCanvas控制器
public class CanvasController : PunBehaviour {

	public GameObject loginPanel;		//游戏登录面板
	public GameObject mainPanel;		//游戏大厅面板

	public Text usernameText;		    //玩家昵称信息
	public Text connectionState;		//网络连接状态

	void Awake(){
        //在游戏启动时，设置PlayFab的TitleId
		PlayFabSettings.TitleId = PlayFabUserData.titleId;
        //设置游戏客户端连接的Photon服务器IP
		if (PlayerPrefs.GetString ("PhotonServerIP") == "" || PlayerPrefs.GetString ("PhotonServerIP") == null) {
			PlayerPrefs.SetString ("PhotonServerIP", "139.196.20.246");
			PhotonNetwork.PhotonServerSettings.ServerAddress = "139.196.20.246";
		}
	}

	//初始化，根据当前客户端连接状态，显示相应的游戏面板
	void Start () {
		if (!PhotonNetwork.connected)
			SetLoginPanelActive ();	
		else
			SetLobbyPanelActive ();
		connectionState.text = "";	//初始化网络连接状态文本信息
	}

//条件编译指令，只在Unity编辑器中（UNITY_EDITOR）编译此段代码
//#if(UNITY_EDITOR)	
	void Update(){		
		//在游戏画面左下角显示当前的网络连接状态
		connectionState.text = PhotonNetwork.connectionStateDetailed.ToString ();
    }
//#endif

	//启用游戏登录面板
	public void SetLoginPanelActive(){
		loginPanel.SetActive (true);				//启用游戏登录面板
		mainPanel.SetActive (false);				//禁用游戏主面板
	}
	//启用游戏大厅面板
	public void SetLobbyPanelActive(){				
		loginPanel.SetActive (false);				//禁用游戏登录面板
		mainPanel.SetActive (true);					//启用游戏主面板
	}

	/**覆写IPunCallback回调函数，当玩家进入游戏大厅时调用
	 * 显示玩家昵称
	 */
	/*public override void OnJoinedLobby(){
        usernameText.text = PhotonNetwork.player.name;
	}*/

	/**覆写IPunCallback回调函数，当客户端断开与Photon服务器的连接时调用
	 * 游戏画面返回游戏登录面板
	 */
	public override void OnConnectionFail(DisconnectCause cause){
		SetLoginPanelActive ();
	}
}
