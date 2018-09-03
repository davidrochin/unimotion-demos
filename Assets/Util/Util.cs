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

        public static RaycastHit CapsuleCastPastItself(Directions point1, Directions point2, float radius, Directions direction) {
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

    public class Directions {

        public static Vector3 MeanDirection(Vector3[] directions) {
            Vector3 meanDirection = directions[0];
            if (directions.Length > 1) {
                for (int i = 1; i < directions.Length; i++) {
                    meanDirection = Vector3.Slerp(meanDirection, directions[i], 0.5f);
                }
            }

            return meanDirection;
        }

    }
}
