using System.Collections.Generic;
using UnityEngine;

public class WeaponHitDetector : MonoBehaviour {

    public LayerMask mask;
    public Collider colliderIgnore;

    Vector3 previousPosition;
    BoxCollider collider;
    List<RaycastHit> hits;

    public ColliderAction OnHit;

    public bool detecting = false;

    float minWeaponSpeed = 5f;

    void Awake () {
        collider = GetComponent<BoxCollider>();
        mask = LayerMask.GetMask(new string[]{ "Characters" });
        hits = new List<RaycastHit>();

        previousPosition = GetColliderWorldPosition();
    }
	
	void Update () {

        if (detecting) {

            //Calculate how fast is the weapon moving
            float weaponSpeed = (GetColliderWorldPosition() - previousPosition).magnitude / Time.deltaTime;

            RaycastHit hit;

            //Detect hit by BoxCasting
            if (Physics.BoxCast(previousPosition, collider.size * 0.5f,
                GetDirectionFromPrevious(), out hit, collider.transform.rotation,
                (GetColliderWorldPosition() - previousPosition).magnitude, mask)) { 

                //If it hit something and it is not the wielder
                if (hit.collider != null && hit.collider != colliderIgnore && weaponSpeed >= minWeaponSpeed) {
                    //Debug.Log("Weapon hit at " + weaponSpeed + " units per second, via BoxCasting");
                    hits.Add(hit);
                    if (OnHit != null) OnHit(hit.collider);
                    //Debug.Break();
                }
            }

            //Detect hit by Overlapping
            if (weaponSpeed >= minWeaponSpeed) {
                Collider[] overlapColliders = Physics.OverlapBox(GetColliderWorldPosition(), collider.size * 0.5f, collider.transform.rotation, mask);
                foreach (Collider c in overlapColliders) {
                    if (c != colliderIgnore) {
                        //Debug.Log("Weapon hit at " + weaponSpeed + " units per second, via Overlapping");
                        if (OnHit != null) OnHit(c);
                    }
                }
            }
            
        }
        
        previousPosition = GetColliderWorldPosition();

    }

    Vector3 GetDirectionFromPrevious() {
        return (GetColliderWorldPosition() - previousPosition).normalized;
    }

    Vector3 GetColliderWorldPosition() {
        return transform.position + transform.rotation * collider.center;
    }

    public void StartDetection(ColliderAction onHitAction) {
        detecting = true;
        OnHit = onHitAction;
    }

    public void StopDetection() {
        detecting = false;
        OnHit = null;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (RaycastHit hit in hits) {
            if(hit.collider != null) {
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
        }

        Gizmos.DrawLine(previousPosition, GetColliderWorldPosition());

        Gizmos.color = Color.blue;
        //Gizmos.DrawRay(GetColliderWorldPosition(), transform.rotation * Vector3.forward);
        //Gizmos.DrawSphere(GetColliderWorldPosition(), 0.05f);
    }

    

}

public delegate void ColliderAction(Collider collider);