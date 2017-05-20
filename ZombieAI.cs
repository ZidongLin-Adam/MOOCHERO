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
			
	private ZombieHealth zombieHealth;		
	private ZombieSoundSensor zombieSoundSensor;
	private ZombieRender zombieRender;			

	private Transform targetPlayer;			


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

}