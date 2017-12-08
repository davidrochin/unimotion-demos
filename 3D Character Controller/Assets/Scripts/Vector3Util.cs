using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Util {

    public static Vector3 Shift(Vector3 vector, Vector3 direction) {
        return Vector3.Cross(vector, direction);
    }

    public static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion rotation) {
        return rotation * (point - pivot) + pivot;
    }

    public static float InnerProduct(Vector3 a, Vector3 b, Vector3 point) {
        Vector3 delta = b - a;
        return (point.x - a.x) * delta.x + (point.y - a.y) * delta.y + (point.z - a.z) * delta.z;
    }

}
