using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Mutation reworked to an interface as no base level implementation is planned
public interface IMutation
{
    //Apply modifications to player mechanics
    void onMutate();
    //In the event that a duplicate mutation is gained, amplify (if possible) its effects instead of generating more components
    void stackMutation();

    void Initialize();
    
}
