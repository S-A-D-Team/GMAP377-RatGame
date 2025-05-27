using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputCollection : MonoBehaviour
{
    //Input key states
    public Vector2 MoveInput { get; private set; }
    public bool RunHeld { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool ClimbPressed { get; private set; }
    public bool BiteHeld { get; private set; }
    public bool BiteReleased { get; private set; }
    //Mostly for testing stats
    public bool RefreshPressed { get; private set; }
    public bool DebugMovePressed { get; private set; }
    
    [Header("Movement Inputs")]
    private float horInput;
    private float vertInput;

    [Header("Jump Input Buffering")]
    [SerializeField]
    private float jumpBuffer = 0.1f;
    private float lastJumpPress = -100f;

    [Header("Key Bindings")]
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    [SerializeField]
    private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode refreshStatsKey = KeyCode.R;
    [SerializeField]
    private KeyCode climbKey = KeyCode.F;
    [SerializeField]
    private KeyCode debugMoveUpKey = KeyCode.L;
    [SerializeField]
    private KeyCode biteKey = KeyCode.E;

    private void Update()
    {
        horInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(horInput, vertInput);

        ClimbPressed = Input.GetKeyDown(climbKey);
        RefreshPressed = Input.GetKeyDown(refreshStatsKey);
        RunHeld = Input.GetKey(runKey);
        JumpHeld = Input.GetKey(jumpKey);
        JumpPressed = Input.GetKeyDown(jumpKey);
        BiteHeld = Input.GetKey(biteKey);
        DebugMovePressed = Input.GetKeyDown(debugMoveUpKey);
        BiteReleased = Input.GetKeyUp(biteKey);
        if (JumpPressed)
        {
            lastJumpPress = Time.time;
        }
        //if (Input.GetKeyDown(KeyCode.Escape) && !UIManager.Instance.isTutorialActive)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.onPause();
            Time.timeScale = 0.0f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    public bool BufferedJump()
    {
        return Time.time < lastJumpPress + jumpBuffer;
    }

}
