using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class CharacterMotor : MonoBehaviour {

    #region User Settings

    [Header("Size")]
    [Tooltip("The total height of the player capsule.")] public float height;
    [Tooltip("The radius of the player capsule.")] public float radius;

    [Header("Walking")]
    [Tooltip("How fast the character should walk (in m/s).")] [Range(0.01f, 10)] public float walkSpeed = 7f;

    [Header("Turning")]
    [Tooltip("How fast the character should turn (in degrees/s).")] public float turningSpeed = 400f;

    [Header("Jumping")]
    [Range(1f, 10f)] public float jumpForce = 10f;
    public JumpStyle jumpStyle;
    public bool canJumpWhileSliding = true;

    [Header("Collision")]
    [Tooltip("A mask that defines what are the objects the Character can collide with.")]
    public LayerMask collisionMask;

    [Header("Rigidbody Interaction")]
    public bool canPushRigidbodies = true;
    public LayerMask rigidbodiesLayer;

    #endregion

    #region Events

    public event Action OnWalk;
    public event Action OnRun;
    public event Action OnJump;
    public event Action OnLand;
    public event Action OnFrameStart;
    public event Action OnFrameFinish;

    #endregion

    [Header("Debug")]
    public Vector3 velocity;

    public CharacterMotorState state;

    bool jump; 
    Vector3 inputVector;
    Vector3 inputVectorSmoothed;

    const float SkinWidth = 0.01f;

    void Start() {
        // Test events
        OnWalk += () => Debug.Log("OnWalk");
        OnRun += () => Debug.Log("OnRun");
        OnJump += () => Debug.Log("OnJump");
        OnLand += () => Debug.Log("OnLand");
    }

    void Update() {

        FollowFloor();

        Unblock();

        // Apply gravity if necessary (terminal velocity of a human in freefall is about 53 m/s)
        if (!state.grounded && Vector3.Dot(velocity, Physics.gravity.normalized) < 50f) {
            velocity = velocity + Physics.gravity * 2f * Time.deltaTime;
        }

        //Unblock();

        // Apply movement from input
        inputVectorSmoothed = inputVector; inputVector = Vector3.zero;
        Move(inputVectorSmoothed * walkSpeed * Time.deltaTime);

        // Apply movement from velocity
        Move(velocity * Time.deltaTime);

        

        // Push away Rigidbodies
        Collider[] cols = Physics.OverlapCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, rigidbodiesLayer);
        foreach (Collider col in cols) {
            col.GetComponent<Rigidbody>().AddExplosionForce(50f, transform.position, 10f);
        }

        // Rotate feet towards gravity direction
        Quaternion fromToRotation = Quaternion.FromToRotation(transform.up, -Physics.gravity.normalized);
        transform.rotation = fromToRotation * transform.rotation;

    }

    void LateUpdate() {

        FollowFloor();
        Unblock();
        CheckGrounded();
        StickToSlope();

        if (OnFrameFinish != null) { OnFrameFinish(); }
        
    }

    #region Private methods

    void Move(Vector3 delta) {

        if (delta == Vector3.zero) { return; }

        int slideCount = 0;

        // Store the position from before moving
        Vector3 startingPos = transform.position;

        // Capsule cast to delta
        float lastDistance = 0f;
        RaycastHit[] hits = Physics.CapsuleCastAll(
            transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
            radius, delta.normalized, delta.magnitude, collisionMask); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance);
        bool didHit = (hits.Length > 0 ? true : false); Vector3 lastNormal = (hits.Length > 0 ? hits[0].normal : Vector3.zero);
        System.Array.Sort(hits, (x,y) => x.distance.CompareTo(y.distance));

        // Move and slide on the hit plane
        if (didHit) {

            Debug.DrawRay(hits[0].point, hits[0].normal);

            slideCount++;

            // Move until it the point it hits
            transform.position += delta.normalized * hits[0].distance + hits[0].normal * SkinWidth;

            // Calculate the direction in which the Character should slide
            Vector3 slideDirection = Vector3.Cross(Vector3.Cross(hits[0].normal, delta.normalized), hits[0].normal).normalized;
            debugDirection = slideDirection;
            float slideMagnitude = Mathf.Clamp(Vector3.Dot(delta.normalized * (delta.magnitude - hits[0].distance), slideDirection), 0f, float.MaxValue);

            // Cast to see if the Character is free to move or must slide on another plane
            hits = Physics.CapsuleCastAll(
                transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                radius, slideDirection, slideMagnitude, collisionMask);
            didHit = (hits.Length > 0 ? true : false); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance); lastNormal = (hits.Length > 0 ? hits[0].normal : lastNormal);

            Vector3 remainingDelta = delta.normalized * (delta.magnitude - lastDistance);

            // If the Character cannot move freely
            while (didHit && slideCount < 20) {

                Debug.DrawRay(hits[0].point, hits[0].normal);

                lastNormal = hits[0].normal;
                slideCount++;

                // Slide util it hits
                transform.position += slideDirection * hits[0].distance + hits[0].normal * SkinWidth;

                // Calculate the direction in which the Character should slide
                Vector3 previousDelta = slideDirection * slideMagnitude;
                slideDirection = Vector3.Cross(Vector3.Cross(hits[0].normal, slideDirection.normalized), hits[0].normal).normalized;
                slideMagnitude = Mathf.Clamp(Vector3.Dot(previousDelta.normalized * (previousDelta.magnitude - hits[0].distance), slideDirection), 0f, float.MaxValue);
                debugDirection = slideDirection;

                // Cast to see if the Character is free to move or must slide on another plane
                hits = Physics.CapsuleCastAll(
                    transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                    radius, slideDirection, slideMagnitude, collisionMask);
                didHit = (hits.Length > 0 ? true : false); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance); lastNormal = (hits.Length > 0 ? hits[0].normal : lastNormal);
                System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

                // Calculate how much delta is left to travel
                if (didHit) {
                    remainingDelta = remainingDelta.normalized * (remainingDelta.magnitude - hits[0].distance);
                }
            }

            // If the Character is free to move
            if (!didHit) {
                transform.position += slideDirection * Mathf.Clamp(Vector3.Dot(remainingDelta, slideDirection), 0f, float.MaxValue);
            }
        }

        // If the cast didn't hit anything, just move
        else {
            transform.position += delta;
        }

        // Check if this is a valid position. If not, return to the position from before moving
        bool invalidPos = Physics.CheckCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, collisionMask);
        Collider[] colliders = Physics.OverlapCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, collisionMask);

        if (invalidPos) {
            Debug.LogError("Character Motor " + name + " got stuck with " + slideCount + " slides. (" + colliders[0].gameObject.name + ")");
            stuckPosition = transform.position;
            transform.position = startingPos;
        }
    }

    void CheckGrounded() {

        // Save whether if the Character was grounded or not before the check
        state.previouslyGrounded = state.grounded;

        // Check the floor beneath (even if the Character is not touching it)
        RaycastHit floorHit; bool didHit = Physics.SphereCast(transform.position + transform.up * height - transform.up * radius, radius, -transform.up, out floorHit, float.MaxValue, collisionMask);
        if (didHit) {
            state.floor = floorHit.transform;
        } else {
            state.floor = null;
        }

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.up * height - transform.up * radius, radius + SkinWidth * 2f, -transform.up, height - radius * 2f, collisionMask);
        if (hits.Length > 0 && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            bool validFloor = false;

            foreach (RaycastHit hit in hits) {

                float angle = Vector3.Angle(-Physics.gravity.normalized, hit.normal);
                state.floorAngle = angle;

                if (Vector3.Dot(hit.normal, -Physics.gravity.normalized) > 0f && angle < 90f && !(hit.distance == 0f && hit.point == Vector3.zero) ) {
                    bool onCylinder = Vector3.Distance(transform.position + transform.up * Vector3.Dot(hit.point - transform.position, transform.up), hit.point) <= radius ? true : false;
                    
                    if(onCylinder)
                        validFloor = true;
                }
            }

            if (validFloor) {
                state.grounded = true;
                velocity = Vector3.zero;
            } else {
                state.grounded = false;
            }

        } else {
            state.grounded = false;
        }
    }

    bool CheckInvalid() {
        return Physics.CheckCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, collisionMask);
    }

    void Unblock() {
        Collider[] cols = Physics.OverlapCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, collisionMask);
        if (cols.Length > 0) {

            state.stuck = true;

            foreach (Collider col in cols) {

                Vector3 closestPointInCollider = col.ClosestPoint(GetPartPosition(Part.Center));
                Vector3 closestPointInCapsule = ClosestPoint(closestPointInCollider);

                // Calculate where the character needs to move to get unblocked
                Vector3 delta = closestPointInCollider - closestPointInCapsule;
                //Debug.Log(delta.magnitude);

                // Move
                //transform.position += delta + delta.normalized * SkinWidth;
                transform.position += delta.normalized * (delta.magnitude + SkinWidth);

                debugPoint = closestPointInCollider;
                debugPoint2 = closestPointInCapsule;

                //Debug.Break();

                break;
            }

        } else {
            state.stuck = false;
        }
    }

    void FollowFloor() {

        //If we are detecting a floor transform
        if (state.floor != null) {

            //If it is a different floor from the last one
            if (state.floorState.transform == null || state.floorState.transform != state.floor) {
                state.floorState = TransformState.From(state.floor);
            }

            //Follow the floor transform if grounded
            else if (state.grounded) {

                //Follow position
                transform.position += state.floor.position - state.floorState.position;
                //Move(state.floor.position - state.floorState.position);

                //Follow rotation
                Quaternion dif = Quaternion.FromToRotation(state.floorState.forward, state.floor.forward);
                transform.rotation = dif * transform.rotation;
                Vector3 delta = transform.position - state.floor.position;
                delta = dif * delta; transform.position = state.floor.position + delta;

                //state.floorState = TransformState.From(state.floor);
            }

            state.floorState = TransformState.From(state.floor);

        } else {
            state.floorState = TransformState.Empty();
        }
    }

    void StickToSlope() {
        RaycastHit hit;
        bool didHit = Physics.SphereCast(transform.position + transform.up * radius, radius, Physics.gravity.normalized, out hit, radius, collisionMask);

        if (state.previouslyGrounded && didHit && Vector3.Angle(-Physics.gravity.normalized, hit.normal) <= 45f) {

            Vector3 hyp;
            float topAngle = Vector3.Angle(Physics.gravity.normalized, -hit.normal);
            float bottomAngle = 180f - topAngle - 90f;
            hyp = -Physics.gravity.normalized * (SkinWidth / Mathf.Sin(Mathf.Deg2Rad * bottomAngle)) * Mathf.Sin(Mathf.Deg2Rad * 90f);

            transform.position += Physics.gravity.normalized * hit.distance + hyp;
            state.grounded = true;
            velocity = Vector3.zero;
        }
    }

    Vector3 ClosestPoint(Vector3 point) {
        float segment = Mathf.Clamp(Vector3.Dot(point - transform.position, transform.up), 0f + radius, height - radius);
        Vector3 segmentPosition = transform.position + transform.up * segment;
        return segmentPosition + (point - segmentPosition).normalized * radius;
    }

    Vector3 GetPartPosition(Part part) {
        switch (part) {
            case Part.BottomSphere:
                return transform.position + transform.up * radius;
            case Part.TopSphere:
                return transform.position + transform.up * height - transform.up * radius;
            case Part.Center:
                return transform.position + transform.up * height * 0.5f;
            case Part.Bottom:
                return transform.position;
            case Part.Top:
                return transform.position + transform.up * height;
            default:
                return transform.position;
        }
    }

    #endregion

    #region Public methods

    public void Walk(Vector3 delta) {
        //inputVector = Vector3.ClampMagnitude(delta, 1f);
        inputVector = delta;
    }

    public void Jump() {
        if (state.grounded || Debug.isDebugBuild) {
            velocity = velocity - Physics.gravity.normalized * jumpForce;
            state.grounded = false;
            if (OnJump != null) OnJump();
        }
    }

    public void Crouch() {
        throw new System.NotImplementedException();
    }

    public bool Stand() {
        throw new System.NotImplementedException();
    }

    public void TurnTowards(Vector3 direction) {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, -Physics.gravity.normalized), turningSpeed * Time.deltaTime);
    }

    #endregion

    public Vector3 debugDirection;
    public Vector3 stuckPosition;
    public Vector3 debugPoint;
    public Vector3 debugPoint2;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.up * height - transform.up * radius, radius);
        Gizmos.color = (state.grounded ? Color.blue : Color.green);
        Gizmos.DrawWireSphere(transform.position + transform.up * radius, radius);
        Gizmos.DrawRay(transform.position, inputVectorSmoothed);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, debugDirection);

        //Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        Gizmos.DrawWireSphere(stuckPosition + transform.up * radius, radius);
        Gizmos.DrawWireSphere(stuckPosition + transform.up * height - transform.up * radius, radius);

        Gizmos.DrawSphere(debugPoint, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(debugPoint2, 0.05f);
    }

    public enum Part { BottomSphere, TopSphere, Center, Top, Bottom }
    public enum MovementStyle { Raw, Smoothed }
    public enum JumpStyle { TotalControl, FixedVelocity, SmoothControl }

}

[System.Serializable]
public class CharacterMotorState {
    public float forwardMove;
    public float rightMove;
    public float moveSpeed;

    public bool moving;
    public bool grounded;
    public bool previouslyGrounded;

    public bool stuck;

    public Vector3 lastDelta;
    public float floorAngle;

    public Transform floor;
    public TransformState floorState;

    public CharacterMotorState() {

    }

    public void UpdateToAnimator(Animator anim) {
        //anim.SetFloat("forwardMove", Mathf.MoveTowards(anim.GetFloat("forwardMove"), forwardMove, 1f * Time.deltaTime));
        //anim.SetFloat("rightMove", Mathf.MoveTowards(anim.GetFloat("rightMove"), rightMove, 1f * Time.deltaTime));

        anim.SetFloat("forwardMove", forwardMove);
        anim.SetFloat("rightMove", rightMove);
        anim.SetFloat("moveSpeed", moveSpeed);

        anim.SetBool("grounded", grounded);
        anim.SetBool("moving", moving);

    }

    public void Reset() {
        forwardMove = 0f;
        moveSpeed = 0f;
        rightMove = 0f;
        floor = null;
    }
}

public delegate void Action();
public delegate void FloatAction(float n);