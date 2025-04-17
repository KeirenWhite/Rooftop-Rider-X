using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.Processors;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class DeathScript : MonoBehaviour
{
    public Respawn respawn;
    public GetCheckpoint getCheckpoint;
    public GameObject deathCanvas;
    public TMP_Text currentScore;
    public TMP_Text currentLives;
    [HideInInspector] public bool dead = false;
    public GameObject bike;
    public CarRaycast bikeScript;
    private float input;
    public AudioSource fallSound;
    public GameObject musicManager;

    void Start()
    {
        deathCanvas.SetActive(false);
    }

    private void OnRespawn(InputValue value)
    {
        input = value.Get<float>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Bike"))
        {
            dead = true;
            deathCanvas.SetActive(true);
            fallSound.Play();
            currentLives.text = string.Format("Lives Remaining: {0}", getCheckpoint.lifeCounter);
            currentScore.text = string.Format("Current Score: {0}", getCheckpoint.score);
            bike.SetActive(false);
            musicManager.SetActive(false);
          
            /*if (dead)
            {
                if(input > 0)
                {
                    bike.SetActive(true);
                    respawn.RespawnBike();
                    dead = false;
                    deathCanvas.SetActive(false);
                }
            }*/
            //RespawnBike();
        }
    }

    private void Continue()
    {
        if (dead)
        {
            if (input > 0 && dead == true)
            {
                musicManager.SetActive(true);
                bike.SetActive(true);            
                respawn.RespawnBike();
                dead = false;
                deathCanvas.SetActive(false);
                return;
            }
        }
        else
        {
            return;
        }
    }
   

    // Update is called once per frame
    void FixedUpdate()
    {
        Continue();
    }
}
