using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon;

public class CreateRoomController : PunBehaviour {

	public GameObject createRoomPanel;		//创建房间面板
	public GameObject roomLoadingWindow;		//禁用游戏房间加载提示信息
	public Text roomName;					//房间名称文本
	public Text roomNameHint;				//房间名称提示文本
	public GameObject maxPlayerToggle;		//最大玩家个数开关组
	private byte[] maxPlayerNum = { 2, 4 }; //最大玩家个数

    public Image mapImage;                  //地图缩略图

    private ExitGames.Client.Photon.Hashtable customProperty;
    private string mapName;
    private int mapIndex;
    private List<string> mapKeys;

    //创建房间面板激活时调用
    void OnEnable(){
		roomNameHint.text = "";	//清空房间名称提示文本
        
        mapKeys = new List<string>(GameInfo.maps.Keys);
        mapIndex = 0;
        mapName = mapKeys[mapIndex];
    }

	//"确认创建"按钮事件处理函数
	public void ClickConfirmCreateRoomButton(){
		RoomOptions roomOptions=new RoomOptions();
		RectTransform toggleRectTransform = maxPlayerToggle.GetComponent<RectTransform> ();
		int childCount = toggleRectTransform.childCount;
		//根据最大玩家个数开关组的打开情况，确认房间最大玩家个数
		for (int i = 0; i < childCount; i++) {
			if (toggleRectTransform.GetChild (i).GetComponent<Toggle> ().isOn == true) {
				roomOptions.MaxPlayers= maxPlayerNum [i];
				break;
			}
		}
        //更新游戏房间的地图
        customProperty = new ExitGames.Client.Photon.Hashtable()
        {
            {"MapName", mapName}
        };
        roomOptions.CustomRoomProperties = customProperty;
        
		RoomInfo[] roomInfos = PhotonNetwork.GetRoomList();	//获取游戏大厅内所有游戏房间
		bool isRoomNameRepeat = false;
		//遍历游戏房间，检查新创建的房间名是否与已有房间重复
		foreach (RoomInfo info in roomInfos) {
			if (roomName.text == info.name) {
				isRoomNameRepeat = true;
				break;
			}
		}
		//如果房间名称重复，房间名称提示文本显示"房间名称重复！"
		if (isRoomNameRepeat) {
			roomNameHint.text = "房间名称重复!";
		}
		//否则，根据玩家设置的房间名、房间玩家人数创建房间
		else {
			PhotonNetwork.CreateRoom (roomName.text, roomOptions, TypedLobby.Default);	//在默认游戏大厅中创建游戏房间
			createRoomPanel.SetActive (false);	//禁用创建房间面板
			roomLoadingWindow.SetActive (true);	//启用游戏房间加载提示信息
		}
	}

	//"取消创建"按钮事件处理函数
	public void ClickCancelCreateRoomButton(){
		createRoomPanel.SetActive (false);		//禁用创建房间面板
		roomNameHint.text = "";					//清空房间名称提示文本
	}

    //上一张地图
    public void ClickMapLeftButton()
    {
        int length = mapKeys.Count;
        mapIndex--;
        if (mapIndex < 0) mapIndex = length - 1;
        mapName = mapKeys[mapIndex];
        mapImage.sprite = GameInfo.maps[mapName];
    }
    //下一张地图
    public void ClickMapRightButton()
    {
        int length = mapKeys.Count;
        mapIndex++;
        if (mapIndex >= length) mapIndex = 0;
        mapName = mapKeys[mapIndex];
        mapImage.sprite = GameInfo.maps[mapName];
    }
}
