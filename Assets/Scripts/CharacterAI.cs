using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : MonoBehaviour {

    public State state;
    public Transform target;

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

        navMeshAgent.destination = target.position;
        health.OnDeath += delegate () { navMeshAgent.enabled = false; };
        health.OnRevive += delegate () { navMeshAgent.enabled = true; };
    }
	
	// Update is called once per frame
	void Update () {

        if (navMeshAgent.enabled) {

            //Do not move if Character is locked
            if (character.lockState.lockAll) {
                Stop();
            } else {
                Resume();
            }

            if (state == State.Idle) {

                navMeshAgent.destination = transform.position;

                if (GetDistanceToTarget() <= 10f) {
                    state = State.Chasing;
                }

            } 
            
            
            if (state == State.Chasing) {

                //Chase Target
                if (target != null) {
                    navMeshAgent.destination = target.position;
                } else {
                    navMeshAgent.destination = target.position;
                }

                //Attack when close to target
                if (GetDistanceToTarget() <= 1.5f) {
                    character.ForceRotateTowards((target.position - transform.position).normalized, 25f);
                    Stop();
                    equipment.UseItem(Hand.Right);
                } else {
                    Resume();
                }

                if (GetDistanceToTarget() > 10f) {
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
        return (target.position - transform.position).magnitude;
    }

    public enum State { Idle, Chasing }

}
