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

        //Caminar y correr
        if(character.state == Character.State.OnGround || character.state == Character.State.OnAir) {
            if (GetInputVector() != Vector3.zero) {
                character.RotateTowards(GetInputVector(), 400f * Time.deltaTime);
                float speed = Vector3.ClampMagnitude(GetInputVector(), 1f).magnitude;
                if (Input.GetButton("Walk")) { speed = Mathf.Clamp(speed, 0f, 0.5f); } 
                else { speed = Mathf.Clamp(speed, 0.4f, 1f); }
                character.Move(transform.forward, speed);
            }
        }

        //Movimiento sobre ladera
        if (character.state == Character.State.OnLedge) {
            if (Input.GetAxisRaw("Horizontal") > 0f) {
                character.LedgeMove(Vector3.right);
            } else if (Input.GetAxisRaw("Horizontal") < 0f) {
                character.LedgeMove(Vector3.left);
            }
        }

        if (Input.GetButtonDown("Jump")) {
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
        Vector3 transDirection = Camera.main.transform.TransformDirection(input);

        //Hacer que el Vector no apunte hacia arriba.
        transDirection = new Vector3(transDirection.x, 0f, transDirection.z);
        return transDirection;
    }

    public enum InputType { Normal, Raw }
}
