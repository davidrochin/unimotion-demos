using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour {

    public InputType inputType; 

    public ControlLockState lockState;

    //References
    Character character;
    Equipment equipment;

    void Awake () {
        character = GetComponent<Character>();
        equipment = GetComponent<Equipment>();
    }
	
	void Update () {

        //Caminar y correr
        if (lockState.canMove && GetInputVector() != Vector3.zero) {
            character.RotateTowards(GetInputVector(), 400f * Time.deltaTime);
            //float speed = Vector3.ClampMagnitude(GetInputVector(), 1f).magnitude;
            //if (Input.GetButton("Walk")) { speed = Mathf.Clamp(speed, 0f, 0.5f); } 
            //else { speed = Mathf.Clamp(speed, 0.4f, 1f); }
            character.Move(transform.forward, GetInputMagnitude());
        }

        if (Input.GetButtonDown("Jump")) {
            character.Jump();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            character.Roll();
        }

        if (Input.GetMouseButtonDown(0)) {
            equipment.UseRightHandItem();
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
        //Vector3 transDirection = Camera.main.transform.TransformDirection(input);
        Quaternion tempQ = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
        //Debug.Log(tempQ);
        Vector3 transDirection = tempQ * input;
        
        //Hacer que el Vector no apunte hacia arriba.
        //transDirection = new Vector3(transDirection.x, 0f, transDirection.z).normalized;
        finalMovementVector = transDirection;
        return transDirection;
    }

    float GetInputMagnitude() {
        Vector3 input = Vector3.zero;
        if (inputType == InputType.Normal) { input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")); } 
        else if (inputType == InputType.Raw) { input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")); }
        return Vector3.ClampMagnitude(input, 1f).magnitude;
    }

    Vector3 finalMovementVector;
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, finalMovementVector);
    }

    public enum InputType { Normal, Raw }
}

[System.Serializable]
public class ControlLockState {
    public bool canMove = true;
    public bool canTurn = true;
    public bool canJump = true;
    public bool canRoll = true;

    public ControlLockState() {

    }
}
