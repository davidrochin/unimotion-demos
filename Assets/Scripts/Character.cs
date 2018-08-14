using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class Character : MonoBehaviour {

    public float height, radius;

    [Header("Movement")]

    [Range(0.01f, 10f)]
    public float speed = 5f;
    public float rotationSpeed = 400f;

    [Range(1f, 10f)]
    public float jumpForce = 5f;

    public LayerMask mask;
    public float rollSpeed = 1f;

    [Header("Debug")]
    public Vector3 velocity;
    public Vector3 lookDirection;

    public CharacterState state;

    //Información del entorno
    public float groundHeight;

    //Triggers
    bool jump;

    //Events
    public Action OnJump;
    public Action OnHighFall;

    Vector3 moveVector;
    Vector3 inputVector;

    [HideInInspector]
    Animator animator;

    #region Monobehaviours

    void Awake() {
        animator = GetComponent<Animator>();
        lookDirection = transform.forward;
    }

    void Update() {

        
    }

    #endregion

    #region Private methods

    public void Move(Vector3 delta) {

        RaycastHit hit; bool didHit = Physics.CapsuleCast(
            transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
            radius, delta.normalized, out hit, delta.magnitude, mask);

        if (didHit) {
            do {

                debugDirection = Vector3.Cross(Vector3.Cross(hit.normal, delta.normalized), hit.normal);

                didHit = Physics.CapsuleCast(
                transform.position + transform.up * radius, transform.position + transform.up * height - transform.up * radius,
                radius, delta.normalized, out hit, delta.magnitude, mask);


            } while (false);
        } else {
            transform.position += delta;
        }
    }

    #endregion

    #region Public methods

    public void Jump() {
        if (state.grounded) {
            jump = true;
        }
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

    //Combat
    public bool blocking = false;
    public bool attacking = false;
    public bool rolling = false;
    public bool dead = false;

    public float groundHeight = float.MinValue;
    public Vector3 velocity;

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

        anim.SetFloat("velocityX", velocity.x);
        anim.SetFloat("velocityY", velocity.y);
        anim.SetFloat("velocityZ", velocity.z);
    }

    public void Reset() {
        forwardMove = 0f;
        moveSpeed = 0f;
        rightMove = 0f;
    }
}

public delegate void Action();
public delegate void FloatAction(float n);
