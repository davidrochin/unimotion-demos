using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterNavAgent : MonoBehaviour {

    NavMeshAgent agent;

    public Animator animator;
    public Transform target;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start () {

    }
	
	void Update () {
        if((target.position - transform.position).magnitude > 3f) {
            agent.destination = target.position;
        }
        
        UpdateAnimator();
    }

    public void UpdateAnimator() {
        if (animator != null) {
            animator.SetFloat("Forward Move", Vector3.Dot(agent.velocity, transform.forward));
            animator.SetFloat("Strafe Move", Vector3.Dot(agent.velocity, transform.right));
            animator.SetFloat("Move Speed", agent.velocity.magnitude);
            animator.SetFloat("Max Move Speed", agent.speed);
            animator.SetFloat("Upwards Speed", Vector3.Dot(agent.velocity, -Physics.gravity.normalized));
            animator.SetFloat("Sideways Speed", (agent.velocity - Vector3.Project(agent.velocity, -Physics.gravity.normalized)).magnitude);
            animator.SetBool("Grounded", agent.isOnNavMesh);
            animator.SetBool("Sliding", false);
            animator.SetBool("Stuck", agent.isPathStale);
        }
    }
}
