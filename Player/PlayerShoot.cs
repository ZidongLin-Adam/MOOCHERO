using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityStandardAssets.CrossPlatformInput;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

public class PlayerShoot : PunBehaviour {

	public int shootingDamage = 10;				//射击伤害
	public float shootingRange = 50.0f;			//射击范围
	public float timeBetweenShooting = 0.2f;	//射击间隔
    public float recoilForce = 1.0f;            //后坐力

	public Transform gunTransform;			//枪械对象的Transform属性
	public Transform gunBarrelEnd;			//枪口对象的Transform属性
	public GameObject gunShootingEffect;	//枪口射击效果
	public GameObject bulletEffect;			//子弹爆炸效果
	public AudioClip shootingAudio;			//枪械射击音效

    public GameObject lineRenderer;
    private LineRenderer lineObj;

    public GameObject bloodEffect;          //冒血特效

    private Vector3 gunInitLocalPos;        //枪的初始位置

    private Camera myCamera;
	[HideInInspector]
	public Vector3 shootingPosition 		//枪械射击位置
		= new Vector3(-0.238f,1.49f,0.065f);

	PlayerHealth playerHealth;
	bool isShooting;
	Ray ray;
	RaycastHit hitInfo;
	float timer;

	//初始化
	void Start(){ 
		playerHealth = GetComponent<PlayerHealth> ();	//获取玩家PlayerHealth组件
        myCamera = Camera.main;
		timer = 0.0f;                                   //初始化玩家与上次射击的间隔
        if (photonView.isMine)
        {
            //根据本地玩家的天赋等级技能，设置玩家的射击参数
            Dictionary<string, string> gunData = null;
            foreach (CatalogItem i in GameInfo.catalogItems)
            {
                if (i.ItemClass == PlayFabUserData.equipedWeapon)
                {
                    gunData = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(i.CustomData);
                    break;
                }
            }
            Dictionary<string, string> skillData = null;
            float value;
            int myShootingDamage = 0;
            float myShootingRange = 0.0f, myTimeBetweenShooting = 0.0f, myRecoilForce = 1.0f;
            foreach (KeyValuePair<string, string> kvp in gunData)
            {
                switch(kvp.Key)
                {
                    //计算枪械威力
                    case "枪械威力":
                        skillData = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(GameInfo.titleData["ShootingDamageSkill"]);
                        value = float.Parse(kvp.Value) * (1.0f + float.Parse(skillData["Level" + PlayFabUserData.shootingDamageSkillLV.ToString()]) / 100);
                        myShootingDamage = (int)value;
                        break;
                    //计算枪械射程
                    case "枪械射程":
                        skillData = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(GameInfo.titleData["ShootingRangeSkill"]);
                        value = float.Parse(kvp.Value) * (1.0f + float.Parse(skillData["Level" + PlayFabUserData.shootingRangeSkillLV.ToString()]) / 100);
                        myShootingRange = value;
                        break;
                    //计算射击间隔
                    case "射击间隔":
                        skillData = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(GameInfo.titleData["ShootingIntervalSkill"]);
                        value = float.Parse(kvp.Value) * (1.0f - float.Parse(skillData["Level" + PlayFabUserData.shootingIntervalSkillLV.ToString()]) / 100);
                        myTimeBetweenShooting = value;
                        break;
                    //计算后坐力（暂时不影响玩家射击）
                    case "后坐力":
                        myRecoilForce = float.Parse(kvp.Value);
                        break;
                }
            }
            //初始化其他节点中，该玩家的射击参数
            photonView.RPC("Init", PhotonTargets.All, PlayFabUserData.equipedWeapon, myShootingDamage, myShootingRange, myTimeBetweenShooting, myRecoilForce);
        }
	}

	[PunRPC]
	void Init(string equipedWeapon,int newShootingDamage,float newShootingRange, float newTimeBetweenShooting,float newRecoilForce)
    {
        shootingDamage = newShootingDamage;
        shootingRange = newShootingRange;
        timeBetweenShooting = newTimeBetweenShooting;
        recoilForce = newRecoilForce;
        IKController ikController = GetComponent<IKController> ();
        //根据玩家装备的枪械，实例化对应的枪械
		foreach (KeyValuePair<string,Sprite> kvp in GameInfo.guns) {
			Transform newGunTransform = transform.Find (kvp.Key);
			if (newGunTransform.name == equipedWeapon) {
				newGunTransform.gameObject.SetActive (true);
				if(photonView.isMine)
					newGunTransform.parent = Camera.main.transform;
                //设置动画IK
				ikController.leftHandObj = newGunTransform.Find ("LeftHandObj");
				ikController.rightHandObj = newGunTransform.Find ("RightHandObj");
				ikController.lookObj = newGunTransform.Find ("GunBarrelEnd");

				gunTransform = newGunTransform;
				gunBarrelEnd = newGunTransform.Find ("GunBarrelEnd");
				shootingAudio = GameInfo.gunShootingAudios [equipedWeapon];

                gunInitLocalPos = newGunTransform.localPosition;
			} else
				newGunTransform.gameObject.SetActive (false);
		}
	}

	//每帧执行，处理射击逻辑
	void Update () {
		if (!photonView.isMine || !playerHealth.isAlive)	//如果玩家对象不属于本地客户端，或者玩家已死亡，解说函数执行
			return;
		timer += Time.deltaTime;	//更新与上次射击的间隔
		if (CrossPlatformInputManager.GetButton("Fire1") && timer >= timeBetweenShooting) {	//玩家按下射击键，与上次射击的间隔超过射击间隔
			timer = 0.0f;			//将玩家与上次射击的间隔清零
				
            photonView.RPC ("Shoot", PhotonTargets.MasterClient, PhotonNetwork.player,myCamera.transform.position);	//使用RPC,调用MasterClient的Shoot函数，函数参数为发起射击的玩家
            gunTransform.localPosition = gunInitLocalPos + Vector3.back * 0.3f * recoilForce;
		}
        posRectify();
        enemyInfoCheck();
	}
    //修正由于枪后坐力产生的位移
    void posRectify()
    {
        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, gunInitLocalPos, Time.deltaTime * 10.0f);
    }
    //每一帧射出射线检测
    void enemyInfoCheck()
    {
        Vector3 rotatedPos = transform.rotation * shootingPosition; //随着角色的旋转，射击的初始位置也需要跟着旋转
        ray.origin = rotatedPos + transform.position;	//设置射击射线的起始端点
        ray.direction = gunTransform.forward;				//设置射击射线的方向
        GameManager.gm.UIController.EnableEnemyHpLabel(false);
        if (Physics.Raycast(ray, out hitInfo, shootingRange))
        {
            GameObject go = hitInfo.collider.gameObject;			//获取被击中的游戏对象
            if (go.tag == "Player")//如果击中玩家
            {								
                PlayerHealth playerHealth = go.GetComponent<PlayerHealth>();
                if (playerHealth.team != GetComponent<PlayerHealth>().team)
                {
                    GameManager.gm.UIController.EnableEnemyHpLabel(true);
                    GameManager.gm.UIController.updateEnemyHpUI(playerHealth.currentHP, playerHealth.maxHP);
                }
                GameManager.gm.UIController.updateCrossIconColor(Color.red);
            }
            else if (go.tag == "Zombie" || go.tag == "Skeleton")//如果击中僵尸或者守护者
            {						
                ZombieHealth zh = go.GetComponent<ZombieHealth>();
                GameManager.gm.UIController.EnableEnemyHpLabel(true);
                GameManager.gm.UIController.updateEnemyHpUI(zh.currentHP, zh.maxHP);
                GameManager.gm.UIController.updateCrossIconColor(Color.red);
            }
            else
            {
                GameManager.gm.UIController.updateCrossIconColor(Color.white);
            }
        }
        else
        {
            GameManager.gm.UIController.updateCrossIconColor(Color.white);
        }
    }

	/**RPC函数，执行射击逻辑
	 * 该函数只能由MasterClient调用
	 */
	[PunRPC]
	void Shoot(PhotonPlayer attacker,Vector3 shootPosition){
		if (!PhotonNetwork.isMasterClient)		//如果玩家不是MasterClient，结束函数的执行
			return;
        Vector3 rotatedPos = transform.rotation * shootingPosition; //随着角色的旋转，射击的初始位置也需要跟着旋转
        ray.origin = rotatedPos + transform.position;	//设置射击射线的起始端点
		ray.direction = gunTransform.forward;				//设置射击射线的方向

        var hitTag = ""; //击中物体的Tag
        if (lineRenderer != null && lineObj == null)
        {
             GameObject go= Instantiate(lineRenderer,
                Vector3.zero,
                Quaternion.identity) as GameObject;

            lineObj = go.GetComponent<LineRenderer>();
        }

		Vector3 bulletEffectPosition;						//子弹爆炸效果的位置

		//发出射击射线，判断是否击中物体
		if (Physics.Raycast (ray, out hitInfo, shootingRange)) {	//如果射线击中游戏对象
			GameObject go = hitInfo.collider.gameObject;			//获取被击中的游戏对象
			if (go.tag == "Player") {								//如果击中玩家
                PlayerHealth playerHealth = go.GetComponent<PlayerHealth>();
				if (playerHealth.team != GetComponent<PlayerHealth> ().team) {	//如果被击中玩家队伍与攻击者玩家队伍不同
					playerHealth.TakeDamage (shootingDamage, attacker);			//被击中玩家扣血
				}
			} else if (go.tag == "Zombie" || go.tag == "Guard" || go.tag == "Skeleton") {						//如果击中僵尸
				ZombieHealth zh = go.GetComponent<ZombieHealth> ();
                if (zh != null)
                {
                    zh.TakeDamage(shootingDamage, shootPosition,attacker);		//僵尸扣血
				}
			}
            hitTag = go.tag;
			bulletEffectPosition = hitInfo.point;					//击中游戏对象，子弹爆炸效果的位置在击中点
		}else
			bulletEffectPosition = ray.origin + shootingRange * ray.direction;	//如果未击中，子弹爆炸效果为子弹失效的位置

        if (lineObj != null)
        {
            lineObj.SetWidth(0.02F, 0.02F);
            lineObj.SetPosition(0, ray.origin);
            lineObj.SetPosition(1, bulletEffectPosition);
        }
		photonView.RPC ("ShootEffect", PhotonTargets.AllViaServer, bulletEffectPosition,hitTag);//使用RPC，调用所有玩家对象的ShootEffect函数，显示射击效果
	}

	//RPC函数，显示射击效果
	[PunRPC]
	void ShootEffect(Vector3 bulletEffectPosition , string hitTag){
		AudioSource.PlayClipAtPoint (shootingAudio, transform.position);	//播放射击音效
		if (gunShootingEffect != null && gunBarrelEnd != null) {			//播放枪口射击效果				
			GameObject shootingEffect = Instantiate (gunShootingEffect, 
				                gunBarrelEnd.position, 
				                gunBarrelEnd.rotation) as GameObject;
			shootingEffect.transform.parent = gunBarrelEnd;
		}
		if (bulletEffect != null) {											//播放子弹爆炸效果			
			Instantiate (bulletEffect, 
				bulletEffectPosition, 
				Quaternion.identity);
            //var bloodSpring = GameObject.Instantiate(bloodEffect, bulletEffectPosition, Quaternion.identity);
            //GameObject.Destroy(bloodSpring, 1.0f);
		}
        if (photonView.isMine && bloodEffect)//玩家并不关心其他玩家的表现，所以只是自己打中僵尸时冒血
        {
            if (hitTag == "Zombie" || hitTag == "Skeleton" || hitTag == "Guard")
            {
                var bloodSpring = GameObject.Instantiate(bloodEffect,bulletEffectPosition,Quaternion.identity);
                GameObject.Destroy(bloodSpring, 0.2f);
            }
        }

	}
}
