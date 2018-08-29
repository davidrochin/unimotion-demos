using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class Character : MonoBehaviour {

    public float height, radius;

    [Header("Movement")]

    [Range(0.01f, 10)]
    public float speed = 7f;
    public float rotationSpeed = 400f;

    [Range(1f, 10f)]
    public float jumpForce = 10f;

    public LayerMask mask;
    public float rollSpeed = 1f;

    [Header("Debug")]
    public Vector3 velocity;
    public Vector3 relativeVelocity;
    public Vector3 lookDirection;

    public CharacterState state;

    //Información del entorno
    public float groundHeight;

    //Triggers
    bool jump;

    //Events
    public Action OnJump;
    public Action OnHighFall;

    Vector3 inputVector;

    const float SkinWidth = 0.01f;

    void Awake() {
        lookDirection = transform.forward;
    }

    bool controlUpdate = false;

    void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            controlUpdate = !controlUpdate;
        }

        if (controlUpdate) {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                ControlledUpdate();
            }
        } else {
            ControlledUpdate();
        }
    }

    void LateUpdate() {
        if (controlUpdate) {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                ControlledUpdate();
            }
        } else {
            ControlledLateUpdate();
        }
    }

    void ControlledUpdate() {

        // Apply gravity if necessary (terminal velocity of a human in freefall is about 53 m/s)
        if (!state.grounded && Vector3.Dot(velocity, Physics.gravity.normalized) < 50f) {
            velocity = velocity + Physics.gravity * 2f * Time.deltaTime;
        }

        Unstuck();

        Move(velocity * Time.deltaTime + inputVector * speed * Time.deltaTime);
        inputVector = Vector3.zero;

        Collider[] cols = Physics.OverlapCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius);
        foreach (Collider col in cols) {
            col.GetComponent<Rigidbody>().AddExplosionForce(50f, transform.position, 10f);
        }

        Quaternion fromToRotation = Quaternion.FromToRotation(transform.up, -Physics.gravity.normalized);
        transform.rotation = fromToRotation * transform.rotation;

    }

    void ControlledLateUpdate() {

        FollowFloor();
        CheckGrounded();
        StickToSlope();
    }

    #region Private methods

    void Move(Vector3 delta) {

        int slideCount = 0;

        FollowFloor();

        //Store the position from before moving
        Vector3 startingPos = transform.position;

        //Capsule cast to delta
        float lastDistance = 0f;
        RaycastHit[] hits = Physics.CapsuleCastAll(
            transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
            radius, delta.normalized, delta.magnitude, mask); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance);
        bool didHit = (hits.Length > 0 ? true : false); Vector3 lastNormal = (hits.Length > 0 ? hits[0].normal : Vector3.zero);

        if (hits.Length > 1) { Debug.Log(hits.Length); /*Debug.Break();*/ }

        //Move and slide on the hit plane
        if (didHit) {

            slideCount++;

            //[This could be replaced for something better]
            debugPosition = transform.position;
            transform.position += delta.normalized * hits[0].distance + hits[0].normal * SkinWidth;

            //Calculate the direction in which the Character should slide
            Vector3 slideDirection = Vector3.Cross(Vector3.Cross(hits[0].normal, delta.normalized), hits[0].normal).normalized;
            debugDirection = slideDirection;
            float slideMagnitude = Mathf.Clamp(Vector3.Dot(delta.normalized * (delta.magnitude - hits[0].distance), slideDirection), 0f, float.MaxValue);

            //Cast to see if the Character is free to move or must slide on another plane
            Debug.Log("Dot: " + slideMagnitude);
            hits = Physics.CapsuleCastAll(
                transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                radius, slideDirection, slideMagnitude, mask);
            didHit = (hits.Length > 0 ? true : false); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance); lastNormal = (hits.Length > 0 ? hits[0].normal : lastNormal);

            Vector3 remainingDelta = delta.normalized * (delta.magnitude - lastDistance);
            Debug.Log("Remaining Delta: " + remainingDelta);

            //If the Character cannot move freely
            while (didHit && slideCount < 1000) {

                lastNormal = hits[0].normal;
                slideCount++;

                //Slide util it hits
                debugPosition = transform.position;
                transform.position += slideDirection * hits[0].distance + hits[0].normal * SkinWidth;

                //Calculate the direction in which the Character should slide
                Vector3 previousDelta = slideDirection * slideMagnitude;
                slideDirection = Vector3.Cross(Vector3.Cross(hits[0].normal, slideDirection.normalized), hits[0].normal).normalized;
                slideMagnitude = Mathf.Clamp(Vector3.Dot(previousDelta.normalized * (previousDelta.magnitude - hits[0].distance), slideDirection), 0f, float.MaxValue);
                debugDirection = slideDirection;

                //Debug.Break();

                //Cast to see if the Character is free to move or must slide on another plane
                Debug.Log("Dot: " + slideMagnitude);
                hits = Physics.CapsuleCastAll(
                    transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                    radius, slideDirection, slideMagnitude, mask);
                didHit = (hits.Length > 0 ? true : false); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance); lastNormal = (hits.Length > 0 ? hits[0].normal : lastNormal);

                //Calculate how much delta is left to travel
                if (didHit) {
                    remainingDelta = remainingDelta.normalized * (remainingDelta.magnitude - hits[0].distance);
                    Debug.Log("Remaining Delta: " + remainingDelta);
                }
            }

            //If the Character is free to move
            if (!didHit) {

                debugPosition = transform.position;

                Debug.Log("Dot: " + Mathf.Clamp(Vector3.Dot(remainingDelta, slideDirection), 0f, float.MaxValue));
                transform.position += slideDirection * Mathf.Clamp(Vector3.Dot(remainingDelta, slideDirection), 0f, float.MaxValue);
                //Debug.DrawLine(GetPartPosition(Part.Top), GetPartPosition(Part.Top) + slideDirection * Mathf.Clamp(Vector3.Dot(remainingDelta.normalized * (remainingDelta.magnitude - lastDistance), slideDirection), 0f, float.MaxValue), Color.black, 1f);

                /*if (slideCount > 1f) {
                    Debug.DrawRay(GetPartPosition(Part.Top) + Vector3.up, delta, Color.black, 1f);
                    Debug.DrawRay(GetPartPosition(Part.Top) + Vector3.up, remainingDelta, Color.red, 1f);
                    Debug.DrawRay(GetPartPosition(Part.Top) + Vector3.up, slideDirection, Color.blue, 1f);
                    Debug.Log("This is slide #" + slideCount);
                    Debug.Break();
                }*/
            }
        } 
        
        //If the cast didn't hit anything, just move
        else {
            debugPosition = transform.position;
            transform.position += delta;
            //Debug.DrawLine(GetPartPosition(Part.Top), GetPartPosition(Part.Top) + delta, Color.black, 1f);
        }

        //Check if this is a valid position. If not, return to the position from before moving
        bool invalidPos = Physics.CheckCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, mask);
        if (invalidPos) {
            Debug.Break();
            Debug.LogError("Got stuck. " + slideCount + " slides.");
            stuckPosition = transform.position;
            transform.position = startingPos;
        }
    }

    void CheckGrounded() {

        //Save whether if the Character was grounded or not before the check
        state.previouslyGrounded = state.grounded;

        //Check the floor beneath (even if the Character is not touching it)
        RaycastHit floorHit; bool didHit = Physics.SphereCast(transform.position + transform.up * height - transform.up * radius, radius, -transform.up, out floorHit, float.MaxValue, mask);
        if (didHit) {
            state.floor = floorHit.transform;
        } else {
            state.floor = null;
        }

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.up * height - transform.up * radius, radius, -transform.up, height - radius * 2f + 0.04f, mask);
        if(hits.Length > 0 && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            bool validFloor = false;

            foreach (RaycastHit hit in hits) {

                float angle = Vector3.Angle(-Physics.gravity.normalized, hit.normal);
                state.floorAngle = angle;
                if(Vector3.Dot(hit.normal, -Physics.gravity.normalized) > 0f && angle <= 45f) {
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

    bool IsPositionValid() {
        return Physics.CheckCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, mask);
    }

    void Unstuck() {
        Collider[] cols = Physics.OverlapCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius);
        if(cols.Length > 0) {
            state.stuck = true;
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
        bool didHit = Physics.SphereCast(transform.position + transform.up * radius, radius, Physics.gravity.normalized, out hit, radius, mask);

        if (state.previouslyGrounded && didHit && Vector3.Angle(-Physics.gravity.normalized, hit.normal) <= 45f) {

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
        velocity = velocity - Physics.gravity.normalized * jumpForce;
        state.grounded = false;
    }

    public void RotateTowards(Vector3 direction) {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, -Physics.gravity.normalized), rotationSpeed * Time.deltaTime);
    }

    #endregion

    public Vector3 debugDirection;
    public Vector3 stuckPosition;
    public Vector3 debugPosition;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.up * radius, radius);
        Gizmos.DrawWireSphere(transform.position + transform.up * height - transform.up * radius, radius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, debugDirection);

        //Gizmos.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        Gizmos.DrawWireSphere(stuckPosition + transform.up * radius, radius);
        Gizmos.DrawWireSphere(stuckPosition + transform.up * height - transform.up * radius, radius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(debugPosition + transform.up * radius, radius);
        Gizmos.DrawWireSphere(debugPosition + transform.up * height - transform.up * radius, radius);
    }

    public enum Part { BottomSphere, TopSphere, Center, Top, Bottom }

}

[System.Serializable]
public class CharacterState {
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

    public CharacterState() {

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
