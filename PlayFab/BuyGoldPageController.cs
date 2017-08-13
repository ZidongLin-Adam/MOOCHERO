using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
//金币购买界面控制
public class BuyGoldPageController : MonoBehaviour {
    
    public GameObject processPanel;             //金币购买时的提示面板
    public Text hintLabel;                      //提示面板的文本信息
    public GameObject backButton;               //提示面板的返回按钮
    public Text singlePaymentDiamondCost;       //“单次购买”花费的钻石
    public Text singlePaymentGoldGet;           //“单次购买”获得的金币
    public Text multiplePaymentDiamondCost;     //“购买5次”花费的钻石
    public Text multiplePaymentGoldGet;         //“购买5次”获得的金币

    public Text goldCurrencyCount;              //显示玩家当前金币数量
    public Text diamondCurrencyCount;           //显示玩家当前钻石数量

	//“单次购买”按钮的响应事件
	public void ClickSingleBuyGoldButton()
	{
        //显示processPanel面板，提示正在购买金币
        processPanel.SetActive(true);
        //检测玩家钻石数量（diamondCurrencyCount）是否充足
        if (int.Parse(diamondCurrencyCount.text) < int.Parse(singlePaymentDiamondCost.text))
        {
            //余额不足,则直接return，并显示返回按钮
            hintLabel.text = "钻石不足，无法购买";
            backButton.SetActive(true);
            return;
        }
        backButton.SetActive(false);
        hintLabel.text = "处 理 中 ...";
        //使用PlayFabClientAPI.ExecuteCloudScript函数，调用CloudScript的ExchangeGold函数(通过request)
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "ExchangeGold",
            FunctionParameter = new { DC = singlePaymentDiamondCost.text, GC = singlePaymentGoldGet.text },
            GeneratePlayStreamEvent = true
        };
       /*
       * ExchangeGold函数的调用需要两个参数DC与GC，分别表示消耗的钻石数量和获得的金币数量。
       * ExchangeGold函数调用成功，执行OnExecuteCloudScript函数，提示玩家购买成功，更新玩家的货币数量
       * ExchangeGold函数调用失败，执行OnPlayFabError函数，提示玩家购买失败，在控制台输出错误原因
       */
       PlayFabClientAPI.ExecuteCloudScript(request,OnExecuteCloudScript,OnPlayFabError);
    }

	//“购买5次”按钮的响应事件,基本逻辑同上
	public void ClickMultipleBuyGoldButton()
	{
        processPanel.SetActive(true);
        if (int.Parse(diamondCurrencyCount.text) < int.Parse(multiplePaymentDiamondCost.text))
        {
            hintLabel.text = "钻石不足，无法购买";
            backButton.SetActive(true);
            return;
        }
        backButton.SetActive(false);
        hintLabel.text = "处 理 中...";
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "ExchangeGold",
            FunctionParameter = new { DC = multiplePaymentDiamondCost.text, GC = multiplePaymentGoldGet.text },
            GeneratePlayStreamEvent = true
        };
        PlayFabClientAPI.ExecuteCloudScript(request, OnExecuteCloudScript, OnPlayFabError);
	}

    //ExecuteCloudScript执行成功时，执行该函数
    void OnExecuteCloudScript(ExecuteCloudScriptResult result)
    {
        //获取ExchangeGold函数的返回值，包括玩家的钻石和金币的数量
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object diamondCurrencyResult, goldCurrencyResult;
        jsonResult.TryGetValue("diamondCurrencyResult", out diamondCurrencyResult);
        jsonResult.TryGetValue("goldCurrencyResult", out goldCurrencyResult);

        //更新玩家钻石和金币的数量
        diamondCurrencyCount.text = diamondCurrencyResult.ToString();
        goldCurrencyCount.text = goldCurrencyResult.ToString();

        //提示玩家金币购买成功
        hintLabel.text = "购买成功";
        backButton.SetActive(true);
    }

    //ExecuteCloudScript执行失败时，执行该函数
    void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError(error.Error);

        //提示玩家金币购买失败
        hintLabel.text = "购买失败";
		backButton.SetActive(true);
    }
}
