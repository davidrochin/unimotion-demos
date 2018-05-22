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

	void Awake () {
        collider = GetComponent<BoxCollider>();
        mask = LayerMask.GetMask(new string[]{ "Characters" });
        hits = new List<RaycastHit>();

        previousPosition = GetColliderWorldPosition();
    }
	
	void Update () {

        if (detecting) {
            RaycastHit hit;

            if (Physics.BoxCast(previousPosition, collider.size * 0.5f,
                GetDirectionFromPrevious(), out hit, collider.transform.rotation,
                (GetColliderWorldPosition() - previousPosition).magnitude, mask)) {

                if (hit.collider != null && hit.collider != colliderIgnore) {
                    hits.Add(hit);
                    if (OnHit != null) OnHit(hit.collider);
                    //Debug.Break();
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