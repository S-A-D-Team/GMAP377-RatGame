using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setpiece : MonoBehaviour
{

    [SerializeField]
    private ISetpieceEvent spEvent;
    public Sector sector;

    // Start is called before the first frame update
    void Start()
    {
        spEvent = GetComponent<ISetpieceEvent>();
        sector = GetComponent<Sector>();
        SetpieceManager.Instance.RegisterSetpiece(this);
    }

    public void TriggerSetpieceEvent()
    {
        spEvent.TriggerEvent();
    }
}
