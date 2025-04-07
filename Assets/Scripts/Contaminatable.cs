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

    [Space]
    [Header("Contamination Material")]
    private Material mat; // Assign your material with the shader
    [SerializeField]
    private string lerpProperty = "_Contamination_Lerp";

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
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    protected virtual void Update() 
    {

    }

    public void AddBuildUp(float _buildUpValue)
	{
        contaminationBuildup += _buildUpValue;
    }

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag.ToLower().Contains("player"))
		{
            //HEre we can add more buildup based on perks?
            contaminationValue += 2f;
            AddBuildUp(5f);
            StartCoroutine(LerpRoutine(mat));
        }
	}

    //Considering this might be used elsewhere too, 
    //I might consider moving this to a big utility functions script file later
    private IEnumerator LerpRoutine(Material _material)
    {
        float _halfDuration = 1f;

        // 0 to 1
        for (float t = 0; t < _halfDuration; t += Time.deltaTime)
        {
            float lerpValue = t / _halfDuration;
            _material.SetFloat(lerpProperty, lerpValue);
            yield return null;
        }

        // 1 to 0
        for (float t = 0; t < _halfDuration; t += Time.deltaTime)
        {
            float lerpValue = 1 - (t / _halfDuration);
            _material.SetFloat(lerpProperty, lerpValue);
            yield return null;
        }

        _material.SetFloat(lerpProperty, 0f); // just to be sure it ends at 0
    }
}

