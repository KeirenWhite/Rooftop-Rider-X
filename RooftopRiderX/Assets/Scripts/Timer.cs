using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class Timer : MonoBehaviour
{
    public IEnumerator enumerator;
    //public int startTime;
    public int time;
    public int addTime;
    private WaitForSeconds waitSeconds;
    public TMP_Text timerText;
    public GetCheckpoint getCheckpoint;
    public GameObject bike;
    [SerializeField] private SpawnManagerScriptableObject spawnManager;
    //public DeathScript dead;
    //public Canvas deathCanvas;


    void Start()
    {
        

        waitSeconds = new WaitForSeconds(1);

        StartCoroutine(CountdownTimer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CountdownTimer()
    {
        while (time > 0 /*&& !deathCanvas.isActiveAndEnabled*/)
        {
            yield return waitSeconds;
            time--;

            UpdateTimerDisplay();
        }

        spawnManager.AddNewScore("TheGoatSamulock", getCheckpoint.score);

        bike.SetActive(false);

        SceneManager.LoadScene("GameOver");
    }

    private void UpdateTimerDisplay()
    {
        // Format the timer text to show MM:SS
        timerText.text = string.Format("{0}", time);
    }
}
