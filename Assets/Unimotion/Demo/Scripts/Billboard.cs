using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {
	void Update () {
        //transform.LookAt(Camera.main.transform.position);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
	}
}
