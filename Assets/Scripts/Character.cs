using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour {

    #region Variables

    [Header("Information")]
    public new string name;

    [Header("Combat")]
    public Transform lookTarget;

    [Header("Movement")]

    [Range(0.01f, 10f)]
    public float speed = 5f;
    public float rotationSpeed = 400f;

    [Range(1f, 10f)]
    public float jumpForce = 5f;

    public LayerMask mask;
    public float rollSpeed = 1f;

    [Header("Debug")]
    public float yForce = 0f, xForce = 0f;
    public Vector3 lookDirection;

    public CharacterState state;

    public LockState lockState;

    //Información del entorno
    public float groundHeight;

    //Triggers
    bool jump;

    //Events
    public Action OnJump;
    public Action OnHighFall;
    public Action OnAttack;
    public Action OnRoll;
    public Action OnStartBlocking;
    public Action OnStopBlocking;

    Vector3 moveVector;
    Vector3 inputVector;

    [HideInInspector]
    public CharacterController characterController;
    Animator animator;
    Health health;
    Stamina stamina;
    Equipment equipment;

    NavMeshAgent navMeshAgent;

    #endregion

    #region Monobehaviours

    void Awake() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        stamina = GetComponent<Stamina>();
        equipment = GetComponent<Equipment>();

        lookDirection = transform.forward;
    }

    void Start() {
        if (health != null) {

            //Establish what happens when the Character dies
            health.OnDeath += delegate () {
                lookTarget = null;
                state.dead = true;
            };

            //Establish what happens when the Character revives
            health.OnRevive += delegate () {
                state.dead = false;
            };

            //Establish what happens when the Character takes damage
            health.OnDamage += delegate (float damage) {
                if (!state.dead && state.grounded) {
                    animator.Play("Take Damage");
                }
            };
        }    
    }

    void Update() {

        //Save initial position to calculate velocity later
        Vector3 initPos = transform.position;

        //If governed by Navigation Mesh
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled) {
            if (navMeshAgent.isOnOffMeshLink) {
                state.grounded = false;
            } else {
                state.grounded = true;
            }
            state.forwardMove = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
            state.moveSpeed = navMeshAgent.velocity.magnitude;
        } 
        
        //If governed by Physics
        else {
            ApplyGravity();
            CheckForJump();
            CalculateMoveVector();

            //Move Unity's CharacterController and detect if it is grounded (it's necessary to move first, because Unity)
            characterController.Move(moveVector + inputVector * speed * Time.deltaTime);
            state.grounded = characterController.isGrounded;

            //Stick to slope if necessary
            StickToSlope();

            //If there is a Combat Target, change the Look Direction to it
            if ((health == null || health.isAlive) && lookTarget != null && !state.rolling) {
                Vector3 toTarget = lookTarget.position - transform.position;
                lookDirection = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
            }

            ForceRotateTowards(lookDirection, 5f);
            
            if (state.grounded) { state.grounded = true; } else { state.grounded = false; }
            state.moveSpeed = inputVector.magnitude * speed;

        }

        UpdateAnimator();

        //Calcular la velocidad
        state.velocity = transform.position - initPos;

        //Resetear todo lo que se tiene que resetear
        if (inputVector == Vector3.zero) { state.forwardMove = Mathf.MoveTowards(state.forwardMove, 0f, 2f * Time.deltaTime); }
        
        inputVector = Vector3.zero;
        moveVector = Vector3.zero;

        if(state.velocity == Vector3.zero && state.moving) {
            state.moving = false;
        }

        state.previouslyGrounded = state.grounded;
    }

    #endregion

    #region Private methods

    void ApplyGravity() {
        //Debug.Log("grounded=" + grounded + ", yForce=" + yForce);
        if (!state.grounded || yForce > 0) {
            yForce = yForce + (Physics2D.gravity.y * Time.deltaTime);
        } else {
            yForce = -0.1f;
            yForce = -1;
        }
    }

    void StickToSlope() {

        //Measure how high is the floor
        Vector3 start = transform.position + characterController.center + Vector3.up * characterController.height * 0.5f + Vector3.down * characterController.radius;
        RaycastHit hit; Physics.SphereCast(start, characterController.radius, Vector3.down, out hit, float.MaxValue, mask);
        float distanceToFloor = Mathf.Abs(hit.point.y - characterController.bounds.min.y);

        //Stick to floor if necessary (when going down slopes of stairs)
        if (distanceToFloor <= 0.5f && state.previouslyGrounded && !state.grounded && moveVector.y <= 0f) {
            //Debug.Break();
            transform.position = transform.position + Vector3.down * distanceToFloor;
            //moveVector += Vector3.down * distanceToFloor * 2f;
            state.grounded = true; state.grounded = true;
        }
    }

    void CalculateMoveVector() {
        moveVector = moveVector + new Vector3(0f, yForce * Time.deltaTime, 0f);
    }

    void CheckForJump() {
        if (jump) {
            yForce = jumpForce;
            state.grounded = false;
            jump = false;
        }
    }

    void UpdateAnimator() {
        state.UpdateToAnimator(animator);
    }

    #endregion

    #region Public methods

    public void Move(Vector3 direction, float speed) {
        if((health == null || health.isAlive) && lockState.canMove && !lockState.lockAll) {
            inputVector = direction.normalized * speed;

            //Debug.Log(Vector3.Angle(inputVector, transform.rotation * Vector3.forward));

            Vector3 i = transform.InverseTransformDirection(inputVector);
            //Vector3 i = transform.InverseTransformDirection(velocity);
            state.forwardMove = Mathf.MoveTowards(state.forwardMove, i.z, 2f * Time.deltaTime);
            state.rightMove = Mathf.MoveTowards(state.rightMove, i.x, 2f * Time.deltaTime);

            state.moving = true;
        }
    }

    public void Jump() {
        if((health == null || health.isAlive) && lockState.canJump && !lockState.lockAll) {
            if (state.grounded) {
                jump = true;
            }
        }
    }

    public bool Roll(Vector3 direction) {
        if (state.grounded && lockState.canRoll && !lockState.lockAll && (stamina == null || stamina.Consume(100f))) {
            lookDirection = direction;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            //animator.Play("Roll");
            animator.SetTrigger("roll");
            state.rolling = true;
            return true;
        } else {
            return false;
        }
    }

    public bool Attack() {
        Weapon equipedWeapon = equipment.equipedWeapon;
        if (equipment != null && !state.attacking && !state.blocking && !state.rolling && (stamina == null || stamina.HasAny())) {
            
            //Pick a random attack animation
            int attackAnimation = 0;
            attackAnimation = (int)(equipedWeapon).moves[Random.Range(0, (equipedWeapon).moves.Length)];
            animator.SetInteger("attackType", attackAnimation);
            animator.SetTrigger("attack");

            //Consume Stamina
            /*if (stamina != null) {
                stamina.Consume(equipedWeapon.damage);
            }*/

            return true;
        } else {
            return false;
        }
    }

    public bool StartBlocking() {
        if (state.grounded && !state.attacking && !state.rolling) {
            if (stamina == null || stamina.current >= 10f) {
                animator.SetBool("blocking", true);
                state.blocking = true;
            }
        }
        return true;
    }

    public bool StopBlocking() {
        animator.SetBool("blocking", false);
        state.blocking = false;
        return true;
    }

    public void RotateTowards(Vector3 direction, float speed) {
        if ((health == null || health.isAlive) && lockState.canRotate && !lockState.lockAll) {
            //ForceRotateTowards(direction, speed);
            lookDirection = direction;
        }
    }

    public void ForceRotateTowards(Vector3 direction, float speed) {
        Vector3 procesedDirection = new Vector3(direction.x, 0f, direction.z);
        //Debug.Log("Procesed Dir = " + procesedDirection + ", Raw dir = " + direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(procesedDirection), speed);
    }

    #endregion

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
