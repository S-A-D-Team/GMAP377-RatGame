using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseOfHearing : MonoBehaviour
{
    [SerializeField]
    private float hearingRange;
    // Start is called before the first frame update
    void Start()
    {
         GameManager.Instance.addListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hear(GameObject source, Vector3 location, float intensity)
    {
        //Debug.Log("Listener Name: " + gameObject.name);
        if (Vector3.Distance(location, gameObject.transform.position) > hearingRange)
        {
            //Debug.Log("Not in range");
            return;
        }
        //Debug.Log("Heard: " + source.name + " at location: " + location + " with an intensity of: " + intensity);
    }
}
