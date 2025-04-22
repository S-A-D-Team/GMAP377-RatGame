using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Static utility class for Mutations to apply stacking effects
public static class MutationUtils
{
    public static float ApplyStackedMultiplier(int stacks, float baseMultiplier = 2f, float maxMultiplier = 3f, float decay = 3f)
    {
        float decayStep = 1f - Mathf.Exp(-stacks / decay);
        float decayedMultiplier = Mathf.Lerp(baseMultiplier, maxMultiplier, decayStep);
        float finalMultiplier = Mathf.Clamp(decayedMultiplier, baseMultiplier, maxMultiplier);
        return finalMultiplier;
    }

    //Overload version for designers
    public static float ApplyStackedMultiplier(AnimationCurve curveTool, int stacks, float baseMultiplier = 2f, float maxMultiplier = 3f)
    {
        float decayedMultiplier = baseMultiplier * curveTool.Evaluate(stacks);
        //No need for designers to set clamps on the curve if they don't want to
        float finalMultiplier = Mathf.Clamp(decayedMultiplier, baseMultiplier, maxMultiplier);
        return finalMultiplier;
    }

    public static float ApplyMultiplicativeDecay(float baseValue, int stacks, float minValue = 0.5f, float decay = 0.9f)
    {
        float decayedMultiplier = baseValue * Mathf.Pow(decay, stacks);
        float finalMultiplier = Mathf.Clamp(decayedMultiplier, minValue, baseValue);
        return finalMultiplier;
    }

    //Overload version for designers
    public static float ApplyMultiplicativeDecay(AnimationCurve curveTool, float baseValue, int stacks, float minValue = 0.5f)
    {
        float decayedMultiplier = baseValue * curveTool.Evaluate(stacks);
        float finalMultiplier = Mathf.Clamp(decayedMultiplier, minValue, baseValue);
        return finalMultiplier;
    }
}
