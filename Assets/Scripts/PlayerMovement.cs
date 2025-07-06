using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private RatStats.hungerLevel hungerLevel;
    private float hungerPenalty;
    //Currently no way to affect this, likely to be tied to a mutation or other infection mechanic
    private float hungerTolerance;
    //Set true when Ravenous to enable specialized behavior
    private bool tooHungryToCare = false;

    //TODO: Incorporate stamina costs into action checking
    //Likely to create a utility class for managing stamina drain/regen
    private float stamina;
    private float staminaRegen;
    private float maxStamina;
    private float regenDelay;
    private float lastStamUse;
    [Header("Stamina Costs Per Action")]
    [SerializeField]
    private float runStamDrain = 0.1f;
    [SerializeField]
    private float climbStamDrain = 0.1f;
    [SerializeField]
    private float biteStamDrain = 0.25f;

    [Header("Base Movement")]
    [SerializeField]
    [Tooltip("The base walking speed of the player")]
    public float speed;
    [SerializeField]
    private Transform orientation;
    [SerializeField]
    private float groundDrag;
    [SerializeField]
    [Tooltip("Max steepness of a ground traversible slope")]
    private float maxInclination = 45f;

    [Header("Running")]
    [SerializeField]
    public float runSpeed;
    private bool running;

    [Header("Ground Check")]
    [SerializeField]
    private LayerMask whatIsGround;
    private bool grounded;
    private bool wasGroundedLastFrame = true;

    [Header("Climb Check")]
    private bool climbing = false;
    [SerializeField]
    private int climbCooldown = 90;
    private bool climbToggleQueued = false;

    [Header("Jumping")]
    [SerializeField]
    private float jumpChargeRate;
    [SerializeField]
    public float minJumpForce;
    [SerializeField]
    public float maxJumpForce;
    [SerializeField]
    private int jumpCooldown = 10;
    [SerializeField]
    [Tooltip("Affects wall jump height")]
    private float verticalBounce = 0.75f;
    [SerializeField]
    [Tooltip("Affects push from wall")]
    private float horizontalBounce = 6f;
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

    [Header("State Materials")]
    [SerializeField]
    private PhysicMaterial noFriction;
    [SerializeField]
    private PhysicMaterial idleFriction;

    //Collects movement inputs
    private PlayerInputCollection playerInput;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private BoxCollider ratBox;
    private Vector3 groundNormal = Vector3.up;
    private Vector3 climbDirection;
    private Vector3 wallJumpDirection;
    private Vector3 attachDirection;
    private Vector3 jumpDirection = Vector3.up;

    [Space]
    [Header("RandomSpawn")]
    [SerializeField]
    private List<Transform> spawnAreas;

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

    //private int debugStateChecks = 3;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputCollection>();
        ratBox = GetComponent<BoxCollider>();
    }
    // Start is called before the first frame update
    void Start()
    {
        rb.freezeRotation = true;
        rb.useGravity = true;
        canJump = true;
        jumpStarted = false;
        climbing = false;
        groundNormal = orientation.up;
        climbDirection = orientation.up;
        wallJumpDirection = -orientation.forward;
        attachDirection = orientation.forward;
        

        GameManager.Instance.RegisterPlayer(gameObject);
        playerStats = GameManager.Instance.ratStats;
        //Initialize stats from rat stats
        RefreshStats();
    }

    //called at the end of tutorial
    public void spawnRandom()
    {
        if (spawnAreas.Count > 0)
        {
            transform.position = spawnAreas[Random.Range(0, spawnAreas.Count - 1)].position;
        }
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
        //Might have this refresh hunger to full instead for testing purposes
        hungerPenalty = playerStats.hungerPenalty;
        hungerTolerance = playerStats.hungerTolerance;
        hungerLevel = playerStats.currentHungerLevel;
        
        staminaRegen = playerStats.stamRegen;
        stamina = playerStats.stamina;
        maxStamina = playerStats.staminaCap;
        regenDelay = playerStats.stamRegenDelay;


        canBite = playerStats.canBite;
        canClimb = playerStats.canClimb;
    }

    private void Update()
    {
        if (playerInput.RefreshPressed)
        {
            RefreshStats();
        }
        //Ascertain player intention based on input and position
        ActionStateUpdate();
        HandleClimbToggle();

        //Ignore any stamina interactions if ravenous - it is essentially infinite in this state
        if (!tooHungryToCare)
        {
            //Apply any stamina regeneration after a delay from last stamina consuming action
            if (!running && !climbing && Time.time - lastStamUse >= regenDelay && stamina < maxStamina)
            {
                regenStamina();
            }
            else if (running)
            {
                useStamina(runStamDrain * Time.deltaTime);
            }
            else if (climbing)
            {
                useStamina(climbStamDrain * Time.deltaTime);
            }
        }

        UIManager.Instance.showClimb(climbing);

        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(GameManager.Instance.winTheGame());
        }
    }


    private void FixedUpdate()
    {
        //For movement along surfaces up to a certain steepness
        CheckGround();
        //Smoothen forces on the player in an air-to-ground transition, reduces slipperyness and bounciness
        bool isLanding = !wasGroundedLastFrame && grounded;
        if (isLanding)
        {
            OnLanding();
        }
        //Frame-sensitive state information
        wasGroundedLastFrame = grounded;
        if (climbToggleQueued)
        {
            climbToggleQueued = false;
            TryClimbToggle();

        }
        //Ascertain player positioning relative to surfaces
        PositionalStateUpdate();
        //Apply movement based on the above combination of surface checks, input, current positional state
        MovePlayer();
        //Adjust true movement to valid ranges
        SpeedControl();
        //Swap player material depending on physics needs
        MaterialUpdate();
    }

    public void AdjustStatsToHunger()
    {
        float truePenalty = hungerPenalty - hungerTolerance;
        //Starving - Full: Receive a discretely decaying stat modifier given default range of [0.25, 1.1] for 0 hunger tolerance
        //Ravenous: Receive an emergency positive stat modifier to aid in last stand attempts
        float statMultiplier = hungerLevel == RatStats.hungerLevel.Ravenous ? 1.25f : (int)hungerLevel / truePenalty;

        //Enable stamina bypassing behavior when near death
        tooHungryToCare = hungerLevel == RatStats.hungerLevel.Ravenous;
        Debug.Log("Too hungry to care? " + tooHungryToCare);

        //Currently only affects directional input-based mobility, modifying the jumping might add too much complexity to level design given current jump height variability
        speed = playerStats.walkSpeed * statMultiplier;
        runSpeed = playerStats.runSpeed * statMultiplier;
        climbSpeed = playerStats.climbSpeed * statMultiplier;
        airMovement =  playerStats.airSpeed * statMultiplier;
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

            //Adjust air handling based on speed
            //Harder to steer at high speed, more precise at lower speed
            if (!grounded)
            {
                Vector3 airDrift = GetInputDirection();
                float lateralSpeed = flatVelocity.magnitude;

                float airControl = Mathf.Lerp(1.2f, 0.6f, lateralSpeed / runSpeed);

                float adjustedHandling = airControl * (running ? 0.9f : 1.1f);

                Vector3 adjustedAirForce = airDrift * airMovement * adjustedHandling;
                rb.AddForce(adjustedAirForce, ForceMode.Force);
            }
            
            //Checks if speed exceeds max
            if (flatVelocity.magnitude > contextSpeedCap)
            {
                //Sets speed to maxed out speed
                Vector3 limitedVelocity = flatVelocity.normalized * contextSpeedCap;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }

    private void MaterialUpdate()
    {
        //Suppress sliding around small mesh geometry (Because everyone hates the books and boxes)
        if (grounded)
        {
            if (playerInput.MoveInput != Vector2.zero)
            {
                if (ratBox.sharedMaterial != noFriction)
                {
                    ratBox.sharedMaterial = noFriction;
                }
            }
            else
            {
                if (ratBox.sharedMaterial != idleFriction)
                {
                    ratBox.sharedMaterial = idleFriction;
                }
            }
        }
        else
        {
            if (ratBox.sharedMaterial != noFriction)
            {
                ratBox.sharedMaterial = noFriction;
            }
        }
    }

    //Reset the player's jump and smoothen out their landing
    //Calculation subject to scale based on movement parameters (So you can't precision platform with Mach 7 speed)
    private void OnLanding()
    {
        jumpStarted = false;
        canJump = true;
        currentVertical = verticalAction.Idle;

        StopCoroutine(FullHop());

        Vector3 smoothedVelocity = rb.velocity;
        //Stops the rat from bouncing off slopes
        float surfaceSpeed = Vector3.Dot(smoothedVelocity, groundNormal);
        if (surfaceSpeed > 0f)
        {
            smoothedVelocity -= groundNormal * surfaceSpeed;
        }
        //Prevents sliding on idle landing
        if (playerInput.MoveInput == Vector2.zero)
        {
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(smoothedVelocity, groundNormal);
            smoothedVelocity -= lateralVelocity;

            //Defensive measure to prevent sliding along small, mostly flat mesh collision
            if (Vector3.Angle(groundNormal, Vector3.up) < 10f)
            {
                smoothedVelocity.x = 0f;
                smoothedVelocity.z = 0f;
            }
        }
        
        rb.velocity = smoothedVelocity;
        //Lightly anchor the rat to the surface to counteract other uneven forces
        rb.AddForce(-groundNormal * 100f, ForceMode.Acceleration);

    }

    private void HandleClimbToggle()
    {
        if (playerInput.ClimbPressed && canClimb)
        {
            climbToggleQueued = true;
        }
    }

    IEnumerator HandleJump()
    {
        jumpStarted = true;
        ShortHop();

        //Make sure jump is held during jump squat to allow for full hop transition
        //Prevents situations where players press jump, release, and then quickly hold again within frame window leading to full hop
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
            StartCoroutine(FullHop());
        }
        else
        {
            currentVertical = verticalAction.ShortHopping;
        }

        StartCoroutine(JumpCooldown());
    }
    
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
        Vector3 wallJumpForce = wallJumpDirection * horizontalBounce + Vector3.up * ((minJumpForce + maxJumpForce) / 2f);
        rb.AddForce(wallJumpForce, ForceMode.Impulse);
        StartCoroutine(ClimbCooldown());
        StartCoroutine(JumpCooldown());
    }

    //Update grounded state and direction of grounded movement for sloped movement
    private void CheckGround() 
    {
        Vector3 ratCenter = transform.TransformPoint(ratBox.center);
        Vector3 halfBounds = Vector3.Scale(ratBox.size * 0.5f, transform.lossyScale) * 0.95f;
        Vector3 castDirection = transform.TransformDirection(Vector3.down);
        Vector3 normalizedCastDir = castDirection.normalized;
        Vector3 absNormCastDir = new Vector3(Mathf.Abs(normalizedCastDir.x), Mathf.Abs(normalizedCastDir.y), Mathf.Abs(normalizedCastDir.z));
        float extentDown = Vector3.Dot(halfBounds, absNormCastDir);
        float castDistance = extentDown + 0.05f;
        Vector3 castOrigin = ratCenter - normalizedCastDir * 0.01f;
        
        //Checks for both even and uneven ground based off the current player collider's geometry
        if (Physics.BoxCast(castOrigin, halfBounds, Vector3.down, out RaycastHit hit, transform.rotation, castDistance, whatIsGround))
        {
            //Check to see if the hit surface is too steep to traverse by ground movement
            grounded = Vector3.Angle(hit.normal, transform.up) <= maxInclination;
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
        if (!Application.isPlaying)
        {
            return;
        }
        //For ground boxcasting
        Vector3 ratCenter = transform.TransformPoint(ratBox.center);
        Vector3 halfBounds = Vector3.Scale(ratBox.size * 0.5f, transform.lossyScale) * 0.95f;
        Vector3 castDirection = transform.TransformDirection(Vector3.down);
        Vector3 normalizedCastDir = castDirection.normalized;
        Vector3 absNormCastDir = new Vector3(Mathf.Abs(normalizedCastDir.x), Mathf.Abs(normalizedCastDir.y), Mathf.Abs(normalizedCastDir.z));
        float extentDown = Vector3.Dot(halfBounds, absNormCastDir);
        float castDistance = extentDown + 0.05f;
        Vector3 castOrigin = ratCenter - normalizedCastDir * 0.01f;
        
        //Starting point, matrices used to allign casts with rat rotation
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.matrix = Matrix4x4.TRS(castOrigin, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfBounds * 2);

        //Ending point
        Gizmos.color = grounded ? Color.blue : Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(castOrigin + castDirection * castDistance, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfBounds * 2);

        //Raycast path
        Gizmos.color = grounded ? Color.white : Color.black;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(castOrigin, castOrigin + castDirection * castDistance);

        if (Physics.BoxCast(castOrigin, halfBounds, Vector3.down, out RaycastHit hit, transform.rotation, castDistance, whatIsGround))
        {
            //Point of contact
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(hit.point, 0.02f);

            //Direction of normal between rat and contact surface
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.2f);
        }

        //For climb raycasting
        Gizmos.color = climbing ? Color.blue : Color.red;
        Vector3 climbOrigin = transform.TransformPoint(ratBox.center);
        Vector3 direction = climbing ? attachDirection : orientation.TransformDirection(Vector3.forward);
        Vector3 normalizedDir = direction.normalized;
        Vector3 absNormDir = new Vector3(Mathf.Abs(normalizedDir.x), Mathf.Abs(normalizedDir.y), Mathf.Abs(normalizedDir.z));
        float extentForward = Vector3.Dot(halfBounds, absNormDir);
        float distance = extentForward + 0.05f;
        Vector3 climbEndpoint = climbOrigin + direction * distance;
        Gizmos.DrawLine(climbOrigin, climbEndpoint);
    }

    private bool NearClimbable()
    {
        //For entering climb state, base off cardinal proximity
        if (!climbing)
        {
            return IsValidClimbable();
        }
        //For exiting climb state, base off direction originally attached from
        else
        {
            return !IsDetached();
        }
    }

    private bool IsValidClimbable()
    {
        Vector3 ratCenter = transform.TransformPoint(ratBox.center);
        Vector3 halfBounds = Vector3.Scale(ratBox.size * 0.5f, transform.lossyScale) * 0.95f;
        Vector3 castDirection = orientation.TransformDirection(Vector3.forward);
        Vector3 normalizedCastDir = castDirection.normalized;
        Vector3 absNormCastDir = new Vector3(Mathf.Abs(normalizedCastDir.x), Mathf.Abs(normalizedCastDir.y), Mathf.Abs(normalizedCastDir.z));
        float extentDown = Vector3.Dot(halfBounds, absNormCastDir);
        float rayDistance = extentDown + 0.05f;
        Vector3 castOrigin = ratCenter - normalizedCastDir * 0.01f;
        
        //Checks in front of player orientation transform for climbables (orientation obj currently needed given that the rat doesn't rotate)
        if (Physics.Raycast(castOrigin, castDirection, out RaycastHit hit, rayDistance))
        {
            //Secondary check to make sure the object is even marked as climbable instead of using a Layer Mask
            if (hit.collider.TryGetComponent<ClimbableSurface>(out var _))
            {
                Debug.Log("Yep near climbable");
                float contactAngle = Mathf.Floor(Vector3.Angle(hit.normal, Vector3.up));
                
                Debug.Log(contactAngle);
                //Surface must be sufficiently steep, climb must be unlocked (mutation) and not on cooldown
                bool validAngle = (contactAngle > maxInclination && contactAngle <= 90f) && canClimb;
                if (validAngle)
                {
                    Debug.Log("Yep valid angle");
                    wallJumpDirection = (hit.normal + Vector3.up * verticalBounce).normalized;
                    attachDirection = normalizedCastDir;
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

    private bool IsDetached()
    {
        Vector3 ratCenter = transform.TransformPoint(ratBox.center);
        Vector3 halfBounds = Vector3.Scale(ratBox.size * 0.5f, transform.lossyScale) * 0.95f;
        Vector3 castDirection = orientation.TransformDirection(Vector3.forward);
        Vector3 normalizedCastDir = castDirection.normalized;
        Vector3 absNormCastDir = new Vector3(Mathf.Abs(normalizedCastDir.x), Mathf.Abs(normalizedCastDir.y), Mathf.Abs(normalizedCastDir.z));
        float extentDown = Vector3.Dot(halfBounds, absNormCastDir);
        float rayDistance = extentDown + 0.05f;
        Vector3 castOrigin = ratCenter - normalizedCastDir * 0.01f;
        
        if(!Physics.Raycast(castOrigin, attachDirection, out RaycastHit hit, rayDistance))
        {
                return true;
        }
        else if (!hit.collider.TryGetComponent<ClimbableSurface>(out var _))
        {
            return true;
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

    //Toggles climbing behavior as necessary, checks ground vs. air otherwise
    private void TryClimbToggle()
    {
        //Detach
        if (currentPositional == positionalState.Climbing)
        {
            climbing = false;
            StartCoroutine(ClimbCooldown());
            currentPositional = grounded ? positionalState.Grounded : positionalState.Airborne;

        }
        //Attach to climbable surfaces too steep to walk/run up, but no more than 90 degree, so long as climb is unlocked/off cooldown
        else if (NearClimbable() && canClimb && hasStamina(climbStamDrain))
        {
            climbing = true;
            currentPositional = positionalState.Climbing;
        }
        
    }

    private void PositionalStateUpdate()
    {
        
        //Auto-update as a defensive measure
        if (currentPositional != positionalState.Climbing)
        {
            climbing = false;
            currentPositional = grounded ? positionalState.Grounded : positionalState.Airborne;
        }

        //Wall jump, edge case where contact with climbable surface is lost, and running out of stamina
        if (currentPositional == positionalState.Climbing && ((climbing && !NearClimbable()) || !canClimb || !hasStamina(climbStamDrain)))
        {
            Debug.Log("You lost climb contact");
            climbing = false;
            //Make sure cooldown isn't triggered more than once per climbing state exit
            if (canClimb)
            {
                StartCoroutine(ClimbCooldown());
            }
            
            currentPositional = grounded ? positionalState.Grounded : positionalState.Airborne;
        }
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
                    if (playerInput.RunHeld && hasStamina(runStamDrain))
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
                
                if (playerInput.BufferedJump() && canJump)
                {
                    Debug.Log("Wall jump intended");
                    currentVertical = verticalAction.WallJumping;
                    if (playerInput.MoveInput != Vector2.zero)
                    {
                        if (playerInput.RunHeld && hasStamina(runStamDrain))
                        {
                            currentLateral = lateralAction.Running;
                        }
                        else
                        {
                            currentLateral = lateralAction.Walking;
                        }
                    }
                }
                else if (playerInput.MoveInput.y != 0f)
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
                    if (playerInput.RunHeld && hasStamina(runStamDrain))
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

    //Act on player input intent based on position (Not all actions are available in all positions)
    private void MovePlayer()
    {
        rb.drag = grounded ? groundDrag : 0f;
        rb.useGravity = !climbing;

        switch (currentPositional)
        {
            case positionalState.Climbing:
                running = false;
                if (currentLateral == lateralAction.Scaling)
                {
                    //Can only climb vertically, not horizontally
                    moveDirection = climbDirection * playerInput.MoveInput.y;
                    UIManager.Instance.playClimb();
                    rb.AddForce(3f * climbSpeed * moveDirection.normalized, ForceMode.Force);
                }
                else if (currentLateral == lateralAction.Idle)
                {
                    UIManager.Instance.pauseClimb();
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
                    rb.AddForce(2.5f * speed * moveDirection, ForceMode.Force);
                }
                else if (currentLateral == lateralAction.Running)
                {
                    running = true;
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), groundNormal).normalized;
                    rb.AddForce(moveDirection * runSpeed * 2.5f, ForceMode.Force);
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
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), Vector3.up).normalized;
                    rb.AddForce(moveDirection * airMovement, ForceMode.Acceleration);
                }
                else if (currentLateral == lateralAction.Running)
                {
                    running = true;
                    moveDirection = Vector3.ProjectOnPlane(GetInputDirection(), Vector3.up).normalized;
                    rb.AddForce(moveDirection * (airMovement * 1.25f), ForceMode.Acceleration);
                }
                else
                {
                    running = false;
                }
                break;

        }
    }

    //Make sure player has the stamina to use an intended action
    private bool hasStamina(float stamCost)
    {
        if (stamina - stamCost <= 0) { Debug.Log("Need more stam"); }
        return stamina - stamCost >= 0;
    }

    //Slap this at the end of any code block implementing a stamina draining behavior
    private void useStamina(float stamCost)
    {
        GameManager.Instance.changeStamina(-stamCost);
        stamina = playerStats.stamina;
        lastStamUse = Time.time;
    }

    private void regenStamina()
    {
        GameManager.Instance.changeStamina(staminaRegen * Time.deltaTime);
        stamina = playerStats.stamina;
    }
}
