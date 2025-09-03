using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HollowBonesMutation : PhysicalMutation, IMutation, IToggleable
{
    private Hole biteLogic;
    [SerializeField]
    private string obtainedStr = "Your bones have hollowed out. At least you'll be quieter!";
    [SerializeField]
    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        biteLogic = rat.gameObject.GetComponent<Hole>();
        Debug.Log("Hollow Bones Mutation Obtained");
    }

    public override void onMutate()
    {
        UIManager.Instance.cueMutation(obtainedStr);
        GameObject toRemove = GameManager.Instance.mutationPool.Find(obj => obj.name == "HollowBonesMutation");
        if (toRemove != null)
        {
            GameManager.Instance.mutationPool.Remove(toRemove);
        }
        if (!rat.hasHollowBones)
        {
            rat.soundMultiplier = rat.soundMultiplier / 2f;
        } else
        {
            Debug.Log("Hollow bones already active");
        }
        rat.hasHollowBones = true;
        notifyFlag();
    }

    public void Toggle()
    {
        rat.hasHollowBones = !rat.hasHollowBones;
        notifyFlag();
    }

    public bool getCurrentFlag()
    {
        return rat.hasHollowBones;
    }

    public void setCurrentFlag(bool flag)
    {
        rat.hasHollowBones = flag;
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
