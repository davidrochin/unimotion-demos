using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public LayerMask mask;

    private CharacterMotor motor;

    Vector3 point;

    void Awake() {
        motor = GetComponent<CharacterMotor>();
    }

    void Update() {

        if (!motor.Grounded) {
            RaycastHit hit;
            Physics.Raycast(transform.position, motor.fullVelocity.normalized, out hit, 3f, mask, QueryTriggerInteraction.Ignore);

            if (hit.collider != null) {
                motor.customGravity = -hit.normal * Physics.gravity.magnitude;
            }
        }
        
    }
}
