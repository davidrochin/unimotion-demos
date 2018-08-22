using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    public Vector3 secondPositionOffset;
    public float speed = 1f;
    public float waitTime = 1f;
    public float rotationSpeed = 0f;

    Vector3 startPos;
    Vector3 endPos;

    public State state = State.Going;

    void Start () {
        startPos = transform.position;
        endPos = transform.position + secondPositionOffset;
	}
	
	void Update () {

        switch (state) {
            case State.Going: {
                    transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
                    if (transform.position == endPos) {
                        state = State.Waiting;
                        StartCoroutine(WaitAndChangeState(waitTime, State.Returning));
                    }
                    break;
                }
            case State.Returning: {
                    transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
                    if (transform.position == startPos) {
                        state = State.Waiting;
                        StartCoroutine(WaitAndChangeState(waitTime, State.Going));
                    }
                    break;
                }
        }

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
	}

    IEnumerator WaitAndChangeState(float seconds, State newState) {
        yield return new WaitForSeconds(seconds);
        state = newState;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        if (!Application.isPlaying) {
            Gizmos.DrawWireCube(transform.position + secondPositionOffset, transform.localScale);
            Gizmos.DrawLine(transform.position, transform.position + secondPositionOffset);
        }  
    }

    public enum State { Going, Returning, Waiting }
}
