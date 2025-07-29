using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SenseOfSmell : MonoBehaviour
{
    public UnityEvent<Collider> onTriggerEnter;
    public UnityEvent<Collider> onTriggerStay;
    public UnityEvent<Collider> onTriggerExit;
    SphereCollider smellSphere;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<SphereCollider>() != null)
        {
            smellSphere = gameObject.GetComponent<SphereCollider>();
            //Debug.Log("Scent Range: " + GameManager.Instance.ratStats.scentRange);
            //smellSphere.radius = 1f * GameManager.Instance.ratStats.scentRange;
        }
        else
        {
            smellSphere = gameObject.AddComponent<SphereCollider>();
            smellSphere.isTrigger = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onTriggerEnter != null)
        {
            onTriggerEnter?.Invoke(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (onTriggerStay != null)
        {
            onTriggerStay?.Invoke(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (onTriggerExit != null)
        {
            onTriggerExit?.Invoke(other);
        }
    }
    //will probably be changed later
    public void changeRadius(float change)
    {
        Debug.Log("change = " + change);
        smellSphere.radius = 0.5f * change;
    }
}
