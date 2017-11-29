using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour {

    public InputType inputType;

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

        if (character.state == Character.State.OnLedge) {
            if (Input.GetAxisRaw("Horizontal") > 0f) {
                character.LedgeMove(Vector3.right);
            } else if (Input.GetAxisRaw("Horizontal") < 0f) {
                character.LedgeMove(Vector3.left);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if(character.state == Character.State.OnLedge) {
                character.Climb();
            } else {
                character.Jump();
            }
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            character.Roll();
        }
	}

    Vector3 GetInputVector() {
        Vector3 input = Vector3.zero;

        if(inputType == InputType.Normal) {
            input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        } else if (inputType == InputType.Raw) {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        }

        //Transformar la direccion para que sea relativa a la camara.
        Vector3 transDirection = Camera.main.transform.TransformDirection(input).normalized;

        //Hacer que el Vector no apunte hacia arriba.
        transDirection = new Vector3(transDirection.x, 0f, transDirection.z).normalized;
        return transDirection;
    }

    public enum InputType { Normal, Raw }
}
