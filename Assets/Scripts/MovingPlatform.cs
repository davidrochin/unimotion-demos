using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    public Vector3 secondPositionOffset;
    public float speed = 1f;

    Vector3 startPos;
    Vector3 endPos;

    void Start () {
        startPos = transform.position;
        endPos = transform.position + secondPositionOffset;
	}
	
	void Update () {
        transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
	}

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + secondPositionOffset, transform.localScale);
        Gizmos.DrawLine(transform.position, transform.position + secondPositionOffset);
    }
}
