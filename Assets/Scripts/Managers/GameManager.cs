using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ContaminationManager.Instance.thresholdPassed += OnThresholdTrigger;
    }

    public void onPlayerTrapped()
    {
        //more thngs to do when player dies
        UIManager.Instance.TriggerDeathEffect();
    }

    public void OnThresholdTrigger(float threshold)
    {
        Debug.Log("Threshold " + threshold + "% reached.");
        //Handle mutation and potential environment updates from here
        //May send such logic to a different handler or entirely different manager later though depending on specificiations (MutationManager/EnvironmentManager?)
    }
}
