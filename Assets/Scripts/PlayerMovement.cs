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



    //Collects movement inputs
    private float horInput;
    private float vertInput;

    private Vector3 moveDirection;
    private Rigidbody rb;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        canJump = true;
        currentJump = minJumpForce;
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

        //Jump Input Check
        if(Input.GetKey(jumpKey) && grounded && canJump)
        {
            if (currentJump < maxJumpForce)
            {
                currentJump += Time.deltaTime * jumpForce;
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

        if(Input.GetKey(refreshStatsKey))
        {
            RefreshStats();
        }
    }

    private void MovePlayer()
    {
        //Calculate movemnt based on inputs
        moveDirection = orientation.forward * vertInput + orientation.right * horInput;

        if (grounded)
        {
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
        //Limit the speed based on movement state
        float contextSpeedCap = running ? runSpeed : speed;
        
        //Limits the max speed that the player can reach
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //Checks if speed execeeeds max
        if (flatVelocity.magnitude > contextSpeedCap)
        {
            //Sets speed to maxed out speed
            Vector3 limitedVelocity = flatVelocity.normalized * contextSpeedCap;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
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
}
