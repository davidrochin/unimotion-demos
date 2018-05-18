using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour {

    [Header("Movement")]

    [Range(0.01f, 10f)]
    public float speed = 5f;

    [Range(1f, 10f)]
    public float jumpForce = 5f;

    public LayerMask mask;
    public float rollSpeed = 1f;

    [Header("Debug")]
    public bool grounded = false;
    public bool walking = false;
    public bool running = false;
    public bool rolling = false;
    public float yForce = 0f, xForce = 0f;
    public Vector3 velocity;

    public CharacterStateInfo stateInfo;

    //Información del entorno
    public float groundHeight;

    //Triggers
    bool jump;

    //Events
    public Action OnJump;
    public Action OnHighFall;

    Vector3 moveVector;
    Vector3 inputVector;

    CharacterController characterController;
    Animator animator;
    LedgeGrabber ledgeGrabber;
    Health health;

    void Awake() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    void Update() {

        //Save initial position to calculate velocity later
        Vector3 initPos = transform.position;

        CheckGroundHeight();
        ApplyGravity();
        CheckForJump();
        CalculateMoveVector();

        //Move Unity's CharacterController and detect if it is grounded (it's necessary to move first, because Unity)
        characterController.Move(moveVector + inputVector * speed * Time.deltaTime);
        if (!grounded && characterController.isGrounded && Mathf.Abs(velocity.y) / Time.deltaTime > 15f) {
            if (OnHighFall != null) OnHighFall();
        }
        grounded = characterController.isGrounded;

        if (grounded) { stateInfo.grounded = true; } else { stateInfo.grounded = false; }
        stateInfo.forwardSpeed = inputVector.magnitude * speed;
        //IF ROLLING
        if (rolling) {
            rollTimer += Time.deltaTime;

            CheckGroundHeight();
            ApplyGravity();
            CalculateMoveVector();

            moveVector += transform.forward * rollSpeed * Time.deltaTime;
            inputVector = Vector3.zero;

            //Mover el CharacterController y detectar si está en el piso (es necesario moverlo primero por cosas de Unity)
            characterController.Move(moveVector + inputVector * speed * Time.deltaTime);
            grounded = characterController.isGrounded;
            //if (grounded) { state = State.Rolling; } else { state = State.OnAir; }

            //Revisar si ya se acabó la animacion
            if (rollTimer >= animator.GetCurrentAnimatorStateInfo(0).length) {
                //state = State.OnGround;
            }
        }

        UpdateAnimator();

        //Resetear todo lo que se tiene que resetear
        if (inputVector == Vector3.zero) { stateInfo.forwardMove = Mathf.MoveTowards(stateInfo.forwardMove, 0f, 2f * Time.deltaTime); }
        moveVector = Vector3.zero;
        inputVector = Vector3.zero;

        //Calcular la velocidad
        velocity = transform.position - initPos;
    }

    #region Acciones Internas

    void ApplyGravity() {
        //Debug.Log("grounded=" + grounded + ", yForce=" + yForce);
        if (!grounded || yForce > 0) {
            yForce = yForce + (Physics2D.gravity.y * Time.deltaTime);
        } else {
            yForce = -0.1f;
            yForce = -1;
        }
    }

    void CheckGroundHeight() {

        //Measure how high is the floor
        RaycastHit hit = RaycastUtil.RaycastPastItself(gameObject, transform.position + characterController.center, Vector3.down, characterController.height / 2f + 4f, mask);
        if (hit.collider != null) {
            groundHeight = hit.point.y;
        } else {
            groundHeight = float.MinValue;
        }
    }

    void CalculateMoveVector() {
        moveVector = moveVector + new Vector3(0f, yForce * Time.deltaTime, 0f);
    }

    void CheckForJump() {
        if (jump) {
            yForce = jumpForce;
            grounded = false;
            jump = false;
        }
    }

    void UpdateAnimator() {
        stateInfo.UpdateToAnimator(animator);
    }

    #endregion

    #region Acciones Básicas

    public void Move(Vector3 direction, float speed) {
        if(health == null || health.isAlive) {
            inputVector = direction.normalized * speed;
            Vector3 i = transform.InverseTransformDirection(inputVector);
            stateInfo.forwardMove = Mathf.MoveTowards(stateInfo.forwardMove, i.z, 2f * Time.deltaTime);
            //stateInfo.forwardMove = i.z;
            stateInfo.rightMove = i.x;
        }
    }

    public void Jump() {
        if(health == null || health.isAlive) {
            if (grounded) {
                jump = true;
            }
        }
    }

    float rollTimer = 0f;
    public void Roll() {
        if (grounded) {
            //rolling = true;
            //rollTimer = 0f;
            animator.SetTrigger("roll");
        }
    }

    public void RotateTowards(Vector3 direction, float speed) {
        if (health == null || health.isAlive) {
            ForceRotateTowards(direction, speed);
        }
    }

    public void ForceRotateTowards(Vector3 direction, float speed) {
        Vector3 procesedDirection = new Vector3(direction.x, 0f, direction.z);
        //Debug.Log("Procesed Dir = " + procesedDirection + ", Raw dir = " + direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(procesedDirection), speed);
    }

    public void SetFeet(Vector3 pos) {
        Vector3 realCenter = transform.position + characterController.center;
        Vector3 feetPosition = new Vector3(realCenter.x, characterController.bounds.min.y, realCenter.z);
        transform.position = pos + (transform.position - feetPosition);
    }

    #endregion

    public enum State { OnGround, OnAir, OnLedge, Rolling, ClimbingLedge, Dead }
}

[System.Serializable]
public class CharacterStateInfo {
    public float forwardMove;
    public float forwardSpeed;
    public float rightMove;

    public bool grounded;
    public float groundHeight = float.MinValue;

    public CharacterStateInfo() {

    }

    public void UpdateToAnimator(Animator anim) {
        anim.SetFloat("forwardMove", forwardMove);
        anim.SetFloat("forwardSpeed", forwardSpeed);
        anim.SetFloat("rightMove", rightMove);
        anim.SetBool("grounded", grounded);
    }

    public void Reset() {
        forwardMove = 0f;
        forwardSpeed = 0f;
        rightMove = 0f;
    }
}

public delegate void Action();
public delegate void FloatAction(float n);
