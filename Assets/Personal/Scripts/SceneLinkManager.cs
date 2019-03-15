using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unimotion;

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

}
