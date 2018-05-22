using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Health))]
public class HealthEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Kill")) {
            ((Health)target).Kill();
        }
        if (GUILayout.Button("Revive")) {
            ((Health)target).Revive();
        }
        GUILayout.EndHorizontal();
    }
}