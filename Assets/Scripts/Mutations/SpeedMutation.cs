using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMutation : MetaMutation, IMutation, IStackable
{
    [SerializeField]
    private float runSpeedMultiplier = -200f;
    [SerializeField]
    private float walkSpeedMultiplier = -100f;
    [SerializeField]
    private float minDrag = -2f;
    [SerializeField]
    private float maxDrag = -1f;

    private float initialWalkSpeed;
    private float initialRunSpeed;
    private float initialDrag;

    [Header("Messages for first and subsequent gains of this mutation")]
    [SerializeField]
    private string obtainedStr = "You feel a hurry to your scurry (Speed Up!)";
    [SerializeField]
    private string stackStr = "You feel slippier (Drag Down!)";
  

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        initialWalkSpeed = rat.walkSpeed;
        initialRunSpeed = rat.runSpeed;
        initialDrag = rat.groundDrag;
        if (stacks < 0) stacks = 0;
        if (runSpeedMultiplier < 0) runSpeedMultiplier = 1.5f;
        if (walkSpeedMultiplier < 0) walkSpeedMultiplier = 1.15f;
        if (decay < 0) decay = 0.9f;
        if (minDrag < 0) minDrag = rat.groundDrag / 2f;
        if (maxDrag < 0) maxDrag = rat.groundDrag;
    }

    public void SetStacks(int s)
    {
        //Refactor this into the interface or the Utils maybe since the code is the same between Jump and Speed
        if (s <= 0)
        {
            //0 out stacks for negative values
            ResetStacks();
            return;
        }

        else if (s == stacks)
        {
            return;
        }

        RecalculateStacks(s);
    }

    private void RecalculateStacks(int newStacks)
    {
        if (newStacks == 1)
        {
            rat.runSpeed = initialRunSpeed * runSpeedMultiplier;
            rat.walkSpeed = initialWalkSpeed * walkSpeedMultiplier;
        }
        else
        {
            if (!useEditorCurve)
            {
                rat.groundDrag = MutationUtils.ApplyMultiplicativeDecay(maxDrag, newStacks, minDrag, decay);

            }
            else
            {
                rat.groundDrag = MutationUtils.ApplyMultiplicativeDecay(decayCurve, maxDrag, newStacks, minDrag);
            }
        }

        stacks = newStacks;
    }

    private void ResetStacks()
    {
        rat.walkSpeed = initialWalkSpeed;
        rat.runSpeed = initialRunSpeed;
        rat.groundDrag = initialDrag;
        stacks = 0;
    }
    public override void onMutate()
    {
        UIManager.Instance.mutationPointsGainCueText.text = obtainedStr;
        stackMutation();
    }

    public override void stackMutation()
    {
        //Ensure stacks is non-negative
        stacks = Mathf.Max(0, stacks);

        //Flat multiplier to run and walk speed on first stack, subsequent stacks decay the drag exponentially (faster acceleration, slippier to control)
        //Designed to increase burst movement per stack in exchange for long-term control
        if (stacks == 0)
        {
            rat.runSpeed *= runSpeedMultiplier;
            rat.walkSpeed *= walkSpeedMultiplier;
        }
        else
        {
            if (!useEditorCurve)
            {
                rat.groundDrag = MutationUtils.ApplyMultiplicativeDecay(maxDrag, stacks, minDrag, decay);
                
            }
            else
            {
                rat.groundDrag = MutationUtils.ApplyMultiplicativeDecay(decayCurve, maxDrag, stacks, minDrag);
            }
        }
        stacks++;
        UIManager.Instance.mutationPointsGainCueText.text = stackStr + " x" + stacks.ToString();
    }
}
