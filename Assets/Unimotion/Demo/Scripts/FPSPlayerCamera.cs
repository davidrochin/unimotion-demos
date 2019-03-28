using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unimotion;

public class FPSPlayerCamera : MonoBehaviour {

    [Header("Target Settings")]
    public CharacterMotor target;
    public float cameraHeight = 1.7f;

    [Header("Orbit Settings")]
    public float orbitSpeed = 2.5f;
    public LayerMask obstructionLayer;

    Collider playerCollider;

    public virtual void Awake() {

        DontDestroyOnLoad(this);

        if (target != null) {
            playerCollider = target.GetComponent<Collider>();
            target.OnFrameFinish += Follow;
            /*target.OnGravityAlign += delegate (Quaternion delta) {
                Debug.Log(delta);
                transform.rotation
            };*/
        }
    }

    public virtual void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
        }
    }

    public void Follow() {

        if (enabled == false) { return; }

        //Get the real target position (add offset)
        Vector3 realTarget = target.transform.position - target.customGravity.normalized * cameraHeight;

        //Make a vector from mouse/joystick movement
        Vector3 input = Vector3.zero;
        if (!Application.isMobilePlatform) {
            input = new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f);
        }
        input = input + new Vector3(Input.GetAxis("Camera Horizontal") * 60f * Time.deltaTime, Input.GetAxis("Camera Vertical") * 60f * Time.deltaTime, 0f);

        //Rotate the Camera
        transform.RotateAround(transform.position, -target.GetGravity().normalized, input.x * orbitSpeed);
        transform.RotateAround(transform.position, transform.right, input.y * orbitSpeed);

        //transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
        //transform.rotation = target.transform.rotation * transform.rotation;

        transform.rotation = Quaternion.LookRotation(transform.forward, target.transform.up);

        float maxDistance = 0f;

        //Put the Camera around the target
        Vector3 desiredPosition = realTarget;
        transform.position = desiredPosition;

    }

    public void SetTarget(CharacterMotor newTarget) {
        target = newTarget;
        if (target != null) {
            playerCollider = target.GetComponent<Collider>();
            target.OnFrameFinish += Follow;
        }
    }

    public static RaycastHit RaycastPastItself(Collider col, Vector3 startPos, Vector3 direction, float lenght, LayerMask mask, QueryTriggerInteraction queryTriggerInteraction) {
        RaycastHit[] rayHits = Physics.RaycastAll(startPos, direction, lenght, mask, queryTriggerInteraction);
        foreach (RaycastHit hit in rayHits) {
            if (hit.collider != col) {
                return hit;
            }
        }
        return new RaycastHit();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        if (target != null) {
            Gizmos.DrawSphere(target.transform.position - Physics.gravity * cameraHeight, 0.1f);
        }
    }

}
