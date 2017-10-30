using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour {

    Character character;

	void Awake () {
        character = GetComponent<Character>();
	}
	
	void Update () {

        if (Input.GetKey(KeyCode.LeftShift)) {
            character.Run(GetInputVector());
        } else {
            character.Walk(GetInputVector());
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            character.Jump();
        }
	}

    Vector3 GetInputVector() {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        return Camera.main.transform.TransformDirection(input).normalized;
    }
}
