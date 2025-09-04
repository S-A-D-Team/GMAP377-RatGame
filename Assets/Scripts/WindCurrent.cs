using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCurrent : MonoBehaviour
{
    private BoxCollider triggerArea;
    private ParticleSystem windVisual;
    private Vector3 windDirection;
    [SerializeField]
    private float windMagnitude = 15f;
    // Start is called before the first frame update
    void Start()
    {
        triggerArea = GetComponent<BoxCollider>();
        windVisual = GetComponent<ParticleSystem>();
        if (windVisual.isPlaying)
        {
            windVisual.Stop();
        }
        triggerArea.enabled = false;
    }

    public void EnableCurrent(Vector3 direction)
    {
        windDirection = direction;
        triggerArea.enabled = true;
        var shape = windVisual.shape;
        shape.rotation = Quaternion.LookRotation(windDirection).eulerAngles;
        windVisual.Play();

    }

    public void DisableCurrent()
    {
        triggerArea.enabled = false;
        windVisual.Stop();
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
