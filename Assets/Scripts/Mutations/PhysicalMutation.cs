using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Feature based mutations (Ability perks, stacking may only have visual effects)
public abstract class PhysicalMutation : MonoBehaviour
{
    public abstract void onMutate();
    public abstract void stackMutation();

    [SerializeField]
    protected RatStats rat;
    
}
