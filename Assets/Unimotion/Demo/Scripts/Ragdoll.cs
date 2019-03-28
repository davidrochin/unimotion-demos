using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unimotion;

public class Ragdoll : MonoBehaviour {

    public CharacterMotor character;
    public Animator animator;
    public float torsoRadius = 0.1f;

    private void Start() {
        character = GetComponent<CharacterMotor>();

        character.OnFrameFinish += delegate () {
            //Build();
            //Debug.Break();
        };
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Y)) {
            Activate();
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            Deactivate();
        }
    }

    public void Activate() {
        GetComponent<CharacterMotor>().enabled = false;
        animator.enabled = false;

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbs.Length; i++) {
            rbs[i].isKinematic = false;
        }
    }

    public void Deactivate() {
        GetComponent<CharacterMotor>().enabled = true;
        animator.enabled = true;

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbs.Length; i++) {
            rbs[i].isKinematic = true;
        }
    }

    public void Build() {
        GameObject helper = new GameObject("Helper");
        CapsuleCollider capsule;
        Transform bone;

        bone = animator.GetBoneTransform(HumanBodyBones.Hips);
        helper.transform.position = bone.transform.position;
        helper.transform.rotation = bone.transform.rotation;
        capsule = helper.AddComponent<CapsuleCollider>();
        capsule.radius = torsoRadius;
        capsule.height = torsoRadius * 3f;
        capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
    }
}
