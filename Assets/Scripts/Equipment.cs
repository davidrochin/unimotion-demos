using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class Equipment : MonoBehaviour {

    [Header("Hands")]
    public Shield equipedShield;
    public Weapon equipedWeapon;

    public GameObject shieldObject; 
    public GameObject weaponObject;

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
    Stamina stamina;
    Character character;

    void Awake() {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        stamina = GetComponent<Stamina>();
        character = GetComponent<Character>();

        leftHandle = animator.GetBoneTransform(HumanBodyBones.LeftHand).Find("Handle.L");
        rightHandle = animator.GetBoneTransform(HumanBodyBones.RightHand).Find("Handle.R");
    }

    private void Start() {
        UpdateHandItems();
    }

    void Update() {

    }

    void UpdateHandItems() {

        Destroy(shieldObject);

        if (equipedShield != null) {
            shieldObject = Instantiate(equipedShield.prefab);
            shieldObject.transform.localScale = Vector3.one;
            shieldObject.transform.parent = leftHandle;
            shieldObject.transform.localPosition = Vector3.zero;
            shieldObject.transform.localRotation = Quaternion.identity;
            shieldObject.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
        }

        if (equipedWeapon != null) {
            weaponObject = Instantiate(equipedWeapon.prefab);
            weaponObject.transform.localScale = Vector3.one;
            weaponObject.transform.parent = rightHandle;
            weaponObject.transform.localPosition = Vector3.zero;
            weaponObject.transform.localRotation = Quaternion.identity;
            weaponObject.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);

            if(equipedWeapon is Weapon) {
                weaponObject.GetComponent<WeaponHitDetector>().colliderIgnore = GetComponent<Collider>();
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

public enum Hand { Left, Right }
