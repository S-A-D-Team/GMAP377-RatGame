using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows for tweakable contamination presets for the ContaminationManager to access
[CreateAssetMenu(fileName = "ContaminationSettings", menuName = "Contamination/Settings", order = 1)]
public class ContaminationSettings : ScriptableObject
{
    [Header("Initial Level")]
    public float initialContaminationLevel;

    [Header("Thresholds")]
    public List<float> contaminationThresholds;
}
