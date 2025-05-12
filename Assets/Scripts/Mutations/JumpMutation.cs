using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpMutation : MetaMutation, IMutation, IStackable
{
    //min/max are made specific to this mutation in the event that other multiplicable modifier perks do not clamp within a given range
    [SerializeField]
    private float minJumpMultiplier = -200f;
    [SerializeField]
    private float maxJumpMultiplier = -100f;

    private float initialJumpForce;

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
        //Cache the base jump force in case it needs to be reset
        initialJumpForce = rat.maxJumpForce;
        //Default intitialization values for designer reference if not set in the inspector already
        if (stacks < 0) stacks = 0;
        if (minJumpMultiplier < 0) minJumpMultiplier = 2f;
        if (maxJumpMultiplier < 0) maxJumpMultiplier = 3f;
        if (decay < 0) decay = 3f;
    }

    public void SetStacks(int s)
    {
        if (s < 0)
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

    private void ResetStacks()
    {
        rat.maxJumpForce = initialJumpForce;
        stacks = 0;
    }

    private void RecalculateStacks(int newStacks)
    {
        //Might have to edit this like speed mutation as well
        //Programmatic version, modifiable by changing the serialized min/max/decay directly
        if (!useEditorCurve)
        {
            float finalMultiplier = MutationUtils.ApplyStackedMultiplier(newStacks, minJumpMultiplier, maxJumpMultiplier, decay);
            rat.maxJumpForce *= finalMultiplier;
        }
        //Designer friendly version if the editor option is toggled on and a specific, visualized curving option is required
        //min/max changes still affect this but decay does not
        else
        {
            float finalMultiplier = MutationUtils.ApplyStackedMultiplier(decayCurve, newStacks, minJumpMultiplier, maxJumpMultiplier);
            rat.maxJumpForce *= finalMultiplier;
        }

        stacks = newStacks;
    }

    public override void onMutate()
    {
        //Currently just applies the stat bonus, this should also apply the proper UI elements and/or other front-end requirements when available
        stackMutation();
    }

    //Flat minimum multiplier at 1 with subsequent stacks providing smooth, diminishing returns up to a hard capped maximum multiplier (resulting in a soft cap of stacks)
    //Logic handled in its own function to avoid redundant behavior when the mutation is stacked in GameManager
    public override void stackMutation()
    {
        //Ensure stacks is non-negative
        stacks = Mathf.Max(0, stacks);

        //Might have to edit this like speed mutation as well
        //Programmatic version, modifiable by changing the serialized min/max/decay directly
        if (!useEditorCurve)
        {
            float finalMultiplier = MutationUtils.ApplyStackedMultiplier(stacks, minJumpMultiplier, maxJumpMultiplier, decay);
            rat.maxJumpForce *= finalMultiplier;
        }
        //Designer friendly version if the editor option is toggled on and a specific, visualized curving option is required
        //min/max changes still affect this but decay does not
        else
        {
            float finalMultiplier = MutationUtils.ApplyStackedMultiplier(decayCurve, stacks, minJumpMultiplier, maxJumpMultiplier);
            rat.maxJumpForce *= finalMultiplier;
        }
        stacks++;
    }
}
