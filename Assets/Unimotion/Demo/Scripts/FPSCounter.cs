using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

    Text text;

	void Awake () {
        text = GetComponent<Text>();
	}

    private void Update() {
        text.text = (int)(1f / Time.unscaledDeltaTime) + " FPS";
    }

}
