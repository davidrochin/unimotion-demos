using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour {

    #region Variables

    [Header("Information")]
    public new string name;
    public Faction faction;

    [Header("Combat")]
    public Transform combatTarget;
    public CombatStateInfo combatState;

    [Header("Movement")]

    [Range(0.01f, 10f)]
    public float speed = 5f;
    public float rotationSpeed = 400f;

    [Range(1f, 10f)]
    public float jumpForce = 5f;

    public LayerMask mask;
    public float rollSpeed = 1f;

    [Header("Debug")]
    public bool grounded = false;
    public bool previousGrounded = false;
    public bool walking = false;
    public bool running = false;
    public bool rolling = false;
    public float yForce = 0f, xForce = 0f;
    public Vector3 lookDirection;
    public Vector3 velocity;

    public CharacterStateInfo stateInfo;

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
                combatTarget = null;
                combatState.isDead = true;
            };

            //Establish what happens when the Character revives
            health.OnRevive += delegate () {
                combatState.isDead = false;
            };

            //Establish what happens when the Character takes damage
            health.OnDamage += delegate (float damage) {
                if (!combatState.isDead && grounded) {
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
                stateInfo.grounded = false;
            } else {
                stateInfo.grounded = true;
            }
            stateInfo.forwardMove = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
            stateInfo.forwardSpeed = navMeshAgent.velocity.magnitude;
        } 
        
        //If governed by Physics
        else {
            ApplyGravity();
            CheckForJump();
            CalculateMoveVector();

            //Move Unity's CharacterController and detect if it is grounded (it's necessary to move first, because Unity)
            characterController.Move(moveVector + inputVector * speed * Time.deltaTime);
            grounded = characterController.isGrounded;

            //Stick to slope if necessary
            StickToSlope();

            //If there is a Combat Target, change the Look Direction to it
            if ((health == null || health.isAlive) && combatTarget != null && !combatState.isRolling) {
                Vector3 toTarget = combatTarget.position - transform.position;
                lookDirection = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
            }

            ForceRotateTowards(lookDirection, 25f);
            
            if (grounded) { stateInfo.grounded = true; } else { stateInfo.grounded = false; }
            stateInfo.forwardSpeed = inputVector.magnitude * speed;

        }

        UpdateAnimator();

        //Resetear todo lo que se tiene que resetear
        if (inputVector == Vector3.zero) { stateInfo.forwardMove = Mathf.MoveTowards(stateInfo.forwardMove, 0f, 2f * Time.deltaTime); }
        
        inputVector = Vector3.zero;
        moveVector = Vector3.zero;

        //Calcular la velocidad
        velocity = transform.position - initPos;

        previousGrounded = grounded;
    }

    #endregion

    #region Private methods

    void ApplyGravity() {
        //Debug.Log("grounded=" + grounded + ", yForce=" + yForce);
        if (!grounded || yForce > 0) {
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
        if (distanceToFloor <= 0.5f && previousGrounded && !grounded && moveVector.y <= 0f) {
            //Debug.Break();
            transform.position = transform.position + Vector3.down * distanceToFloor;
            //moveVector += Vector3.down * distanceToFloor * 2f;
            grounded = true; stateInfo.grounded = true;
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

    #region Public methods

    public void Move(Vector3 direction, float speed) {
        if((health == null || health.isAlive) && lockState.canMove && !lockState.lockAll) {
            inputVector = direction.normalized * speed;
            Vector3 i = transform.InverseTransformDirection(inputVector);
            stateInfo.forwardMove = Mathf.MoveTowards(stateInfo.forwardMove, i.z, 2f * Time.deltaTime);
            //stateInfo.forwardMove = i.z;
            stateInfo.rightMove = i.x;
        }
    }

    public void Jump() {
        if((health == null || health.isAlive) && lockState.canJump && !lockState.lockAll) {
            if (grounded) {
                jump = true;
            }
        }
    }

    public bool Roll(Vector3 direction) {
        if (grounded && lockState.canRoll && !lockState.lockAll && (stamina == null || stamina.Consume(100f))) {
            lookDirection = direction;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            //animator.Play("Roll");
            animator.SetTrigger("roll");
            combatState.isRolling = true;
            return true;
        } else {
            return false;
        }
    }

    public bool Attack() {
        Weapon equipedWeapon = equipment.equipedWeapon;
        if (equipment != null && !combatState.isAttacking && !combatState.isBlocking && !combatState.isRolling && (stamina == null || stamina.HasAny())) {
            
            //Pick a random attack animation
            int attackAnimation = 0;
            attackAnimation = (int)(equipedWeapon).moves[Random.Range(0, (equipedWeapon).moves.Length)];
            animator.SetInteger("attackType", attackAnimation);
            animator.SetTrigger("attack");

            //Consume Stamina
            if (stamina != null) {
                stamina.Consume(equipedWeapon.damage);
            }

            return true;
        } else {
            return false;
        }
    }

    public bool StartBlocking() {
        if (grounded && !combatState.isAttacking && !combatState.isRolling) {
            if (stamina == null || stamina.current >= 10f) {
                animator.SetBool("blocking", true);
                combatState.isBlocking = true;
            }
        }
        return true;
    }

    public bool StopBlocking() {
        animator.SetBool("blocking", false);
        combatState.isBlocking = false;
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

public enum Faction { Human, Hollow }

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

[System.Serializable]
public class CombatStateInfo {

    public bool isBlocking = false;
    public bool isAttacking = false;
    public bool isRolling = false;
    public bool isDead = false;
}

public delegate void Action();
public delegate void FloatAction(float n);
