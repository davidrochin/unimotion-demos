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

    [HideInInspector]
    Animator animator;

    const float SkinWidth = 0.01f;

    void Awake() {
        animator = GetComponent<Animator>();
        lookDirection = transform.forward;
    }

    void Update() {

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

    void LateUpdate() {

        FollowFloor();
        CheckGrounded();
        StickToSlope();
    }

    #region Private methods

    void Move(Vector3 delta) {

        FollowFloor();

        //Store the position from before moving
        Vector3 startingPos = transform.position;

        //Capsule cast to delta
        float lastDistance = 0f;
        RaycastHit[] hits = Physics.CapsuleCastAll(
            transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
            radius, delta.normalized, delta.magnitude, mask); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance);
        bool didHit = (hits.Length > 0 ? true : false);

        Debug.Log(hits.Length); if(hits.Length > 1) { Debug.Break(); }

        //Move and slide on the hit plane
        if (didHit) {

            //[This could be replaced for something better]
            transform.position += delta.normalized * hits[0].distance + hits[0].normal * SkinWidth;

            /*transform.position += delta.normalized * hit.distance;
            float angleA = 180f - 90f - Vector3.Angle(delta.normalized, -hit.normal);
            float cSide = ((SkinWidth / Mathf.Sin(Mathf.Deg2Rad * angleA)) * Mathf.Sin(Mathf.Sin(Mathf.Deg2Rad * 90f))); cSide = Mathf.Clamp(cSide, 0f, delta.magnitude);
            transform.position = transform.position - delta.normalized * cSide;
            Debug.Log("Angle A = " + angleA + ", Formula = " + ((SkinWidth / Mathf.Sin(Mathf.Deg2Rad * angleA)) * Mathf.Sin(Mathf.Sin(Mathf.Deg2Rad * 90f))) + ", Delta = " + delta.magnitude);*/

            Vector3 slideDirection = Vector3.Cross(Vector3.Cross(hits[0].normal, delta.normalized), hits[0].normal).normalized;
            debugDirection = slideDirection;

            hits = Physics.CapsuleCastAll(
                transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                radius, slideDirection, Vector3.Dot(delta.normalized * (delta.magnitude - hits[0].distance), slideDirection), mask);
            didHit = (hits.Length > 0 ? true : false); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance);

            Vector3 remainingDelta = delta.normalized * (delta.magnitude - lastDistance);
            
            while (didHit) {

                //Slide util it hits
                transform.position += slideDirection * hits[0].distance + hits[0].normal * SkinWidth;

                /*transform.position += slideDirection.normalized * (hit.distance - cSide);
                angleA = 180f - 90f - Vector3.Angle(slideDirection.normalized, -hit.normal);
                cSide = ((SkinWidth / Mathf.Sin(Mathf.Deg2Rad * angleA)) * Mathf.Sin(Mathf.Sin(Mathf.Deg2Rad * 90f))); cSide = Mathf.Clamp(cSide, 0f, slideDirection.magnitude);
                transform.position = transform.position - slideDirection.normalized * cSide;*/

                slideDirection = Vector3.Cross(Vector3.Cross(hits[0].normal, slideDirection.normalized), hits[0].normal).normalized;
                debugDirection = slideDirection;

                hits = Physics.CapsuleCastAll(
                    transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                    radius, slideDirection, Vector3.Dot(remainingDelta.normalized * (remainingDelta.magnitude - hits[0].distance), slideDirection), mask);
                didHit = (hits.Length > 0 ? true : false); lastDistance = (hits.Length > 0 ? hits[0].distance : lastDistance);

                remainingDelta = remainingDelta.normalized * (remainingDelta.magnitude - lastDistance);

            }

            if (!didHit) {
                //transform.position += slideDirection * Vector3.Dot(delta.normalized * (delta.magnitude - hit.distance + cSide), slideDirection);
                transform.position += slideDirection * Vector3.Dot(delta.normalized * (delta.magnitude - lastDistance), slideDirection);
            }

        } 
        
        //If the cast didn't hit anything, just move
        else {
            transform.position += delta;
        }

        //Check if this is a valid position. If not, return to the position from before moving
        bool invalidPos = Physics.CheckCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, mask);
        if (invalidPos) {
            //Debug.Break();
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

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.up * radius, radius);
        Gizmos.DrawWireSphere(transform.position + transform.up * height - transform.up * radius, radius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, debugDirection);
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
