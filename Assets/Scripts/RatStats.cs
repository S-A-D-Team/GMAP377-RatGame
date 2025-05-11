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
    public float airSpeed;
    public float jumpChargeRate;
    public float maxJumpForce;
    public bool canBite;
    public bool canClimb;
    public float groundDrag;
    public float hunger;
    public float stamina;
    public float staminaCap;
    public float stamRegen;
    public float stamRegenDelay;
    public hungerLevel currentHungerLevel;
    public enum hungerLevel
    {
        Full = 110,
        Content = 100,
        Peckish = 75,
        Hungry = 50,
        Starving = 25,
        Ravenous = 0
    }
    public float hungerPenalty;
    public float hungerTolerance;
    
    //Player will have to grab the hunger level
    //When applying stats, will have to apply a modifier based on that hunger level
    //110 / 100 = 1.1x modifier to speed
    //100 / 100 1x modifier
    //hungertolerance = 5
    //hungerpenalty = 100 - tolerance
    

}
