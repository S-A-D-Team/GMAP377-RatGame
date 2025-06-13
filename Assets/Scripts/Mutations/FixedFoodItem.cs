using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Food item that automatically grants a specific mutation (or nothing)
//DEMONSTRATION/TESTING PURPOSES ONLY
public class FixedFoodItem : FoodItem
{
    public enum fixedMutations
    {
        SPEED,
        JUMP,
        BITE,
        CLIMB,
        NONE
    };

    //MUST BE SET IN INSPECTOR
    public fixedMutations mutationGranted;

    protected override void atMinutePass()
    {
        contaminationValue = Mathf.Clamp(contaminationValue, 0f, 100f);
        if (contaminationValue >= 100f && canGrantPoints)
        {
            canGrantPoints = false;
            int trueMutationYield = mutationYield * (int)potency;
            string pointsGained = "You gained " + trueMutationYield.ToString() + " mutation points";
            UIManager.Instance.cueMutation(pointsGained);
            AddFixedMutation();

            if (isWinCondition)
            {
                ContaminationManager.Instance.ActivateWinCondition(this);
            }
        }
        //Update its entry in the manager
        ContaminationManager.Instance.CalculateContaminationLevel(this, contaminationValue);
    }

    private void AddFixedMutation()
    {
        switch (mutationGranted)
        {
            case fixedMutations.SPEED:
                GameManager.Instance.AddMutation<SpeedMutation>();
                break;
            case fixedMutations.JUMP:
                GameManager.Instance.AddMutation<JumpMutation>();
                break;
            case fixedMutations.BITE:
                GameManager.Instance.AddMutation<BiteMutation>();
                break;
            case fixedMutations.CLIMB:
                GameManager.Instance.AddMutation<ClimbMutation>();
                break;
            case fixedMutations.NONE:
                break;
        }
    }
}
