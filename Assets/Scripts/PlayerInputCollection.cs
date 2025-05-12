using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputCollection : MonoBehaviour
{
    /*[Header("Running")]
    private bool running;

    [Header("Jumping")]
    private bool canJump;
    [SerializeField]
    private float jumpCooldown;
    [SerializeField]
    private float minJumpForce;
    [SerializeField]
    private float maxJumpForce;
    private float currentJump;

    [Header("Movement Inputs")]
    private float horInput;
    private float vertInput;

    [Header("Key Bindings")]
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    [SerializeField]
    private KeyCode runKey = KeyCode.LeftShift;

    private PlayerMovement player;

    public float HorInput
    {
        get { return horInput;  }
    }

    public float VertInput
    {
        get { return vertInput;  }
    }

    public bool Running
    {
        get { return running; }
    }

    public float CurrentJump
    {
        get { return currentJump; }
    }

    // Start is called before the first frame update
    void Start()
    {
        canJump = true;
        currentJump = minJumpForce;
        player = GetComponent<PlayerMovement>();
    }

    public void CollectInputs(bool grounded)
    {
        //Take in inputs
        horInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        //Jump Input Check
        if (Input.GetKey(jumpKey) && grounded && canJump)
        {
            if (currentJump < maxJumpForce)
            {
                currentJump += Time.deltaTime * player.JumpForce;
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

                player.Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        //Running Check
        if (Input.GetKey(runKey) && grounded)
        {
            running = true;
        }
        else
        {
            running = false;
        }
    }

    public void ResetJump()
    {
        currentJump = minJumpForce;
        canJump = true;
    }*/
}
