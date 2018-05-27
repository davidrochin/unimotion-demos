using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : MonoBehaviour {

    public State state;
    public State combatState;
    public Character target;

    public Vector3 home;

    NavMeshAgent navMeshAgent;
    Health health;
    Animator animator;
    Equipment equipment;
    Character character;

	// Use this for initialization
	void Start () {
        navMeshAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
        equipment = GetComponent<Equipment>();
        character = GetComponent<Character>();

        navMeshAgent.destination = target.transform.position;
        health.OnDeath += delegate () { navMeshAgent.enabled = false; };
        health.OnRevive += delegate () { navMeshAgent.enabled = true; };

        home = transform.position;
    }
	
	void Update () {

        if (navMeshAgent.enabled) {

            //Do not move if Character is locked
            if (character.lockState.lockAll) {
                Stop();
            } else {
                Resume();
            }

            //When Idle
            if (state == State.Idle) {
                navMeshAgent.destination = transform.position;
                if (GetDistanceToTarget() <= 10f && !character.combatState.isDead) {
                    state = State.Attacking;
                }

            }
            
            //When Attacking
            if (state == State.Attacking) {

                //Determine if it can keep chasing the target
                if (target != null && !target.combatState.isDead && GetDistanceToTarget() <= 10f) {

                    //Chase the target
                    navMeshAgent.destination = target.transform.position;

                    //Block incoming attacks
                    if (target.combatState.isAttacking) {
                        character.StartBlocking();
                    } else {
                        character.StopBlocking();
                    }

                    //Attack if too close
                    if (GetDistanceToTarget() <= 1.5f) {
                        character.Attack();
                    }

                } else {
                    state = State.ReturningHome;
                }
            }

            //When Returning Home
            if (state == State.ReturningHome) {
                navMeshAgent.destination = home;
                if (Vector3.Distance(transform.position, home) <= 1f) {
                    state = State.Idle;
                }
            }
            
        }
  
    }

    void Stop() {
        navMeshAgent.enabled = false;
        navMeshAgent.enabled = true;
    }

    void Resume() {
        navMeshAgent.enabled = true;
    }

    float GetDistanceToTarget() {
        return (target.transform.position - transform.position).magnitude;
    }

    public enum State { Idle, Attacking, ReturningHome }
    public enum CombatState { Idle, Attacking, Blocking, Rounding }

}
