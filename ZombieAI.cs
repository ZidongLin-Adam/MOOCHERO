using UnityEngine;
using System.Collections;
using Photon;
public class ZombieAI : PunBehaviour {

	public enum FSMState	
	{
		Wander,
		Track,	
		Attack,	
		Dying,
		Dead	
	}

	public float currentSpeed = 0.0f;	
	public float wanderSpeed = 0.9f;	
	public float trackingSpeed = 4.0f;	
	public float wanderScope = 15.0f;	
	public float attackRange = 1.5f;		
	public float attackFieldOfView = 60.0f;	
	public float attackInterval = 0.8f;		
	public int attackDamage = 10;		
	public FSMState curState;			

	private Vector3 previousPos = Vector3.zero;	
	private float stopTime = 0;				
	private float attackTime = 0.0f;			
	private float disappearTimer = 0.0f;	
	private bool disappeared = false;		

	private Transform zombieTransform;		
	private Animator animator;				
	private NavMeshAgent agent;				
	private ZombieHealth zombieHealth;		
	private ZombieSoundSensor zombieSoundSensor;
	private ZombieRender zombieRender;			

	private Transform targetPlayer;			
	void OnEnable()
	{
		zombieTransform = transform;				
		animator = GetComponent<Animator>();		
		agent = GetComponent<NavMeshAgent>();		

		zombieHealth = GetComponent<ZombieHealth> ();	
		zombieSoundSensor = GetComponentInChildren<ZombieSoundSensor> ();	
		zombieRender = GetComponent<ZombieRender>();	

		targetPlayer = null;						
		curState = FSMState.Wander;					
	}

	public void requestDisable(){
		photonView.RPC ("DisableZombie", PhotonTargets.All);
	}

	[PunRPC]
	void DisableZombie()
	{
		zombieTransform.gameObject.SetActive (false);
	}
		

	void FixedUpdate()
	{

		if (PhotonNetwork.isMasterClient) 
		{
			FSMUpdate ();
		}
	}

	void FSMUpdate()
	{
		if (GameManager.gm.state != GameManager.GameState.Playing) {
			if (curState == FSMState.Attack || curState == FSMState.Track) {
				curState = FSMState.Wander;
				animator.SetBool ("isAttack", false);
			}
		}
		switch (curState)
		{
		case FSMState.Wander: 
			UpdateWanderState();
			break;
		case FSMState.Track:
			UpdateTrackState();
			break;
		case FSMState.Attack:
			UpdateAttackState();
			break;
		case FSMState.Dying:
			UpdateDyingState();
			break;
		case FSMState.Dead:
			UpdateDeadState ();
			break;
		}
	}

	protected bool AgentDone()
	{
		return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
	}
	void UpdateWanderState()
	{
		targetPlayer = zombieSoundSensor.getNearestPlayer ();
		if (targetPlayer != null && GameManager.gm.state == GameManager.GameState.Playing) {
			curState = FSMState.Track;
			agent.ResetPath ();
			return;
		}

		if (AgentDone ()) {
			Vector3 randomRange = new Vector3 ((Random.value - 0.5f) * 2 * wanderScope, 0, (Random.value - 0.5f) * 2 * wanderScope);
			Vector3 nextDestination = zombieTransform.position + randomRange;

			agent.destination = nextDestination;

		} 
		else if(stopTime > 1.0f)
		{
			Vector3 nextDestination = zombieTransform.position - zombieTransform.forward * (Random.value) * wanderScope;

			agent.destination = nextDestination;

		}

		Vector3 targetVelocity = Vector3.zero;
		if (agent.desiredVelocity.magnitude > wanderSpeed) {
			targetVelocity = agent.desiredVelocity.normalized * wanderSpeed;
		} else {
			targetVelocity = agent.desiredVelocity;
		}
		agent.velocity = targetVelocity;
		currentSpeed = agent.velocity.magnitude;

		animator.SetFloat("Speed", currentSpeed);

		if (previousPos == Vector3.zero) 
		{
			previousPos = zombieTransform.position;
		}
		else 
		{
			Vector3 posDiff = zombieTransform.position - previousPos;
			if (posDiff.magnitude > 0.5) {
				previousPos = zombieTransform.position;
				stopTime = 0.0f;
			} else {
				stopTime += Time.deltaTime;
			}
		}

		if (zombieRender != null && zombieRender.isCrazy == true)
			photonView.RPC ("ZombieSetNormal", PhotonTargets.All);
	}

	void UpdateTrackState()
	{
		targetPlayer = zombieSoundSensor.getNearestPlayer ();
		if (targetPlayer == null) {
			curState = FSMState.Wander;
			agent.ResetPath ();
			return;
		}
		if (Vector3.Distance(targetPlayer.position, zombieTransform.position)<=attackRange) {
			curState = FSMState.Attack;
			agent.ResetPath ();
			return;
		}

		Vector3 targetVelocity = Vector3.zero;
		if (agent.desiredVelocity.magnitude > trackingSpeed) {
			targetVelocity = agent.desiredVelocity.normalized * trackingSpeed;
		} else {
			targetVelocity = agent.desiredVelocity;
		}
		agent.velocity = targetVelocity;
		currentSpeed = agent.velocity.magnitude;
			
		animator.SetFloat("Speed", currentSpeed);

		if (zombieRender != null && zombieRender.isCrazy == false)
			photonView.RPC ("ZombieSetCrazy", PhotonTargets.All);
	}

	void UpdateAttackState()
	{

	}

	void UpdateDyingState()
	{
		
	}

	void UpdateDeadState()
	{

	}

}