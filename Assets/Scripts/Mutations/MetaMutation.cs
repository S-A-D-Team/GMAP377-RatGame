using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attribute based mutations (Numerically stacking perks)
public abstract class MetaMutation : MonoBehaviour
{
    [SerializeField]
    protected int stacks = -1;
    [SerializeField]
    protected float decay = -1f;
    [SerializeField]
    [Tooltip("This is a designer tool for manually controlling the curving of decay")]
    protected AnimationCurve decayCurve;
    [SerializeField]
    [Tooltip("Toggle this on if you would like to use the AnimationCurve to control stacking")]
    protected bool useEditorCurve = false;

    [SerializeField]
    protected RatStats rat;
    
    public abstract void onMutate();
    public abstract void stackMutation();

}
