using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbMutation : PhysicalMutation, IMutation, IToggleable
{
    private PlayerMovement climbLogic;
    [SerializeField]
    private string obtainedStr = "You feel like inclines can no longer decline you (Climb Unlocked!)";
    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        climbLogic = rat.gameObject.GetComponent<PlayerMovement>();
        Debug.Log("You can climb now");
    }

    public override void onMutate()
    {
        UIManager.Instance.cueMutation(obtainedStr);
        //One-and-done mutation, removed from mutation pool after unlocked
        GameObject toRemove = GameManager.Instance.mutationPool.Find(obj => obj.name == "ClimbMutation");
        if (toRemove != null)
        {
            GameManager.Instance.RemoveFromPool(toRemove);
        }
        rat.canClimb = true;
        notifyFlag();
    }

    public void Toggle()
    {
        rat.canClimb = !rat.canClimb;
        notifyFlag();
    }

    public bool getCurrentFlag()
    {
        return rat.canClimb;
    }

    public void setCurrentFlag(bool flag)
    {
        rat.canClimb = flag;
        notifyFlag();
    }

    public void notifyFlag()
    {
        climbLogic.RefreshStats();
    }

    public override void stackMutation()
    {
        //Future consideration: Visual changes on stacks
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
