using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoMenu : MonoBehaviour {

    public RectTransform sidePanel;
    public Button togglePanelButton;

    [Header("Dropdowns")]
    public Dropdown walkDropdown;
    public Dropdown jumpDropdown;

    [Header("Sliders")]
    public Slider walkSpeedSlider;
    public Slider jumpForceSlider;

    bool panelOpen = false;
    Vector2 panelOpenPosition;

    PlayerCamera camera;

	void Awake () {
        camera = FindObjectOfType<PlayerCamera>();

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        panelOpenPosition = sidePanel.anchoredPosition;
        sidePanel.anchoredPosition = Vector2.zero;

        togglePanelButton.onClick.AddListener(delegate () {
            TogglePanel();
        });

        // Walk Behaviour
        walkDropdown.onValueChanged.AddListener(delegate (int value) {
            GameObject.FindWithTag("Player").GetComponent<CharacterMotor>().walkBehaviour = (CharacterMotor.WalkBehaviour) value + 1;
        });

        // Walk Speed
        walkSpeedSlider.onValueChanged.AddListener(delegate (float value) {
            GameObject.FindWithTag("Player").GetComponent<CharacterMotor>().walkSpeed = value;
        });

        // Jump Behaviour
        jumpDropdown.onValueChanged.AddListener(delegate (int value) {
            GameObject.FindWithTag("Player").GetComponent<CharacterMotor>().jumpBehaviour = (CharacterMotor.JumpBehaviour)value + 1;
        });

        // Jump Force
        jumpForceSlider.onValueChanged.AddListener(delegate (float value) {
            GameObject.FindWithTag("Player").GetComponent<CharacterMotor>().jumpForce = value;
        });

        UpdateValues();

    }
	
	void Update () {

        if (Input.GetKeyDown(KeyCode.Q)) {
            TogglePanel();
        }

        if (panelOpen) {
            sidePanel.anchoredPosition = Vector2.MoveTowards(sidePanel.anchoredPosition, panelOpenPosition, 1500f * Time.deltaTime);
            //sidePanel.anchoredPosition = new Vector2(sidePanel.sizeDelta.x * 0.5f, sidePanel.anchoredPosition.y);
        } else {
            sidePanel.anchoredPosition = Vector2.MoveTowards(sidePanel.anchoredPosition, Vector2.zero, 1500f * Time.deltaTime);
            //sidePanel.anchoredPosition = new Vector2(sidePanel.sizeDelta.x * 0.5f, sidePanel.anchoredPosition.y);
        }

    }

    public void TogglePanel() {
        panelOpen = !panelOpen;

        if (panelOpen) {
            camera.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            camera.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SwapCharacter(GameObject prefab) {
        GameObject oldCharacter = GameObject.FindGameObjectWithTag("Player");
        
        GameObject newCharacter = Instantiate(prefab, oldCharacter.transform.position, oldCharacter.transform.rotation, null);
        FindObjectOfType<PlayerCamera>().SetTarget(newCharacter.GetComponent<CharacterMotor>());

        DestroyImmediate(oldCharacter);

        UpdateValues();
    }

    public void UpdateValues() {
        CharacterMotor character = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMotor>();
        /*walkDropdown.value = (int) character.walkBehaviour - 1;
        jumpDropdown.value = (int) character.jumpBehaviour - 1;
        walkSpeedSlider.value = character.walkSpeed;*/
        character.walkBehaviour = (CharacterMotor.WalkBehaviour)walkDropdown.value + 1;
        character.jumpBehaviour = (CharacterMotor.JumpBehaviour) jumpDropdown.value + 1;
        character.walkSpeed = walkSpeedSlider.value;
    }
}
