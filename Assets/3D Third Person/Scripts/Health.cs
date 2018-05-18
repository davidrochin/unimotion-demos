using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public float health = 200f;
    public float maxHealth = 200f;

    public bool isAlive = true;

    private void Awake() {
        GetComponent<Character>().OnHighFall += delegate () {
            Debug.Log("Uuuh");
            SubstractHealth(200f);
        };
        CheckDeath();
    }

    public void SubstractHealth(float quantity) {
        health = Mathf.Clamp(health - quantity, 0f, maxHealth);
        CheckDeath();
    }

    public void AddHealth(float quantity) {
        health = Mathf.Clamp(health + quantity, 0f, maxHealth);
        CheckDeath();
    }

    void CheckDeath() {
        if(health <= 0f) {
            GetComponent<Animator>().SetBool("dead", true);
            isAlive = false;
        } else {
            GetComponent<Animator>().SetBool("dead", false);
            isAlive = true;
        }
    }

    private void OnGUI() {
        if (gameObject.tag.Equals("Player")) {
            GUI.Box(new Rect(0f, 0f, maxHealth, 20f), "");
            GUI.Box(new Rect(0f, 0f, health, 20f), "" + health);
        }
    }
}
