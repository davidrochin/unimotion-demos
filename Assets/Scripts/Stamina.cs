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
    Equipment equipment;

	void Awake () {
        characterControl = GetComponent<CharacterControl>();
        character = GetComponent<Character>();
        equipment = GetComponent<Equipment>();
    }
	
	void Update () {

        timeSinceDepleted += Time.deltaTime;

        if ((character == null || (!character.state.attacking && !character.state.blocking && !character.state.rolling)) && timeSinceDepleted >= 2f) {
            current = Mathf.Clamp(current + 60f * Time.deltaTime, 0f, max);
        }
        
	}

    public bool Consume(float quantity) {
        if (current > 0f) {
            //Substract to current Stamina
            current = Mathf.Clamp(current - quantity, 0f, max);

            if (current <= 0f) {
                timeSinceDepleted = 0f;

                //Stop blocking
                if (character != null) {
                    character.StopBlocking();
                }
            }
            return true;
        } else {
            timeSinceDepleted = 0f;
            return false;
        }
    }

    public bool ConsumeFromWeapon() {
        if (equipment != null) {
            Weapon weapon = equipment.equipedWeapon;
            if (weapon != null) {
                return Consume(weapon.damage);
            }
        }
        return false;
    }

    public bool Has(float quantity) {
        if (current >= quantity) {
            return true;
        } else {
            return false;
        }
    }

    public bool HasAny() {
        if (current > 0f) {
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
