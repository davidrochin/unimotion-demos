using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public float health = 200f;
    public float maxHealth = 200f;

    private void OnGUI() {
        if (gameObject.tag.Equals("Player")) {
            GUILayout.Box("Health: " + health);
        }
    }
}
