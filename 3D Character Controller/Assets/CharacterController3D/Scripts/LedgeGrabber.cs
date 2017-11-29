using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabber : MonoBehaviour {

    public Vector3 grabPoint;
    public float grabMaxDistance = 2f;

    public Vector3 closestLedgePoint;
    public LedgeEdge ledgeEdge;
    public bool inLedgeRange = false;

    private void Update() {
        GetClosestLedgePoint();
    }

    public void GetClosestLedgePoint() {
        //Calcular el grabPoint real
        Vector3 realGrabPoint = transform.position + transform.rotation * grabPoint;

        //Iterar por todos los Ledge de la escena
        foreach (Ledge ledge in FindObjectsOfType<Ledge>()) {

            //Iterar por todos los Edge del Ledge
            if (ledge.ledgeEdges != null) {
                foreach (LedgeEdge edge in ledge.ledgeEdges) {

                    //Revisar si está en rango del segmento del Edge
                    if(InSegmentRange(edge.a.position, edge.b.position, realGrabPoint)) {

                        //Proyectar el punto en el Edge
                        Vector3 point = Vector3.Project(realGrabPoint - edge.a.position, (edge.b.position - edge.a.position).normalized) + edge.a.position;

                        //Revisar si está dentro de la distancia maxima permitida
                        if (Vector3.Distance(realGrabPoint, point) <= grabMaxDistance) {
                            closestLedgePoint = point;
                            inLedgeRange = true;
                            ledgeEdge = edge;
                            return;
                        }
                    }
                }
            }
        }

        //No se encontró nada
        inLedgeRange = false;
    }

    public void MoveClosestLedgePoint(Vector3 direction, float distance) {

    }

    public bool InSegmentRange(Vector3 start, Vector3 end, Vector3 point) {
        Vector3 delta = end - start;
        float innerProduct = (point.x - start.x) * delta.x + (point.y - start.y) * delta.y + (point.z - start.z) * delta.z;
        return innerProduct >= 0 && innerProduct <= delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
    }

    public static Vector3 ProjectOnLedgeEdge(Vector3 point, LedgeEdge le) {
        return Vector3.Project(point - le.a.position, (le.b.position - le.a.position).normalized) + le.a.position;
    }

    private void OnDrawGizmosSelected() {

        //Dibujar el punto de agarrado
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.rotation * grabPoint, 0.05f);

        //Dibujar una linea del punto de agarrado hacia la ladera más cercana
        if (inLedgeRange) {
            Gizmos.DrawLine(transform.position + transform.rotation * grabPoint, closestLedgePoint);
        }
    }
}
