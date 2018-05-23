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

    public class Physics {

        public static RaycastHit CapsuleCastPastItself(Vector3 point1, Vector3 point2, float radius, Vector3 direction) {
            return new RaycastHit();
        }

    }

    public class Texture2D {

        public static UnityEngine.Texture2D CreateEmpty(Color color) {
            UnityEngine.Texture2D tex = new UnityEngine.Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

    }
}
