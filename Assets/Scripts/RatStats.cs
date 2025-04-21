using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Container class for relevant player data (attributes, unlocked mechanics)
//Read by various movement mechanic scripts
//Written to by mutations
//Minorly amusing to say out loud in a serious development context

public class RatStats : MonoBehaviour
{
    public float runSpeed;
    public float walkSpeed;
    public float climbSpeed;
    public float maxJumpForce;
    public bool canBite;
    public bool canClimb;
    public float groundDrag;

}
