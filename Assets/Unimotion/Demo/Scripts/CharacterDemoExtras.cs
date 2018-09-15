using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDemoExtras : MonoBehaviour {

    [Header("Landing Spring Animation")]
    public float springDistance = 0.1f;
    public float timerMultiplier = 0.5f;
    float spring = 0f;
    float timer = 0f;

    [Header("Running Animation Multiplier")]
    public float factor = 1.1f;

    CharacterMotor character;
    Animator animator;
    SkinnedMeshRenderer renderer;

    Transform[] bones;
    Quaternion[] bonesOriginalRotations;

    Vector3 dpos;

    Transform hips;
    Transform spine;
    Transform chest;
    Transform leftFoot;
    Transform rightFoot;

    void Awake () {
        character = GetComponent<CharacterMotor>();
        animator = GetComponent<Animator>();
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        bones = renderer.rootBone.GetComponentsInChildren<Transform>();

        hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        rightFoot = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
    }

    void Start() {
        character.OnLand += delegate () {
            timer = 0f;
        };
    }

    void Update () {
        timer = Mathf.Clamp(timer + timerMultiplier * Time.deltaTime, 0f, 3f);
        spring = Mathf.Sin(timer);
	}

    void LateUpdate() {
        hips.position = hips.position - transform.up * spring * springDistance;
        spine.position = spine.position - transform.up * spring * springDistance;
        chest.position = chest.position - transform.up * spring * springDistance;
        leftFoot.position = leftFoot.position + transform.up * spring * springDistance;
        rightFoot.position = rightFoot.position + transform.up * spring * springDistance;
    }

}
