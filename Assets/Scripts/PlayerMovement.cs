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

    [Header("Ground Check")]
    [SerializeField]
    private float playerHeight;
    [SerializeField]
    private LayerMask whatIsGround;
    private bool grounded;

    [Header("Jumping")]
    [SerializeField]
    private float airMovement;
    [SerializeField]
    private float jumpForce;

    private PlayerInputCollection playerInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    //Temp get/setter to access player attribute for a mutation (refer to GameManager.cs)
    public float JumpForce
    {
        get { return jumpForce; }
        set { jumpForce = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //Subject to change but currently let's the Game Manager keep track of the player
        GameManager.Instance.RegisterPlayer(this);
        playerInput = GetComponent<PlayerInputCollection>();
    }

    // Update is called once per frame
    void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.01f, whatIsGround);

        playerInput.CollectInputs(grounded);
        SpeedControl();

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

    private void MovePlayer()
    {
        //Calculate movemnt based on inputs
        moveDirection = orientation.forward * playerInput.VertInput + orientation.right * playerInput.HorInput;

        if (grounded)
        {
            if (playerInput.Running)
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

    private void SpeedControl()
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

    public void Jump()
    {
        //Makes sure y speed is 0
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * playerInput.CurrentJump, ForceMode.Impulse);

        playerInput.ResetJump();
    }
}
