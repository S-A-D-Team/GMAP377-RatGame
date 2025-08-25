using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector : MonoBehaviour
{
    public Transform minBound;
    public Transform maxBound;

    private BoxCollider triggerArea;

    // Start is called before the first frame update
    void Start()
    {
        SetpieceManager.Instance.RegisterSector(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Auto generate the sector's trigger area based on any scene changes
    private void OnValidate()
    {
        if (minBound != null && maxBound != null)
        {
            UpdateBounds();
        }
    }

    private void UpdateBounds()
    {
        Vector3 min = minBound.position;
        Vector3 max = maxBound.position;

        Vector3 size = Vector3.Max(max - min, Vector3.zero);
        Vector3 center = (min + max) * 0.5f;

        if (triggerArea == null)
        {
            triggerArea = GetComponent<BoxCollider>();
            if (triggerArea == null)
            {
                triggerArea = gameObject.AddComponent<BoxCollider>();
            }
        }

        triggerArea.isTrigger = true;
        triggerArea.size = size;
        triggerArea.center = transform.InverseTransformPoint(center);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            SetpieceManager.Instance.UpdatePlayerSector(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("player"))
        {
            SetpieceManager.Instance.ExitPlayerSector();
        }
    }
}
