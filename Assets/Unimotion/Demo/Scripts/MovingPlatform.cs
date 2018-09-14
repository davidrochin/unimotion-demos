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

    [HideInInspector]
    public State state = State.Going;

    Mesh mesh;

    void Start () {
        startPos = transform.position;
        endPos = transform.position + secondPositionOffset;
        mesh = (GetComponent<MeshFilter>() != null ? GetComponent<MeshFilter>().sharedMesh : null);
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

    bool noMesh = false;
    private void OnDrawGizmos() {
        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);
        if (!Application.isPlaying) {
            if (mesh != null) {
                Gizmos.DrawMesh(mesh, transform.position + secondPositionOffset, transform.rotation, transform.localScale);
            } else if(noMesh == false) {
                mesh = (GetComponent<MeshFilter>() != null ? GetComponent<MeshFilter>().sharedMesh : null);
                if(mesh == null) { noMesh = true; }
            }

            Gizmos.DrawLine(transform.position, transform.position + secondPositionOffset);
        }  
    }

    public enum State { Going, Returning, Waiting }
}
