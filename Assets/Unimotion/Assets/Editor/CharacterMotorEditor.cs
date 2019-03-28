using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(CharacterMotor))]
[CanEditMultipleObjects]
public class CharacterMotorEditor : Editor {

    CharacterMotor motor;
    CapsuleCollider collider;
    Animator animator;

    GUIStyle sectionHeaderStyle = new GUIStyle();
    GUIStyle sectionPanel = new GUIStyle();

    // Serialized properties
    SerializedProperty walkSpeed;

    private void OnEnable() {
        motor = (CharacterMotor)target;
        collider = motor.GetComponent<CapsuleCollider>();

        sectionHeaderStyle.fontStyle = FontStyle.Bold;
        sectionHeaderStyle.alignment = TextAnchor.MiddleCenter;

        //sectionPanel = EditorStyles.helpBox;
        //sectionPanel.padding = new RectOffset(4, 4, 4, 4);
    }

    bool debugFoldedOut = false;
    bool constFoldedOut = false;

    public override void OnInspectorGUI() {

        serializedObject.Update();

        collider.center = new Vector3(0f, collider.height * 0.5f, 0f);

        // Capsule center warning
        if (collider.center.x != 0f || collider.center.z != 0f) {
            EditorGUILayout.HelpBox("Capsule Collider center must have zero X and Z values. This will be fixed automatically on Play Mode.", MessageType.Warning);
        }

        // Capsule direction warning
        if (collider.direction != 1) {
            EditorGUILayout.HelpBox("Capsule Collider direction must be Y-Axis. This will be fixed automatically on Play Mode.", MessageType.Warning);
        }

        // Locomotion Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Locomotion", EditorStyles.boldLabel);

        // Walking
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("canWalk"), new GUIContent("Can walk"));
        if (motor.canWalk) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkBehaviour"), new GUIContent("Walking"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkSpeed"), new GUIContent("Speed"));
            if (motor.walkBehaviour == CharacterMotor.WalkBehaviour.Smoothed) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("walkSmoothness"), new GUIContent("Smoothness"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothDirection"), new GUIContent("Smooth direction"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothSpeed"), new GUIContent("Smooth speed"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeLimit"), new GUIContent("Max Slope"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeBehaviour"), new GUIContent("Slope Behaviour"));
        }

        EditorGUILayout.EndVertical();

        // Jumping
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("canJump"), new GUIContent("Can jump"));
        if (motor.canJump) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airBehaviour"), new GUIContent("Air"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"), new GUIContent("Force"));
            if (motor.airBehaviour == CharacterMotor.AirBehaviour.SmoothControl) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("airControl"), new GUIContent("Air control"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canJumpWhileSliding"), new GUIContent("Jump while sliding"));
        }

        EditorGUILayout.EndVertical();

        // Turning
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("canTurn"), new GUIContent("Can turn"));
        if (motor.canTurn) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("turnBehaviour"), new GUIContent("Turning"));
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionQuality"), new GUIContent("Quality"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("characterCollisionBehaviour"), new GUIContent("Character Collision"));
        if (motor.characterCollisionBehaviour == CharacterMotor.CharacterMotorCollisionBehaviour.SoftPush) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterPushForce"), new GUIContent("Push force"));
        }
        EditorGUILayout.EndVertical();

        // Rigidbodies
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rigidbodyCollisionBehaviour"), new GUIContent("Rigidbody Collision"));
        if (motor.rigidbodyCollisionBehaviour == CharacterMotor.RigidbodyCollisionBehaviour.Push) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rigidbodyPushForce"), new GUIContent("Push force"));
        }
        EditorGUILayout.EndVertical();

        // Gravity
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityBehaviour"), new GUIContent("Behaviour"));
        if (motor.gravityBehaviour == CharacterMotor.GravityBehaviour.UseCustom) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("customGravity"), new GUIContent("Gravity"));
        } else if (motor.gravityBehaviour == CharacterMotor.GravityBehaviour.UseGlobal) {
            EditorGUILayout.HelpBox("Global character gravity can only be modified via code, setting the static field CharacterMotor.globalGravity", MessageType.Info);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("alignToGravity"), new GUIContent("Align"));
        if (motor.alignToGravity) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityAlignmentType"), new GUIContent("Alignment"));
        }
        EditorGUILayout.EndVertical();

        // Animation
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outputToAnimator"), new GUIContent("Output to Animator"));
        if (motor.outputToAnimator) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"), new GUIContent("Animator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothMoveParameters"), new GUIContent("Smooth move parameters"));

            // Create Animation Parameters Button
            if (GUILayout.Button("Create Animator Parameters")) {
                animator = motor.animator;
                if (animator != null && EditorUtility.DisplayDialog("Current parameters will be deleted", "This function creates parameters in the Animator Controller for the Character Motor to fill. If you continue, all other parameters will be deleted.", "Continue", "Cancel")) {
                    AnimatorController ac = (AnimatorController)animator.runtimeAnimatorController;
                    ac.parameters = new AnimatorControllerParameter[0];
                    ac.AddParameter("Input/Forward", AnimatorControllerParameterType.Float);
                    ac.AddParameter("Input/Sideways", AnimatorControllerParameterType.Float);
                    ac.AddParameter("Input/Magnitude", AnimatorControllerParameterType.Float);
                    ac.AddParameter("Input/Non Up or Down", AnimatorControllerParameterType.Float);

                    ac.AddParameter("Speed/Upwards", AnimatorControllerParameterType.Float);
                    ac.AddParameter("Speed/Sideways", AnimatorControllerParameterType.Float);

                    ac.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
                    ac.AddParameter("Sliding", AnimatorControllerParameterType.Bool);
                    ac.AddParameter("Stuck", AnimatorControllerParameterType.Bool);
                } else {
                    // Alert: Animator not configured
                }
            }

        }
        EditorGUILayout.EndVertical();

        // Constants
        EditorGUILayout.Space();
        constFoldedOut = EditorGUILayout.Foldout(constFoldedOut, "Constants");

        if (constFoldedOut) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skinWidth"), new GUIContent("Skin width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("terminalSpeed"), new GUIContent("Terminal speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("floorFriction"), new GUIContent("Floor friction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airFriction"), new GUIContent("Air friction"));
            EditorGUILayout.EndVertical();
        }

        // Debug
        debugFoldedOut = EditorGUILayout.Foldout(debugFoldedOut, "Debug");

        if (debugFoldedOut) {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Velocity: " + motor.velocity, EditorStyles.helpBox);
            EditorGUILayout.LabelField("Full Velocity: " + motor.fullVelocity, EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(motor.Grounded ? "Grounded" : "Not Grounded", EditorStyles.helpBox, GUILayout.MaxWidth(300));
            EditorGUILayout.LabelField(motor.state.sliding ? "Sliding" : "Not Sliding", EditorStyles.helpBox, GUILayout.MaxWidth(300));
            EditorGUILayout.LabelField(motor.state.stuck ? "Stuck" : "Not Stuck", EditorStyles.helpBox, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        // Redraw inspector if the application is playing in the Editor
        if (EditorApplication.isPlaying && debugFoldedOut) {
            Repaint();
        }

        //DrawDefaultInspector();
    }
}
