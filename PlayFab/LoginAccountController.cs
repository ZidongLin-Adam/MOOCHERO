using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

//玩家账号登录控制器
public class LoginAccountController: MonoBehaviour {

	public GameObject loginMainPanel;       //主登录面板
	public GameObject loginAccountPanel;    //账号登录面板
	public GameObject registerPanel;        //注册面板
	public InputField account;              //玩家账号/邮箱输入栏
	public InputField password;             //登录密码
	public Text errorMessage;               //错误提示信息
	public GameObject loginingWindow;       //“账号登录中”提示窗口

    public GameObject loginPanel;           //登录面板
	public GameObject mainPanel;            //游戏主面板

    //玩家账号登录面板启动后调用，初始化界面信息
	void OnEnable(){
		errorMessage.text = "";
        account.text = PlayerPrefs.GetString("Account", "");    //获取玩家上次登录成功的账号信息
		password.text = "";
		loginingWindow.SetActive(false);
	}

    //“登录”按钮响应事件
	public void ClickLoginButton(){
        /* 学生作业：实现玩家账号/邮箱登录的功能
         * 作业提示：
         * 首先，启用UI，提示玩家账号正在登录
         * 其次在游戏客户端检测一些简单的非法输入
         * 最后，使用PlayFab的Client API实现玩家账号/邮箱登录
         * 账号登录：先使用LoginWithPlayFabRequest声明账号登录请求，在使用PlayFabClientAPI.LoginWithPlayFab发起请求；
         * 邮箱登录：先使用LoginWithEmailAddressRequest声明账号登录请求，在使用PlayFabClientAPI.LoginWithEmailAddress发起请求。
         * 登录成功，调用OnLoginResult函数，连接Photon服务器，游戏进入游戏主面板
         * 登录失败，根据错误类型提示玩家登录失败原因，在控制台输出错误信息
         */
        //在游戏客户端检测一些简单的非法输入
        if (account.text == "")
            errorMessage.text = "请输入账号/邮箱";
        else if(password.text == "")
            errorMessage.text = "请输入密码";
        else
        {
            //非法输入通过后
            errorMessage.text = "";
            //启用UI，提示玩家账号正在登录
            loginingWindow.GetComponentInChildren<Text>().text = "账 号 登 录 中 ...";
            loginingWindow.SetActive(true);
            //最后，使用PlayFab的Client API实现玩家账号/邮箱登录
            //邮箱登录：先使用LoginWithEmailAddressRequest声明账号登录请求，在使用PlayFabClientAPI.LoginWithEmailAddress发起请求。
            if (account.text.Contains("@"))
            {
                LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
                {
                    Email = account.text,
                    Password = password.text
                };
                PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginResult, OnPlayFabError);//登录成功，调用OnLoginResult函数，连接Photon服务器，游戏进入游戏主面板;登录失败，根据错误类型提示玩家登录失败原因，在控制台输出错误信息
            }
            else//账号登录：先使用LoginWithPlayFabRequest声明账号登录请求，在使用PlayFabClientAPI.LoginWithPlayFab发起请求；
            {
                LoginWithPlayFabRequest request = new LoginWithPlayFabRequest()
                {
                    Username = account.text,
                    Password = password.text
                };
                PlayFabClientAPI.LoginWithPlayFab(request, OnLoginResult, OnPlayFabError);//登录成功，调用OnLoginResult函数，连接Photon服务器，游戏进入游戏主面板;登录失败，根据错误类型提示玩家登录失败原因，在控制台输出错误信息
            }
        }
    }

    //账号登录成功后，保存玩家信息，连接Photon服务器
    void OnLoginResult(LoginResult result){
		PlayerPrefs.SetString ("Account", account.text);    //保存玩家的登录信息
        //在PlayFab保存玩家信息
        PlayFabUserData.playFabId = result.PlayFabId;
		PlayFabUserData.username = account.text;
		loginingWindow.SetActive(false);

        //连接Photon服务器
		PhotonNetwork.ConnectUsingSettings ("1.0");
		PhotonNetwork.player.name = account.text;

		loginPanel.SetActive (false);               //禁用游戏登录面板
        mainPanel.SetActive(true);                  //启用游戏主面板
	}

    //登录请求失败时调用，根据错误类型提示玩家登录失败原因
	void OnPlayFabError(PlayFabError error){
		loginingWindow.SetActive(false);
		Debug.Log ("Get an error:" + error.Error);
		if (error.Error == PlayFabErrorCode.InvalidParams && (error.ErrorDetails.ContainsKey ("Username") || (error.ErrorDetails.ContainsKey("Email")))){
			errorMessage.text = "账号/邮箱输入不符合规范";
		} else if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey ("Password")) {
			errorMessage.text = "登录密码长度应为6~24位";
		} else if (error.Error == PlayFabErrorCode.AccountNotFound) {
			errorMessage.text = "账号/邮箱不存在";
		} else if (error.Error == PlayFabErrorCode.InvalidUsernameOrPassword || error.Error == PlayFabErrorCode.InvalidEmailOrPassword) {
			errorMessage.text = "登录密码错误";
		} else if (error.Error == PlayFabErrorCode.AccountBanned) {
			errorMessage.text = "账号/邮箱已被锁定";
		} else {
			errorMessage.text = "未知错误";
		}
	}

    //“注册”按钮的响应函数
	public void ClickRegisterButton(){
		loginAccountPanel.SetActive (false);
		registerPanel.SetActive (true);
	}

    //“取消”按钮的响应函数
	public void ClickCancelButton(){
		loginAccountPanel.SetActive (false);
		loginMainPanel.SetActive (true);
	}
}
