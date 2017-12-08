using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRootMotion : StateMachineBehaviour {

    public bool useRootMotion;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.applyRootMotion = useRootMotion;
    }

}
