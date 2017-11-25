using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabber : MonoBehaviour {

    //Lo saqué de internet. Falta entenderlo para continuar
    public bool InSegmentRange(Vector3 start, Vector3 end, Vector3 point) {
        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float innerProduct = (point.x - start.x) * dx + (point.y - start.y) * dy;
        return innerProduct >= 0 && innerProduct <= dx * dx + dy * dy;
    }
}
