using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(Ledge))]
public class LedgeEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        Ledge script = (Ledge)target;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate nodes")) {
            script.AutoCalculateNodes();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        if (GUILayout.Button("Clear all nodes")) {
            script.ClearNodes();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        GUILayout.EndHorizontal();
    }

}