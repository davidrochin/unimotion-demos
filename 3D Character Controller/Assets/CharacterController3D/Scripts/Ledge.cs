using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour {

    [Header("Debug")]
    public bool drawNodes = false;
    public bool drawNodeNormals = false;
    public bool drawTriangleNormals = false;

    public List<LedgeNode> ledgeNodes;
    public List<LedgeTriangle> ledgeTriangles;
    public List<LedgeNode> outerNodes;
    public List<Vector3> baseOfWallPositions;

    public void AutoCalculateNodes() {

        //Inicializar las listas
        ledgeTriangles = new List<LedgeTriangle>();
        ledgeNodes = new List<LedgeNode>();
        outerNodes = new List<LedgeNode>();
        baseOfWallPositions = new List<Vector3>();

        CalculateTriangles();
        DeleteNonLedges();
        RemoveLostVertexTriangles();

        JoinDuplicates();
        //ShowNodesText();
        //ShowTrianglesText();
        DeleteInnerEdges();
        DeleteWallBaseConnections();
        RemoveLooseNodes();

        baseOfWallPositions = null;
    }

    public void ClearNodes() {
        ledgeNodes = null;
        ledgeTriangles = null;
        outerNodes = null;
        baseOfWallPositions = null;
        TextDebug.DeleteAll(transform);
    }

    void CalculateTriangles() {

        //Obtener la malla, sus triangulos y sus vertices
        Mesh mesh = GetComponent<MeshCollider>().sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        Debug.Log("triangles.lenght = " + triangles.Length + ", vertices.lenght = " + vertices.Length + ", normals.lenght = " + normals.Length);

        //Iterar por cada triangulo
        for (int x = 0; x < triangles.Length; x = x + 3) {

            LedgeNode ln1 = null, ln2 = null, ln3 = null;

            //Obtener los indices
            int ind1 = triangles[x], ind2 = triangles[x + 1], ind3 = triangles[x + 2];

            //Convertir los vertices a LedgeNode
            ln1 = new LedgeNode(Vector3Util.RotateAroundPoint(vertices[ind1] + transform.position, transform.position, transform.rotation));
            ln2 = new LedgeNode(Vector3Util.RotateAroundPoint(vertices[ind2] + transform.position, transform.position, transform.rotation));
            ln3 = new LedgeNode(Vector3Util.RotateAroundPoint(vertices[ind3] + transform.position, transform.position, transform.rotation));

            //Extraer las normales
            ln1.normal = transform.rotation * normals[ind1];
            ln2.normal = transform.rotation * normals[ind2];
            ln3.normal = transform.rotation * normals[ind3];

            //Definirles a que Ledge pertenecen
            ln1.ledge = this; ln2.ledge = this; ln3.ledge = this;

            //Si los 3 vertices existen, conectarlos
            if (ln1 != null && ln2 != null && ln3 != null) {
                ln1.connectedNodes = new int[] { ln2.id, ln3.id };
                ln2.connectedNodes = new int[] { ln1.id, ln3.id };
                ln3.connectedNodes = new int[] { ln1.id, ln2.id };

                //Añadir el triangulo a la lista
                LedgeTriangle tri = new LedgeTriangle(new LedgeNode[] { ln1, ln2, ln3 });
                tri.ledge = this;
                ledgeTriangles.Add(tri);
            }

            //Agregar a la lista los vertices
            if (ln1 != null) { ledgeNodes.Add(ln1); }
            if (ln2 != null) { ledgeNodes.Add(ln2); }
            if (ln3 != null) { ledgeNodes.Add(ln3); }
        }

        //ledgeNodes = nodeList.ToArray();
        Debug.Log("Calculations completed with " + ledgeNodes.Count + " LedgeNodes, " + ledgeTriangles.Count + " LedgeTriangles.");
    }

    void DeleteNonLedges() {

        LedgeNode[] temp = ledgeNodes.ToArray();
        foreach (LedgeNode n in temp) {
            bool delete = false;
            float angle = Vector3.Angle(Vector3.up, n.normal);
            //TextDebug.CreateText(n.position + n.normal, "" + angle);

            //Si el angulo de la normal es más de 45 grados borrarlo
            if (angle > 45f) {
                delete = true;

                //Revisar si es la base de un muro
                foreach (int currentCon in n.connectedNodes) {
                    if(GetNode(currentCon) != null) {
                        Vector3 dirToNode = n.DirectionToNode(GetNode(currentCon));
                        if (Vector3.Angle(Vector3.up, dirToNode) < 45f) {
                            baseOfWallPositions.Add(n.position);
                        }
                    }
                    
                }
            }
            if (delete) { ledgeNodes.Remove(n); }
        }
    }

    void JoinDuplicates() {

        //Iterar por los nodos
        for (int x = 0; x < ledgeNodes.Count; x++) {

            //Buscar los nodos que estan exactamente en el mismo lugar que el nodo actual
            LedgeNode[] nodesAtPos = SearchForNodesAtPosition(ledgeNodes[x].position);

            //Debug.Log("Node " + x + " has " + nodesAtPos.Length + " repeated nodes (incluiding itself).");

            //Iterar por los nodos duplicados
            foreach (LedgeNode dupNode in nodesAtPos) {
                if (dupNode != ledgeNodes[x]) {

                    //Debug.Log("Reconnecting node " + x);
                    ledgeNodes[x].AddConnections(GetNodes(dupNode.connectedNodes));
                    dupNode.ReconnectConnected(ledgeNodes[x]);
                    //nodesToDelete.Add(dupNode);
                    ReplaceNodesInTriangles(dupNode, ledgeNodes[x]);
                    ledgeNodes.Remove(dupNode);
                }
            }
        }
    }

    void ReplaceNode(LedgeNode toKeep, LedgeNode toReplace) {

        //Pasar las conexiones del nodo a reemplazar al nodo que se quedará
        toReplace.PassConnectionsToNode(toKeep);
        toKeep.RemoveConnectedDuplicates();

        foreach (int n in toReplace.connectedNodes) {
            for (int x = 0; x < GetNode(n).connectedNodes.Length; x++) {
                if (GetNode(GetNode(n).connectedNodes[x]) == toReplace) {
                    GetNode(n).connectedNodes[x] = toKeep.id;
                }
            }
        }

        ReplaceNodesInTriangles(toReplace, toKeep);

        //Remover el nodo de la lista de nodos de este Ledge
        ledgeNodes.Remove(toReplace);
    }

    void SearchForNodeDuplicates() {
        foreach (LedgeNode n in ledgeNodes) {
            foreach (LedgeNode n2 in ledgeNodes) {
                if (n.position == n2.position && n != n2) {
                    Debug.LogWarning("Found two or more LedgeNodes in the same position: " + n.position + ", " + n2.position);
                    TextDebug.CreateText(n.position, "Duplicate here", transform);
                    return;
                }
            }
        }
    }

    void IdentifyOuterNodes() {

        List<LedgeNode> nonOuterNodes = new List<LedgeNode>();

        //Identificar los nodos exteriores
        for (int x = 0; x < ledgeNodes.Count; x++) {
            //Si solo tiene 2 conexiones, es seguro que es un nodo exterior
            if (ledgeNodes[x].connectedNodes.Length == 2) {
                outerNodes.Add(ledgeNodes[x]);
            } else {
                nonOuterNodes.Add(ledgeNodes[x]);
            }
        }

    }

    void DeleteInnerEdges() {
        List<LedgeTriangle> tris = new List<LedgeTriangle>(ledgeTriangles);

        //Remover lineas compartidas entre triangulos
        for (int x = 0; x < tris.Count; x++) {
            for (int y = 0; y < tris.Count; y++) {
                if (tris[x] != tris[y] && tris[x].SharesEdge(tris[y])) {
                    //Debug.Log("Triangles " + tris[x].id + " and " + tris[y].id + " share edge.");
                    tris[x].RemoveSharedEdge(tris[y]);
                }
            }
        }

        //Remover nodos sueltos
        /*LedgeNode[] temp = ledgeNodes.ToArray();
        foreach (LedgeNode node in temp) {
            if(node.connectedNodes.Length < 1) {
                ledgeNodes.Remove(node);
            }
        }*/
    }

    void DeleteWallBaseConnections() {
        LedgeNode[] temp = ledgeNodes.ToArray();
        foreach (LedgeNode node in temp) {
            //Revisar si es un nodo que está en la base de un muro
            if (baseOfWallPositions.Find(a => a == node.position) != Vector3.zero) {
                
                //Revisar sus conexiones para ver si estan conectados con otro nodo de base de muro
                foreach (int con in node.connectedNodes) {
                    LedgeNode n = GetNode(con);
                    if(n != node && baseOfWallPositions.Find(a => a == n.position) != Vector3.zero) {
                        node.RemoveConnection(n);
                    }
                }

            }
        }
    }

    void RemoveLooseNodes() {
        //Remover nodos sueltos
        LedgeNode[] temp = ledgeNodes.ToArray();
        foreach (LedgeNode node in temp) {
            if (node.connectedNodes.Length < 1) {
                ledgeNodes.Remove(node);
            }
        }
    }

    LedgeNode[] SearchForNodesAtPosition(Vector3 pos) {
        List<LedgeNode> nodesAtPos = new List<LedgeNode>();

        foreach (LedgeNode node in ledgeNodes) {
            if (node.position == pos) { nodesAtPos.Add(node); }
        }

        return nodesAtPos.ToArray();

    }

    bool SearchNode(LedgeNode node) {
        foreach (LedgeNode n in ledgeNodes) {
            if (n == node) {
                return true;
            }
        }
        return false;
    }

    public LedgeNode GetNode(int id) {
        foreach (LedgeNode ln in ledgeNodes) {
            if(ln.id == id) {
                return ln;
            }
        }
        return null;
    }

    public LedgeNode[] GetNodes(int[] ids) {
        List<LedgeNode> temp = new List<LedgeNode>();
        foreach (int id in ids) {
            LedgeNode n = GetNode(id);
            if(n != null) { temp.Add(n); }
        }
        return temp.ToArray();
    }

    void ReplaceNodesInTriangles(LedgeNode toReplace, LedgeNode toKeep) {
        //Debug.Log("Voy a reemplazar el nodo " + toReplace.id + " por el nodo " + toKeep.id);
        for (int x = 0; x < ledgeTriangles.Count; x++) {
            for (int y = 0; y < ledgeTriangles[x].nodes.Length; y++) {
                if (ledgeTriangles[x].nodes[y] == toReplace) {
                    //Debug.Log("Replacing node in triangle");
                    ledgeTriangles[x].nodes[y] = toKeep;
                }
            }
        }
    }

    void RemoveLostVertexTriangles() {

        List<LedgeTriangle> toRemove = new List<LedgeTriangle>();

        //Buscar los Triangulos que perdieron nodos
        foreach (LedgeTriangle tri in ledgeTriangles) {
            foreach (LedgeNode node in tri.nodes) {
                if (!SearchNode(node)) {
                    toRemove.Add(tri);
                }
            }
        }

        //Remover dichos triangulos
        foreach (LedgeTriangle tri in toRemove) {
            ledgeTriangles.Remove(tri);
        }
    }

    void ShowNodesText() {
        foreach (LedgeNode node in ledgeNodes) {
            TextDebug.CreateText(node.position + node.normal, node.id + "", transform);
        }
    }

    void ShowTrianglesText() {
        foreach (LedgeTriangle tri in ledgeTriangles) {
            string nodesText = "";
            foreach (LedgeNode n in tri.nodes) {
                nodesText = nodesText + n.id + " ";
            }
            TextDebug.CreateText(tri.center + tri.normal, "Triangle " + tri.id + "\n" + nodesText, transform);
        }
    }

    private void OnDrawGizmosSelected() {

        if (ledgeNodes != null) {
            foreach (LedgeNode node in ledgeNodes) {

                Gizmos.color = Color.red;

                //Dibujar el nodo
                if (drawNodes) {
                    Gizmos.DrawSphere(transform.position + node.position, 0.05f);
                }

                //Dibujar lineas hacia los nodos conectados
                if (node.connectedNodes != null) {
                    foreach (int conNode in node.connectedNodes) {
                        /*Debug.Log(node.position);
                        Debug.Log(GetNode(conNode));*/
                        Gizmos.DrawLine(node.position, GetNode(conNode).position);
                    }
                }

                //Dibujar la normal del nodo
                if (drawNodeNormals) {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(node.position, node.normal);
                }
            }
        }

        /*if (baseOfWallPositions != null) {
            foreach (Vector3 pos in baseOfWallPositions) {
                //Dibujar el nodo
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + pos, 0.06f);
            }
        }*/

        if (ledgeTriangles != null && drawTriangleNormals) {
            Gizmos.color = Color.green;
            foreach (LedgeTriangle tri in ledgeTriangles) {
                Gizmos.DrawRay(tri.center, tri.normal);
            }
        }
    }

}

[System.Serializable]
public class LedgeNode {

    static int nextId = 0;

    public int id;
    public Ledge ledge;
    public Vector3 position;
    public Vector3 normal;
    public int[] connectedNodes;

    public LedgeNode(Vector3 pos) {
        position = pos;
        id = nextId; nextId++;
    }

    public void RemoveConnectedDuplicates() {
        List<int> newConnected = new List<int>();
        foreach (int node in connectedNodes) {
            if (newConnected.Find(x => x == node) == -1) {
                if (ledge.GetNode(node) != this) {
                    newConnected.Add(node);
                }
            }
        }
        connectedNodes = newConnected.ToArray();
    }

    public void PassConnectionsToNode(LedgeNode toNode) {
        List<int> newConnected = new List<int>();
        foreach (int node in connectedNodes) {
            if (ledge.GetNode(node) != toNode) {
                newConnected.Add(node);
            }
        }
        foreach (int node in toNode.connectedNodes) {
            newConnected.Add(node);
        }
        toNode.connectedNodes = newConnected.ToArray();
    }

    public void AddConnections(LedgeNode[] newNodes) {
        List<int> temp = new List<int>(connectedNodes);
        foreach (LedgeNode node in newNodes) {
            temp.Add(node.id);
        }
        connectedNodes = temp.ToArray();
    }

    public void RemoveConnection(LedgeNode node) {
        List<int> conNodes = new List<int>(connectedNodes);
        foreach (int n in conNodes) {
            if (n == node.id) {
                conNodes.Remove(n);
                break;
            }
        }
        connectedNodes = conNodes.ToArray();
    }

    //Este metodo es para cambiar les reconnecciones en otros nodos que se conecten con este.
    public void ReconnectConnected(LedgeNode newNode) {
        foreach (int n in connectedNodes) {
            for (int x = 0; x < ledge.GetNode(n).connectedNodes.Length; x++) {
                if (ledge.GetNode(ledge.GetNode(n).connectedNodes[x]) == this) { ledge.GetNode(n).connectedNodes[x] = newNode.id; }
            }
        }
    }

    public Vector3 DirectionToNode(LedgeNode to) {
        //Debug.Log("to=" + to); Debug.Log("pos=" + position);
        Vector3 dir = to.position - position;
        return dir.normalized;
    }

}

[System.Serializable]
public class LedgeTriangle {
    static int nextId = 0;

    public int id;
    public LedgeNode[] nodes;
    public Vector3 normal;
    public Vector3 center;
    public Ledge ledge;

    public LedgeTriangle(LedgeNode[] nds) {
        nodes = nds;
        normal = (nds[0].normal + nds[1].normal + nds[2].normal) / 3;
        center = (nds[0].position + nds[1].position + nds[2].position) / 3;
        id = nextId; nextId++;
    }

    public bool SharesEdge(LedgeTriangle tri) {
        //Encontrar que nodos comparten
        List<LedgeNode> sharedNodes = new List<LedgeNode>();
        foreach (LedgeNode n in nodes) {
            foreach (LedgeNode n2 in tri.nodes) {
                if (n == n2 && sharedNodes.Find(a => a == n) == null) {
                    sharedNodes.Add(n);
                }
            }
        }

        if (sharedNodes.Count >= 2) {
            return true;
        }
        return false;
    }

    public void RemoveSharedEdge(LedgeTriangle tri) {
        //Encontrar que nodos comparten
        List<int> sharedNodes = new List<int>();
        foreach (LedgeNode n in nodes) {
            foreach (LedgeNode n2 in tri.nodes) {
                if (n == n2 && !sharedNodes.Contains(n.id)) {
                    sharedNodes.Add(n.id);
                }
            }
        }
        //Debug.Log(sharedNodes.Count + " shared nodes found.");
        //Borrar las conexiones entre esos nodos
        foreach (int n in sharedNodes) {
            foreach (int cn in ledge.GetNode(n).connectedNodes) {
                if (sharedNodes.Contains(cn) && n != cn) {
                    //Debug.Log("Removiendo conexiones");
                    ledge.GetNode(n).RemoveConnection(ledge.GetNode(cn));
                    ledge.GetNode(cn).RemoveConnection(ledge.GetNode(n));
                }
            }
        }
    }
}
