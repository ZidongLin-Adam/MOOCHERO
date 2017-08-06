using UnityEngine;
using System.Collections;
using Photon;

public class PlayerHealth : PunBehaviour{

	public int killScore = 1;				//玩家对象被击杀增加击杀者的分数
	public int maxHP = 100;					//玩家对象满血血量
	public float respawnTime = 5.0f;		//玩家对象死亡后重生时间
    public float disableTime = 3.5f;        //玩家对象死亡后消失时间（小于重生时间）
	public float invincibleTime = 3.0f;		//玩家对象无敌时间

	[HideInInspector]public string team;			//玩家对象队伍
	[HideInInspector]public bool isAlive;		    //玩家对象是否存活
	[HideInInspector]public int currentHP;		    //玩家对象当前生命值
	[HideInInspector]public bool invincible = false;//玩家对象是否无敌

    Color invincibleColor = new Color(1.0f, 1.0f, 0.0f);    //无敌颜色：黄
    Color enemyColor = new Color(1.0f, 0.0f, 0.0f);         //敌人颜色：红

    GameObject gun;
	float timer;
	Animator anim;
	Rigidbody rigid;
	Collider colli;

	//初始化
	void Start () {
		Init ();	//初始化玩家对象生命值相关属性
		anim = GetComponent<Animator> ();			//获取玩家对象动画控制器
		rigid = GetComponent<Rigidbody> ();			//获取玩家对象刚体组件
		colli = GetComponent<CapsuleCollider> ();   //获取玩家对象胶囊碰撞体
        if (!photonView.isMine) return;				//如果不是本地玩家对象，结束函数运行
        photonView.RPC("InitGun", PhotonTargets.All, PlayFabUserData.equipedWeapon);
        photonView.RPC ("UpdateHP", PhotonTargets.Others, currentHP);				//使用RPC，更新其他客户端中该玩家对象当前血量
		if (PhotonNetwork.player.customProperties ["Team"].ToString () == "AttackerTeam")	//设置玩家对象队伍
			team = "AttackerTeam";
		else if(PhotonNetwork.player.customProperties["Team"].ToString()=="DefenderTeam")
			team = "DefenderTeam";
		photonView.RPC ("SetTeam", PhotonTargets.Others, team);		//使用RPC，设置其他客户端中该玩家对象的队伍
	}
    [PunRPC]
    void InitGun(string equipedWeapon)
    {
        gun = transform.Find(equipedWeapon).gameObject;
    }
	//初始化玩家对象生命值相关属性
	void Init(){
		currentHP = maxHP;
		isAlive = true;		
		timer = 0.0f;
	}

	//每帧执行一次，检查玩家无敌状态
	void Update () {
		if (!photonView.isMine)		//不是本地玩家对象，结束函数执行
			return;
		timer += Time.deltaTime;	//累加玩家对象的无敌时间
		if (invincible == true && timer > invincibleTime)					//使用RPC，设置所有客户端该玩家对象的无敌状态
			photonView.RPC ("SetInvincible", PhotonTargets.All, false);
		else if (invincible == false && timer <= invincibleTime)
			photonView.RPC ("SetInvincible", PhotonTargets.All, true);
	}

	//RPC函数，设置玩家的无敌状态
	[PunRPC]
	void SetInvincible(bool isInvincible){
		invincible = isInvincible;
        if (isInvincible)
            SetMaterials(1.0f, invincibleColor);
        else 
        {
            if (team != GameManager.gm.localPlayer.GetComponent<PlayerHealth>().team)
                SetMaterials(1.0f, enemyColor);
            else
                SetMaterials(0.0f, enemyColor);
        }
	}

	//玩家扣血函数，只有MasterClient可以调用
	public void TakeDamage(int damage,PhotonPlayer attacker){
		if (!isAlive || invincible)				//玩家死亡或者无敌，不执行扣血函数
			return;
		if (PhotonNetwork.isMasterClient) {		//MasterClient调用
            int newHP = currentHP - damage;
            if (newHP <= 0 && attacker!=null) {                 //如果玩家受到攻击后死亡
                newHP = 0;
                GameManager.gm.AddScore (killScore, attacker);      //击杀者增加分数
            }
            photonView.RPC("UpdateHP", PhotonTargets.All, newHP);   //更新所有客户端，该玩家对象的生命值
        }
	}
    public void TakeDamageFromZombie(int damage)
    {
        if (!isAlive || invincible)				//玩家死亡或者无敌，不执行扣血函数
            return;
        if (PhotonNetwork.isMasterClient){		//MasterClient调用
            int newHP = currentHP - damage;
            if (newHP <= 0)
            {                 //如果玩家受到攻击后死亡
                newHP = 0;
            }
            photonView.RPC("UpdateHP", PhotonTargets.All, newHP);	//更新所有客户端，该玩家对象的生命值
        }

    }

	//玩家加血函数
	public void requestAddHP(int value)
	{
		photonView.RPC ("AddHP", PhotonTargets.MasterClient, value);	//使用RPC,向MasterClient发起加血请求
	}

	//RPC函数，增加玩家血量
	[PunRPC]
	public void AddHP(int value)
	{
		if (!PhotonNetwork.isMasterClient)		//加血函数只能由MasterClient调用
			return;
		if (!isAlive || currentHP == maxHP)		//玩家已死亡，或者玩家满血，不执行加血逻辑
			return;
		currentHP += value;				//玩家加血
		if (currentHP > maxHP) {		//加血后，玩家生命值不能超过最大生命值
			currentHP = maxHP;
		}
		photonView.RPC ("UpdateHP", PhotonTargets.All, currentHP);	//使用RPC，更新所有客户端，该玩家对象的血量
	}


	//RPC函数，更新玩家血量
	[PunRPC]
	void UpdateHP(int newHP)
	{
        if (photonView.isMine &&  currentHP > newHP)
            GameManager.gm.UIController.showDamageFlash();
		currentHP = newHP;		//更新玩家血量
		if (currentHP <= 0) {	//如果玩家已死亡
			isAlive = false;
			if (photonView.isMine) {					//如果是本地客户端
				anim.SetBool ("isDead", true);			//播放玩家死亡动画
				Invoke ("PlayerSpawn", respawnTime);	//使用invoke函数，复活玩家
                GameManager.gm.UpdateDeath();
			}
            else
            {
                Invoke("PlayerDisable", disableTime);
            }
			rigid.useGravity = false;		//禁用玩家重力
			colli.enabled = false;			//禁用玩家碰撞体
			gun.SetActive (false);			//禁用玩家枪械
			anim.applyRootMotion = true;	//玩家位置与朝向受动画影响
			GetComponent<IKController> ().enabled = false;	//禁用IK
		}
	}

	//玩家复活函数
	void PlayerSpawn(){
		photonView.RPC ("PlayerReset", PhotonTargets.AllViaServer);	//使用RPC，初始化复活时的玩家属性
		Transform spawnTransform = null;
		int rand = Random.Range (0, 4);						//随机获得玩家复活位置
		if (PhotonNetwork.player.customProperties ["Team"].ToString () == "AttackerTeam")
			spawnTransform = GameManager.gm.attackerTeamSpawnTransform [rand];
		else if(PhotonNetwork.player.customProperties["Team"].ToString()=="DefenderTeam")
			spawnTransform = GameManager.gm.defenderTeamSpawnTransform [rand];
		transform.position = spawnTransform.position;		//玩家在随机位置复活
		transform.rotation = Quaternion.identity;
	}

    //禁用玩家对象
    void PlayerDisable()
    {
        gameObject.SetActive(false);
    }

	//RPC函数，初始化复活时的玩家属性
	[PunRPC]
	void PlayerReset(){
		Init ();		//初始化玩家血量与无敌状态
        gameObject.SetActive(true);
		rigid.useGravity = true;			//启用玩家重力
		colli.enabled = true;				//启用玩家碰撞体
		gun.SetActive (true);				//启用玩家枪械
		anim.SetBool ("isDead", false);		//播放玩家停驻动画
		anim.applyRootMotion = false;		//玩家位置与朝向不受动画影响
		GetComponent<IKController> ().enabled = true;	//启用IK
    }

	//RPC函数，设置玩家队伍
	[PunRPC]
	void SetTeam(string newTeam){
		team = newTeam;
        //SetMaterials(_RimC);
    }

    //设置敌方队伍泛红
    void SetMaterials(float rimBool,Color rimColor)
    {
        Renderer[] rends = GetComponentsInChildren<SkinnedMeshRenderer>();
        int rendCount = rends.Length;
        for(int i = 0; i < rendCount; i++)
        {
            int materialCount = rends[i].materials.Length;
            for(int j=0;j<materialCount;j++)
                if(rends[i].materials[j].shader.name == "Custom/body_shader")
                {
                    rends[i].materials[j].SetColor("_RimColor", rimColor);
                    rends[i].materials[j].SetFloat("_RimBool", rimBool);
                }
        }
    }
}
