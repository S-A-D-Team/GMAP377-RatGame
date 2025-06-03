using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSafeZone : MonoBehaviour
{
    public bool isTouchingWallBack = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SafeZone"))
        {
            isTouchingWallBack = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SafeZone"))
        {
            isTouchingWallBack = false;
        }
    }
}
