using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContaminationSpread : MonoBehaviour
{
    [Tooltip("Rate of contamination (per hour) of foods within its proximity.")]
    public float contaminationRate;
    [SerializeField]
    [Tooltip("Collider that defines the area where it can contaminate")]
    private SphereCollider contamizationZone;
    [SerializeField]
    [Tooltip("(Internal) List of foods in its contamination range")]
    private List<Contaminatable> otherContaminationsInZone;


	private void Awake()
	{
        //subscribe to when a minute passes from the timer
        GameTimer.Instance.minutePassed += atMinutePass;
    }

	// Start is called before the first frame update
	void Start()
    {
        //Make sure Collider is there
        if (!contamizationZone) contamizationZone = GetComponent<SphereCollider>();
        //if still not there
        if (!contamizationZone)
        {
            contamizationZone = gameObject.AddComponent<SphereCollider>();
            contamizationZone.radius = 1f;
        }
        contamizationZone.isTrigger = true;

        //initial trigger query

        Collider[] _initialTriggerEnter = Physics.OverlapSphere(contamizationZone.center, contamizationZone.radius);
        foreach (Collider _col in _initialTriggerEnter)
        {
            Debug.Log(gameObject + " adding " +_col.gameObject + " to list");
            // Check if the overlapping object has the Contaminate script
            Contaminatable _contamTest = _col.GetComponent<Contaminatable>();
            if (_contamTest)
            {
                //only add if not already there
                if (!otherContaminationsInZone.Contains(_contamTest))
                    otherContaminationsInZone.Add(_contamTest);
            }
        }
        if (!otherContaminationsInZone.Contains(gameObject.GetComponent<Contaminatable>()))
            otherContaminationsInZone.Remove(gameObject.GetComponent<Contaminatable>());
    }

    private void atMinutePass()
	{
        foreach (Contaminatable _contamObj in otherContaminationsInZone)
        {
            _contamObj.AddBuildUp(contaminationRate);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject + " adding " + other.gameObject + " to list");

        // Check if the object has the Contaminate script
        Contaminatable _contamTest = other.GetComponent<Contaminatable>();
        if (_contamTest)
        {
            //only add if not already there
            if(!otherContaminationsInZone.Contains(_contamTest)) 
                otherContaminationsInZone.Add(_contamTest);
        }
    }

	private void OnTriggerExit(Collider other)
	{
        Debug.Log(gameObject + " REMOVING " + other.gameObject + " to list");
        // Check if the object has the Contaminate script
        Contaminatable _contamTest = other.GetComponent<Contaminatable>();
        if (_contamTest)
        {
            //only if already there
            if (otherContaminationsInZone.Contains(_contamTest))
                otherContaminationsInZone.Remove(_contamTest);
        }
    }
}
