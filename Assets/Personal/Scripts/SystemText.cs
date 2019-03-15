using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemText : MonoBehaviour {

    Text uiText;
    float remainingTime = 0f;

    void Awake() {
        uiText = GetComponent<Text>();
    }

    void Update() {
        uiText.enabled = remainingTime > 0f;
        remainingTime -= Time.deltaTime;
    }

    public void ShowText(string text) {
        Debug.Log(text);
        remainingTime = 5f;
        uiText.text = text;
    }
}
