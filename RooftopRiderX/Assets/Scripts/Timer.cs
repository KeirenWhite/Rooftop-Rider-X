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
    public Respawn respawn;
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
        while (time > 0)
        {
            yield return waitSeconds;

            time--;        

            UpdateTimerDisplay();
        }

        spawnManager.AddNewScore("TheGoatSamulock", getCheckpoint.score);

        /*respawn.RespawnBike();
        getCheckpoint.lifeCounter--;
        getCheckpoint.lives.text = string.Format("Lives: {0}", getCheckpoint.lifeCounter);
        time = 120;
        UpdateTimerDisplay();*/

        bike.SetActive(false);

        SceneManager.LoadScene("GameOver");
    }

    private void UpdateTimerDisplay()
    {
        timerText.text = string.Format("{0}", time);
    }
}
