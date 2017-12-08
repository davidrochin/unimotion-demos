using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastUtil {

    //Este metodo es para hacer un Raycast ignorando el colisionador de este mismo objeto
    public static RaycastHit RaycastPastItself(GameObject go, Vector3 startPos, Vector3 direction, float lenght, LayerMask mask) {
        RaycastHit[] rayHits = Physics.RaycastAll(startPos, direction, lenght, mask);
        foreach (RaycastHit hit in rayHits) {
            if (hit.collider.gameObject != go) {
                return hit;
            }
        }
        return new RaycastHit();
    }
}
