using UnityEngine;

[SelectionBase]
[AddComponentMenu("Unimotion/Character Motor")]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterMotor : MonoBehaviour {

    #region User Settings

    // Walking
    public WalkBehaviour walkBehaviour;
    [Tooltip("How fast the character should walk (in m/s).")] [Range(0.01f, 20)] public float walkSpeed = 7f; public float maxWalkMagnitude = 2f;
    [Range(0f, 32f)] public float walkSmoothness = 16f;
    public bool smoothDirection = true;
    public bool smoothSpeed = true;

    // Slopes
    [Tooltip("The maximum slope angle the Character can climb.")] [Range(5f, 85f)] public float slopeLimit = 50f;
    public SlopeBehaviour slopeBehaviour = SlopeBehaviour.Slide;

    // Turning
    public TurnBehaviour turnBehaviour = TurnBehaviour.Normal;
    [Tooltip("How fast the character should turn (in degrees/s).")] public float turnSpeed = 400f;

    // Jumping
    [Range(1f, 30f)] public float jumpForce = 10f;
    public JumpBehaviour jumpBehaviour = JumpBehaviour.TotalControl;
    [Range(0f, 10f)] public float airControl = 4f;
    public bool canJumpWhileSliding = true;

    // Masks
    [Tooltip("A mask that defines what are the objects the Character can collide with.")]
    public LayerMask collisionMask;
    public LayerMask characterMask;
    public LayerMask waterMask;

    // Collision
    public Quality collisionQuality = Quality.High;
    public CharacterMotorCollisionBehaviour characterCollisionBehaviour;
    [Range(0.01f, 5f)] public float characterPushForce = 4f;

    // Rigidbody Interaction
    public RigidbodyCollisionBehaviour rigidbodyCollisionBehaviour;
    [Tooltip("The force this character applies to rigidbodies (in Newtons)")] public float rigidbodyPushForce = 600;
    public LayerMask rigidbodyMask;

    // Animation
    public bool outputToAnimator;
    public Animator animator;
    public bool smoothMoveParameters = true;

    #endregion

    #region Events

    public event Action OnWalk;
    public event Action OnStop;
    public event Action OnJump;
    public event Action OnGroundedChange;
    public event Action OnLand;
    public event Action OnFrameStart;
    public event Action OnFrameFinish;
    public event Action OnDepenetrate;
    public event Action OnCrush;

    #endregion

    #region Properties

    public bool Moving { get { return input.magnitude > 0f; } }

    #endregion

    // Debug
    public Vector3 velocity;

    public CharacterMotorState state;

    new CapsuleCollider collider;

    bool jump;
    Vector3 input;
    Vector3 procesedInput;
    public Vector3 inputVectorCached; /// This is for the animator to use

    public float skinWidth = 0.01f;
    public float terminalSpeed = 50f;
    public float floorFriction = 16f;
    public float airFriction = 3f;
    public float mass = 1f;

    private void Awake() {
        collider = GetComponent<CapsuleCollider>();

        // Make sure SphereCollider center is correct
        collider.center = new Vector3(0f, collider.height * 0.5f, 0f);
        collider.direction = 1;
    }

    void Start() {
        // Test events
        //OnWalk += () => Debug.Log("OnWalk");
        OnJump += () => Debug.Log("OnJump");
        OnLand += () => Debug.Log("OnLand");
        OnCrush += () => Debug.LogWarning("OnCrush");
        //OnDepenetrate += () => Debug.Log("OnDepenetrate");
    }

    void Update() {

        groundedAtBeginning = Grounded;

        FollowFloor();

        if (!ValidatePosition()) { Depenetrate(); }

        // Apply gravity if necessary (terminal velocity of a human in freefall is about 53 m/s)
        if ((!Grounded || state.sliding) && Vector3.Dot(velocity, Physics.gravity.normalized) < 50f) {
            //velocity = velocity + Physics.gravity * 2f * Time.deltaTime;

            if (Vector3.Dot(velocity, -Physics.gravity.normalized) >= 0f) {
                velocity = velocity + Physics.gravity * Time.deltaTime;
            } else {
                velocity = velocity + Physics.gravity * 2f * Time.deltaTime;
            }
        }

        /// Store current procesed input for the OnStop event later
        Vector3 tmpInput = procesedInput;

        // Apply movement from input
        if (Grounded) {
            switch (walkBehaviour) {
                case WalkBehaviour.Normal:
                    procesedInput = input; input = Vector3.zero;
                    Move(procesedInput * walkSpeed * Time.deltaTime);
                    break;
                case WalkBehaviour.Smoothed:
                    procesedInput = Vector3.MoveTowards(procesedInput, input, walkSmoothness * Time.deltaTime); input = Vector3.zero;
                    Move(procesedInput * walkSpeed * Time.deltaTime);
                    break;
            }
        } else {
            switch (jumpBehaviour) {
                case JumpBehaviour.TotalControl:
                    procesedInput = input; input = Vector3.zero;
                    Move(procesedInput * walkSpeed * Time.deltaTime);
                    break;
                case JumpBehaviour.FixedVelocity:
                    Move(procesedInput * walkSpeed * Time.deltaTime);
                    break;
                case JumpBehaviour.SmoothControl:
                    procesedInput = Vector3.MoveTowards(procesedInput, input, airControl * Time.deltaTime); input = Vector3.zero;
                    Move(procesedInput * walkSpeed * Time.deltaTime);
                    break;
            }
        }

        inputVectorCached = procesedInput;

        /// OnStop event
        if (OnStop != null && procesedInput.magnitude <= 0f && tmpInput.magnitude > 0f) { OnStop(); }

        // Apply movement from velocity
        Move(velocity * Time.deltaTime);

        Push();

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
        if (Grounded && !state.sliding) {

            // Change velocity so it doesnt go towards the floor
            velocity = velocity - Physics.gravity.normalized * Vector3.Dot(velocity, Physics.gravity.normalized);

            // Apply floor friction
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, state.floorCollider.material.dynamicFriction * 50f * Time.deltaTime);
            //velocity = Vector3.MoveTowards(velocity, Vector3.zero, floorFriction * Time.deltaTime);
        } else {

            // Apply air friction
            float towardsGravity = Vector3.Dot(velocity, Physics.gravity.normalized);
            velocity = velocity - Physics.gravity.normalized * towardsGravity;
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, airFriction * Time.deltaTime);
            velocity = velocity + Physics.gravity.normalized * towardsGravity;

        }

        StickToSlope();
        if (!ValidatePosition()) { Depenetrate(); }

        /// Evento OnGroundedChange
        if (OnGroundedChange != null && groundedAtBeginning != Grounded) {
            OnGroundedChange();
        }

        // Termina fisicas --------------------------------------------------------------------------

        UpdateAnimator();

        inputVectorCached = Vector3.zero;

        if (OnFrameFinish != null) { OnFrameFinish(); }

    }

    #region Private methods

    public void Move(Vector3 delta) {

        // Do not do anything if delta is zero
        if (delta == Vector3.zero) { return; }

        int slideCount = 0;

        int[] maxIterations = { 50, 20, 5, 2 };

        // Calculate the LayerMask to use
        LayerMask finalCollisionMask = collisionMask;
        if (characterCollisionBehaviour == CharacterMotorCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | characterMask; }
        if (rigidbodyCollisionBehaviour == RigidbodyCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | rigidbodyMask; }

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
            transform.position += delta.normalized * (hit.distance - skinWidth);
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

            Vector3 remainingDelta = delta.normalized * (delta.magnitude - lastDistance) * 0.95f;

            // If the Character cannot move freely
            while (didHit && slideCount < maxIterations[(int)collisionQuality]) {
                slideCount++;

                Debug.DrawRay(hit.point, hit.normal);
                lastNormal = hit.normal;

                // Slide util it hits
                previousPos = transform.position;
                transform.position += slideDirection.normalized * (hit.distance - skinWidth);
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
                    remainingDelta = remainingDelta.normalized * (remainingDelta.magnitude - hit.distance) * 0.95f;
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

    void Push() {

        // Push Characters
        if (characterCollisionBehaviour == CharacterMotorCollisionBehaviour.Push || characterCollisionBehaviour == CharacterMotorCollisionBehaviour.SoftPush) {

            Collider[] characters = Overlap(characterMask, QueryTriggerInteraction.Ignore);

            if (characters.Length > 0) {
                for (int i = 0; i < characters.Length; i++) {
                    Vector3 direction; float distance;
                    Physics.ComputePenetration(collider, transform.position, transform.rotation, characters[i], characters[i].transform.position, characters[i].transform.rotation, out direction, out distance);

                    if (characterCollisionBehaviour == CharacterMotorCollisionBehaviour.Push) {
                        characters[i].GetComponent<CharacterMotor>().Move(direction * -distance);
                    } else if (characterCollisionBehaviour == CharacterMotorCollisionBehaviour.SoftPush) {
                        characters[i].GetComponent<CharacterMotor>().Move(-direction * characterPushForce * Time.deltaTime);
                    }

                }
            }

        }

    }

    void CheckGrounded() {

        // Save whether if the Character was grounded or not before the check
        state.previouslyGrounded = Grounded;

        // Check the floor beneath (even if the Character is not touching it)
        RaycastHit floorHit; bool didHit = Physics.SphereCast(transform.position + transform.up * collider.height - transform.up * collider.radius, collider.radius, -transform.up, out floorHit, float.MaxValue, collisionMask);
        if (didHit) {
            state.floor = floorHit.transform;
            state.floorCollider = floorHit.collider;
        } else {
            state.floor = null;
            state.floorCollider = null;
        }

        state.sliding = true;

        // Check for ground
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.up * collider.height - transform.up * collider.radius, collider.radius, -transform.up, collider.height - collider.radius * 2f + collider.radius, collisionMask);

        if (hits.Length > 0 && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            bool validFloor = false;

            // Check each hit for valid ground
            foreach (RaycastHit hit in hits) {

                // Calculate the angle of the floor
                float angle = Vector3.Angle(-Physics.gravity.normalized, hit.normal);
                state.floorAngle = angle;

                if (Vector3.Dot(hit.normal, -Physics.gravity.normalized) > 0f && angle < 85f && !(hit.distance == 0f && hit.point == Vector3.zero) && Vector3.Distance(hit.point, GetPartPosition(Part.BottomSphere)) <= collider.radius + skinWidth * 4f) {

                    // Check if it should slide
                    if (angle < slopeLimit) {
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
                Grounded = true;

                // If the player was previously ungrounded, run the OnLand event
                if (state.previouslyGrounded == false && OnLand != null) { OnLand(); }

            } else {
                Grounded = false;
                state.sliding = false;
            }

        } else {
            state.sliding = false;
            Grounded = false;
        }
    }

    bool ValidatePosition() {

        // Calculate the LayerMask to use
        LayerMask finalCollisionMask = collisionMask;
        if (characterCollisionBehaviour == CharacterMotorCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | characterMask; }
        if (rigidbodyCollisionBehaviour == RigidbodyCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | rigidbodyMask; }

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
            else if (Grounded) {

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

    void Depenetrate() {

        int[] maxIterations = { 100, 50, 20, 2 };
        int iterations = 0;

        // Calculate the LayerMask to use
        LayerMask finalCollisionMask = collisionMask;
        if (characterCollisionBehaviour == CharacterMotorCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | characterMask; }
        if (rigidbodyCollisionBehaviour == RigidbodyCollisionBehaviour.Collide) { finalCollisionMask = finalCollisionMask | rigidbodyMask; }

        Collider[] cols = Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal);
        bool stuck = (cols.Length > 0 ? true : false);
        //Debug.Log("stuck: " + stuck + ". length: " + cols.Length);

        if (stuck) {
            state.stuck = true;

            foreach (Collider col in cols) {
                Vector3 v; float f;
                //Debug.Log((col != collider) + ", " + Physics.ComputePenetration(collider, transform.position + collider.center, transform.rotation, col, col.transform.position, col.transform.rotation, out v, out f));

                Vector3 direction; float distance;
                if (col != collider && Physics.ComputePenetration(collider, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out direction, out distance)) {
                    transform.position += direction * Mathf.Clamp((distance + skinWidth), 0f, terminalSpeed * Time.deltaTime);
                    Debug.DrawRay(GetPartPosition(Part.Center), direction, Color.magenta);
                }

                break;

            }

            iterations++;

            cols = Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal);
            stuck = (cols.Length > 0 ? true : false);

            while (stuck && iterations < maxIterations[(int)collisionQuality]) {

                foreach (Collider col in cols) {
                    Vector3 direction; float distance;
                    if (Physics.ComputePenetration(collider, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out direction, out distance)) {
                        transform.position += direction * Mathf.Clamp((distance + skinWidth), 0f, terminalSpeed * Time.deltaTime);
                        Debug.DrawRay(GetPartPosition(Part.Center), direction, Color.magenta);
                    }
                    break;
                }

                iterations++;

                cols = Overlap(finalCollisionMask, QueryTriggerInteraction.UseGlobal);
                stuck = (cols.Length > 0 ? true : false);

            }

            if (iterations >= maxIterations[(int)collisionQuality] && OverlapCenter(finalCollisionMask, QueryTriggerInteraction.Ignore).Length > 0 && OnCrush != null) { OnCrush(); }

            if (OnDepenetrate != null) {
                OnDepenetrate();
            }
        } else {
            state.stuck = false;
        }

    }

    void StickToSlope() {

        RaycastHit hit;
        bool didHit = Physics.SphereCast(transform.position + transform.up * collider.radius, collider.radius, Physics.gravity.normalized, out hit, collider.radius, collisionMask);

        if (state.previouslyGrounded && didHit && Vector3.Angle(-Physics.gravity.normalized, hit.normal) <= 45f && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            Vector3 hyp;
            float topAngle = Vector3.Angle(Physics.gravity.normalized, -hit.normal);
            float bottomAngle = 180f - topAngle - 90f;
            hyp = -Physics.gravity.normalized * (skinWidth / Mathf.Sin(Mathf.Deg2Rad * bottomAngle)) * Mathf.Sin(Mathf.Deg2Rad * 90f);

            transform.position += Physics.gravity.normalized * hit.distance + hyp;
            Grounded = true;
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

    public Collider[] Overlap(LayerMask mask, QueryTriggerInteraction queryTriggerInteraction) {

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

    public RaycastHit Cast(Vector3 direction, float distance, LayerMask mask, QueryTriggerInteraction queryTriggerInteraction) {
        throw new System.NotImplementedException();
    }

    public void AddForce(Vector3 force) {
        velocity += force.normalized * Mathf.Sqrt((force.magnitude / mass));
    }

    #endregion

    #region Public methods

    public void Walk(Vector3 delta) {
        if (walkBehaviour != WalkBehaviour.None) {
            input = delta;
            if (OnWalk != null && procesedInput.magnitude <= 0f && input.magnitude > 0f) { OnWalk(); }
        }
    }

    public void Jump() {
        if ((Grounded || Debug.isDebugBuild) && jumpBehaviour != JumpBehaviour.None) {
            velocity = velocity - Physics.gravity.normalized * jumpForce;
            Grounded = false;
            if (OnJump != null) {
                OnJump();
            }
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

    public void ForceTurnTowards(Vector3 direction) {
        transform.rotation = Quaternion.LookRotation(direction, -Physics.gravity.normalized);
    }

    public void UpdateAnimator() {
        if (outputToAnimator && animator != null) {

            const float Roughness = 4f;

            float forwardMove = Vector3.Dot(inputVectorCached, transform.forward);
            animator.SetFloat("Forward Move", smoothMoveParameters ? Mathf.MoveTowards(animator.GetFloat("Forward Move"), forwardMove, Roughness * Time.deltaTime) : forwardMove);

            float strafeMove = Vector3.Dot(inputVectorCached, transform.right);
            animator.SetFloat("Strafe Move", smoothMoveParameters ? Mathf.MoveTowards(animator.GetFloat("Strafe Move"), strafeMove, Roughness * Time.deltaTime) : strafeMove);

            float speed = (inputVectorCached - Vector3.Project(inputVectorCached, -Physics.gravity.normalized)).magnitude;
            animator.SetFloat("Move Speed", smoothMoveParameters ? Mathf.MoveTowards(animator.GetFloat("Move Speed"), speed, Roughness * Time.deltaTime) : speed);

            animator.SetFloat("Move", inputVectorCached.magnitude);
            animator.SetFloat("Max Move Speed", maxWalkMagnitude);
            animator.SetFloat("Upwards Speed", Vector3.Dot(velocity, -Physics.gravity.normalized));
            animator.SetFloat("Sideways Speed", (velocity - Vector3.Project(velocity, -Physics.gravity.normalized)).magnitude);
            animator.SetBool("Grounded", Grounded);
            animator.SetBool("Sliding", state.sliding);
            animator.SetBool("Stuck", state.stuck);
        }
    }

    #endregion

    public Vector3 debugDirection;
    public Vector3 stuckPosition;
    public Vector3 debugPoint;
    public Vector3 debugPoint2;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, procesedInput);

        Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, debugDirection);

        //Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        //Gizmos.DrawWireSphere(stuckPosition + transform.up * collider.radius, collider.radius);
        //Gizmos.DrawWireSphere(stuckPosition + transform.up * collider.height - transform.up * collider.radius, collider.radius);

        Gizmos.DrawSphere(debugPoint, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(debugPoint2, 0.05f);
    }

    public enum Quality { Extreme, High, Medium, Low }

    public enum Part { BottomSphere, TopSphere, Center, Top, Bottom }
    public enum MovementStyle { Raw, Smoothed }

    public enum WalkBehaviour { None, Normal, Smoothed }
    public enum JumpBehaviour { None, TotalControl, FixedVelocity, SmoothControl }
    public enum TurnBehaviour { None, Normal, Persistant, Instant }
    public enum SlopeBehaviour { Slide, PreventClimbing, PreventClimbingAndSlide }
    public enum RigidbodyCollisionBehaviour { Ignore, Collide, Push }
    public enum CharacterMotorCollisionBehaviour { Ignore, Collide, Push, SoftPush }

    private bool grounded;
    private bool groundedAtBeginning;
    public bool Grounded {
        get {
            return grounded;
        }
        set {
            grounded = value;
        }
    }
}

[System.Serializable]
public class CharacterMotorState {

    public bool sliding;
    public bool previouslyGrounded;

    public bool stuck;

    public float floorAngle;

    public Transform floor;
    public Collider floorCollider;
    public TransformState floorState;

    public CharacterMotorState() {

    }

    public void Reset() {
        floor = null;
    }
}

public delegate void Action();
public delegate void FloatAction(float n);

public delegate void OnWalkHandler(Vector3 direction, float speed);
public delegate void OnLandHandler(float impactSpeed, float distanceFallen);
public delegate void OnJumpHandler();
public delegate void OnCrushHandler();