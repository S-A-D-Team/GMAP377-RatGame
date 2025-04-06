using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField]
    private Transform cameraPos;

    // Update is called once per frame
    void Update()
    {
        //Keeps the camera in line with the player
        transform.position = cameraPos.position;
    }
}
