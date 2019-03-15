using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour {

    public static Interactable active;

    public LayerMask characterLayer;
    public abstract string[] Actions { get; set; }

    public int selected = 0;

    private void Awake() {
        itemStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, alignment = TextAnchor.MiddleCenter };
        itemStyleSelected = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, alignment = TextAnchor.MiddleCenter };
    }

    public abstract void DoSelectedAction();

    public void SelectNext() {
        selected = Mathf.Clamp(selected + 1, 0, Actions.Length - 1);
    }

    public void SelectPrevious() {
        selected = Mathf.Clamp(selected - 1, 0, Actions.Length - 1);
    }

    protected void Update() {
        Collider[] cols = Physics.OverlapSphere(transform.position, 1f, characterLayer);
        bool detected = false;
        foreach (Collider col in cols) {
            Unimotion.Player player = col.GetComponent<Unimotion.Player>();
            if (player != null && player.photonView.IsMine) {
                detected = true;
                active = this;
                break;
            }
        }

        if (!detected && active == this) {
            active = null;
        }
    }

    private void OnGUI() {
        if (active) {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            screenPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

            for (int i = 0; i < Actions.Length; i++) {
                string action = Actions[i];

                Rect rect = new Rect(
                    Screen.width - MenuMargin - ItemWidth,
                    Screen.height - MenuMargin - ItemHeight * (Actions.Length - i) - ItemSeparation * (Actions.Length - i),
                    ItemWidth, ItemHeight);

                if (selected == i) {
                    GUI.DrawTexture(rect, TextureUtil.FromColor(Color.white));
                } else {
                    GUI.DrawTexture(rect, TextureUtil.FromColor(new Color(0.2f, 0.2f, 0.2f)));
                }

                GUI.Box(rect, action, selected == i ? itemStyleSelected : itemStyle);
            }
        }

    }

    private static GUIStyle itemStyle;
    private static GUIStyle itemStyleSelected;

    private static float ItemWidth = 200f;
    private static float ItemHeight = 25f;
    private static float ItemSeparation = 5f;
    private static float MenuMargin = 10f;
}
