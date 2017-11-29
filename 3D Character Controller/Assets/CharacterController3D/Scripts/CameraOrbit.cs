using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {

    [Header("Target Settings")]
    public Transform target;
    public float distance = 5f;
    public Vector3 targetOffset;

    [Header("Orbit Settings")]
    public float orbitSpeed = 10f;
    public LayerMask obstructionLayer;

    Camera camera;

	void Awake () {
        Cursor.lockState = CursorLockMode.Locked;
        camera = GetComponent<Camera>();
	}
	
	void LateUpdate () {
        //Obtener el objetivo real (sumarle el offset)
        Vector3 realTarget = target.position + targetOffset;

        //Obtener un vector a partir del movimiento del mouse
        Vector3 input = new Vector3(Input.GetAxis("Mouse X") , Input.GetAxis("Mouse Y"), 0f);

        Quaternion finalRotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x + input.y * Time.deltaTime * orbitSpeed * -1f, 
            transform.rotation.eulerAngles.y + input.x * Time.deltaTime * orbitSpeed, 
            0f);
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
        Vector3 desiredPosition = realTarget + transform.forward * -1f * realDistance;
        transform.position = desiredPosition;

        if (Input.GetKeyDown(KeyCode.Z)) {
            Cursor.lockState = CursorLockMode.None;
        }

        //Hacer zoom si se mueve la ruedita del ratón
        distance = distance - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100f;
	}
}
