using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDemoAI : MonoBehaviour {

    public float wanderRadius = 10f;

    bool moving = false;

    Vector3 destination;
    Vector3 initialPos;

    CharacterMotor character;

	void Start () {
        character = GetComponent<CharacterMotor>();
        initialPos = transform.position;

        destination = RandomInRadius();
        moving = true;

        StartCoroutine(ChangeDestinationEvery(2f, 6f));
	}

    private void Update() {
        if (moving) {
            Vector3 delta = (RemoveY(destination) - RemoveY(transform.position));
            character.Walk(delta.normalized * Mathf.Clamp(1f, 0f, delta.magnitude));
            character.TurnTowards(delta.normalized);

            if(Vector3.Distance(RemoveY(transform.position), RemoveY(destination)) <= 0.1f) {
                moving = false;
            }
        }
    }

    Vector3 RandomInRadius() {
        return initialPos + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * Random.Range(0f, wanderRadius);
    }

    Vector3 RemoveY(Vector3 v) {
        return new Vector3(v.x, 0f, v.z);
    }

    IEnumerator ChangeDestinationEvery(float minSeconds, float maxSeconds) {
        while (true) {
            yield return new WaitForSeconds(Random.Range(minSeconds, maxSeconds));
            destination = RandomInRadius();
            moving = true;
        }
    }

    private void OnDrawGizmosSelected() {
        if (Application.isPlaying) {
            Gizmos.DrawWireSphere(initialPos, wanderRadius);
        } else {
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
        } 
    }
}
