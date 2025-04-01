using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;
using TMPro;

public class GetCheckpoint : MonoBehaviour
{
    private int score = 0;
    public GameObject[] redBluePool;
    private int redBlueIndex;
    private GameObject redBlueCurrentPoint;
    public ArrowPoint arrowPoint;
    public TMP_Text scoreText;
    

    private void Start()
    {
        //redBluePool = GameObject.FindGameObjectsWithTag("RedBlue");
        redBlueIndex = Random.Range(0, redBluePool.Length);
        redBlueCurrentPoint = redBluePool[redBlueIndex];

        UpdateCounterDisplay();

        foreach (GameObject objective in redBluePool)
        {
            Objective objectiveScript = objective.GetComponent<Objective>();
            if (objectiveScript != null)
            {
                objectiveScript.objectiveManager = this;
            }
        }

        SpawnObjective();
    }

    private void UpdateCounterDisplay()
    {
        scoreText.text = string.Format("Score: {0}", score);
        Debug.Log(score);
    }

    private void SpawnObjective()
    {
        redBlueIndex = Random.Range(0, redBluePool.Length);
        redBlueCurrentPoint = redBluePool[redBlueIndex];
        redBlueCurrentPoint.SetActive(true);
        arrowPoint.ChangeTarget(redBlueCurrentPoint.transform.position);

    }

    public void GotObjective(Objective objective)
    {
        objective.gameObject.SetActive(false);
        score += 100;
        UpdateCounterDisplay();
        SpawnObjective();
    }
}
