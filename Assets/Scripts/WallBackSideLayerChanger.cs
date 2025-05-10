using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WallBackSideLayerChanger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="player")
        {
            gameObject.layer = LayerMask.NameToLayer("WallBack");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "player")
        {
            gameObject.layer = LayerMask.NameToLayer("Objects");
        }
    }
}
