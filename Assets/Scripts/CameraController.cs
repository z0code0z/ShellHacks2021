using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public GameObject Player;
    public GameObject CameraAnchor;
    public GameObject ActualCamera;

    public float Sensitiviy = 1f;

    float X_Rotation = 0;
    float Y_Rotation = 0;

    bool Died = false;

    public void Death()
    {

        Died = true;

    }

    void Update()
    {
        if (Died) { return; }

        float MouseX = Input.GetAxis("Mouse X") * Sensitiviy;
        float MouseY = Input.GetAxis("Mouse Y") * Sensitiviy;


        X_Rotation = Mathf.Clamp(X_Rotation - MouseY, -90, 90);

        Y_Rotation += MouseX;

        Player.transform.eulerAngles = new Vector3(0, Y_Rotation, 0);
        ActualCamera.transform.localEulerAngles = new Vector3(X_Rotation, 0, 0);


    }

    
}
