using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabber : MonoBehaviour {

    [Header("Hands")]
    [Tooltip("Define que tan separadas estarán las manos del punto de agarrado.")]
    public float handSeparation = 0.4f;

    [Header("Debug")]
    public bool canGrab = true;

    public Vector3 grabPoint;
    public float grabMaxDistance = 2f;

    public Vector3 closestLedgePoint;
    public LedgeEdge ledgeEdge;
    public bool inLedgeRange = false;

    public void GetClosestLedgePoint() {
        if (canGrab) {
            //Calcular el grabPoint real
            Vector3 realGrabPoint = transform.position + transform.rotation * grabPoint;

            //Iterar por todos los Ledge de la escena
            foreach (Ledge ledge in FindObjectsOfType<Ledge>()) {

                //Iterar por todos los Edge del Ledge
                if (ledge.ledgeEdges != null) {
                    foreach (LedgeEdge edge in ledge.ledgeEdges) {

                        //Revisar si está en rango del segmento del Edge
                        if (InSegmentRange(edge.a.position, edge.b.position, realGrabPoint)) {

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
        
    }

    public void MoveAlongEdge(Vector3 point, Vector3 direction, float distance, Vector3 relativeDir) {
        Vector3 newPoint = point + direction * distance;
        float perc = PercentageOfLedge(ledgeEdge, newPoint);
        //Debug.Log(perc);

        //Revisar si se pasó del Ledge
        if(perc < 0f || perc > 1f) {
            Debug.Log("Se pasó");
            //Debug.Log("Distance " + Vector3.Distance(closestLedgePoint, ledgeEdge.a.position));
            TransferToNextEdge(newPoint, direction, relativeDir);
        } else {
            closestLedgePoint = ProjectOnLedgeEdge(newPoint, ledgeEdge);
        }
    }

    void TransferToNextEdge(Vector3 point, Vector3 edgeMoveDirection, Vector3 relativeDir) {

        //Calcular el producto interno normalizado para saber si está en A o en B
        float perc = PercentageOfLedge(ledgeEdge, point);
        LedgeNode currentNode = null;
        LedgeNode excludeNode = null;
        LedgeNode nextEdgeNode = null;

        if (perc < 0f) {
            currentNode = ledgeEdge.a;
            excludeNode = ledgeEdge.b;
        } else if(perc > 1f) {
            currentNode = ledgeEdge.b;
            excludeNode = ledgeEdge.a;
        }

        //Buscar el siguiente Edge
        foreach (int nodeId in currentNode.connectedNodes) {
            if(nodeId != excludeNode.id) {
                nextEdgeNode = ledgeEdge.ledge.GetNode(nodeId);
                break;
            }
        }

        //Imprimir angulo
        Vector3 to = (nextEdgeNode.position - currentNode.position).normalized;
        float angle = Vector3.Angle(transform.forward, to);
        Debug.Log(angle + ", " + edgeMoveDirection);

        //Moverse al siguiente Edge
        if(angle >= 90f) {
            ledgeEdge = ledgeEdge.ledge.GetEdge(currentNode, nextEdgeNode);
            closestLedgePoint = currentNode.position;
            //closestLedgePoint = currentNode.position + (nextEdgeNode.position - currentNode.position).normalized * 0.5f;
        } else {
            Debug.Log("Se hizo correccion de rotacion");
            if (relativeDir == Vector3.left) { transform.position = Vector3Util.RotateAroundPoint(transform.position, currentNode.position, Quaternion.Euler(0f, 90f - angle, 0f)); }
            else if (relativeDir == Vector3.right) { transform.position = Vector3Util.RotateAroundPoint(transform.position, currentNode.position, Quaternion.Euler(0f, -90f - angle, 0f)); }
            ledgeEdge = ledgeEdge.ledge.GetEdge(currentNode, nextEdgeNode);
            closestLedgePoint = currentNode.position;
            //closestLedgePoint = currentNode.position + (nextEdgeNode.position - currentNode.position).normalized * 0.5f; 
        }
    }

    public bool InSegmentRange(Vector3 start, Vector3 end, Vector3 point) {
        Vector3 delta = end - start;
        float innerProduct = (point.x - start.x) * delta.x + (point.y - start.y) * delta.y + (point.z - start.z) * delta.z;
        return innerProduct >= 0 && innerProduct <= delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
    }

    public static float InnerProduct(Vector3 a, Vector3 b, Vector3 point) {
        Vector3 delta = b - a;
        return (point.x - a.x) * delta.x + (point.y - a.y) * delta.y + (point.z - a.z) * delta.z;
    }

    public static float NormalizedInnerProduct(Vector3 a, Vector3 b, Vector3 point) {
        Vector3 delta = b - a;
        float innerProduct = (point.x - a.x) * delta.x + (point.y - a.y) * delta.y + (point.z - a.z) * delta.z;
        return innerProduct / (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
    }

    public float PercentageOfLedge(LedgeEdge edge, Vector3 point) {
        return NormalizedInnerProduct(edge.a.position, edge.b.position, point);
    }

    public static Vector3 ProjectOnLedgeEdge(Vector3 point, LedgeEdge le) {
        return Vector3.Project(point - le.a.position, (le.b.position - le.a.position).normalized) + le.a.position;
    }

    private void OnDrawGizmosSelected() {

        //Calcular el punto de agarrado real
        Vector3 realGrabPoint = transform.position + transform.rotation * grabPoint;

        //Dibujar el punto de agarrado
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.rotation * grabPoint, 0.05f);

        //Dibujar la separación desde el punto de agarrado
        Gizmos.DrawLine(realGrabPoint, realGrabPoint + Quaternion.Euler(0f, 90f, 0f) * transform.forward * handSeparation);
        Gizmos.DrawLine(realGrabPoint, realGrabPoint + Quaternion.Euler(0f, -90f, 0f) * transform.forward * handSeparation);

        //Dibujar una linea del punto de agarrado hacia la ladera más cercana
        if (inLedgeRange) {
            Gizmos.DrawLine(realGrabPoint, closestLedgePoint);
        }
    }
}
