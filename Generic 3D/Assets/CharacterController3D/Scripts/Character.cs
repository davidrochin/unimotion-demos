using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[SelectionBase]
[RequireComponent(typeof (CharacterController))]
public class Character : MonoBehaviour {

    public float walkSpeed = 2f;
    public float runMultiplier = 2f;
    public float jumpForce = 0.17f;
    public LayerMask mask;

    public bool grounded = false;
    public bool walking = false;
    public bool running = false;
    float yForce = 0f, xForce = 0f;

    //Triggers
    bool jump;

    Vector3 moveVector;
    Vector3 inputVector;

    CharacterController characterController;
    Animator animator;

	void Awake () {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
	}
	
	void Update () {
        CheckGrounded();
        ApplyGravity();
        CheckForJump();
        CalculateMoveVector();

        characterController.Move(moveVector + inputVector * walkSpeed * Time.deltaTime);
        moveVector = Vector3.zero;
        inputVector = Vector3.zero;

        UpdateAnimator();
    }

    void ApplyGravity() {
        if (!grounded) {
            yForce = yForce + (Physics2D.gravity.y * Time.deltaTime * 0.03f);
        } else {
            yForce = 0f;
        }
    }

    void CheckGrounded() {
        RaycastHit hit = RaycastPastItself(transform.position + characterController.center, Vector3.down, characterController.height / 2f + 0.05f, mask);
        if (hit.collider != null) {
            grounded = true;
        } else {
            grounded = false;
        }
    }

    void CalculateMoveVector() {
        //Sumarle la gravedad
        moveVector = moveVector + new Vector3(0f, yForce, 0f);
    }

    void CheckForJump() {
        if (jump) {
            yForce = jumpForce;
            grounded = false;
            jump = false;
        }
    }

    void UpdateAnimator() {

        animator.SetBool("walking", walking);
        animator.SetBool("running", running);
        animator.SetBool("grounded", grounded);

        walking = false; running = false;
    }

    //Acciones
    public void Walk(Vector3 direction) {
        inputVector = inputVector + direction;

        if (direction != Vector3.zero) {
            RotateTowardsDirection(direction);
            walking = true;
        }
        
    }

    public void Run(Vector3 direction) {
        direction = direction * runMultiplier;
        Walk(direction);

        if(direction != Vector3.zero) {
            running = true;
        }
        
    }

    public void Jump() {
        jump = true;
    }

    public void RotateTowardsDirection(Vector3 direction) {
        Vector3 procesedDirection = new Vector3(direction.x, 0f, direction.z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(procesedDirection), 400f * Time.deltaTime);
    }

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
}
