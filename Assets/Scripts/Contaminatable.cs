using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contaminatable : MonoBehaviour
{
    [Header("Contamination")]
    [SerializeField]
    [Tooltip("Contamination of this Item (100 means maximum contamination)")]
    private float contaminationValue = 0;
    [SerializeField]
    [Tooltip("Contamination buildup of this item (per hour)")]
    private float contaminationBuildup = 0;

    //does this obj has ContaminationSpread
    private bool canSpread = false;

    //private float startTime;
    
    protected virtual void atMinutePass()
	{
        contaminationValue += contaminationBuildup / 60f;
        contaminationValue = Mathf.Clamp(contaminationValue, 0f, 100f);

        if (canSpread) GetComponent<ContaminationSpread>().contaminationRate = contaminationValue / 100;
    }

    protected virtual void Awake()
	{
        //startTime = GameTimer.Instance.getgameMinutesElapsed();
        //subscribe to when a minute passes from the timer
        GameTimer.Instance.minutePassed += atMinutePass;
    }

    // Start is called before the first frame update
    protected virtual void Start() 
    {
        ContaminationSpread _contamSpreadCheck = GetComponent<ContaminationSpread>();
        //if it exists, then true, otherwise false
        canSpread = _contamSpreadCheck;
    }

    // Update is called once per frame
    protected virtual void Update() 
    {

    }

    public void AddBuildUp(float _buildUpValue)
	{
        contaminationBuildup += _buildUpValue;
    }
}
