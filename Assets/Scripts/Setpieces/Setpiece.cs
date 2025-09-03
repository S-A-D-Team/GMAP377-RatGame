using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setpiece : MonoBehaviour
{
    private ISetpieceEvent spEvent;
    public Sector sector;
    public bool isRepeatable;

    // Start is called before the first frame update
    void Start()
    {
        spEvent = gameObject.GetComponent<ISetpieceEvent>();
        SetpieceManager.Instance.RegisterSetpiece(this);
    }

    public void TriggerSetpieceEvent()
    {
        spEvent.TriggerEvent();
        if (!isRepeatable)
        {
            SetpieceManager.Instance.UnregisterSetpiece(this);
        }
    }
}
