using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Util {

    public static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion rotation) {
        return rotation * (point - pivot) + pivot;
    }

}
