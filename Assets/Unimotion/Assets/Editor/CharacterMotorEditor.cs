using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterMotor))]
[CanEditMultipleObjects]
public class CharacterMotorEditor : Editor {

    CharacterMotor motor;

    GUIStyle sectionHeaderStyle = new GUIStyle();
    GUIStyle sectionPanel = new GUIStyle();

    // Serialized properties
    SerializedProperty walkSpeed;

    private void OnEnable() {
        motor = (CharacterMotor) target;

        sectionHeaderStyle.fontStyle = FontStyle.Bold;
        sectionHeaderStyle.alignment = TextAnchor.MiddleCenter;

        //sectionPanel = EditorStyles.helpBox;
        //sectionPanel.padding = new RectOffset(4, 4, 4, 4);
    }

    bool debugFoldedOut = false;

    public override void OnInspectorGUI() {

        serializedObject.Update();

        // Locomotion Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Locomotion", EditorStyles.boldLabel);

        // Walking
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("walkBehaviour"), new GUIContent("Walking"));
        if (motor.walkBehaviour != CharacterMotor.WalkBehaviour.None) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkSpeed"), new GUIContent("Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeLimit"), new GUIContent("Max Slope"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeBehaviour"), new GUIContent("Slope Behaviour"));
        }

        EditorGUILayout.EndVertical();

        // Jumping
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpBehaviour"), new GUIContent("Jumping"));
        if (motor.jumpBehaviour != CharacterMotor.JumpBehaviour.None) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"), new GUIContent("Force"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canJumpWhileSliding"), new GUIContent("Jump while sliding"));
        }

        EditorGUILayout.EndVertical();

        // Turning
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnBehaviour"), new GUIContent("Turning"));
        if (motor.turnBehaviour != CharacterMotor.TurnBehaviour.None) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("turnSpeed"), new GUIContent("Speed"));
        }

        EditorGUILayout.EndVertical();

        // Masks Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Masks", EditorStyles.boldLabel);

        // Masks
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionMask"), new GUIContent("Collision"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("characterMask"), new GUIContent("Characters"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rigidbodyMask"), new GUIContent("Rigidbodies"));

        EditorGUILayout.EndVertical();

        // Collision Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Collision", EditorStyles.boldLabel);

        // Characters
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterMotorCollisionBehaviour"), new GUIContent("Character Collision"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rigidbodyCollisionBehaviour"), new GUIContent("Rigidbody Collision"));
        EditorGUILayout.EndVertical();

        // Collision Section
        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        debugFoldedOut = EditorGUILayout.Foldout(debugFoldedOut, "Debug");

        if (debugFoldedOut) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Hi!");
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        /*EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();*/

        serializedObject.ApplyModifiedProperties();

        // Redraw inspector if the application is playing in the Editor
        if (EditorApplication.isPlaying) {
            Repaint();
        }

        //DrawDefaultInspector();
    }
}
