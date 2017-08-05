using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

    public enum MonState
    {
        Wander,
        Track,
        Attack,
        Dying,
        Dead
    }

    public float wanderScope = 10.0f;
    public float wanderSpeed = 2.0f;


    private MonState currentState;
    private Transform selfTransform;
    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 previousPos = Vector3.zero; 
    private float stopTime = 0;
    private float currentSpeed;



    void Awake()
    {
        selfTransform = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        currentState = MonState.Wander;
        
        
    }

    private void updateMonState()
    {
        switch (currentState)
        {
            case MonState.Wander : UpdateWanderState();
                break;
            case MonState.Track : UpdateTrackState();
                break;
            case MonState.Attack : UpdateAttackState();
                break;
            case MonState.Dying : UpdateDyingState();
                break;
            case MonState.Dead: UpdateDeadState();
                break;
        }
    }


    //检测是否到达目的地
    protected bool AgentDone()
    {
        //if((!agent.pathPending) && (agent.remainingDistance <= agent.stoppingDistance == true))
            //Debug.Log(gameObject.name+"到达目标了！");
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private void UpdateWanderState()
    {
        

        if (AgentDone())
        {
            Vector3 randomRange = new Vector3((Random.value - 0.5f) * 2 * wanderScope, 0, (Random.value - 0.5f) * 2 * wanderScope);
            Vector3 nextDestination = selfTransform.position + randomRange;

            agent.destination = nextDestination;
            //Debug.Log(gameObject.name+"现在出发！目标是" + agent.destination + "====随机点是" + randomRange);
        }
        else if (stopTime > 1.0f)
        {
            Vector3 nextDestination = selfTransform.position - selfTransform.forward * (Random.value) * wanderScope;
            agent.destination = nextDestination;
        }

        //检测是否被卡住
        if (previousPos == Vector3.zero)
        {
            previousPos = selfTransform.position;
        }
        else
        {
            Vector3 posDiff = selfTransform.position - previousPos;
            if (posDiff.magnitude > 0.5)
            {
                previousPos = selfTransform.position;
                stopTime = 0.0f;
            }
            else
            {
                stopTime += Time.deltaTime;
            }
        }
        //限制游荡的速度
        Vector3 targetVelocity = Vector3.zero;
        if (agent.desiredVelocity.magnitude > wanderSpeed)
        {
            targetVelocity = agent.desiredVelocity.normalized * wanderSpeed;
        }
        else
        {
            targetVelocity = agent.desiredVelocity;
        }
        agent.velocity = targetVelocity;
        currentSpeed = agent.velocity.magnitude;

        animator.SetFloat("WalkSpeed", currentSpeed);
        //Debug.Log("desiredVelocity: " + agent.desiredVelocity.magnitude);
        //Debug.Log("velocity: " + agent.velocity.magnitude);

    }

    private void UpdateTrackState()
    {

    }

    private void UpdateAttackState()
    {

    }

    private void UpdateDyingState()
    {

    }

    private void UpdateDeadState()
    {

    }

    // Use this for initialization
    void Start () {
        
        
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    private void FixedUpdate()
    {
        updateMonState();
    }
}
