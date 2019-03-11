using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLinkManager : MonoBehaviour {

    public static SceneLinkManager instance;


    void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Update() {
        
    }

    public void TeleportPlayer(SceneLink link, Player player) {
        TeleportPlayer(link.scene, link.markerId, player);
    }

    public void TeleportPlayer(string sceneName, string markerId, Player player) {

        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> tmpDelegate = null;
        tmpDelegate = delegate (Scene scene, LoadSceneMode mode) {
            Marker[] markers = FindObjectsOfType<Marker>();
            foreach (Marker m in markers) {
                if (markerId.Equals(m.id)) {
                    player.transform.position = m.transform.position;
                    player.transform.rotation = Quaternion.LookRotation(m.transform.forward, -Physics.gravity.normalized);
                    break;
                }
            }
            SceneManager.sceneLoaded -= tmpDelegate;
        };
        
        SceneManager.sceneLoaded += tmpDelegate;
        SceneManager.LoadScene(sceneName);
    }

}
