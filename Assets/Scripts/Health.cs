using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public float current = 200f;
    public float max = 200f;

    [HideInInspector]
    public bool isAlive = true;

    //References
    Character character;
    CharacterControl characterControl;
    Animator animator;

    //Events
    public event Action OnDeath;
    public event Action OnRevive;
    public event FloatAction OnDamage;

    private void Awake() {
        character = GetComponent<Character>();
        characterControl = GetComponent<CharacterControl>();
        animator = GetComponent<Animator>();

        character.OnHighFall += delegate () {
            Debug.Log("Uuuh");
            SubstractHealth(200f);
        };
        CheckDeath();
    }

    public void Damage(float quantity, bool damageAnimation) {
        current = Mathf.Clamp(current - quantity, 0f, max);
        if (OnDamage != null) { OnDamage(quantity); }
        CheckDeath();
    }

    public void SubstractHealth(float quantity) {
        Damage(quantity, false);
    }

    public void AddHealth(float quantity) {
        current = Mathf.Clamp(current + quantity, 0f, max);
        CheckDeath();
    }

    public void Kill() {
        current = 0f;
        GetComponent<Animator>().SetBool("dead", true);
        isAlive = false;
        if (OnDeath != null) OnDeath();
    }

    public void Revive() {
        current = max;
        GetComponent<Animator>().SetBool("dead", false);
        isAlive = true;
        if (OnRevive != null) OnRevive();
    }

    void CheckDeath() {
        if(current <= 0f) {
            Kill();
        }
    }

    private void OnGUI() {

        if(characterControl == null && current > 0f) {
            Texture2D backTex = Util.Texture2D.CreateEmpty(Color.black);
            Texture2D frontTex = Util.Texture2D.CreateEmpty(Color.red);

            Vector2 barSize = new Vector2(80f, 5f);
            Vector2 barPos = Camera.main.WorldToScreenPoint(character.transform.position + character.characterController.center + Vector3.up * character.characterController.height * 0.4f);
            barPos = new Vector2(barPos.x, (Screen.height - barPos.y));

            //GUI.Box(new Rect(barPos - Vector2.right * barSize.x * 0.5f - Vector2.up * 15f, barSize), "" + current);
            GUI.DrawTexture(new Rect(barPos - Vector2.right * barSize.x * 0.5f - Vector2.up * 15f, barSize), backTex);
            GUI.DrawTexture(new Rect(barPos - Vector2.right * barSize.x * 0.5f - Vector2.up * 15f, new Vector2((current / max) * barSize.x, barSize.y)), frontTex);
        }

        if (characterControl != null) {
            GUI.DrawTexture(new Rect(0f, 0f, max, 15f), Util.Texture2D.CreateEmpty(Color.black));
            GUI.DrawTexture(new Rect(0f, 0f, current, 15f), Util.Texture2D.CreateEmpty(Color.red));
        }

    }

}
