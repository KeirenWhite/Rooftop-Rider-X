using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GetCheckpoint : MonoBehaviour
{
    private int score;
    public GameObject[] redBluePool;
    private int redBlueIndex;
    private GameObject redBlueCurrentPoint;

    private void Start()
    {
        //redBluePool = GameObject.FindGameObjectsWithTag("RedBlue");
        redBlueIndex = Random.Range(0, redBluePool.Length);
        redBlueCurrentPoint = redBluePool[redBlueIndex];

        SpawnObjective();
    }
    /*private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bike")
        {
            gameObject.SetActive(false);
            redBluePool.
        }
    }*/

    private void SpawnObjective()
    {
        redBlueCurrentPoint.SetActive(true);
    }
}
