using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StinkyMutation : PhysicalMutation, IMutation, IToggleable
{
    private Hole biteLogic;
    [SerializeField]
    private string obtainedStr = "You really smell, maybe enough to drive off a cat!";
    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        biteLogic = rat.gameObject.GetComponent<Hole>();
        Debug.Log("Stinky Mutation Obtained");
    }

    public override void onMutate()
    {
        //UIManager.Instance.cueMutation(obtainedStr);
        GameObject toRemove = GameManager.Instance.mutationPool.Find(obj => obj.name == "StinkyMutation");
        if (toRemove != null)
        {
            GameManager.Instance.mutationPool.Remove(toRemove);
        }
        rat.hasStinky = true;
        notifyFlag();
    }

    public void Toggle()
    {
        rat.hasStinky = !rat.canBite;
        notifyFlag();
    }

    public bool getCurrentFlag()
    {
        return rat.hasStinky;
    }

    public void setCurrentFlag(bool flag)
    {
        rat.hasStinky = flag;
        notifyFlag();
    }

    public void notifyFlag()
    {
        biteLogic.isEnabled = rat.canBite;
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
