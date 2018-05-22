using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public float health = 200f;
    public float maxHealth = 200f;

    [HideInInspector]
    public bool isAlive = true;

    //References
    Character character;
    CharacterControl charControl;

    //Events
    public event Action OnDeath;
    public event Action OnRevive;

    private void Awake() {
        character = GetComponent<Character>();
        charControl = GetComponent<CharacterControl>();

        character.OnHighFall += delegate () {
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

    public void Kill() {
        health = 0f;
        GetComponent<Animator>().SetBool("dead", true);
        isAlive = false;
        if (OnDeath != null) OnDeath();
    }

    public void Revive() {
        health = maxHealth;
        GetComponent<Animator>().SetBool("dead", false);
        isAlive = true;
        if (OnRevive != null) OnRevive();
    }

    void CheckDeath() {
        if(health <= 0f) {
            Kill();
        }
    }

    private void OnGUI() {

        if(charControl == null && health > 0f) {
            Vector2 barSize = new Vector2(150f, 20f);
            Vector2 barPos = Camera.main.WorldToScreenPoint(character.transform.position + character.characterController.center + Vector3.up * character.characterController.height * 0.5f);
            barPos = new Vector2(barPos.x, (Screen.height - barPos.y));

            GUI.Box(new Rect(barPos - Vector2.right * barSize.x * 0.5f - Vector2.up * 15f, barSize), "" + health);
        }

    }
}
