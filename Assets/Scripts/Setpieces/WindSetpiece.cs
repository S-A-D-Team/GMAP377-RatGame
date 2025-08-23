using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSetpiece : ISetpieceEvent
{
    //Safety and danger areas do not need to be set at first but the random bounds do
    public List<Transform> safetySpots;
    public List<Transform> dangerSpots;
    public Transform minRandomBound;
    public Transform maxRandomBound;

    //The trigger area to apply the wind force, set in inspector
    public WindCurrent current;

    private enum destinationType
    {
        RANDOM = 0,
        SAFETY = 1,
        DANGER = 2
    }

    private destinationType destination;
    private Vector3 targetArea;

    public void TriggerEvent()
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
                float zPos = Random.Range(minRandomBound.position.z, maxRandomBound.position.z);
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

        current.EnableCurrent(targetArea);
    }
}
