using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unimotion;

[SelectionBase]
public class VirtualJoystick : MonoBehaviour, IDragHandler {

    public int id = 0;
    public int touchId = -1;

    public Image thumbstick;
    public Image joystickFrame;

    public CharacterMotor character;

    public Vector2 input;

    float maxMagnitude;

    public void OnDrag(PointerEventData eventData) {
        touchId = eventData.pointerId;
        thumbstick.rectTransform.anchoredPosition += eventData.delta;
    }

    void Awake () {
		if(!Application.isMobilePlatform) {
            Destroy(joystickFrame.gameObject);
            Destroy(gameObject);
        }
	}

    private void Start() {
        //Calculate how much the Thumbstick can move from the center of the Frame
        maxMagnitude = joystickFrame.rectTransform.sizeDelta.x * 0.5f - thumbstick.rectTransform.sizeDelta.x * 0.5f;
    }

    void Update () {

        //Limit the position of the Thumbstick relative to the Frame
        thumbstick.rectTransform.anchoredPosition = Vector2.ClampMagnitude(thumbstick.rectTransform.anchoredPosition, maxMagnitude);

        bool touchIdPresent = false;
        foreach (Touch touch in Input.touches) {
            if(touch.fingerId == touchId) {
                touchIdPresent = true;
                break;
            }
        }
        if (!touchIdPresent) {
            touchId = -10;
            thumbstick.rectTransform.anchoredPosition = Vector2.zero;
        }

        Vector3 thumbstickDelta = thumbstick.rectTransform.anchoredPosition; 
        input = thumbstickDelta / maxMagnitude;

	}

    public static Vector2 GetInput(int id) {
        foreach (VirtualJoystick vj in FindObjectsOfType<VirtualJoystick>()) {
            if(vj.id == id) {
                return vj.input;
            }
        }
        return Vector2.zero;
    }

    public static VirtualJoystick GetById(int id) {
        foreach (VirtualJoystick vj in FindObjectsOfType<VirtualJoystick>()) {
            if (vj.id == id) {
                return vj;
            }
        }
        return null;
    }
}
