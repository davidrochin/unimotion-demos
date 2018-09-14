using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetToGroundBehaviour : StateMachineBehaviour {

    public LayerMask mask;
    public float upwardsCorrection = 0.1f;
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

        float castDistance = 10f;

        RaycastHit hit;

        if(Physics.Raycast(leftFootPos + Vector3.up, Vector3.down, out hit, castDistance, mask)) {
            Debug.Log(true);
            Debug.DrawRay(hit.point, hit.normal);
            Vector3 leftFootGoal = hit.point;
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootGoal - Physics.gravity.normalized * upwardsCorrection);
        }

        Debug.Log(Physics.Raycast(rightFootPos + Vector3.up, Vector3.down, out hit, castDistance, mask));

        if (Physics.Raycast(rightFootPos + Vector3.up, Vector3.down, out hit, 10f, mask)) {
            Debug.DrawRay(hit.point, hit.normal);
            Vector3 rightFootGoal = hit.point;
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootGoal - Physics.gravity.normalized * upwardsCorrection);
        }

    }
}
