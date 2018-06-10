using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    [Header("Target Settings")]
    public Character player;
    public float distance = 2.94f;
    public Vector3 targetOffset = new Vector3(0f, 1.09f, 0f);

    [Header("Orbit Settings")]
    public float orbitSpeed = 50f;
    public LayerMask obstructionLayer;

    Camera camera;

	void Awake () {
        Cursor.lockState = CursorLockMode.Locked;
        camera = GetComponent<Camera>();
	}

    void LateUpdate () {

        //Get the real target position (add offset)
        Vector3 realTarget = player.transform.position + targetOffset;

        //When not targeting any enemy
        if (player.lookTarget == null) {

            //Make a vector from mouse/joystick movement
            Vector3 input = new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f);
            input = input + new Vector3(Input.GetAxis("Camera Horizontal") * 2f, Input.GetAxis("Camera Vertical") * 2f, 0f);

            //Calculate what rotation needs the Camera...
            Quaternion finalRotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x + input.y * Time.deltaTime * orbitSpeed,
                transform.rotation.eulerAngles.y + input.x * Time.deltaTime * orbitSpeed,
                0f);

            //...and apply it
            transform.localRotation = finalRotation;

            //Antes de acomodarse en la distancia necesaria, revisar si hay una obstrucción para no pasar de ella
            RaycastHit hit = RaycastUtil.RaycastPastItself(player.gameObject, realTarget, transform.forward * -1f, distance, obstructionLayer);

            float realDistance = 0f;
            if (hit.collider != null) {
                realDistance = hit.distance;
            } else {
                realDistance = distance;
            }

            //Acomodarse en la distancia necesaria
            Vector3 desiredPosition = realTarget - transform.forward * (realDistance - 0.2f);
            transform.position = desiredPosition;
        } 
        
        //When targeting an enemy
        else {
            Vector3 direction = (player.lookTarget.transform.position - player.transform.position).normalized;
            transform.position = player.transform.position + Vector3.up * 2.5f - direction * 2.275434f;
            transform.localRotation = Quaternion.LookRotation((player.lookTarget.position - transform.position).normalized);
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        //Hacer zoom si se mueve la ruedita del ratón
        distance = distance - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100f;
	}
}
