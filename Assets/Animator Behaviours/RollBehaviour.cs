using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollBehaviour : StateMachineBehaviour {

    //OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        Character character = animator.GetComponent<Character>();
        if (character != null) {
            character.combatState.isRolling = true;
        }
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        Character character = animator.GetComponent<Character>();
        if (character != null) {
            character.combatState.isRolling = false;
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
