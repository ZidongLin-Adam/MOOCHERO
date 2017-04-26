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

    public float wanderScope = 15.0f;

    private MonState currentState;
    private Transform selfTransform;
    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 previousPos = Vector3.zero; 
    private float stopTime = 0;                 
                                                


    void Awake()
    {
        selfTransform = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        currentState = MonState.Wander;
        agent.speed = 20.0f;
        
    }

    private void updateMonState()
    {
        switch (currentState)
        {
            case MonState.Wander : updateWanderState();
                break;
            case MonState.Track : updateTrackState();
                break;
            case MonState.Attack : updateAttackState();
                break;
            case MonState.Dying : updateDyingState();
                break;
            case MonState.Dead: updateDeadState();
                break;
        }
    }
    protected bool AgentDone()
    {
        //if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance == true)
            //Debug.Log("我到达目标了！");
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private void updateWanderState()
    {
        /*bool reachDestination = true;
        Vector3 randomDestination = new Vector3(selfTransform.position.x + Random.Range(-5.0f, 5.0f), 0, selfTransform.position.z + Random.Range(-5.0f, 5.0f));
        if (reachDestination == true)
        {
            randomDestination = new Vector3(selfTransform.position.x + Random.Range(-5.0f, 5.0f), 0, selfTransform.position.z + Random.Range(-5.0f, 5.0f));
            reachDestination = false;
            agent.destination = selfTransform.position + randomDestination;
            Debug.Log("我现在出发！目标是"+ agent.destination + "====随机点是"+ randomDestination);
        }
        if (selfTransform.position == randomDestination)
        {
            reachDestination = true;
            Debug.Log("我到达目标了！");
        }*/
        if (AgentDone())
        {
            Vector3 randomRange = new Vector3((Random.value - 0.5f) * 2 * wanderScope, 0, (Random.value - 0.5f) * 2 * wanderScope);
            Vector3 nextDestination = selfTransform.position + randomRange;

            agent.destination = nextDestination;
            //Debug.Log("我现在出发！目标是" + agent.destination + "====随机点是" + randomRange);
        }
        else if (stopTime > 1.0f)
        {
            Vector3 nextDestination = selfTransform.position - selfTransform.forward * (Random.value) * wanderScope;
            agent.destination = nextDestination;
        }


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

        animator.SetFloat("WalkSpeed", 2.0f);
    }

    private void updateTrackState()
    {

    }

    private void updateAttackState()
    {

    }

    private void updateDyingState()
    {

    }

    private void updateDeadState()
    {

    }

    // Use this for initialization
    void Start () {
        
        
    }
	
	// Update is called once per frame
	void Update () {
        
    }
}
