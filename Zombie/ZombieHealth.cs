using UnityEngine;
using System.Collections;
using Photon;
public class ZombieHealth :PunBehaviour{

	public int currentHP = 10;		//僵尸当前生命值
	public int maxHP = 10;			//僵尸最大生命值
	public int killScore = 5;		//僵尸被击杀后，玩家的得分
	public AudioClip enemyHurtAudio;		//僵尸受伤音效
    public string OwnerTag = "Zombie"; //拥有者标识 有可能是僵尸 或者是守护者

	[HideInInspector]
	public Vector3 damageDirection = Vector3.zero;	//保存僵尸收到攻击时，攻击者所在的方向
	[HideInInspector]
	public bool getDamaged = false;					//保存僵尸是否收到攻击

	public bool IsAlive {
		get {
			return currentHP > 0;
		}
	}
    //仅仅由masterclient调用
	public void TakeDamage(int damage, Vector3 shootPosition , PhotonPlayer attacker){
		if (!IsAlive)
			return;
		//更新僵尸生命值
        if (OwnerTag == "Guard")
        {
            if (Vector3.Distance(shootPosition, GetComponent<GuardAI>().SourcePointTransform.position) >(GuardAI.guardActiveRange / 2))
            {//在圆圈之外攻击守护者会没有伤害 并显示提示文本
                damage = 0;
                photonView.RPC("UpdateHintText", attacker);
            }
            GuardSensor.gs.addPlayerDamageRecord(attacker,damage); //添加伤害记录
        }
		currentHP -= damage;
		if (currentHP <= 0 ) currentHP = 0;
        if (IsAlive)
        {
            //记录僵尸的中枪状态
            getDamaged = true;
            //记录僵尸受到攻击时，玩家所在的方向
            damageDirection = shootPosition - transform.position;
            damageDirection.Normalize();
        }
        else { }

        this.photonView.RPC("UpdateHP", PhotonTargets.All, currentHP);
        if (enemyHurtAudio != null)             //在敌人位置处播放敌人受伤音效
            photonView.RPC("PlayHurtAudio", PhotonTargets.All);
	}
    //重置生命值
    public void resetHp()
    {
        currentHP = maxHP;
        this.photonView.RPC("UpdateHP", PhotonTargets.All, currentHP);
    }
    //更新生命值
    [PunRPC]
    void UpdateHP(int newHp)
    {
        currentHP = newHp;
    }
    //设置更新提示文本
    [PunRPC]
    void UpdateHintText()
    {
        GameManager.gm.UIController.showHintText("请站在光圈内攻击守护者");
    }
    //播放受伤音频
    [PunRPC]
    void PlayHurtAudio()
    {
        AudioSource.PlayClipAtPoint(enemyHurtAudio, transform.position);
    }
}
