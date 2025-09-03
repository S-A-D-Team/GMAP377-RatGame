using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StinkyMutation : PhysicalMutation, IMutation, IToggleable
{
    private PlayerMovement stinkyLogic;
    [SerializeField]
    private string obtainedStr = "You really smell, maybe enough to drive off a cat!";
    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        stinkyLogic = rat.gameObject.GetComponent<PlayerMovement>();
        Debug.Log("Stinky Mutation Obtained");
    }

    public override void onMutate()
    {
        UIManager.Instance.cueMutation(obtainedStr);
        rat.hasStinky = true;
        notifyFlag();
    }

    public void Toggle()
    {
        rat.hasStinky = !rat.hasStinky;
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
        stinkyLogic.RefreshStats();
    }


    public override void stackMutation()
    {
        //Reroll if gained again while active
        if (rat.hasStinky)
        {
            GameManager.Instance.RandomMutate();
        }
        else
        {
            setCurrentFlag(true);
        }
        
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
