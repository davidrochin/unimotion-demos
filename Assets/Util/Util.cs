using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util {
    public class Input {

        public static Vector3 GetMouseMovement() {
            //Obtener un vector a partir del movimiento del mouse y del Joystick
            Vector3 input = new Vector3(UnityEngine.Input.GetAxis("Mouse X"), -UnityEngine.Input.GetAxis("Mouse Y"), 0f);
            input = input + new Vector3(UnityEngine.Input.GetAxis("Camera Horizontal") * 2f, UnityEngine.Input.GetAxis("Camera Vertical") * 2f, 0f);
            return input;
        }

    }
}
