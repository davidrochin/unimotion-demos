using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterControl : MonoBehaviour {

    public InputType inputType; 

    //References
    Character character;

    void Awake () {
        character = GetComponent<Character>();
    }

    private void Start() {
        Camera.main.GetComponent<PlayerCamera>().player = character;
    }

    void Update () {

        Vector3 input = GetInputVector();
        if(GetInputMagnitude() > 0.05f) {
            character.RotateTowards(GetInputVector());
            character.Walk(GetInputVector() * GetInputMagnitude());
        }

        if (Input.GetButtonDown("Jump")) {
            character.Jump();
        }

        if (Input.GetKeyDown(KeyCode.G)) {
            Physics.gravity = -Physics.gravity;
        }

        /*if (Input.GetKeyDown(KeyCode.P)) {
            Debug.Break();
        }*/

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
