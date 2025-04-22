using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.SceneManagement;

public class GetCheckpoint : MonoBehaviour
{
    public int score = 0;
    public GameObject[] redBluePool;
    private int redBlueIndex;
    private GameObject redBlueCurrentPoint;
    public GameObject arrow;
    private GameObject arrowObject;
    public int lifeCounter = 3;
    public TMP_Text scoreText;
    public TMP_Text lives;
    public GameObject bike;
    public AudioSource audioSource;
    public Timer time;
    [SerializeField] private SpawnManagerScriptableObject spawnManager;

    [SerializeField] private Material redMat;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material yellowMat;
    [SerializeField] private Material greenMat;
    [SerializeField] private Material grayMat;

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

    private void Update()
    {
        GameOver();
    }
    public void UpdateCounterDisplay()
    {
        scoreText.text = string.Format("Score: {0}", score);
        Debug.Log(score);
    }

    private void SpawnObjective()
    {
        redBlueIndex = Random.Range(0, redBluePool.Length);
        redBlueCurrentPoint = redBluePool[redBlueIndex];
        redBlueCurrentPoint.SetActive(true);

        MeshRenderer arrowMat = arrow.GetComponent<MeshRenderer>();

        switch (redBlueCurrentPoint.tag)
        {
            default:
            case "Red":
                arrowMat.material = redMat;
                break;
            case "Blue":
                arrowMat.material = blueMat;
                break;
            case "Yellow":
                arrowMat.material = yellowMat;
                break;
            case "Green":
                arrowMat.material = greenMat;
                break;
            case "Gray":
                arrowMat.material = grayMat;
                break;
        }

        arrow.GetComponent<ArrowPoint>().ChangeTarget(redBlueCurrentPoint.transform.position);
        
    }

    public void GotObjective(Objective objective)
    {
        objective.gameObject.SetActive(false);
        score += 1000;
        audioSource.Play();
        UpdateCounterDisplay();
        SpawnObjective();
        time.time += time.addTime;
    }

    private void ExtraLife()
    {

    }

    private void GameOver()
    {
        if (lifeCounter <= 0)
        {
            spawnManager.AddNewScore("TheGoatSamulock", score);

            bike.SetActive(false);

            SceneManager.LoadScene("GameOver");
        }
    }
}
