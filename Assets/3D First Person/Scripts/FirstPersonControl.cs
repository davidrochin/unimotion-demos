using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonController {
    public class FirstPersonControl : MonoBehaviour {

        public float visionHeight;
        public float eyeSeparation = 0.1f;

        public float movementSpeed = 10f;

        public Camera leftEyeCam;
        public Camera rightEyeCam;

        public Transform headTransform;


        void Awake() {
            CreateCameras();
        }

        void Update() {

            //Poner las camaras en su lugar
            Vector3[] cameraPositions = GetCameraPositions();
            leftEyeCam.transform.position = cameraPositions[0];
            rightEyeCam.transform.position = cameraPositions[1];

            //Obtener un vector a partir del movimiento del mouse
            Vector2 input = Util.Input.GetMouseMovement();

            //Generar una nueva rotacion a partir de los valores del movimiento del mouse
            Quaternion finalRotation = Quaternion.Euler(
            headTransform.rotation.eulerAngles.x + input.y * Time.deltaTime * 50f,
            headTransform.rotation.eulerAngles.y + input.x * Time.deltaTime * 50f,
            0f);

            //Aplicar la rotacion previamente generada
            headTransform.rotation = finalRotation;

            //Generar un vector de movimiento a partir de los ejes Horizontales y Verticales
            Vector3 moveDirection = (
                Vector3.zero + headTransform.transform.right * Input.GetAxisRaw("Horizontal") + 
                headTransform.transform.forward * Input.GetAxisRaw("Vertical"));

            //Normalizar la direccion de movimiento
            moveDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;

            //Sumar el vector de movimiento a la posicion de este objeto
            transform.position += moveDirection * movementSpeed * Time.deltaTime;

        }

        public void CreateCameras() {

            //Crear el objeto que representa la cabeza del usuario y emparentarlo al usuario
            headTransform = new GameObject("Head").transform;
            headTransform.position = transform.position + Vector3.up * visionHeight;
            headTransform.parent = transform;

            //Crear las camaras izquierda y derecha
            leftEyeCam = new GameObject("Left Eye").AddComponent<Camera>();
            rightEyeCam = new GameObject("Right Eye").AddComponent<Camera>();

            //Emparentar las camaras a la cabeza
            leftEyeCam.transform.parent = headTransform;
            rightEyeCam.transform.parent = headTransform;

            //Establecer las rectas de las camaras para que una se renderize de un lado y la otra del otro
            leftEyeCam.rect = new Rect(0f, 0f, 0.5f, 1f);
            rightEyeCam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }

        public Vector3[] GetCameraPositions() {
            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position + Vector3.up * visionHeight + -headTransform.right * eyeSeparation * 0.5f;
            positions[1] = transform.position + Vector3.up * visionHeight + headTransform.right * eyeSeparation * 0.5f;
            return positions;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * visionHeight);
            //Gizmos.DrawSphere(transform.position + Vector3.up * visionHeight, 0.04f);
        }
    }
}


