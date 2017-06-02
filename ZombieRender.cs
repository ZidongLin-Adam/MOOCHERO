using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
public class ZombieRender : MonoBehaviour {
	

	private Renderer[] rends;	//僵尸皮肤渲染器数组
	private int rendCnt = 0;	//僵尸皮肤渲染器数组计数器
	[HideInInspector]
	public bool isCrazy;		//僵尸是否狂暴化

	void Start()
	{
		//获取僵尸身体各部分的皮肤渲染器
		rends = GetComponentsInChildren<SkinnedMeshRenderer>();
		//获取皮肤渲染器的个数
		rendCnt = rends.Length;
		//初始化僵尸狂暴化状态为false，表示僵尸未狂暴化
		isCrazy = false;
	} 

	//进入狂暴模式
	public void SetCrazy()
	{
		//把僵尸皮肤渲染器材质属性EnableRim，在着色器中名为_RimBool，设置为1.0开启狂暴效果。
		for(int i=0;i<rendCnt;i++)
			rends [i].material.SetFloat ("_RimBool", 1.0f);
		isCrazy = true;
	}

	//进入正常（非狂暴）模式
	public void SetNormal()
	{
		//把僵尸皮肤渲染器材质属性EnableRim，在着色器中名为_RimBool，设置为0.0关闭狂暴效果。
		for(int i=0;i<rendCnt;i++)
			rends [i].material.SetFloat ("_RimBool", 0.0f);
		isCrazy = false;
	}
}
