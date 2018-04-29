using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateBehaviour : StateMachineBehaviour {

    public Character.State onStateEnter;
    public Character.State onStateUpdate;
    public Character.State onStateExit;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	
	}

}
