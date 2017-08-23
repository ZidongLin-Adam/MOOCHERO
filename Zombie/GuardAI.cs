using UnityEngine;
using System.Collections;
using Photon;
public class GuardAI : PunBehaviour {

	public enum GuardFSMState
	{
        Idle,       //空闲状态
		Chase,		//追踪状态
		Attack,		//攻击状态
		Dead,		//死亡状态
	}
	public float runSpeed = 4.0f;				//	guard奔跑速度
    public Transform SourcePointTransform;     //guard守护原点
    static public float guardActiveRange = 30f;              //guard活动范围

	public float attackRange = 4.0f;			//	guard攻击距离
	public float attackFieldOfView = 60.0f;		//	guard攻击夹角
	public float attackInterval = 0.8f;			//	guard攻击间隔
	public int attackDamage = 10;				//	guard攻击力
	public AudioClip attackAudio; 		        //	guard攻击音效

	public GuardFSMState currentState;				//guard当前状态
	public float currentSpeed = 0.0f;			//guard当前速度

	private float attackTimer = 0.0f;			//guard攻击计时器

	private NavMeshAgent		agent;			//导航代理组件
	private Animator			animator;		//动画控制器组件
	private Transform 			guardTransform;//guardtransform组件
	private ZombieHealth	guardHealth;		//guard生命值管理组件
	private GuardSensor   guardSensor;			//guard感知器组件
	
    private Transform targetPlayer;				//guard感知范围内的玩家

	private bool firstInDead = true;			//guard是否首次进入死亡状态
    private float disappearTimer = 0.0f;
    //启动时调用
    void Start()
	{
		//获取guard的各种组件
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		guardHealth = GetComponent<ZombieHealth> ();
		guardSensor = GetComponentInChildren<GuardSensor> ();
		guardTransform = transform;
		//把guard感知到的玩家字段设置为null
		targetPlayer = null;
		
        //获取guard感知范围
        var ob = GameObject.Find("GuardSenseArea");
        guardActiveRange = ob.GetComponent<MeshFilter>().mesh.bounds.size.x * ob.transform.localScale.x;

	}
    //守护者使能 禁用/启用守护者的一些组件
    [PunRPC]
    public void EnableGuard(bool enable)
    {
        targetPlayer = null;
        agent.enabled = enable;
        guardHealth.enabled = enable;
        guardSensor.enabled = enable;
        GetComponent<CapsuleCollider>().enabled = enable;
        GetComponent<Rigidbody>().useGravity = enable;
        GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
    }
	//相当于让守护者开始工作
	public void Born()
	{
		//把guard感知到的玩家字段设置为null血量
		targetPlayer = null;
		//把guard的初始化状态设置为游荡状态，
		currentState = GuardFSMState.Idle;
		//初始化guard生命值
		guardHealth.currentHP = guardHealth.maxHP;
		//启用导航代理组件

        EnableGuard(true);
        if(PhotonNetwork.isMasterClient)
		    agent.ResetPath ();

		//启用动画控制器
		animator.applyRootMotion = false;

		firstInDead = true;

	}
	//定期更新guard状态机的状态
	void FixedUpdate()
	{
        if (PhotonNetwork.isMasterClient /*&& AIInitFinish*/)
        {
            FSMUpdate();
        }
	}
	//guard状态机更新函数
	void FSMUpdate()
	{
		//根据guard当前的状态调用相应的状态处理函数
		switch (currentState)
		{
		case GuardFSMState.Idle: 
			UpdateIdleState();
			break;
		case GuardFSMState.Chase:
			UpdateChaseState();
			break;
		case GuardFSMState.Attack:
			UpdateAttackState();
			break;
		case GuardFSMState.Dead:
			UpdateDeadState ();
			break;
		}
		//如果guard处于非死亡状态，但是生命值减为0，那么进入死亡状态
		if (currentState != GuardFSMState.Dead && !guardHealth.IsAlive) 
		{
			currentState = GuardFSMState.Dead;
		}
	}

	//判断guard是否在一次导航中到达了目的地
	protected bool AgentDone()
	{
		return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
	}

	//限制guard的当前移动速度，更新动画状态机
	private void setMaxAgentSpeed(float maxSpeed)
	{
		Vector3 targetVelocity = Vector3.zero;
		if (agent.desiredVelocity.magnitude > maxSpeed) {
			targetVelocity = agent.desiredVelocity.normalized * maxSpeed;
		} else {
			targetVelocity = agent.desiredVelocity;
		}
		agent.velocity = targetVelocity;
		currentSpeed = agent.velocity.magnitude;
		//设置动画状态
		animator.SetFloat("Speed", currentSpeed);
	}
    //空闲状态处理函数
    void UpdateIdleState()
    {
        targetPlayer = guardSensor.getNearbyPlayer();
        if (targetPlayer != null)
        {
            currentState = GuardFSMState.Chase;
            agent.ResetPath();
            animator.SetBool("isAttack", false);
            return;
        }
        //设置移动目标为守护原点
        agent.SetDestination(SourcePointTransform.position);

        if (AgentDone()) //回到了守护原点
            setMaxAgentSpeed(0);
        else
            setMaxAgentSpeed(runSpeed);
        
    }
	//追踪状态处理函数
	void UpdateChaseState()
	{
		//如果guard感知范围内没有玩家，进入游荡状态
		targetPlayer = guardSensor.getNearbyPlayer ();
		if (targetPlayer == null) {
			currentState = GuardFSMState.Idle;
			agent.ResetPath ();
			return;
		}
		//如果玩家与guard的距离，小于guard的攻击距离，那么进入攻击状态
		if (Vector3.Distance(targetPlayer.position, guardTransform.position)<=attackRange) {
			currentState = GuardFSMState.Attack;
			agent.ResetPath ();
			return;
		}
		//设置移动目标为玩家
		agent.SetDestination (targetPlayer.position);

		//限制追踪的速度
		setMaxAgentSpeed(runSpeed);
	}
    //进攻状态定时更新
	void UpdateAttackState()
	{
		//如果guard感知范围内没有玩家，进入游荡状态
		targetPlayer = guardSensor.getNearbyPlayer ();
		if (targetPlayer == null) {
			currentState = GuardFSMState.Idle ;
			agent.ResetPath ();
			animator.SetBool ("isAttack", false);
			return;
		}
		//如果玩家与guard的距离，大于guard的攻击距离，那么进入追踪状态
		if (Vector3.Distance(targetPlayer.position, guardTransform.position)>attackRange) {
			currentState = GuardFSMState.Chase;
			agent.ResetPath ();
			animator.SetBool ("isAttack", false);
			return;
		}
		PlayerHealth ph = targetPlayer.GetComponent<PlayerHealth> ();
		if (ph != null)
		{
			//计算guard的正前方和玩家的夹角，只有玩家在guard前方才能攻击
			Vector3 direction = targetPlayer.position - guardTransform.position;
			float degree = Vector3.Angle (direction, guardTransform.forward);
			if (degree < attackFieldOfView / 2 && degree > -attackFieldOfView / 2) {
				animator.SetBool ("isAttack", true);
				if (attackTimer > attackInterval) {
					attackTimer = 0;
                    if (attackAudio != null)
                        photonView.RPC("PlayAttackAudio", PhotonTargets.All);
					ph.TakeDamageFromZombie (attackDamage);
				}
				attackTimer += Time.deltaTime;
			} else {
				//如果玩家不在guard前方，guard需要转向后才能攻击
				animator.SetBool ("isAttack", false);
				guardTransform.LookAt(targetPlayer);
			}
		}
		//攻击状态下的敌人应当连续追踪玩家
		agent.SetDestination (targetPlayer.position);
		//限制追踪的速度
		setMaxAgentSpeed(runSpeed);
	}
    //死亡状态定时更新
	void UpdateDeadState()
	{
		//如果guard初次进入死亡状态，那么需要禁用guard的一些组件
        if (firstInDead)
        {
            firstInDead = false;
            animator.SetBool("isAttack", false);
            animator.SetTrigger("toDead");
            animator.SetFloat("Speed", 0);
            animator.applyRootMotion = true;
            EnableGuard(false);
            this.photonView.RPC("EnableGuard", PhotonTargets.Others, false);
        }
        else
        { //超过了一段时间 守护者消失
            disappearTimer += Time.deltaTime;
            if (disappearTimer > 2.5f)
                this.photonView.RPC("guardDisappear", PhotonTargets.All);
        }
	}
    //使守护者消失  守护者死亡后调用
    [PunRPC]
    void guardDisappear()
    {
        this.gameObject.SetActive(false);
    }
    //同步数据函数
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting) 						//本地玩家发送数据
        {
            stream.SendNext(currentState); //同步当前状态 是否第一次进入死亡状态 和动画机的rootMotion标记
            stream.SendNext(firstInDead);
            stream.SendNext(animator.applyRootMotion);
        }
        else 										//远程玩家接收数据
        {
            currentState = (GuardFSMState)stream.ReceiveNext();
            firstInDead = (bool)stream.ReceiveNext();
            animator.applyRootMotion = (bool)stream.ReceiveNext();
        }
    }
    //播放进攻音效
    [PunRPC]
    void PlayAttackAudio()
    {
        AudioSource.PlayClipAtPoint(attackAudio, transform.position);
    }
}