using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLink : MonoBehaviour {

    public string scene;
    public string markerId = "1";
    public Vector3 size = new Vector3(2f, 2.5f, 0.5f);

    public LayerMask characterMask;

    void Start() {

    }

    void LateUpdate() { 

        if(fade){
            fadeOverlay = new Color(fadeOverlay.r, fadeOverlay.g, fadeOverlay.b, Mathf.Clamp01(fadeOverlay.a + 1f * Time.deltaTime));
        } else {
            fadeOverlay = new Color(fadeOverlay.r, fadeOverlay.g, fadeOverlay.b, Mathf.Clamp01(fadeOverlay.a - 1f * Time.deltaTime));
        }

        if(fadeOverlay.a >= 1f) {
            faded = true;
            SceneLinkManager.instance.TeleportPlayer(this, player);
        } else {
            faded = false;
        }

        Collider[] cols = Physics.OverlapBox(transform.position, size * 0.5f, transform.rotation, characterMask);
        if (cols.Length > 0) {
            foreach(Collider c in cols){
                Player player = c.GetComponent<Player>();
                if(player != null) {
                    fade = true;
                    this.player = player;
                    break;
                }
            }
        }
	}

    public Player player;
    public bool fade = false;
    public bool faded = false;
    public Color fadeOverlay = Color.clear;
    

    void OnGUI() {

        Texture2D texture = new Texture2D(1, 1);
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                texture.SetPixel(x, y, fadeOverlay);
            }
        }
        texture.Apply();

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
    }

    void OnDrawGizmos() {
        /* Gizmos.color = Color.red;
         Gizmos.DrawWireCube(transform.position + transform.up, new Vector3(0.5f, 2f, 0.5f));*/

        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(Vector3.zero), transform.rotation, transform.lossyScale);

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.color = new Color(0f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
    
}
