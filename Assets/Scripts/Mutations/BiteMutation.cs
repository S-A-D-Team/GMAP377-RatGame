using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiteMutation : PhysicalMutation, IMutation, IToggleable
{
    private Hole biteLogic;
    [SerializeField]
    private string obtainedStr = "You feel like walls may only be a suggestion (Bite Unlocked!)";
    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        biteLogic = rat.gameObject.GetComponent<Hole>();
        Debug.Log("You can chomp now");
    }

    public override void onMutate()
    {
        UIManager.Instance.cueMutation(obtainedStr);
        GameObject toRemove = GameManager.Instance.mutationPool.Find(obj => obj.name == "BiteMutation");
        if (toRemove != null)
        {
            GameManager.Instance.mutationPool.Remove(toRemove);
        }
        rat.canBite = true;
        notifyFlag();
    }

    public void Toggle()
    {
        rat.canBite = !rat.canBite;
        notifyFlag();
    }

    public bool getCurrentFlag()
    {
        return rat.canBite;
    }

    public void setCurrentFlag(bool flag)
    {
        rat.canBite = flag;
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
