using UnityEngine;

public class Door : MonoBehaviour {

    public Type type;
    public float detectionRadius = 1.5f;
    public LayerMask detectionLayer;
    public float slidingSpeed = 2f;
    public float rotatingSpeed = 300f;

    public bool open = false;

    // Adjustments
    public Vector3 detectionOffset;

    // For the rotating type
    private Quaternion closedRot;
    private Quaternion openRot;
    private float openDegrees = 90f;

    // For the sliding type
    private Vector3 closedPos;
    private Vector3 openPos;
    private float slidingDistance = 1.2f;

    private Quaternion initRotation;


    void Start() {
        initRotation = transform.rotation;
        closedPos = transform.position;
        closedRot = transform.rotation;
        openPos = transform.position + transform.right * slidingDistance;
        openRot = Quaternion.RotateTowards(closedRot, Quaternion.LookRotation(transform.right, -Physics.gravity.normalized), openDegrees);
    }

    void Update() {
        if(type == Type.Sliding) {
            transform.position = Vector3.MoveTowards(transform.position, open ? openPos : closedPos, slidingSpeed * Time.deltaTime);
        } else {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, open ? openRot : closedRot, rotatingSpeed * Time.deltaTime);
        }

        if(Physics.OverlapSphere(closedPos + initRotation * detectionOffset, detectionRadius, detectionLayer).Length > 0) {
            open = true;
        } else {
            open = false;
        }
        
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.rotation * detectionOffset, detectionRadius);
    }

    public enum Type { Rotating, Sliding }
}
