using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    public enum ZoneType { Inside, Outside }
    public ZoneType zoneType;
    public Transform teleportTarget;
    public PresetBiteZone biteZone; // Reference to parent logic

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player") && biteZone.bitten && !biteZone.teleported)
        {
            other.transform.position = teleportTarget.position;
            //biteZone.teleported = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("player") && !biteZone.bitten && Input.GetKeyDown(KeyCode.E))
        {
            biteZone.bitten = true;
            biteZone.insideCrack.enabled = false;
            biteZone.outsideCrack.enabled = false;
            biteZone.hole.SetActive(true);
            other.transform.position = teleportTarget.position;
            //biteZone.teleported = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("player") && biteZone.bitten && biteZone.teleported)
        {
            //biteZone.teleported = false;
        }
    }
}
