using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour {

    public float current = 200f;
    public float max = 200f;

    public bool isUsingStamina = false;

    float timeSinceLastConsume = float.MaxValue;

    //References
    CharacterControl characterControl;

	void Awake () {
        characterControl = GetComponent<CharacterControl>();
	}
	
	void Update () {

        timeSinceLastConsume += Time.deltaTime;

        if (!isUsingStamina) {
            current = Mathf.Clamp(current + 60f * Time.deltaTime, 0f, max);
        }
        
	}

    public bool Consume(float quantity) {
        if (current >= quantity && !isUsingStamina) {
            current -= quantity;
            timeSinceLastConsume = 0f;
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
