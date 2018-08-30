using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[SelectionBase]
public class VirtualJoystick : MonoBehaviour, IDragHandler {

    public Image thumbstick;
    public Image joystickFrame;

    public CharacterMotor character;

    public Vector2 output;

    float maxMagnitude;

    public void OnDrag(PointerEventData eventData) {
        Debug.Log(eventData.delta);
        thumbstick.rectTransform.anchoredPosition += eventData.delta;
    }

    void Awake () {
		if(Application.platform != RuntimePlatform.Android) {
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

        if (Input.touchCount <= 0 && Input.GetMouseButton(0) == false) {
            thumbstick.rectTransform.anchoredPosition = Vector2.zero;
        }

        Vector3 thumbstickDelta = thumbstick.rectTransform.anchoredPosition; 
        output = thumbstickDelta / maxMagnitude;

	}
}
