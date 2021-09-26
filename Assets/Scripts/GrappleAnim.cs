using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleAnim : MonoBehaviour
{


    public Vector3 HookPosition;
    public Transform PlayerGrapplePoint;
    public GameObject Hook;
    public LineRenderer lr;

    public GameObject[] Points;


    private void Start()
    {

        Points[5].transform.up = (HookPosition - Points[5].transform.position).normalized;

    }


    private void FixedUpdate()
    {

        transform.position = PlayerGrapplePoint.position;

        var points = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {

            points[i] = Points[i].transform.localPosition;

        }
        lr.SetPositions(points);


        if (Vector3.SqrMagnitude(Hook.transform.position - HookPosition) > 10)
        {


            Hook.transform.position = Vector3.Slerp(Hook.transform.position, HookPosition, 17.5f * Time.fixedDeltaTime);

        }
        else
        {

            Hook.transform.position = HookPosition;

        }




    }






}
