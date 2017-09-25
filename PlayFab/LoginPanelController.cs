using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
//登录面板控制器
public class LoginPanelController : MonoBehaviour {

	public GameObject loginMainPanel;       //登录主面板
	public GameObject loginAccountPanel;    //账号登录面板
	public GameObject registerPanel;        //注册面板
	public GameObject loginingWindow;       //“登录中”提示窗口

	public GameObject serverIPPanel;        //服务器IP设置面板

	public GameObject loginPanel;           //登录面板
	public GameObject mainPanel;            //游戏主面板

    //登录面板启用时调用，初始化面板的显示
	void OnEnable(){
		loginMainPanel.SetActive (true);
		loginAccountPanel.SetActive (false);
		registerPanel.SetActive (false);
		loginingWindow.SetActive(false);
		if (serverIPPanel != null) {
			serverIPPanel.SetActive (false);
		}
	}

    //“账号登录”按钮的响应函数
	public void ClickAccoutLoginButton(){
		loginMainPanel.SetActive (false);
		loginAccountPanel.SetActive (true);
	}

    //“设备登录”按钮的响应函数
	public void ClickDeviceLoginButton(){
        //发起设备（自定义ID）登录请求，使用设备的标识符作为登录的自定义ID
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest () {
			CustomId = SystemInfo.deviceUniqueIdentifier,
			CreateAccount = true,       //如果用户不存在，就自动创建账户
		};
		PlayFabClientAPI.LoginWithCustomID (request, OnLoginResult, OnPlayFabError);
        loginingWindow.GetComponentInChildren<Text>().text = "设 备 登 录 中...";
        loginingWindow.SetActive(true);
    }

    //设备登录成功后调用此函数
	void OnLoginResult(LoginResult result){
		loginingWindow.SetActive(false);
        //保存玩家的playfabid
        PlayFabUserData.playFabId = result.PlayFabId;
        
        //连接Photon服务器
		PhotonNetwork.ConnectUsingSettings ("1.0");
		PhotonNetwork.player.name = "游客" + result.PlayFabId;
		PlayFabUserData.username = PhotonNetwork.player.name;

        //如果玩家登录的是新创建的账号
		if (result.NewlyCreated) {
			string displayName = "游客" + result.PlayFabId;
            //为玩家设置昵称（DisplayName）
			UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest () {
				DisplayName = displayName
			};
			PlayFabClientAPI.UpdateUserTitleDisplayName (request, OnUpdateUserTitleDisplayName, OnPlayFabError);
		} else {
			loginPanel.SetActive (false);				//禁用游戏登录面板
			mainPanel.SetActive (true);					//启用游戏主面板
		}
	}

    //玩家昵称设置成功时调用
	void OnUpdateUserTitleDisplayName(UpdateUserTitleDisplayNameResult result){

        /*  学生作业 2-3：玩家创建账号成功时，为玩家添加默认枪支：AK47
         *  作业提示：
         *  先使用PurchaseItemRequest声明一个道具购买请求，再使用PlayFabClientAPI.PurchaseItem发起道具购买请求
         *  道具购买成功，禁用游戏登录面板，启用游戏主面板（OnPurchaseItemRequest函数）
         *  道具购买失败，在控制台输出失败原因（OnPlayFabError函数）
         */
        PurchaseItemRequest request = new PurchaseItemRequest()
        {
            CatalogVersion = PlayFabUserData.catalogVersion,
            VirtualCurrency = "FR",
            Price = 0,
            ItemId = "AK47"
        };
        PlayFabClientAPI.PurchaseItem(request, OnPurchaseItemResult, OnPlayFabError);

		loginPanel.SetActive(false);                //禁用游戏登录面板
		mainPanel.SetActive(true);                  //启用游戏主面板

    }

    //道具购买成功时调用
	void OnPurchaseItemResult(PurchaseItemResult result){
        loginPanel.SetActive(false);                //禁用游戏登录面板
        mainPanel.SetActive(true);                  //启用游戏主面板
    }

    //PlayFab请求出错时调用，在控制台输出错误信息
    void OnPlayFabError(PlayFabError error){
		Debug.LogError (error.ToString ());
		loginingWindow.SetActive(false);
	}
    //点击“服务器IP设置”按钮，显示服务器IP设置面板
	public void ClickServerIPButton(){
		loginMainPanel.SetActive(false);
		loginAccountPanel.SetActive(false);
		registerPanel.SetActive(false);
		serverIPPanel.SetActive (true);
	}
}
