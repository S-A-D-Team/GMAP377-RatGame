using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimation : MonoBehaviour
{
    // Animation parameters
    public float floatAmplitude = 10f; // How far it floats
    public float floatFrequency = 1f;  // How fast it floats
    public float rotationAmplitude = 5f; // Degrees
    public float rotationFrequency = 0.5f;
    public float scaleAmplitude = 0.05f; 
    public float scaleFrequency = 0.8f;

    public GameObject[] targets;
    public GameObject[] buttonTargets;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private Vector3[] initialScales;

    private Vector3[] buttonInitialPositions;
    private Quaternion[] buttonInitialRotations;
    private Vector3[] buttonInitialScales;

    void Start()
    {
        // Regular targets
        if (targets != null && targets.Length > 0)
        {
            int len = targets.Length;
            initialPositions = new Vector3[len];
            initialRotations = new Quaternion[len];
            initialScales = new Vector3[len];
            for (int i = 0; i < len; i++)
            {
                if (targets[i] != null)
                {
                    Transform t = targets[i].transform;
                    initialPositions[i] = t.localPosition;
                    initialRotations[i] = t.localRotation;
                    initialScales[i] = t.localScale;
                }
            }
        }
        else
        {
            // Animate self if no targets
            initialPositions = new Vector3[1] { transform.localPosition };
            initialRotations = new Quaternion[1] { transform.localRotation };
            initialScales = new Vector3[1] { transform.localScale };
        }

        // Button targets
        if (buttonTargets != null && buttonTargets.Length > 0)
        {
            int len = buttonTargets.Length;
            buttonInitialPositions = new Vector3[len];
            buttonInitialRotations = new Quaternion[len];
            buttonInitialScales = new Vector3[len];
            for (int i = 0; i < len; i++)
            {
                if (buttonTargets[i] != null)
                {
                    Transform t = buttonTargets[i].transform;
                    buttonInitialPositions[i] = t.localPosition;
                    buttonInitialRotations[i] = t.localRotation;
                    buttonInitialScales[i] = t.localScale;
                }
            }
        }
    }

    void Update()
    {
        float tTime = Time.time;

        // Animate regular targets
        if (targets != null && targets.Length > 0)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                Transform t = targets[i].transform;
                float yOffset = Mathf.Sin(tTime * floatFrequency * Mathf.PI * 2) * floatAmplitude;
                t.localPosition = initialPositions[i] + new Vector3(0, yOffset, 0);
                float zRot = Mathf.Sin(tTime * rotationFrequency * Mathf.PI * 2) * rotationAmplitude;
                t.localRotation = initialRotations[i] * Quaternion.Euler(0, 0, zRot);
                float scale = 1f + Mathf.Sin(tTime * scaleFrequency * Mathf.PI * 2) * scaleAmplitude;
                t.localScale = initialScales[i] * scale;
            }
        }
        else
        {
            // Animate self if no targets
            Transform t = transform;
            float yOffset = Mathf.Sin(tTime * floatFrequency * Mathf.PI * 2) * floatAmplitude;
            t.localPosition = initialPositions[0] + new Vector3(0, yOffset, 0);
            float zRot = Mathf.Sin(tTime * rotationFrequency * Mathf.PI * 2) * rotationAmplitude;
            t.localRotation = initialRotations[0] * Quaternion.Euler(0, 0, zRot);
            float scale = 1f + Mathf.Sin(tTime * scaleFrequency * Mathf.PI * 2) * scaleAmplitude;
            t.localScale = initialScales[0] * scale;
        }

        // Animate button targets
        if (buttonTargets != null && buttonTargets.Length > 0)
        {
            for (int i = 0; i < buttonTargets.Length; i++)
            {
                if (buttonTargets[i] == null) continue;
                Transform t = buttonTargets[i].transform;
                float swing = Mathf.Sin(tTime * 1.2f + i) * 3f; 
                float windJitter = Mathf.Sin(tTime * 2.7f + i * 0.5f) * 0.7f; 
                float totalRot = swing + windJitter;
                float yOffset = Mathf.Sin(tTime * 0.8f + i) * 0.7f; 
                t.localPosition = buttonInitialPositions[i] + new Vector3(0, yOffset, 0);
                t.localRotation = buttonInitialRotations[i] * Quaternion.Euler(0, 0, totalRot);
                t.localScale = buttonInitialScales[i]; 
            }
        }
    }
}
