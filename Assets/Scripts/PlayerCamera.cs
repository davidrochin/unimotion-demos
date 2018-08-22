using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    [Header("Target Settings")]
    public Character player;
    public float distance = 2.94f;
    public Vector3 targetOffset = new Vector3(0f, 1.09f, 0f);

    [Header("Orbit Settings")]
    public float orbitSpeed = 2.5f;
    public LayerMask obstructionLayer;

    Camera camera;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        camera = GetComponent<Camera>();
    }

    void LateUpdate() {

        //Get the real target position (add offset)
        Vector3 realTarget = player.transform.position + targetOffset;

        //Make a vector from mouse/joystick movement
        Vector3 input = new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f);
        input = input + new Vector3(Input.GetAxis("Camera Horizontal") * 60f * Time.deltaTime, Input.GetAxis("Camera Vertical") * 60f * Time.deltaTime, 0f);

        //Rotate the Camera
        transform.RotateAround(transform.position, Vector3.up, input.x * orbitSpeed);
        transform.RotateAround(transform.position, transform.right, input.y * orbitSpeed);

        //Antes de acomodarse en la distancia necesaria, revisar si hay una obstrucción para no pasar de ella
        RaycastHit hit = RaycastUtil.RaycastPastItself(player.gameObject, realTarget, transform.forward * -1f, distance, obstructionLayer);

        float maxDistance = 0f;
        Vector3 addition = Vector3.zero;

        if (hit.collider != null) {
            maxDistance = hit.distance;
            addition = hit.normal;
        } else {
            maxDistance = distance;
        }

        //Put the Camera around the target
        Vector3 desiredPosition = realTarget - transform.forward * (maxDistance - 0.1f);
        transform.position = desiredPosition;

        if (Input.GetKeyDown(KeyCode.Z)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        //Zoom if user uses mousewheel
        distance = distance - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100f;
    }
}
