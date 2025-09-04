using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSetpiece : MonoBehaviour, ISetpieceEvent
{
    [SerializeField]
    private float openAngle = 120f;
    public Transform doorHinge;
    private bool isOpen = false;
    public void TriggerEvent()
    {
        isOpen = !isOpen;
        float angle = isOpen ? openAngle : 0f;
        doorHinge.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
}
