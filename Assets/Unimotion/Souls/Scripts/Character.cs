using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public List<Object> movementLockers = new List<Object>();

    CharacterMotor motor;

    void Awake() {
        motor = GetComponent<CharacterMotor>();
        motor.OnLand += Motor_OnLand;
    }

    private void Motor_OnLand() {
        motor.animator.Play("Land");
    }

    void Update() {
        motor.canWalk = movementLockers.Count == 0;
        motor.canJump = movementLockers.Count == 0;
        motor.canTurn = movementLockers.Count == 0;
    }
}
