using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Dummy value for testing a hard coded mutation addition")]
    private float testThreshold = 25f;
    private PlayerMovement ratStat;
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

    //As PlayerMovement is currently the only player specific script, it is used as the reference type for now
    public void RegisterPlayer(PlayerMovement player)
    {
        ratStat = player;
    }

    public void OnThresholdTrigger(float threshold)
    {
        Debug.Log("Threshold " + threshold + "% reached.");
        //Handle mutation and potential environment updates from here
        //May send such logic to a different handler or entirely different manager later though depending on specificiations (MutationManager/EnvironmentManager?)
        //Double the player's jump height for now
        if (threshold == testThreshold && ratStat != null)
        {
            ratStat.JumpForce *= 2f;
        }
    }
}
