using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDebug {

	public static void CreateText(Vector3 position, string text) {
        GameObject go = new GameObject();
        go.transform.position = position;
        TextMesh textMesh = go.AddComponent<TextMesh>();
        go.AddComponent<Billboard>();
        textMesh.text = text;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.1f;
        textMesh.color = Color.red;
    }

    public static void DeleteAll() {
        TextMesh[] tms = GameObject.FindObjectsOfType<TextMesh>();
        foreach (TextMesh tm in tms) {
            GameObject.DestroyImmediate(tm.gameObject);
        }
    }
}
