using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unimotion;

[AddComponentMenu("Unimotion/Character Input")]
[RequireComponent(typeof(CharacterMotor))]
public class CharacterInputGravity : MonoBehaviour {

    public InputType inputType; 

    //References
    CharacterMotor character;

    VirtualJoystick virtualJoystick;

    void Awake () {
        character = GetComponent<CharacterMotor>();
        virtualJoystick = VirtualJoystick.GetById(0);
    }


    private void Start() {
    }

    void Update () {

        float inputMagnitude = GetInputMagnitude();
        Vector3 inputVector = GetInputVector();

        if (inputMagnitude > 0.05f) {
            character.TurnTowards(inputVector);
            character.Walk(inputVector * inputMagnitude * (Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f) * (Input.GetKey(KeyCode.LeftAlt) ? 0.5f : 1f));
        }

        if (Input.GetButtonDown("Jump")) {
            character.Jump();
        }

        if (Input.GetButtonDown("Crouch")) {
            character.Crouch();
        }

        if (Input.GetKeyDown(KeyCode.G)) {
            Physics.gravity = -Physics.gravity;
        }

        if(Input.GetKeyDown(KeyCode.P)) {
            Debug.Break();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            character.velocity = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * Random.Range(20f, 40f) + Vector3.up * 10f;
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            GameObject original = gameObject;
            for (int i = 0; i < 40; i++) {
                Instantiate(original, transform.position, transform.rotation);
            }
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            character.AddForce(transform.forward * 500f);
        }

    }

    Vector3 GetInputVector() {
        Vector3 input = Vector3.zero;

        if(inputType == InputType.Normal) {
            input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        } else if (inputType == InputType.Raw) {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        }

        if (virtualJoystick != null) { input = new Vector3(input.x + virtualJoystick.input.x, 0f, input.y + virtualJoystick.input.y); }

        //Transformar la direccion para que sea relativa a la camara.
        //Vector3 transDirection = Camera.main.transform.TransformDirection(input);
        Quaternion tempQ = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
        //Debug.Log(tempQ);
        Vector3 transDirection = Camera.main.transform.rotation * input;
        
        //Hacer que el Vector no apunte hacia arriba.
        //transDirection = new Vector3(transDirection.x, 0f, transDirection.z).normalized;
        finalMovementVector = transDirection;
        return transDirection.normalized;
    }

    float GetInputMagnitude() {
        Vector3 input = Vector3.zero;

        // Get Input from standard Input methods
        if (inputType == InputType.Normal) { input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")); } 
        else if (inputType == InputType.Raw) { input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")); }

        // Get Input from Virtual Joystick if available
        if (virtualJoystick != null) { input = new Vector3(input.x + virtualJoystick.input.x, 0f, input.y + virtualJoystick.input.y); }

        // Clamp magnitude to 1
        return Vector3.ClampMagnitude(input, 1f).magnitude;
    }

    Vector3 finalMovementVector;
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, finalMovementVector);
    }

    public enum InputType { Normal, Raw }
}
