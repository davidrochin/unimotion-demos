using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class Equipment : MonoBehaviour {

    [Header("Hands")]
    public Item leftHandItem;
    public Item rightHandItem;

    public GameObject leftHandItemOb; 
    public GameObject rightHandItemOb;

    [Header("Body")]
    public Armor headArmor;
    public Armor torsoArmor;
    public Armor legsArmor;

    //Events
    public Action OnItemEquiped;
    public Action OnItemUnequiped;

    //References
    Animator animator;
    Transform leftHandle;
    Transform rightHandle;
    Health health;

    void Awake() {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        leftHandle = animator.GetBoneTransform(HumanBodyBones.LeftHand).Find("Handle.L");
        rightHandle = animator.GetBoneTransform(HumanBodyBones.RightHand).Find("Handle.R");
    }

    private void Start() {
        UpdateHandItems();
    }

    void Update() {

    }

    public void UseLeftHandItem() {
        if(leftHandItem is Weapon) {

        } else if (leftHandItem is Shield) {

        }
    }

    public void UseRightHandItem() {
        if (rightHandItem is Weapon) {
            int attackAnimation = 1;
            GetComponent<Animator>().SetTrigger("attack");
            attackAnimation = (int)((Weapon)rightHandItem).moves[Random.Range(0, ((Weapon)rightHandItem).moves.Length)];
            GetComponent<Animator>().SetInteger("attackType", attackAnimation);
        }
    }

    void UpdateHandItems() {

        Destroy(leftHandItemOb);

        if (leftHandItem != null) {
            leftHandItemOb = Instantiate(leftHandItem.prefab);
            leftHandItemOb.transform.localScale = Vector3.one;
            leftHandItemOb.transform.parent = leftHandle;
            leftHandItemOb.transform.localPosition = Vector3.zero;
            leftHandItemOb.transform.localRotation = Quaternion.identity;
            leftHandItemOb.transform.localRotation = Quaternion.Euler(-90f, -90f, 0f);
        }

        if (rightHandItem != null) {
            rightHandItemOb = Instantiate(rightHandItem.prefab);
            rightHandItemOb.transform.localScale = Vector3.one;
            rightHandItemOb.transform.parent = rightHandle;
            rightHandItemOb.transform.localPosition = Vector3.zero;
            rightHandItemOb.transform.localRotation = Quaternion.identity;
            rightHandItemOb.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);

            if(rightHandItem is Weapon) {
                rightHandItemOb.GetComponent<WeaponHitDetector>().colliderIgnore = GetComponent<Collider>();
            }
        }
        
    }

    Vector3 pos;
    private void OnDrawGizmos() {
        if (leftHandle && rightHandle) {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(leftHandle.position, Vector3.one * 0.05f);
            Gizmos.DrawCube(rightHandle.position, Vector3.one * 0.05f);
        }
    }

}
