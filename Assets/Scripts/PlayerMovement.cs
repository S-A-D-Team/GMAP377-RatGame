using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
    private float jumpCooldown;
    [SerializeField]
    private float airMovement;
    private bool canJump;

    [Header("Key Bindings")]
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    [SerializeField]
    private KeyCode runKey = KeyCode.LeftShift;

    //Temp get/setter to access player attribute for a mutation (refer to GameManager.cs)
    public float JumpForce
    {
        get { return jumpForce; }
        set { jumpForce = value; }
    }
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
        //Subject to change but currently let's the Game Manager keep track of the player
        GameManager.Instance.RegisterPlayer(this);
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
            canJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
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
        //Limits the max speed that the player can reach
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //Checks if speed execeeeds max
        if (flatVelocity.magnitude > speed)
        {
            //Sets speed to maxed out speed
            Vector3 limitedVelocity = flatVelocity.normalized * speed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        //Makes sure y speed is 0
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }
}
