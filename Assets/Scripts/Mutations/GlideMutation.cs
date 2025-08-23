using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMutation : PhysicalMutation, IMutation, IToggleable
{
    private PlayerMovement glideLogic;
    [SerializeField]
    private string obtainedStr = "You feel like you can ride the wind (Glide Unlocked!)";
    public void Initialize()
    {
        rat = GameManager.Instance.ratStats;
        glideLogic = rat.gameObject.GetComponent<PlayerMovement>();
        Debug.Log("You can glide now");
    }

    public override void onMutate()
    {
        UIManager.Instance.cueMutation(obtainedStr);
        //One-and-done mutation, removed from mutation pool after unlocked
        GameObject toRemove = GameManager.Instance.mutationPool.Find(obj => obj.name == "GlideMutation");
        if (toRemove != null)
        {
            GameManager.Instance.mutationPool.Remove(toRemove);
        }
        rat.canGlide = true;
        notifyFlag();
    }

    public void Toggle()
    {
        rat.canGlide= !rat.canGlide;
        notifyFlag();
    }

    public bool getCurrentFlag()
    {
        return rat.canGlide;
    }

    public void setCurrentFlag(bool flag)
    {
        rat.canGlide = flag;
        notifyFlag();
    }

    public void notifyFlag()
    {
        glideLogic.RefreshStats();
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

