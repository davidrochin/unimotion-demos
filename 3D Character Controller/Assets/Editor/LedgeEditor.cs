using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Ledge))]
public class LedgeEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        Ledge script = (Ledge)target;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate nodes")) {
            script.AutoCalculateNodes();
        }

        if (GUILayout.Button("Clear all nodes")) {
            script.ClearNodes();
        }

        GUILayout.EndHorizontal();
    }

}