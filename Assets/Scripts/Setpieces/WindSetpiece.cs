using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSetpiece : MonoBehaviour, ISetpieceEvent
{
    //Safety and danger areas do not need to be set at first but the random bounds do
    public List<Transform> safetySpots;
    public List<Transform> dangerSpots;
    public Transform minRandomBound;
    public Transform maxRandomBound;
    public SpriteRenderer warning;
    public float offsetZ = 0f;
    public Transform windowHinge;

    //The trigger area to apply the wind force, set in inspector
    public WindCurrent current;

    [SerializeField]
    private float eventDuration = 30f;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Color baseColor;
    private enum destinationType
    {
        RANDOM = 0,
        SAFETY = 1,
        DANGER = 2
    }

    private destinationType destination;
    private Vector3 targetArea;

    private void Start()
    {
        closedPosition = windowHinge.localPosition;
        openPosition = closedPosition + new Vector3(0, 2.5f, 0);
        warning.enabled = false;
    }
    public void TriggerEvent()
    {
        StartCoroutine(WindowEvent());
    }

    private Vector3 calculateTargetArea()
    {
        int validDestinationTypes = 3;
        if (safetySpots.Count == 0)
        {
            validDestinationTypes--;
        }
        if (dangerSpots.Count == 0)
        {
            validDestinationTypes--;
        }
        int rn = Random.Range(0, validDestinationTypes);
        destination = (destinationType)rn;
        switch (destination)
        {
            //Pre-condition that at least the min/max bounds are set properly in inspector
            case destinationType.RANDOM:
                float xPos = Random.Range(minRandomBound.position.x, maxRandomBound.position.x);
                float yPos = Random.Range(minRandomBound.position.y, maxRandomBound.position.y);
                float zPos = Random.Range(minRandomBound.position.z + offsetZ, maxRandomBound.position.z - offsetZ);
                targetArea = new Vector3(xPos, yPos, zPos);
                break;
            case destinationType.SAFETY:
                int safeSpot = Random.Range(0, safetySpots.Count);
                targetArea = safetySpots[safeSpot].position;
                break;
            case destinationType.DANGER:
                int dangerSpot = Random.Range(0, dangerSpots.Count);
                targetArea = dangerSpots[dangerSpot].position;
                break;
        }
        return targetArea;
    }

    IEnumerator WindowEvent()
    {
        windowHinge.localPosition = openPosition;
        float t = 0f;
        warning.enabled = true;
        while (t < 3f)
        {
            float transparency = Mathf.PingPong(Time.time * 5f, 1f);
            warning.color = new Color(baseColor.r, baseColor.g, baseColor.b, transparency);
            t += Time.deltaTime;
            yield return null;
        }
        warning.color = baseColor;
        warning.enabled = false;
        current.EnableCurrent(calculateTargetArea());
        yield return new WaitForSeconds(eventDuration);
        windowHinge.localPosition = closedPosition;
        current.DisableCurrent();
    }
}
