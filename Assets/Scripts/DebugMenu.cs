using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : MonoBehaviour {

    bool sixtyFps = false;

    private void OnGUI() {
        sixtyFps = GUILayout.Toggle(sixtyFps, "60 FPS");
    }

    private void FixedUpdate() {
        if (sixtyFps) {
            QualitySettings.vSyncCount = 1;
        } else {
            QualitySettings.vSyncCount = 2;
        }
    }

}
