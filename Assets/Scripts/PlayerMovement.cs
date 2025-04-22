using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private RatStats playerStats;
    //Ability flags
    private bool canBite;
    private bool canClimb;

    private float climbSpeed;

    [Header("Base Movement")]
    [SerializeField]
    [Tooltip("The base walking speed of the player")]
    private float speed;
    [SerializeField]
    private Transform orientation;
    [SerializeField]
    private float groundDrag;

    [Header("Running")]
    [SerializeField]
    private float runSpeed;
    private bool running;

    [Header("Ground Check")]
    [SerializeField]
    private float playerHeight;
    [SerializeField]
    private LayerMask whatIsGround;
    private bool grounded;

    [Header("Climb Check")]
    [SerializeField]
    private float playerWidth;
    [SerializeField]
    private LayerMask whatIsClimbable;
    private bool climbing;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float minJumpForce;
    [SerializeField]
    private float maxJumpForce;
    [SerializeField]
    private float jumpCooldown;
    [SerializeField]
    private float airMovement;
    private bool canJump;
    private float currentJump;

    [Header("Key Bindings")]
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    [SerializeField]
    private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode refreshStatsKey = KeyCode.R;
    [SerializeField]
    private KeyCode climbKey = KeyCode.F;




    //Collects movement inputs
    private float horInput;
    private float vertInput;

    private Vector3 moveDirection;
    private Vector3 jumpDirection;
    private Rigidbody rb;

    private Vector3 groundDirection;
    private Vector3 groundJumpDirection;
    private Vector3 climbDirection;
    private Vector3 wallJumpDirection;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        canJump = true;
        currentJump = minJumpForce;
        groundDirection = orientation.forward;
        climbDirection = orientation.up;
        groundJumpDirection = orientation.up;
        wallJumpDirection = -orientation.forward;
        GameManager.Instance.RegisterPlayer(gameObject);
        playerStats = GameManager.Instance.ratStats;
        //Initialize stats from rat stats
        RefreshStats();
    }

    //Initializes or overrides any editor values for the player stats (useful for sanity checking)
    private void RefreshStats()
    {
        speed = playerStats.walkSpeed;
        runSpeed = playerStats.runSpeed;
        groundDrag = playerStats.groundDrag;
        climbSpeed = playerStats.climbSpeed;
        maxJumpForce = playerStats.maxJumpForce;

        canBite = playerStats.canBite;
        canClimb = playerStats.canClimb;

    }

    // Update is called once per frame
    void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.01f, whatIsGround);

        CollectInputs();
        SPeedControl();

        //Apply movement drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void CollectInputs()
    {
        //Take in inputs
        horInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        //Wall Jump Check
        if (Input.GetKeyDown(jumpKey) && climbing && canJump)
        {
            canJump = false;

            WallJump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        //Jump Input Check
        else if(Input.GetKey(jumpKey) && grounded && canJump)
        {
            if (currentJump < maxJumpForce)
            {
                currentJump += Time.deltaTime * jumpForce;
                Mathf.Clamp(currentJump, minJumpForce, maxJumpForce);
            }
            else
            {
                currentJump = maxJumpForce;
            }
        }
        //Check if jump key has been pressed, jump if so.
        else
        {
            if (currentJump > minJumpForce)
            {
                canJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        //Running Check
        if(Input.GetKey(runKey) && grounded)
        {
            running = true;
        }
        else
        {
            running = false;
        }

        if(Input.GetKeyDown(refreshStatsKey))
        {
            RefreshStats();
        }

        if (Input.GetKeyDown(climbKey) && canClimb)
        {
            //Should probably also adjust the raycast for climbing later, this currently just mirrors the ground raycast
            climbing = Physics.Raycast(transform.position, orientation.forward, playerWidth * 0.5f + 0.01f, whatIsClimbable);
        }
    }

    private void MovePlayer()
    {
        //Calculate movemnt based on inputs
        moveDirection = orientation.forward * vertInput + orientation.right * horInput;
        rb.useGravity = !climbing;

        if (climbing)
        {
            //Can only climb vertically, not horizontally
            moveDirection = climbDirection * vertInput;
            rb.AddForce(moveDirection.normalized * climbSpeed * 3f, ForceMode.Force);
        }
        else if (grounded)
        {
            moveDirection = groundDirection * vertInput + orientation.right * horInput;
            if (running)
            {
                rb.AddForce(moveDirection.normalized * runSpeed * 3f, ForceMode.Force);
            }
            else
            {
                rb.AddForce(moveDirection.normalized * speed * 3f, ForceMode.Force);
            }
        }
        
        else
        {
            rb.AddForce(moveDirection.normalized * speed * 3f * airMovement, ForceMode.Force);
        }
    }

    private void SPeedControl()
    {
        if (climbing)
        {
            //Damp the climbing speed slightly to reduce sliding
            Vector3 climbVelocity = new Vector3(0f, rb.velocity.y * 0.9f, 0f);
            float cappedYVelocity = Mathf.Clamp(climbVelocity.y, -climbSpeed, climbSpeed);
            rb.velocity = new Vector3(0f, cappedYVelocity, 0f);
        }
        else
        {
            //Limit the speed based on movement state
            float contextSpeedCap = running ? runSpeed : speed;

            //Limits the max speed that the player can reach
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //Checks if speed exceeds max
            if (flatVelocity.magnitude > contextSpeedCap)
            {
                //Sets speed to maxed out speed
                Vector3 limitedVelocity = flatVelocity.normalized * contextSpeedCap;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void Jump()
    {
        //Makes sure y speed is 0
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * currentJump, ForceMode.Impulse);

        currentJump = minJumpForce;
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void WallJump()
    {
        climbing = false;
        rb.velocity = Vector3.zero;
        //Modify this to use some sort of variable for the kick off force and vertical force later
        Vector3 wallJumpForce = -transform.forward * (maxJumpForce / 2f) + Vector3.up * minJumpForce;
        rb.AddForce(wallJumpForce, ForceMode.Impulse);
        StartCoroutine(ClimbCooldown());
    }

    IEnumerator ClimbCooldown()
    {
        canClimb = false;
        yield return new WaitForSeconds(0.25f);
        canClimb = playerStats.canClimb;
    }
}
