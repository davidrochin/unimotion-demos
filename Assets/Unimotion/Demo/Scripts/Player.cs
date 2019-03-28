using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unimotion;

public class Player : MonoBehaviour {

    public LayerMask mask;

    private CharacterMotor motor;
    private CapsuleCollider collider;

    Vector3 point;

    void Awake() {
        motor = GetComponent<CharacterMotor>();
        collider = GetComponent<CapsuleCollider>();
    }

    void Update() {

        // Align gravity when jumping against a wall
        if (!motor.Grounded && Input.GetKey(KeyCode.Space)) {
            RaycastHit hit;
            Physics.Raycast(transform.position, motor.fullVelocity.normalized, out hit, 3f, mask, QueryTriggerInteraction.Ignore);

            if (hit.collider != null) {
                motor.customGravity = -hit.normal * Physics.gravity.magnitude;
            }
        }

        // Align gravity when walking normally
        if (motor.Grounded) {
            RaycastHit hit;
            Physics.Raycast(transform.position - motor.GetGravity().normalized * collider.height * 0.5f, motor.GetGravity().normalized, out hit, collider.height * 0.5f + 0.1f, mask, QueryTriggerInteraction.Ignore);

            if (hit.collider != null) {
                motor.customGravity = -hit.normal * Physics.gravity.magnitude;
            }
        }

    }
}
