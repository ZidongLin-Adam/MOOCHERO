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

    private MonState currentState;
    private NavMeshAgent agent;
    private Transform selfTransform;
    //public GameObject moveTarget;
    

    private void agentMove()
    {
        Vector3 randomRange = new Vector3((Random.value - 0.5f) * 2 * 4, 0, (Random.value - 0.5f) * 2 * 4);
        Vector3 nextDestination = selfTransform.position + randomRange;

        agent.destination = nextDestination;
    } 

    private void OnEnable()
    {
        currentState = MonState.Wander;
        selfTransform = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
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
    
    private void updateWanderState()
    {

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
        currentState = MonState.Wander;
        
    }
	
	// Update is called once per frame
	void Update () {
        
    }
}
