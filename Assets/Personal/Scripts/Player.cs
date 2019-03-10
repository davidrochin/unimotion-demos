using UnityEngine;

public class Player : MonoBehaviour {

    void Awake() {
        DontDestroyOnLoad(this);
    }

}
