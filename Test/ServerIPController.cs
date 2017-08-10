using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;
//该脚本用于动态设置Photon服务器IP,方便大家测试
public class ServerIPController : PunBehaviour {

	public GameObject loginMainPanel;   //登录主面板
	public GameObject serverIPPanel;    //服务器IP设置面板
	public InputField IPText;           //服务器IP输入框

	private ServerSettings photonServer;    //PhotonServerSettings配置文件信息

    //初始化，显示Photon服务器IP
	void OnEnable () {
		photonServer = PhotonNetwork.PhotonServerSettings;
		IPText.text = PlayerPrefs.GetString ("PhotonServerIP", "218.19.220.207");
        photonServer.ServerAddress = PlayerPrefs.GetString("PhotonServerIP");
    }

    //确认按钮
	public void ClickConfirmButton(){
		photonServer.ServerAddress = IPText.text;
		PlayerPrefs.SetString ("PhotonServerIP", IPText.text);
        loginMainPanel.SetActive(true);
        serverIPPanel.SetActive(false);
	}

    //取消按钮
	public void ClickCancelButton(){
		loginMainPanel.SetActive (true);
		serverIPPanel.SetActive (false);
	}
}
