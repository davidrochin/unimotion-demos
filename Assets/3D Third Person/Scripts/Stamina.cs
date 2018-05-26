using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour {

    public float current = 200f;
    public float max = 200f;

    float timeSinceDepleted = float.MaxValue;

    //References
    CharacterControl characterControl;
    Character character;

	void Awake () {
        characterControl = GetComponent<CharacterControl>();
        character = GetComponent<Character>();
    }
	
	void Update () {

        timeSinceDepleted += Time.deltaTime;

        if ((character == null || (!character.combatState.isAttacking && !character.combatState.isBlocking && !character.combatState.isRolling)) && timeSinceDepleted >= 3f) {
            current = Mathf.Clamp(current + 60f * Time.deltaTime, 0f, max);
        }
        
	}

    public bool Consume(float quantity) {
        if (current > 0f) {
            current = Mathf.Clamp(current - quantity, 0f, max);
            if (current <= 0f) {
                timeSinceDepleted = 0f;
            }
            return true;
        } else {
            return false;
        }
    }

    private void OnGUI() {
        
        if(characterControl != null) {
            GUI.DrawTexture(new Rect(0f, 15f, max, 15f), Util.Texture2D.CreateEmpty(Color.black));
            GUI.DrawTexture(new Rect(0f, 15f, current, 15f), Util.Texture2D.CreateEmpty(Color.green));
        }

    }
}
