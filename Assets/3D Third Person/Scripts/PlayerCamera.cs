﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    [Header("Target Settings")]
    public Transform target;
    public float distance = 5f;
    public Vector3 targetOffset;

    [Header("Orbit Settings")]
    public float orbitSpeed = 10f;
    public LayerMask obstructionLayer;

    Camera camera;
    Character player;

	void Awake () {
        Cursor.lockState = CursorLockMode.Locked;
        camera = GetComponent<Camera>();
        player = target.GetComponent<Character>();
	}

    void LateUpdate () {
        //Get the real target position (add offset)
        Vector3 realTarget = target.position + targetOffset;

        //Make a vector from mouse/joystick movement
        Vector3 input = new Vector3(Input.GetAxis("Mouse X") , -Input.GetAxis("Mouse Y"), 0f);
        input = input + new Vector3(Input.GetAxis("Camera Horizontal") * 2f, Input.GetAxis("Camera Vertical") * 2f, 0f);

        //Calculate what rotation needs the Camera...
        Quaternion finalRotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x + input.y * Time.deltaTime * orbitSpeed, 
            transform.rotation.eulerAngles.y + input.x * Time.deltaTime * orbitSpeed, 
            0f);

        //...and apply it
        transform.localRotation = finalRotation;

        //Antes de acomodarse en la distancia necesaria, revisar si hay una obstrucción para no pasar de ella
        RaycastHit hit = RaycastUtil.RaycastPastItself(target.gameObject, realTarget, transform.forward * -1f, distance, obstructionLayer);

        float realDistance = 0f;
        if (hit.collider != null) {
            realDistance = hit.distance;
        } else {
            realDistance = distance;
        }

        //Acomodarse en la distancia necesaria
        Vector3 desiredPosition = realTarget - transform.forward * (realDistance - 0.2f);
        transform.position = desiredPosition;

        if (Input.GetKeyDown(KeyCode.Z)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        //Hacer zoom si se mueve la ruedita del ratón
        distance = distance - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100f;
	}
}