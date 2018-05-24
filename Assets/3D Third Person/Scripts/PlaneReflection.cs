using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneReflection : MonoBehaviour {

    ReflectionProbe probe;
    float floorHeight;

	// Use this for initialization
	void Awake () {
        probe = GetComponentInChildren<ReflectionProbe>();
	}
	
	// Update is called once per frame
	void Update () {
        floorHeight = transform.position.y;
        float floorToCamera = Mathf.Abs(Camera.main.transform.position.y - floorHeight);
        probe.transform.position = new Vector3(Camera.main.transform.position.x, floorHeight - floorToCamera, Camera.main.transform.position.z);
	}
}
