using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public string startingSceneName;

    void Awake() {
        DontDestroyOnLoad(this);
    }

    void Start () {
        SceneLinkManager.instance.TeleportPlayer(startingSceneName, "1", FindObjectOfType<Player>());
	}
	
	void Update () {
		
	}
}
