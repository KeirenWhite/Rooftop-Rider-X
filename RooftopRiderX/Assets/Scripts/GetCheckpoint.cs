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
    private void OnTriggerEnter(Collider col)
    {
        col = redBlueCurrentPoint.GetComponent<Collider>();
        if (col.CompareTag("Bike"))
        {
            redBlueCurrentPoint.SetActive(false);
            score += 1;
            redBlueIndex = Random.Range(0, redBluePool.Length);
            redBlueCurrentPoint = redBluePool[redBlueIndex];
            SpawnObjective();

        }
    }

    private void SpawnObjective()
    {
        redBlueCurrentPoint.SetActive(true);

    }
}
