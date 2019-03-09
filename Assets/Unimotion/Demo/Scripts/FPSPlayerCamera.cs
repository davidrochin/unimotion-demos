using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSPlayerCamera : PlayerCamera {

    public override void Awake() {
        base.Awake();
        distance = 0f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void Update() {
        base.Update();
        //character.TurnTowards(transform.forward - Vector3.Project(transform.forward, -Physics.gravity.normalized), 1000f);
    }

}
