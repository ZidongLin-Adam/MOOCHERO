using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

//账号注册面板控制器
public class RegisterPanelController : MonoBehaviour {

	public GameObject loginAccountPanel;    //账号登录面板（注册面板上一级）
	public GameObject registerPanel;        //账号注册面板
	public InputField username;             //玩家账号输入栏
    public InputField email;                //绑定邮箱输入栏
    public InputField password;             //登录密码输入栏
	public InputField confirmPassword;      //确认密码输入栏
	public Text errorMessage;               //错误信息提示
    public GameObject loginingWindow;       //“账号登录中”提示信息

	public GameObject loginPanel;           //登录面板
	public GameObject mainPanel;            //游戏主面板

    //账号注册面板启用时调用
	void OnEnable () {
        //初始化输入框等信息
		username.text = "";         
        email.text = "";
		password.text = "";
		confirmPassword.text = "";
		errorMessage.text = "";
        loginingWindow.SetActive(false);
	}

    //“注册”按钮的响应函数
	public void ClickRegisterButton(){
        //在发送注册请求前，进行简单的非法输入检测
        if(username.text == ""){
            errorMessage.text = "请输入游戏账号";
        }else if(email.text == ""){
            errorMessage.text = "请输入绑定邮箱";
        }else if (password.text == ""){
            errorMessage.text = "请输入登录密码";
        }else if (password.text != confirmPassword.text) {
			errorMessage.text = "登录密码与确认密码不一致";
		} else {//简单的非法输入检测通过
			errorMessage.text = "";
            //向PlayFab发起账号注册请求
            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest(){
                Username = username.text,
                Email = email.text,
				Password = password.text,
				RequireBothUsernameAndEmail = true  //注册信息需要包含注册账号的账号名和绑定邮箱
			};
			PlayFabClientAPI.RegisterPlayFabUser 
			(
				request, 
				OnRegisterResult, 
				OnPlayFabError
			);
			loginingWindow.GetComponentInChildren<Text>().text = "账 号 注 册 中...";
			loginingWindow.SetActive(true);
		}
	}

    //注册成功后
	void OnRegisterResult(RegisterPlayFabUserResult result){

		PlayerPrefs.SetString ("Account", username.text);   
        PlayFabUserData.playFabId = result.PlayFabId;
		PlayFabUserData.username = username.text;

        //连接Photon服务器
		PhotonNetwork.ConnectUsingSettings ("1.0");
		PhotonNetwork.player.name = result.Username;

        /*  玩家创建账号成功时，为玩家添加默认枪支：AK47
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

		loginingWindow.SetActive(false);            //禁用账号登录中提示窗口
		loginPanel.SetActive (false);				//禁用游戏登录面板
		mainPanel.SetActive (true);					//启用游戏主面板
   }

   //道具购买成功后调用
   void OnPurchaseItemResult(PurchaseItemResult result){
       loginingWindow.SetActive(false);            //禁用账号登录中提示窗口
       loginPanel.SetActive (false);				//禁用游戏登录面板
       mainPanel.SetActive (true);					//启用游戏主面板
   }

   //账号注册失败时调用，根据返回的错误类型提示玩家出错原因
   void OnPlayFabError(PlayFabError error){
       loginingWindow.SetActive(false);
       Debug.Log ("Get an error：" + error.Error);
       if ((error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey ("Username")) 
           || error.Error == PlayFabErrorCode.InvalidUsername) {
           errorMessage.text = "游戏账号输入不符合规范";
       }else if ((error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Email")) 
           || error.Error == PlayFabErrorCode.InvalidPassword) {
           errorMessage.text = "邮箱输入不符合规范";
       }else if ((error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Password")) 
           || error.Error == PlayFabErrorCode.InvalidPassword) {
           errorMessage.text = "游戏密码输入不符合规范";
       }else if (error.Error == PlayFabErrorCode.EmailAddressNotAvailable){
           errorMessage.text = "该邮箱已被注册";
       }else if (error.Error == PlayFabErrorCode.UsernameNotAvailable){
           errorMessage.text = "该游戏账号已被注册";
       }else {
           errorMessage.text = "未知错误";
       }
   }

   //取消按钮的事件响应函数
   public void ClickCancelButton(){
       registerPanel.SetActive (false);
       loginAccountPanel.SetActive (true);
   }
}
