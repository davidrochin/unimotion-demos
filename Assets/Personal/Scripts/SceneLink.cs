using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLink : MonoBehaviour {

    public string scene;
    public string markerId;
    public Vector3 size = new Vector3(2f, 2.5f, 0.5f);

    public LayerMask characterMask;

	void Start () {
		
	}
	
	void LateUpdate () {
        Collider[] cols = Physics.OverlapBox(transform.position, size * 0.5f, Quaternion.identity, characterMask);
        if (cols.Length > 0) {
            foreach(Collider c in cols){
                Player player = c.GetComponent<Player>();
                if(player != null) {
                    SceneLinkManager.instance.TeleportPlayer(this, player);
                }
            }
        }
	}

    void OnDrawGizmos() {
       /* Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + transform.up, new Vector3(0.5f, 2f, 0.5f));*/
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, size);

        Gizmos.color = new Color(0f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireCube(transform.position, size);
    }
}
