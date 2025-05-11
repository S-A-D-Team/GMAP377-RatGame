using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO/Consideration: Move player behavior unrelated to movement (such as biting) to separate script (PlayerAbilities.cs?), with each ability toggled by a mutation
//Extend controls for these to input handler and separately determine behavior in PlayerAbilities

public class PlayerMovement : MonoBehaviour
{
    private RatStats playerStats;
    //Ability flags
    private bool canBite;
    private bool canClimb;
    private float climbSpeed;

    //Resources that affect mobility stats/access
    private RatStats.hungerLevel hungerLevel = RatStats.hungerLevel.Content;
    private RatStats.hungerLevel previousHungerLevel;
    private float hunger;
    private float hungerPenalty;
    //Currently no way to affect this, likely to be tied to a mutation or other infection mechanic
    private float hungerTolerance;

    //TODO: Incorporate stamina costs into action checking
    //Likely to create a utility class for managing stamina drain/regen
    private float stamina;
    [Header("Stamina Costs Per Action")]
    [SerializeField]
    private float runStamDrain;
    [SerializeField]
    private float climbStamDrain;
    [SerializeField]
    private float biteStamDrain;

    [Header("Base Movement")]
    [SerializeField]
    [Tooltip("The base walking speed of the player")]
    private float speed;
    [SerializeField]
    private Transform orientation;
    [SerializeField]
    private float groundDrag;
    [SerializeField]
    [Tooltip("Max steepness of a ground traversible slope")]
    private float maxInclination = 45f;

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
    private bool wasGroundedLastFrame = false;

    [Header("Climb Check")]
    [SerializeField]
    private float playerWidth;
    private bool climbing = false;
    [SerializeField]
    private int climbCooldown = 10;

    [Header("Jumping")]
    [SerializeField]
    private float jumpChargeRate;
    [SerializeField]
    private float minJumpForce;
    [SerializeField]
    private float maxJumpForce;
    [SerializeField]
    private int jumpCooldown = 10;
    [SerializeField]
    private float airMovement;
    private bool canJump;
    private bool jumpStarted;
    private float currentJumpForce;
    private float lastJumped;

    [Header("Coyote Jumping")]
    [SerializeField]
    private float coyoteInterval = 0.15f;
    private float lastGrounded;


    //Collects movement inputs
    private PlayerInputCollection playerInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    private Vector3 groundNormal = Vector3.up;
    private Vector3 climbDirection;
    private Vector3 wallJumpDirection;
    private Vector3 jumpDirection = Vector3.up;

    //States determined by input and relative positioning of player

    //Movement away from surfaces
    //ShortHopping is only when grounded
    //FullHopping is only when still airborne and holding the jump key from ShortHopping
    //WallJumping is only when climbing
    private enum verticalAction
    {
        Idle,
        JumpSquatting,
        ShortHopping,
        FullHopping,
        WallJumping
    }
    
    //Movement along surfaces
    //Scaling is only when attempting to move vertically/through z-axis while climbing
    private enum lateralAction
    {
        Idle,
        Walking,
        Running,
        Scaling
    }

    //Grounded: On a flat/moderately steep surface
    //Climbing: On a surface within a certain steepness range, and climb has been activated
    //Airborne: No surfaces being touched
    private enum positionalState
    {
        Grounded,
        Climbing,
        Airborne
    }

    private lateralAction currentLateral;
    private verticalAction currentVertical;
    private positionalState currentPositional;

    private int debugStateChecks = 3;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputCollection>();
    }
    // Start is called before the first frame update
    void Start()
    {
        rb.freezeRotation = true;
        rb.useGravity = true;
        canJump = true;
        jumpStarted = false;
        /*
        currentLateral = lateralAction.Idle;
        currentVertical = verticalAction.Idle;
        currentPositional = positionalState.Grounded;
        */
        groundNormal = orientation.up;
        climbDirection = orientation.up;
        previousHungerLevel = hungerLevel;
        wallJumpDirection = -orientation.forward;

        GameManager.Instance.RegisterPlayer(gameObject);
        playerStats = GameManager.Instance.ratStats;
        //Initialize stats from rat stats
        RefreshStats();
    }

    //Updates values for the player stats
    public void RefreshStats()
    {
        speed = playerStats.walkSpeed;
        runSpeed = playerStats.runSpeed;
        airMovement = playerStats.airSpeed;
        groundDrag = playerStats.groundDrag;
        climbSpeed = playerStats.climbSpeed;
        jumpChargeRate = playerStats.jumpChargeRate;
        maxJumpForce = playerStats.maxJumpForce;
        
        hunger = playerStats.hunger;
        //Might have this refresh hunger to full instead for testing purposes
        hungerLevel = GetHungerLevel();
        previousHungerLevel = hungerLevel;
        hungerPenalty = playerStats.hungerPenalty;
        hungerTolerance = playerStats.hungerTolerance;

        stamina = playerStats.stamina;

        canBite = playerStats.canBite;
        canClimb = playerStats.canClimb;
    }

    private void Update()
    {
        if (playerInput.RefreshPressed)
        {
            RefreshStats();
        }
        //Update stats if a different hunger threshold is reached
        hungerLevel = GetHungerLevel();
        if (hungerLevel != previousHungerLevel)
        {
            AdjustStatsToHunger();
            previousHungerLevel = hungerLevel;
        }
        //Ascertain player intention based on input and position
        ActionStateUpdate();

    }


    private void FixedUpdate()
    {
        //For movement along surfaces up to a certain steepness
        CheckGround();
        //Ascertain player positioning relative to surfaces
        PositionalStateUpdate();
        if (debugStateChecks > 0)
        {
            Debug.Log(currentPositional);
            debugStateChecks--;
        }
        //Apply movement based on the above combination of surface checks, input, current positional state
        MovePlayer();
        //Adjust true movement to valid ranges
        SpeedControl();
    }

    private void AdjustStatsToHunger()
    {
        float truePenalty = hungerPenalty - hungerTolerance;
        //Starving - Full: Receive a discretely decaying stat modifier given default range of [0.25, 1.1] for 0 hunger tolerance
        //Ravenous: Receive an emergency positive stat modifier to aid in last stand attempts
        float statMultiplier = hungerLevel == RatStats.hungerLevel.Ravenous ? 1.25f : (int)hungerLevel / truePenalty;

        //Currently only affects directional input-based mobility, modifying the jumping might add too much complexity to level design given current jump height variability
        speed = playerStats.walkSpeed * statMultiplier;
        runSpeed = playerStats.runSpeed * statMultiplier;
        climbSpeed = playerStats.climbSpeed * statMultiplier;
        airMovement =  playerStats.airSpeed * statMultiplier;
    }

    //Determine hunger level based on what range actual hunger value falls under
    private RatStats.hungerLevel GetHungerLevel()
    {
        if (hunger > (int)RatStats.hungerLevel.Content && hunger <= (int)RatStats.hungerLevel.Full)
        {
            return RatStats.hungerLevel.Full;
        }
        else if (hunger > (int)RatStats.hungerLevel.Peckish && hunger <= (int)RatStats.hungerLevel.Content)
        {
            return RatStats.hungerLevel.Content;
        }
        else if (hunger > (int)RatStats.hungerLevel.Hungry && hunger <= (int)RatStats.hungerLevel.Peckish)
        {
            return RatStats.hungerLevel.Peckish;
        }
        else if (hunger > (int)RatStats.hungerLevel.Starving && hunger <= (int)RatStats.hungerLevel.Hungry)
        {
            return RatStats.hungerLevel.Hungry;
        }
        else if (hunger > (int)RatStats.hungerLevel.Ravenous && hunger <= (int)RatStats.hungerLevel.Starving)
        {
            return RatStats.hungerLevel.Starving;
        }
        else
        {
            return RatStats.hungerLevel.Ravenous;
        }
    }

    private void SpeedControl()
    {
        if (climbing)
        {
            //Damp the climbing speed slightly to reduce sliding
            //Z-axis velocity retained to account for climbing angled surfaces
            Vector3 climbVelocity = new Vector3(0f, rb.velocity.y * 0.9f, rb.velocity.z * 0.9f);
            float cappedYVelocity = Mathf.Clamp(climbVelocity.y, -climbSpeed, climbSpeed);
            float cappedZVelocity = Mathf.Clamp(climbVelocity.z, -climbSpeed, climbSpeed);
            rb.velocity = new Vector3(0f, cappedYVelocity, cappedZVelocity);
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

    private void OnLanding()
    {
        jumpStarted = false;
        canJump = true;
        currentVertical = verticalAction.Idle;

        StopCoroutine(FullHop());
    }

    IEnumerator HandleJump()
    {
        jumpStarted = true;
        ShortHop();

        //Make sure jump is held during jump squat to allow for full hop transition
        //Prevents situations where players press jump, release, and then quickly hold again within frame window leading to full hop
        //Currently 4-frame jump squat window
        float holdWindow = 0.075f;
        float offsetGravity = 4.25f;
        while (holdWindow > 0f)
        {
            if (!playerInput.JumpHeld)
            {
                yield break;
            }
            holdWindow -= Time.deltaTime;
            //Offer a small bit of floatiness during the short hop to smoothen the transition to full hop
            rb.AddForce(jumpDirection * offsetGravity, ForceMode.Acceleration);
            yield return null;
        }
        if (playerInput.JumpHeld)
        {
            currentVertical = verticalAction.FullHopping;
            Debug.Log("Full");
            StartCoroutine(FullHop());
        }
        else
        {
            currentVertical = verticalAction.ShortHopping;
        }

        StartCoroutine(JumpCooldown());
    }
    //jumpStarted determines whether the player should be checking for short hop from ground or extending full hop from air
    //Also prevents full hop behavior if airborne was entered without short hopping
    private void ShortHop()
    {
        bool canCoyoteJump = Time.time < lastGrounded + coyoteInterval;
        bool bufferedJump = playerInput.BufferedJump() && canJump;
        //Short hop, accounts for coyote and buffered timings
        //Always entered from grounded (or briefly after exiting it)
        if ((grounded || canCoyoteJump) && bufferedJump && canJump)
        {
            canJump = false;
            //Makes sure y speed is 0
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            jumpDirection = (Vector3.up + groundNormal).normalized;
            rb.AddForce(jumpDirection * minJumpForce, ForceMode.Impulse);
            //Not currently used but might be useful later to vary the jumping cooldown, prevent instant double jump(?), etc.
            lastJumped = Time.time;
        }
        
    }

    IEnumerator FullHop()
    {
        //Full hop up to variable, capped jump height
        //Always entered from short hop, ends at height cap or jump button release
        currentJumpForce = minJumpForce;
        Debug.Log(playerInput.JumpHeld);
        Debug.Log(currentJumpForce < maxJumpForce);
        Debug.Log(jumpStarted);
		while (playerInput.JumpHeld && currentJumpForce < maxJumpForce && jumpStarted)
		{
			float deltaForce = jumpChargeRate * Time.deltaTime;
			//Calculate the next force increment to apply with a capped jump height
			//Sanity checks the jump height while still allowing current to surpass the max to break sentinel value
			jumpDirection = (Vector3.up + groundNormal).normalized;
			//Increment direction is straight up to create a natural jump arc increase when jumping from angled surfaces
			rb.AddForce(jumpDirection * deltaForce, ForceMode.Impulse);
            currentJumpForce += deltaForce;
            if (!playerInput.JumpHeld)
            {
                break;
            }
            yield return null;
		}
        currentJumpForce = 0f;
    }
   
    private void WallJump()
    {
        climbing = false;
        rb.velocity = Vector3.zero;
        //Modify this to use some sort of variable for the kick off force and vertical force later
        Vector3 wallJumpForce = wallJumpDirection * (maxJumpForce / 2f) + Vector3.up * minJumpForce;
        rb.AddForce(wallJumpForce, ForceMode.Impulse);
        StartCoroutine(ClimbCooldown());
        StartCoroutine(JumpCooldown());
    }

    //Update grounded state and direction of grounded movement for sloped movement
    private void CheckGround() 
    {
        //Checks for both even and uneven ground
        Ray groundCheck = new(transform.position, Vector3.down);
        if (Physics.Raycast(groundCheck, out RaycastHit hit, playerHeight * 0.075f, whatIsGround))
        {
            //Check to see if the hit surface is too steep to traverse by ground movement
            grounded = Vector3.Angle(hit.normal, Vector3.up) <= maxInclination;
            groundNormal = hit.normal;
            lastGrounded = Time.time;
        }
        else
        {
            grounded = false;
        }

    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float distance = playerHeight * 0.075f;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * distance);
    }

    private bool NearClimbable()
    {
        float rayDistance = playerWidth * 0.5f + 0.01f;
        RaycastHit hit;

        //Checks both slightly behind and in front of player for climbables (currently needed given that the rat doesn't rotate)
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, orientation.forward, out hit, rayDistance) ||
            Physics.Raycast(transform.position + Vector3.up * 0.5f, -orientation.forward, out hit, rayDistance))
        {
            //Secondary check to make sure the object is even marked as climbable instead of using a Layer Mask
            if (hit.collider.TryGetComponent<ClimbableSurface>(out var _))
            {
                float contactAngle = Vector3.Angle(hit.normal, Vector3.up);
                //Surface must be sufficiently steep, climb must be unlocked (mutation) and not on cooldown
                bool validAngle = (contactAngle > maxInclination && contactAngle <= 90f) && canClimb;
                if (validAngle)
                {
                    wallJumpDirection = hit.normal;
                    //Inner cross product is a vector perpendicular to both Vector3.up and the surface normal
                    //This causes the outer cross product to produce a vector as far aligned upward along the way as possible
                    climbDirection = Vector3.Cross(hit.normal, Vector3.Cross(Vector3.up, hit.normal)).normalized;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    //Grab the actual directional input to affect the final movement direction along a surface
    private Vector3 GetInputDirection()
    {
        return (orientation.forward * playerInput.MoveInput.y + orientation.right * playerInput.MoveInput.x).normalized;
    }

    IEnumerator ClimbCooldown()
    {
        canClimb = false;
        yield return WaitForFrames(climbCooldown);
        canClimb = playerStats.canClimb;
    }

    IEnumerator JumpCooldown()
    {
        yield return WaitForFrames(jumpCooldown);
        
        while (!grounded && playerInput.JumpPressed)
        {
            yield return null;
        }
            canJump = true;
        
    }

    IEnumerator WaitForFrames(int frameWindow)
    {
        while (frameWindow > 0)
        {
            frameWindow--;
            yield return null;
        }
    }

    private void PositionalStateUpdate()
    {
        //Toggles climbing behavior as necessary, checks ground vs. air otherwise
        if (playerInput.ClimbPressed)
        {
            //Detach
            if (currentPositional == positionalState.Climbing)
            {
                climbing = false;
                StartCoroutine(ClimbCooldown());
                currentPositional = grounded ? positionalState.Grounded : positionalState.Airborne;

            }
            //Attach to climbable surfaces too steep to walk/run up, but no more than 90 degree, so long as climb is unlocked/off cooldown
            else if (NearClimbable() && canClimb)
            {
                climbing = true;
                currentPositional = positionalState.Climbing;
            }
        }
        //Auto-update as a defensive measure
        if (currentPositional != positionalState.Climbing)
        {
            currentPositional = grounded ? positionalState.Grounded : positionalState.Airborne;
        }

        //Wall jump and edge case where contact with climbable surface is lost
        if (currentPositional == positionalState.Climbing && (!NearClimbable() || currentVertical == verticalAction.WallJumping))
        {
            climbing = false;
            currentPositional = grounded ? positionalState.Grounded : positionalState.Airborne;
        }

        if (!wasGroundedLastFrame && grounded)
        {
            OnLanding();
        }
        wasGroundedLastFrame = grounded;
    }

    private void ActionStateUpdate()
    {
        switch (currentPositional)
        {
            case positionalState.Grounded:
                if (playerInput.MoveInput == Vector2.zero)
                {
                    currentLateral = lateralAction.Idle;
                }
                else
                {
                    if (playerInput.RunHeld)
                    {
                        currentLateral = lateralAction.Running;
                    }
                    else
                    {
                        currentLateral = lateralAction.Walking;
                    }
                }
                if (playerInput.JumpPressed && canJump)
                {
                    currentVertical = verticalAction.JumpSquatting;
                    StartCoroutine(HandleJump());
                }
                break;

            case positionalState.Climbing:

                currentVertical = verticalAction.Idle;
                currentLateral = lateralAction.Idle;
                
                if (playerInput.JumpPressed && canJump)
                {
                    currentVertical = verticalAction.WallJumping;
                    if (playerInput.MoveInput != Vector2.zero)
                    {
                        currentLateral = playerInput.RunHeld ? lateralAction.Running : lateralAction.Walking;
                    }
                }
                else if (playerInput.MoveInput.y > 0f)
                {
                    currentLateral = lateralAction.Scaling;
                }
                
                break;
            
            case positionalState.Airborne:
                if (playerInput.MoveInput == Vector2.zero)
                {
                    currentLateral = lateralAction.Idle;
                }
                else
                {
                    if (playerInput.RunHeld)
                    {
                        currentLateral = lateralAction.Running;
                    }
                    else
                    {
                        currentLateral = lateralAction.Walking;
                    }
                }
                break;
        }
    }

    //Later considerations: Also do stamina checking here before each intended action is taken
    //Act on player input intent based on position (Not all actions are available in all positions)
    private void MovePlayer()
    {
        rb.drag = grounded ? groundDrag : 0f;
        rb.useGravity = !climbing;

        switch (currentPositional)
        {
            case positionalState.Climbing:
                if (currentLateral == lateralAction.Scaling)
                {
                    //Can only climb vertically, not horizontally
                    moveDirection = climbDirection * playerInput.MoveInput.y;
                    rb.AddForce(moveDirection.normalized * climbSpeed * 3f, ForceMode.Force);
                }
                if (currentVertical == verticalAction.WallJumping)
                {
                    WallJump();
                }
                break;
            
            case positionalState.Grounded:
                if (currentLateral == lateralAction.Walking)
                {
                    running = false;
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), groundNormal).normalized;
                    rb.AddForce(moveDirection * speed * 3f, ForceMode.Force);
                }
                else if (currentLateral == lateralAction.Running)
                {
                    running = true;
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), groundNormal).normalized;
                    rb.AddForce(moveDirection * runSpeed * 3f, ForceMode.Force);
                }
                else
                {
                    running = false;
                }
                
                break;
            
            case positionalState.Airborne:
                if (currentLateral == lateralAction.Walking)
                {
                    running = false;
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), groundNormal).normalized;
                    rb.AddForce(moveDirection * airMovement * 3f, ForceMode.Force);
                }
                else if (currentLateral == lateralAction.Running)
                {
                    running = true;
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), groundNormal).normalized;
                    rb.AddForce(moveDirection * (airMovement * 1.25f * 3f), ForceMode.Force);
                }
                else
                {
                    running = false;
                }
                break;

        }
    }

    //TENTATIVE STAM FUNCTIONS
    //Make sure player has the stamina to use an intended action
    private bool hasStamina(float stamCost)
    {
        return stamina - stamCost >= 0;
    }

    //Slap this at the end of any code block implementing a stamina draining behavior
    private void useStamina(float stamCost)
    {
        stamina -= stamCost;
    }
}
