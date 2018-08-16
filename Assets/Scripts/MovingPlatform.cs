using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    public Vector3 secondPositionOffset;
    public float speed = 1f;

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
                        state = State.Returning;
                    }
                    break;
                }
            case State.Returning: {
                    transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
                    if (transform.position == startPos) {
                        state = State.Going;
                    }
                    break;
                }
        }
	}

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + secondPositionOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + secondPositionOffset);
    }

    public enum State { Going, Returning }
}
