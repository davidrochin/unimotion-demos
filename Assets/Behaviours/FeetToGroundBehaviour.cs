using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetToGroundBehaviour : StateMachineBehaviour {

    public LayerMask mask;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //mask = LayerMask.NameToLayer("Default");
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        
    }

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Vector3 leftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        Vector3 rightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        
        RaycastHit hit;

        if(Physics.Raycast(leftFootPos + Vector3.up, Vector3.down, out hit, 2f, mask)) {
            Vector3 leftFootGoal = hit.point;
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootGoal);
        }

        if (Physics.Raycast(rightFootPos + Vector3.up, Vector3.down, out hit, 2f, mask)) {
            Vector3 rightFootGoal = hit.point;
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootGoal);
        }

    }
}
