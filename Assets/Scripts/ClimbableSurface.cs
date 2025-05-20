using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableSurface : MonoBehaviour
{
    //Determine if the surface is difficult enough to climb to require stamina
    //Example: Plant vines/stems might be free, but outside walls may not be
    public bool hasStamCost;
    private const string iconPath = "Assets/Sprites-UI/Mutation Icons/Climb Icon 1.png";

    //Allows level designers to see what is marked climbable in the editor after attaching the component
    //Just make sure gizmos are toggled on in scene view (should be by default)
    private void OnDrawGizmos()
    {
        if (Application.isEditor)
        {
            Gizmos.DrawIcon(transform.position, iconPath, true);
        }
    }
}
