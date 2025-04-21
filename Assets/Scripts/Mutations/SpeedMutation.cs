using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMutation : MetaMutation, IMutation
{
    [SerializeField]
    private float runSpeedMultiplier = -200f;
    [SerializeField]
    private float walkSpeedMultiplier = -100f;
    [SerializeField]
    private float minDrag = -2f;
    [SerializeField]
    private float maxDrag = -1f;
    
  

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
        if (stacks < 0) stacks = 0;
        if (runSpeedMultiplier < 0) runSpeedMultiplier = 1.5f;
        if (walkSpeedMultiplier < 0) walkSpeedMultiplier = 1.15f;
        if (decay < 0) decay = 0.9f;
        if (minDrag < 0) minDrag = rat.groundDrag / 2f;
        if (maxDrag < 0) maxDrag = rat.groundDrag;
    }

    public override void onMutate()
    {
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
                float finalMultiplier = MutationUtils.ApplyMultiplicativeDecay(maxDrag, stacks, minDrag, decay);
                rat.groundDrag *= finalMultiplier;
            }
            else
            {
                float finalMultiplier = MutationUtils.ApplyMultiplicativeDecay(decayCurve, maxDrag, stacks, minDrag);
                rat.groundDrag *= finalMultiplier;
            }
        }
        stacks++;
    }
}
