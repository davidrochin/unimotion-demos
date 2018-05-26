using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamageBehaviour : StateMachineBehaviour {

    //public float damageStart = 0.3f;
    //public float damageEnd = 0.9f;

    public float weaponDamage = 100f;
    public List<Collider> alreadyHitColliders;

    WeaponHitDetector weaponHitDetector;

    //OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        weaponDamage = ((Weapon)animator.GetComponent<Equipment>().equipedWeapon).damage;

        //Initialize things
        alreadyHitColliders = new List<Collider>();

        //Get the WeaponHitDetector to detect hits by the weapon
        weaponHitDetector = animator.gameObject.GetComponentInChildren<WeaponHitDetector>();

        if(weaponHitDetector == null) {
            Debug.LogWarning("WeaponDamage StateMachineBehaviour could not find a WeaponHitDetector to use.");
        } else {

            //Start detecting and tell the detector what to do when collider is hit
            weaponHitDetector.StartDetection(delegate (Collider colliderHit) {

                if (alreadyHitColliders.Find(x => x == colliderHit) == null) {
                    //Add this collider to the alreadyHitColliders list so its not hit again on this animation
                    alreadyHitColliders.Add(colliderHit);

                    //Substract Health if possible
                    Health hitColliderHealth = colliderHit.GetComponent<Health>();
                    Character character = colliderHit.GetComponent<Character>();

                    if (hitColliderHealth != null) {
                        if (character != null) {
                            if (character.combatState.isBlocking == false && character.combatState.isRolling == false) {
                                hitColliderHealth.SubstractHealth(weaponDamage, true);
                            } else {

                                //Stagger if is blocking
                                if (character.combatState.isBlocking) {
                                    //animator.Play("Stagger");
                                    animator.SetTrigger("stagger");
                                }
                            }
                        } else {
                            hitColliderHealth.SubstractHealth(weaponDamage, true);
                        }
                        
                    }
                } 
                
            });
        }
	}

	//OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
	//}

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (weaponHitDetector != null) {
            weaponHitDetector.StopDetection();
        }
    }

	// OnStateMove is called before OnStateMove is called on any state inside this state machine
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called before OnStateIK is called on any state inside this state machine
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	//override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
	//
	//}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	//override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
	//
	//}
}
