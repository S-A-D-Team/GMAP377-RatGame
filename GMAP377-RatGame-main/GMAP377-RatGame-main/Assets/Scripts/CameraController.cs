using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float sensX;
    [SerializeField]
    private float sensY;


    [SerializeField]
    private Transform orientation;

    private float xRotate;
    private float yRotate;


    // Start is called before the first frame update
    void Start()
    {
        //Makes cursor not appear on screen and prevents cursor from moving around when playing
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Mouse Inputs
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        //Applies Rotation
        yRotate += mouseX;

        xRotate -= mouseY;
        //Limits Verticle rotation
        xRotate = Mathf.Clamp(xRotate, -90f, 0f);

        // Rotate cam and adjust orientation
        transform.rotation = Quaternion.Euler(xRotate, yRotate, 0);
        if(orientation != null) orientation.rotation = Quaternion.Euler(0, yRotate, 0);

    }
}
