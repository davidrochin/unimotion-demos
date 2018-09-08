using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysics {

	public bool CalculateDepenetration(CapsuleCollider capsule, Collider collider, out Vector3 direction, out float distance) {


        direction = Vector3.zero;
        distance = 0f;
        return false;
    }
}
