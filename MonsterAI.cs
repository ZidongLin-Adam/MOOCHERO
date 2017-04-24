using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void OnEnable()
    {
        currentState = MonState.Wander;
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
