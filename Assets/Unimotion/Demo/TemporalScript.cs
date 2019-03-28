using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unimotion;

public class TemporalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            GetComponent<Rigidbody>().velocity = new Vector3(0f, FindObjectOfType<CharacterMotor>().jumpForce, 0f);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            GetComponent<Rigidbody>().AddForce(transform.forward * 500f);
        }
    }
}
