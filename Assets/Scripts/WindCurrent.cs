using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCurrent : MonoBehaviour
{
    private BoxCollider triggerArea;
    [SerializeField]
    [Tooltip("How long does the gust last?")]
    private float windDuration = 30f;

    private Vector3 windDirection;
    [SerializeField]
    private float windMagnitude = 15f;
    // Start is called before the first frame update
    void Start()
    {
        triggerArea = GetComponent<BoxCollider>();
    }

    public void EnableCurrent(Vector3 direction)
    {
        windDirection = direction;
        triggerArea.enabled = true;
        Invoke(nameof(DisableCurrent), windDuration);

    }

    private void DisableCurrent()
    {
        triggerArea.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            windDirection -= other.transform.position;
            windDirection.Normalize();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("player"))
        {
            Vector3 appliedForce = windDirection * windMagnitude;
            //Request external force handling from player
            PlayerMovement ratMov = other.GetComponent<PlayerMovement>();
            if (ratMov != null)
            {
                ratMov.applyExternalForce(appliedForce);
            }
        }
    }

}
