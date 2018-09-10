using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterMotor : MonoBehaviour {

    #region User Settings

    // Walking
    public WalkBehaviour walkBehaviour;
    [Tooltip("How fast the character should walk (in m/s).")] [Range(0.01f, 20)] public float walkSpeed = 7f;
    [Range(5f, 85f)] public float slopeLimit = 50f;
    public SlopeBehaviour slopeBehaviour = SlopeBehaviour.Slide;

    // Turning
    public TurnBehaviour turnBehaviour = TurnBehaviour.Normal;
    [Tooltip("How fast the character should turn (in degrees/s).")] public float turnSpeed = 400f;

    // Jumping
    [Range(1f, 30f)] public float jumpForce = 10f;
    public JumpBehaviour jumpBehaviour = JumpBehaviour.TotalControl;
    public bool canJumpWhileSliding = true;

    // Masks
    [Tooltip("A mask that defines what are the objects the Character can collide with.")]
    public LayerMask collisionMask;
    public LayerMask characterMask;
    public LayerMask waterMask;

    LayerMask finalCollisionMask;

    // Collision
    public CharacterMotorCollisionBehaviour characterMotorCollisionBehaviour;

    // Rigidbody Interaction
    public RigidbodyCollisionBehaviour rigidbodyCollisionBehaviour;
    public LayerMask rigidbodyMask;

    #endregion

    #region Events

    public event Action OnWalk;
    public event Action OnRun;
    public event Action OnJump;
    public event Action OnLand;
    public event Action OnFrameStart;
    public event Action OnFrameFinish;
    public event Action OnDepenetrate;
    public event Action OnCrush;

    #endregion

    [Header("Debug")]
    public Vector3 velocity;

    public CharacterMotorState state;

    new CapsuleCollider collider;

    bool jump; 
    Vector3 inputVector;
    Vector3 inputVectorSmoothed;

    const float SkinWidth = 0.01f;
    const float TerminalSpeed = 50f;
    const float FloorFriction = 16f;
    const float AirFriction = 4f;

    private void Awake() {
        collider = GetComponent<CapsuleCollider>();

        // Make sure SphereCollider center is correct
        collider.center = new Vector3(0f, collider.height * 0.5f, 0f);
    }

    void Start() {
        // Test events
        OnWalk += () => Debug.Log("OnWalk");
        OnRun += () => Debug.Log("OnRun");
        OnJump += () => Debug.Log("OnJump");
        OnLand += () => Debug.Log("OnLand");
        OnCrush += () => Debug.LogWarning("OnCrush");
        //OnDepenetrate += () => Debug.Log("OnDepenetrate");
    }

    void Update() {

        // Calculate the Final Collision Mask
        finalCollisionMask = collisionMask;
        if (characterMotorCollisionBehaviour == CharacterMotorCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | characterMask; }

        FollowFloor();

        if (!ValidatePosition()) { Depenetrate(); }

        // Apply gravity if necessary (terminal velocity of a human in freefall is about 53 m/s)
        if ((!state.grounded || state.sliding) && Vector3.Dot(velocity, Physics.gravity.normalized) < 50f) {
            //velocity = velocity + Physics.gravity * 2f * Time.deltaTime;

            if(Vector3.Dot(velocity, -Physics.gravity.normalized) >= 0f) {
                velocity = velocity + Physics.gravity * Time.deltaTime;
            } else {
                velocity = velocity + Physics.gravity * 2f * Time.deltaTime;
            }
        }

        // Apply movement from input
        inputVectorSmoothed = inputVector; inputVector = Vector3.zero;
        Move(inputVectorSmoothed * walkSpeed * Time.deltaTime);

        // Apply movement from velocity
        Move(velocity * Time.deltaTime);

        // Push away Rigidbodies
        Collider[] cols = Physics.OverlapCapsule(transform.position + transform.up * collider.radius, transform.position + transform.up * collider.height - transform.up * collider.radius, collider.radius, rigidbodyMask);
        foreach (Collider col in cols) {
            col.GetComponent<Rigidbody>().WakeUp();
        }

        // Rotate feet towards gravity direction
        Quaternion fromToRotation = Quaternion.FromToRotation(transform.up, -Physics.gravity.normalized);
        transform.rotation = fromToRotation * transform.rotation;

    }

    void LateUpdate() {

        FollowFloor();
        if (!ValidatePosition()) { Depenetrate(); }

        CheckGrounded();
        if(state.grounded && !state.sliding) {

            // Change velocity so it doesnt go towards the floor
            velocity = velocity - Physics.gravity.normalized * Vector3.Dot(velocity, Physics.gravity.normalized);

            // Apply floor friction
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, FloorFriction * Time.deltaTime);
        } else {

            // Apply air friction
            float towardsGravity = Vector3.Dot(velocity, Physics.gravity.normalized);
            velocity = velocity - Physics.gravity.normalized * towardsGravity;
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, AirFriction * Time.deltaTime);
            velocity = velocity + Physics.gravity.normalized * towardsGravity;

        }

        StickToSlope();
        if (!ValidatePosition()) { Depenetrate(); }

        if (OnFrameFinish != null) { OnFrameFinish(); }
        
    }

    #region Private methods

    void Move(Vector3 delta) {

        // Do not do anything if delta is zero
        if (delta == Vector3.zero) { return; }

        int slideCount = 0;

        // Store the position from before moving
        Vector3 startingPos = transform.position;

        // Capsule cast to delta
        float lastDistance = 0f;

        RaycastHit hit = UnimotionUtil.CapsuleCastIgnoreSelf(
            transform.position + transform.up * collider.radius, transform.position + transform.up * collider.height - transform.up * collider.radius,
            collider.radius, delta.normalized, delta.magnitude, finalCollisionMask, QueryTriggerInteraction.Ignore, collider);
        lastDistance = (hit.collider != null ? hit.distance : lastDistance);
        bool didHit = (hit.collider != null ? true : false);
        Vector3 lastNormal = (hit.collider != null ? hit.normal : Vector3.zero);


        // Move and slide on the hit plane
        if (didHit) {
            slideCount++;

            Debug.DrawRay(hit.point, hit.normal);

            // Move until it the point it hits
            Vector3 previousPos = transform.position;
            transform.position += delta.normalized * (hit.distance - SkinWidth);
            if (!ValidatePosition()) { Depenetrate(); }

            // Calculate the direction in which the Character should slide
            Vector3 slideDirection = Vector3.Cross(Vector3.Cross(hit.normal, delta.normalized), hit.normal).normalized;
            debugDirection = slideDirection;
            float slideMagnitude = Mathf.Clamp(Vector3.Dot(delta.normalized * (delta.magnitude - hit.distance), slideDirection), 0f, float.MaxValue);

            // Cast to see if the Character is free to move or must slide on another plane
            hit = UnimotionUtil.CapsuleCastIgnoreSelf(
                transform.position + transform.up * collider.radius, transform.position + transform.up * collider.height - transform.up * collider.radius,
                collider.radius, slideDirection, slideMagnitude, finalCollisionMask, QueryTriggerInteraction.Ignore, collider);
                lastDistance = (hit.collider != null ? hit.distance : lastDistance);
            didHit = (hit.collider != null ? true : false);
            lastDistance = (hit.collider != null ? hit.distance : lastDistance);
            lastNormal = (hit.collider != null ? hit.normal : lastNormal);

            Vector3 remainingDelta = delta.normalized * (delta.magnitude - lastDistance);

            // If the Character cannot move freely
            while (didHit && slideCount < 20) {
                slideCount++;

                Debug.DrawRay(hit.point, hit.normal);
                lastNormal = hit.normal;

                // Slide util it hits
                previousPos = transform.position;
                transform.position += slideDirection.normalized * (hit.distance - SkinWidth);
                if (!ValidatePosition()) { Depenetrate(); }

                // Calculate the direction in which the Character should slide
                Vector3 previousDelta = slideDirection * slideMagnitude;
                slideDirection = Vector3.Cross(Vector3.Cross(hit.normal, slideDirection.normalized), hit.normal).normalized;
                slideMagnitude = Mathf.Clamp(Vector3.Dot(remainingDelta, slideDirection), 0f, float.MaxValue);
                debugDirection = slideDirection;

                // Cast to see if the Character is free to move or must slide on another plane
                hit = UnimotionUtil.CapsuleCastIgnoreSelf(
                    transform.position + transform.up * collider.radius, transform.position + transform.up * collider.height - transform.up * collider.radius,
                    collider.radius, slideDirection, slideMagnitude, finalCollisionMask, QueryTriggerInteraction.Ignore, collider);
                    lastDistance = (hit.collider != null ? hit.distance : lastDistance);
                didHit = (hit.collider != null ? true : false);
                lastDistance = (hit.collider != null ? hit.distance : lastDistance);
                lastNormal = (hit.collider != null ? hit.normal : lastNormal);

                // Calculate how much delta is left to travel
                if (didHit) {
                    remainingDelta = remainingDelta.normalized * (remainingDelta.magnitude - hit.distance);
                }
            }

            // If the Character is free to move
            if (!didHit) {
                transform.position += slideDirection * slideMagnitude;
                if (!ValidatePosition()) { Depenetrate(); }
            }
        }

        // If the cast didn't hit anything, just move
        else {
            transform.position += delta;
            if (!ValidatePosition()) { Depenetrate(); }
        }

    }

    void CheckGrounded() {

        // Save whether if the Character was grounded or not before the check
        state.previouslyGrounded = state.grounded;

        // Check the floor beneath (even if the Character is not touching it)
        RaycastHit floorHit; bool didHit = Physics.SphereCast(transform.position + transform.up * collider.height - transform.up * collider.radius, collider.radius, -transform.up, out floorHit, float.MaxValue, collisionMask);
        if (didHit) {
            state.floor = floorHit.transform;
        } else {
            state.floor = null;
        }

        state.sliding = true;

        // Check for ground
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.up * collider.height - transform.up * collider.radius, collider.radius + SkinWidth * 2f, -transform.up, collider.height - collider.radius * 2f, collisionMask);
        if (hits.Length > 0 && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            bool validFloor = false;

            // Check each hit for valid ground
            foreach (RaycastHit hit in hits) {

                // Calculate the angle of the floor
                float angle = Vector3.Angle(-Physics.gravity.normalized, hit.normal);
                state.floorAngle = angle;

                if (Vector3.Dot(hit.normal, -Physics.gravity.normalized) > 0f && angle < 85f && !(hit.distance == 0f && hit.point == Vector3.zero) ) {
                      
                    // Check if it should slide
                    if(angle < slopeLimit) {
                        state.sliding = false;
                    }

                    // Check if the hit point is does not go past the Character cylinder area
                    bool onCylinder = Vector3.Distance(transform.position + transform.up * Vector3.Dot(hit.point - transform.position, transform.up), hit.point) <= collider.radius ? true : false;
                    if (onCylinder) {
                        validFloor = true;
                    }
                }
            }

            // Ground the player if a valid floor was found
            if (validFloor) {
                state.grounded = true;

                // If the player was previously ungrounded, run the OnLand event
                if (state.previouslyGrounded == false && OnLand != null) { OnLand(); }

            } else {
                state.grounded = false;
                state.sliding = false;
            }

        } else {
            state.sliding = false;
            state.grounded = false;
        }
    }

    bool ValidatePosition() {
        return (Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal).Length > 0 ? false : true);
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
        bool didHit = Physics.SphereCast(transform.position + transform.up * collider.radius, collider.radius, Physics.gravity.normalized, out hit, collider.radius, collisionMask);

        if (state.previouslyGrounded && didHit && Vector3.Angle(-Physics.gravity.normalized, hit.normal) <= 45f && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            Vector3 hyp;
            float topAngle = Vector3.Angle(Physics.gravity.normalized, -hit.normal);
            float bottomAngle = 180f - topAngle - 90f;
            hyp = -Physics.gravity.normalized * (SkinWidth / Mathf.Sin(Mathf.Deg2Rad * bottomAngle)) * Mathf.Sin(Mathf.Deg2Rad * 90f);

            transform.position += Physics.gravity.normalized * hit.distance + hyp;
            state.grounded = true;
        }
    }

    Vector3 GetPartPosition(Part part) {
        switch (part) {
            case Part.BottomSphere:
                return transform.position + transform.up * collider.radius;
            case Part.TopSphere:
                return transform.position + transform.up * collider.height - transform.up * collider.radius;
            case Part.Center:
                return transform.position + transform.up * collider.height * 0.5f;
            case Part.Bottom:
                return transform.position;
            case Part.Top:
                return transform.position + transform.up * collider.height;
            default:
                return transform.position;
        }
    }

    #endregion

    #region Physics

    Collider[] Overlap(LayerMask mask, QueryTriggerInteraction queryTriggerInteraction) {

        Collider[] all = Physics.OverlapCapsule(
            transform.position + transform.up * collider.radius,
            transform.position + transform.up * (collider.height - collider.radius), 
            collider.radius, mask, queryTriggerInteraction);

        for (int i = 0; i < all.Length; i++) {
            if (all[i] == collider) {
                all[i] = null;
            }
        }

        return UnimotionUtil.RemoveNull(all);
    }

    Collider[] OverlapCenter(LayerMask mask, QueryTriggerInteraction queryTriggerInteraction) {

        Collider[] all = Physics.OverlapSphere(
            transform.position + transform.up * collider.height * 0.5f,
            collider.radius, mask, queryTriggerInteraction);

        for (int i = 0; i < all.Length; i++) {
            if (all[i] == collider) {
                all[i] = null;
            }
        }

        return UnimotionUtil.RemoveNull(all);
    }

    RaycastHit Cast(Vector3 direction, float distance, LayerMask mask, QueryTriggerInteraction queryTriggerInteraction) {
        throw new System.NotImplementedException();
    }

    void Depenetrate() {

        int maxIterations = 30;
        int iterations = 0;      

        Collider[] cols = Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal);
        bool stuck = (cols.Length > 0 ? true : false);

        if (stuck) {

            state.stuck = true;

            foreach (Collider col in cols) {

                Vector3 direction; float distance;
                if (col != collider && Physics.ComputePenetration(collider, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out direction, out distance)) {
                    transform.position += direction * Mathf.Clamp((distance + SkinWidth), 0f, TerminalSpeed * Time.deltaTime);
                    Debug.DrawRay(GetPartPosition(Part.Center), direction, Color.magenta);
                }

                break;

            }

            iterations++;

            cols = Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal);
            stuck = (cols.Length > 0 ? true : false);

            while (stuck && iterations < maxIterations) {

                foreach (Collider col in cols) {
                    Vector3 direction; float distance;
                    if (Physics.ComputePenetration(collider, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out direction, out distance)) {
                        transform.position += direction * Mathf.Clamp((distance + SkinWidth), 0f, TerminalSpeed * Time.deltaTime);
                        Debug.DrawRay(GetPartPosition(Part.Center), direction, Color.magenta);
                    }
                    break;
                }

                iterations++;

                cols = Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal);
                stuck = (cols.Length > 0 ? true : false);

            }

            if (iterations >= maxIterations && OverlapCenter(finalCollisionMask, QueryTriggerInteraction.Ignore).Length > 0 && OnCrush != null) { OnCrush(); }

            if (OnDepenetrate != null) OnDepenetrate();

        } else {
            state.stuck = false;
        }

    }

    RaycastHit[] preAllocHits = new RaycastHit[20];
    Collider[] preAllocCols = new Collider[20];

    #endregion

    #region Public methods

    public void Walk(Vector3 delta) {
        if (walkBehaviour != WalkBehaviour.None) {
            inputVector = delta;
        }
    }

    public void Jump() {
        if ((state.grounded || Debug.isDebugBuild) && jumpBehaviour != JumpBehaviour.None ) {
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
        if (turnBehaviour == TurnBehaviour.Normal) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, -Physics.gravity.normalized), turnSpeed * Time.deltaTime);
        } else if (turnBehaviour == TurnBehaviour.Instant) {
            transform.rotation = Quaternion.LookRotation(direction, -Physics.gravity.normalized);
        }
    }

    #endregion

    public Vector3 debugDirection;
    public Vector3 stuckPosition;
    public Vector3 debugPoint;
    public Vector3 debugPoint2;

    private void OnDrawGizmosSelected() {
        Gizmos.DrawRay(transform.position, inputVectorSmoothed);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, debugDirection);

        //Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        //Gizmos.DrawWireSphere(stuckPosition + transform.up * collider.radius, collider.radius);
        //Gizmos.DrawWireSphere(stuckPosition + transform.up * collider.height - transform.up * collider.radius, collider.radius);

        Gizmos.DrawSphere(debugPoint, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(debugPoint2, 0.05f);
    }

    public enum Part { BottomSphere, TopSphere, Center, Top, Bottom }
    public enum MovementStyle { Raw, Smoothed }
    public enum WalkBehaviour { None, Normal }
    public enum JumpBehaviour { None, TotalControl, FixedVelocity, SmoothControl }
    public enum TurnBehaviour { None, Normal, Persistant, Instant }
    public enum SlopeBehaviour { Slide, PreventClimbing, PreventClimbingAndSlide }
    public enum RigidbodyCollisionBehaviour { Ignore, Collide, CollideAndPush, Push }
    public enum CharacterMotorCollisionBehaviour { Ignore, Collide, Push }

}

[System.Serializable]
public class CharacterMotorState {
    public float forwardMove;
    public float rightMove;
    public float moveSpeed;

    public bool moving;
    public bool grounded;
    public bool sliding;
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