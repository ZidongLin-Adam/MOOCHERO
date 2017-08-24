using UnityEngine;
using System.Collections;
using Photon;
public class ZombieGenerator : PunBehaviour {
    
	public Transform[] zombieSpawnTransform;	//僵尸的生成池数组
	public int maximumInstanceCount = 3;		//场景中的最大僵尸数量
	public float minGenerateTimeInterval = 5.0f;	//生成僵尸的最小时间间隔
	public float maxGenerateTimeInterval = 20.0f;	//生成僵尸的最大时间间隔
	public string prefabName;					//僵尸预制件

	private float nextGenerationTime = 0.0f;		//下一次生成僵尸的时刻
	private float timer = 0.0f;						//计时器，用于计算生成僵尸的时间
	private GameObject[] instances;					//僵尸数组对象池
	public static Vector3 defaultPosition = new Vector3(33, -6, -8);	//僵尸的默认生成地点

    private bool isWorking = false;
    //初始化生成器
    public void initGenerator()
    {
        if (PhotonNetwork.isMasterClient)
		{
			//初始化僵尸对象池
            instances = new GameObject[maximumInstanceCount];
            for (int i = 0; i < maximumInstanceCount; i++)
            {
				GameObject zombie = PhotonNetwork.InstantiateSceneObject (
					prefabName, 
					defaultPosition, 
					Quaternion.identity, 
					0, 
					null
				);
                zombie.SetActive(true);
				instances[i] = zombie;	//把僵尸放入僵尸对象池
            }
            this.photonView.RPC("InitGeneratorFromPhotonView", PhotonTargets.Others);
        }
    }
    //非主节点获取僵尸池
    [PunRPC]
    void InitGeneratorFromPhotonView()
    {
        instances = GameObject.FindGameObjectsWithTag(prefabName);
    }
	//在僵尸对象池中，找一个处于禁用状态的僵尸对象
	private GameObject GetNextAvailiableInstance ()   {
		for(var i = 0; i < maximumInstanceCount; i++) {
			if(!instances[i].activeSelf)
			{
				return instances[i];
			}
		}
		return null;
	}
    //从僵尸池中找到一个可用的实例
    private int GetNextAvailiableInstanceIndex()
    {
        for (var i = 0; i < maximumInstanceCount; i++)
        {
            if (instances[i].GetComponent<ZombieAI>().disableOrNot())
            {
                return i;
            }
        }
        return -1;
    }
	//在Position参数指定的位置，生成一个僵尸
	private bool generate(Vector3 position)
	{
        int index = GetNextAvailiableInstanceIndex();
        if (index >= 0)
        {
            GameObject zombie = instances[index];
            zombie.GetComponent<ZombieAI>().Born(position);
            return true;
        }
        return false;
	}
    //每帧更新
	void Update () {   
        if (!PhotonNetwork.isMasterClient || !isWorking )
            return;
		//判断是否到达下一次生成僵尸的时间
		if (timer > nextGenerationTime) {

			//选择一个出生地点
			int i = Random.Range(0, zombieSpawnTransform.Length);
			//在选择的出生地生成一只僵尸
			generate (zombieSpawnTransform [i].position);
			//计算下一次生成僵尸的时间
			nextGenerationTime = Random.Range (minGenerateTimeInterval, maxGenerateTimeInterval);
			//清零timer
			timer = 0;
		}
		timer += Time.deltaTime;
	}
    //僵尸池开始工作
    public void generatorStartWorking()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        isWorking = true;
        //开始工作时把僵尸池里的所有未调度僵尸实例生成出来
        for (int i = zombieSpawnTransform.Length - 1; i >= 0; i--)
        {
            generate(zombieSpawnTransform[i].position);
        }
    }
    //池子停止工作
    public void generatorStopWorking()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        isWorking = false;
    }
    //主节点发生变化时调用
    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        if (newMasterClient.isLocal)
        {
            isWorking = true;
        }
    }

}
