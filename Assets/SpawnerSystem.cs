using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSystem : MonoBehaviour
{


    public GameObject[] ObjectPool;
    public GameObject Player;

    public GameObject ActualHook;

    public float XDirectionFloat = 10f;
    public float YDirectionFloat = 5f;
    public float SpawnDistanceTick = 300f;
    public float ActualHookRelativeSpawn = 10f;

    float CurrentZSpawnPoint = 25f;
    public float CurrentDistAdd = 25f;
    float BonusDist = 0f;


    int i = 0;


    private void Update()
    {
        

        if(Mathf.Abs(CurrentZSpawnPoint - Player.transform.position.z) < SpawnDistanceTick)
        {

            //start spawning!!!

            ObjectPool[i].transform.position = new Vector3(Random.Range(-XDirectionFloat, XDirectionFloat), Random.Range(-YDirectionFloat, YDirectionFloat) + 7.5f, CurrentZSpawnPoint);




            //game might already be too hard lol
            //Instantiate(ActualHook, new Vector3(Random.Range(-XDirectionFloat, XDirectionFloat) / 1.2f, Random.Range(-YDirectionFloat, YDirectionFloat)/3f, CurrentZSpawnPoint + ActualHookRelativeSpawn), Quaternion.identity);

            if (BonusDist <= 15f)
            {
                BonusDist += .20f;
            }

            CurrentZSpawnPoint += CurrentDistAdd + BonusDist;


            i++;

            if(i == ObjectPool.Length - 1)
            {
                //reset the count
                i = 0;
            }


        }



    }













}
