using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyHookSystem : MonoBehaviour
{

    public GameObject Rotator;

    float Speed;



    private void Start()
    {

        int i = Random.Range(1, 3);
        if(i == 1)
        {
            Speed = Random.Range(-10f, -1f);

        }
        else
        {
            Speed = Random.Range(1f, 10f);
        }


    }

    private void Update()
    {


        Rotator.transform.Rotate(transform.up,Speed * 15 * Time.deltaTime);


    }







}
