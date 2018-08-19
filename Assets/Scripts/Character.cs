using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class Character : MonoBehaviour {

    public float height, radius;

    [Header("Movement")]

    [Range(0.01f, 10)]
    public float speed = 5f;
    public float rotationSpeed = 400f;

    [Range(1f, 10f)]
    public float jumpForce = 0.1f;

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

    void Awake() {
        animator = GetComponent<Animator>();
        lookDirection = transform.forward;
    }

    void Update() {

        CheckGrounded();

        // Apply gravity if necessary (terminal velocity of a human in freefall is about 53 m/s)
        if (!state.grounded && Vector3.Dot(velocity, Physics.gravity.normalized) < 50f) {
            velocity = velocity + Physics.gravity * 2f * Time.deltaTime;
        }

        Unstuck();

        Move(velocity * Time.deltaTime + inputVector * speed * Time.deltaTime);
        inputVector = Vector3.zero;

        Quaternion fromToRotation = Quaternion.FromToRotation(transform.up, -Physics.gravity.normalized);
        transform.rotation = fromToRotation * transform.rotation;

    }

    void LateUpdate() {

        //FollowFloor();
    }


    #region Private methods

    void Move(Vector3 delta) {

        //Store the position from before moving
        Vector3 startingPos = transform.position;

        RaycastHit hit; bool didHit = Physics.CapsuleCast(
            transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
            radius, delta.normalized, out hit, delta.magnitude, mask);

        if (didHit) {

            //transform.position += delta.normalized * (hit.distance - 0.02f);
            transform.position += delta.normalized * hit.distance + hit.normal * 0.01f;

            Vector3 onPlaneDirection = Vector3.Cross(Vector3.Cross(hit.normal, delta.normalized), hit.normal);

            didHit = Physics.CapsuleCast(
            transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
            radius, onPlaneDirection, out hit, Vector3.Dot(delta.normalized * (delta.magnitude - hit.distance), onPlaneDirection), mask);

            if (!didHit) {
                transform.position += onPlaneDirection * Vector3.Dot(delta.normalized * (delta.magnitude - hit.distance), onPlaneDirection);
            } else {
                transform.position += onPlaneDirection * (hit.distance - 0.02f);
            }

        } else {
            transform.position += delta;
        }

        //Stick to the slope
        didHit = Physics.SphereCast(transform.position + transform.up * radius, radius, Physics.gravity.normalized, out hit, 7.5f * Time.deltaTime, mask);
        if (state.previouslyGrounded && didHit && delta != Vector3.zero) {
            transform.position += Physics.gravity.normalized * hit.distance + hit.normal * 0.01f;
        }

        //Check if this is a valid position. If not, return to the position from before moving
        bool invalidPos = Physics.CheckCapsule(transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius, radius, mask);
        if (invalidPos) {
            transform.position = startingPos;
        }
    }

    void CheckGrounded() {

        FollowFloor();

        state.previouslyGrounded = state.grounded;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.up * height - transform.up * radius, radius, -transform.up, height - radius * 2f + 0.04f, mask);
        if(hits.Length > 0 && Vector3.Dot(velocity, Physics.gravity.normalized) >= 0f) {
            bool validFloor = false;

            foreach (RaycastHit hit in hits) {

                float angle = Vector3.Angle(-Physics.gravity.normalized, hit.normal);
                //Debug.Log(angle);

                // [This could be replaced by an angle measurement]
                if(Vector3.Dot(hit.normal, -Physics.gravity.normalized) > 0f && angle <= 45f) {
                    validFloor = true;

                    state.floor = hit.transform;
                    //transform.parent = hit.transform;
                    //break;
                }
            }

            if (validFloor) {
                state.grounded = true;
                velocity = Vector3.zero;
            } else {
                state.grounded = false;
                state.floor = null;
            }

        } else {
            state.grounded = false;
            state.floor = null;
            //transform.parent = null;
        }
    }

    void Unstuck() {

    }

    void FollowFloor() {

        //Follow the floor transform
        if (state.floor != null) {

            if (state.floorState.transform == null || state.floorState.transform != state.floor) {
                state.floorState = TransformState.From(state.floor);
            } else {

                //Follow position
                transform.position += state.floor.position - state.floorState.position;

                //Follow rotation
                //Quaternion dif = Quaternion.Inverse(state.floorState.rotation) * state.floor.rotation;
                Quaternion dif = Quaternion.FromToRotation(state.floorState.forward, state.floor.forward);
                //transform.rotation = transform.rotation * dif;
                transform.rotation = dif * transform.rotation;
                Vector3 delta = transform.position - state.floor.position;
                delta = dif * delta; transform.position = state.floor.position + delta;

                state.floorState = TransformState.From(state.floor);
            }

        } else {
            state.floorState = TransformState.Empty();
        }
    }

    #endregion

    #region Public methods

    public void Walk(Vector3 delta) {
        inputVector = Vector3.ClampMagnitude(delta, 1f);
    }

    public void Jump() {
        velocity = velocity - Physics.gravity.normalized * jumpForce;
    }

    public void RotateTowards(Vector3 direction, float speed) {
        lookDirection = direction;
    }

    public void ForceRotateTowards(Vector3 direction, float speed) {
        Vector3 procesedDirection = new Vector3(direction.x, 0f, direction.z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(procesedDirection), speed);
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

}

[System.Serializable]
public class CharacterState {
    public float forwardMove;
    public float rightMove;
    public float moveSpeed;

    public bool moving;
    public bool grounded;
    public bool previouslyGrounded;

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
