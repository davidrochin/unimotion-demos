using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterControl : MonoBehaviour {

    public InputType inputType; 

    public LockState lockState;

    public Queue<string> buttonQueue;

    //References
    Character character;
    Equipment equipment;
    Health health;

    void Awake () {
        character = GetComponent<Character>();
        equipment = GetComponent<Equipment>();
        health = GetComponent<Health>();
    }

    private void Start() {
        Camera.main.GetComponent<PlayerCamera>().target = transform;
    }

    void Update () {

        //Caminar y correr
        if (lockState.canMove && GetInputVector() != Vector3.zero) {
            character.RotateTowards(GetInputVector(), 4000f * Time.deltaTime);
            character.Move(GetInputVector(), GetInputMagnitude());
        }

        if (Input.GetButtonDown("Jump")) {
            character.Jump();
        }

        if (Input.GetButtonDown("B")) {
            character.Roll();
        }

        if (Input.GetKeyDown(KeyCode.X)){
            GetComponent<Animator>().Play("Point");
        }

        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("RB")) {
            //equipment.UseRightHandItem();
            equipment.UseItem(Hand.Right);
        }

        if (Input.GetMouseButton(1)) {
            GetComponent<Animator>().SetBool("shieldUp", true);
        } else {
            GetComponent<Animator>().SetBool("shieldUp", false);
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
public class LockState {

    public bool lockAll = false;

    public bool canMove = true;
    public bool canTurn = true;
    public bool canJump = true;
    public bool canRoll = true;
    public bool canRotate = true;

    public LockState() {

    }
}
