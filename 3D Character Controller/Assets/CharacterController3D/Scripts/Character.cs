using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[SelectionBase]
[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour {

    public State state;

    [Header("Walking and Running")]
    public float walkSpeed = 2f;
    public float runMultiplier = 2f;
    public float jumpForce = 0.17f;
    public LayerMask mask;

    [Header("Rolling")]
    public float rollForwardSpeed = 1f;

    [Header("Debug")]
    public bool grounded = false;
    public bool walking = false;
    public bool running = false;
    public bool climbing = false;
    public float yForce = 0f, xForce = 0f;

    //Estados
    CharacterState currentState;
    CharacterState previousState;

    //Información del entorno
    public float groundHeight;

    //Triggers
    bool jump;

    Vector3 moveVector;
    Vector3 inputVector;

    CharacterController characterController;
    Animator animator;
    LedgeGrabber ledgeGrabber;

    void Awake() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        ledgeGrabber = GetComponent<LedgeGrabber>();
    }

    void Update() {

        UpdateAnimator();

        switch (state) {
            //SUELO O AIRE
            case State.OnGround:
            case State.OnAir: {
                    CheckGroundHeight();
                    ApplyGravity();
                    CheckForJump();
                    CalculateMoveVector();

                    //Mover el CharacterController y detectar si está en el piso (es necesario moverlo primero por cosas de Unity)
                    characterController.Move(moveVector + inputVector * walkSpeed * Time.deltaTime);
                    grounded = characterController.isGrounded;
                    if (grounded) { state = State.OnGround; } else { state = State.OnAir; }

                    //Si el personaje está en el aire y tratando de moverse
                    if (state == State.OnAir && inputVector != Vector3.zero && yForce <= 0f) {
                        ledgeGrabber.GetClosestLedgePoint();
                        if (ledgeGrabber.inLedgeRange) {
                            transform.position = ledgeGrabber.closestLedgePoint - transform.rotation * ledgeGrabber.grabPoint;
                            state = State.OnLedge;
                        }
                    }
                    break;
                }

            //AGARRADO DE UNA LADERA
            case State.OnLedge: {
                    
                    //Moverse por la ladera de ser necesario
                    if(ledgeMovingDirection != Vector3.zero) {
                        if (ledgeMovingDirection == Vector3.right) { moveVector = Quaternion.Euler(0f, 90f, 0f) * transform.forward; }
                        if (ledgeMovingDirection == Vector3.left) { moveVector = Quaternion.Euler(0f, -90f, 0f) * transform.forward; }
                        Debug.Log(ledgeMovingDirection + ", " + moveVector);
                        characterController.Move(moveVector * 0.8f * Time.deltaTime);
                        ledgeMovingDirection = Vector3.zero;
                        ledgeGrabber.closestLedgePoint = LedgeGrabber.ProjectOnLedgeEdge(transform.position, ledgeGrabber.ledgeEdge);
                    }

                    //Rotar hacia la ladera
                    ForceRotateTowards((LedgeGrabber.ProjectOnLedgeEdge(transform.position, ledgeGrabber.ledgeEdge) - transform.position).normalized);

                    //Pegar el punto de agarrado a la ladera
                    transform.position = ledgeGrabber.closestLedgePoint - transform.rotation * ledgeGrabber.grabPoint;

                    //CheckForJump();
                    if (jump) {
                        climbing = true;
                    }
                    break;
                }

            //ESCALANDO UN BORDE
            case State.ClimbingLedge: {
                    climbTimer += Time.deltaTime;
                    characterController.enabled = false;

                    //Revisar si ya se acabó la animación
                    if (climbTimer >= animator.GetCurrentAnimatorStateInfo(0).length) {
                        state = State.OnGround;

                        CheckGroundHeight();
                        if (groundHeight != float.MinValue) {
                            SetFeet(new Vector3(transform.position.x, groundHeight, transform.position.z));
                        }
                        characterController.enabled = true;
                    }
                    break;
                }

            //RODANDO
            case State.Rolling: {
                    rollTimer += Time.deltaTime;

                    CheckGroundHeight();
                    ApplyGravity();
                    CalculateMoveVector();

                    moveVector += transform.forward * rollForwardSpeed * Time.deltaTime;
                    inputVector = Vector3.zero;

                    //Mover el CharacterController y detectar si está en el piso (es necesario moverlo primero por cosas de Unity)
                    characterController.Move(moveVector + inputVector * walkSpeed * Time.deltaTime);
                    grounded = characterController.isGrounded;
                    if (grounded) { state = State.Rolling; } else { state = State.OnAir; }

                    //Revisar si ya se acabó la animacion
                    if (rollTimer >= animator.GetCurrentAnimatorStateInfo(0).length) {
                        state = State.OnGround;
                    }
                    break;
                }
        }

        moveVector = Vector3.zero;
        inputVector = Vector3.zero;
    }

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

        //Detectar la altura del piso
        RaycastHit hit = RaycastPastItself(transform.position + characterController.center, Vector3.down, characterController.height / 2f + 4f, mask);
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

        /*Debido a la cantidad de estados que planeo implementar en el animator, decidí controlar
         las transiciones desde el script. Aun así uso un animator porque creo que hace la tarea
         de cambiar animaciones más facil. En el Animator hay varios parametros booleanos, pero
         solo uno puede estar activo a la vez; de esta manera, activo la animación que yo quiera
         desde script.*/

        //Poner todos los bools de animator a false
        SetAllAnimatorBoolsToFalse();

        switch (state) {
            case State.OnGround: {
                    animator.applyRootMotion = true;
                    if (running) {
                        animator.SetBool("running", true);
                    } else if (walking) {
                        animator.SetBool("walking", true);
                    } else {
                        animator.SetBool("idle", true);
                    }
                    break;
                }
            case State.OnAir: {
                    animator.applyRootMotion = false;
                    animator.SetBool("falling", true);
                    break;
                }
            case State.OnLedge: {
                    animator.applyRootMotion = false;
                    if (ledgeMovingDirection == Vector3.zero) {
                        animator.SetBool("hanging", true);
                        animator.SetFloat("ledgeMovingDirection", 0f);
                    } 
                    else {
                        animator.SetBool("movingAlongEdge", true);
                        animator.SetFloat("ledgeMovingDirection", ledgeMovingDirection.x);
                    }
                    break;
                }
            case State.ClimbingLedge: {
                    animator.applyRootMotion = true;
                    animator.SetBool("climbing", true);
                    break;
                }
            case State.Rolling: {
                    animator.applyRootMotion = false;
                    animator.SetBool("rolling", true);
                    break;
                }
        }

        walking = false; running = false;
    }

    void SetAllAnimatorBoolsToFalse() {
        for (int x = 0; x < animator.parameterCount; x++) {
            if (animator.parameters[x].type == AnimatorControllerParameterType.Bool) {
                animator.SetBool(animator.parameters[x].nameHash, false);
            }
        }
    }

    #region Acciones Básicas

    public void Walk(Vector3 direction) {
        inputVector = inputVector + direction;

        if (direction != Vector3.zero) {
            RotateTowards(direction);
            walking = true;
        }
    }

    public void Run(Vector3 direction) {
        direction = direction * runMultiplier;
        Walk(direction);

        if (direction != Vector3.zero) {
            running = true;
        }
    }

    public void Jump() {
        if (state == State.OnGround) {
            jump = true;
            state = State.OnAir;
        }
    }

    float climbTimer = 0f;
    public void Climb() {
        if (state == State.OnLedge) {
            state = State.ClimbingLedge;
            climbTimer = 0f;
        }
    }

    float rollTimer = 0f;
    public void Roll() {
        if (state == State.OnGround) {
            state = State.Rolling;
            rollTimer = 0f;
        }
    }

    public void RotateTowards(Vector3 direction) {
        if (state == State.OnGround || state == State.OnAir) {
            ForceRotateTowards(direction);
        }
    }

    public void ForceRotateTowards(Vector3 direction) {
        Vector3 procesedDirection = new Vector3(direction.x, 0f, direction.z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(procesedDirection), 400f * Time.deltaTime);
    }

    public void SetFeet(Vector3 pos) {
        Vector3 realCenter = transform.position + characterController.center;
        Vector3 feetPosition = new Vector3(realCenter.x, characterController.bounds.min.y, realCenter.z);
        transform.position = pos + (transform.position - feetPosition);
    }

    Vector3 ledgeMovingDirection;
    public void LedgeMove(Vector3 direction) {
        ledgeMovingDirection = direction;
    }

    #endregion

    //Este metodo es para hacer un Raycast ignorando el colisionador de este mismo objeto
    RaycastHit RaycastPastItself(Vector3 startPos, Vector3 direction, float lenght, LayerMask mask) {
        RaycastHit[] rayHits = Physics.RaycastAll(startPos, direction, lenght, mask);
        foreach (RaycastHit hit in rayHits) {
            if (hit.collider.gameObject != gameObject) {
                return hit;
            }
        }
        return new RaycastHit();
    }

    public enum State { OnGround, OnAir, OnLedge, Rolling, ClimbingLedge, Dead }
}

public class CharacterState {
    public bool grounded = false;
    public float groundHeight = float.MinValue;

    public CharacterState() {

    }
}
