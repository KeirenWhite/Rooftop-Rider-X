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
    private bool dead = false;
    public GameObject bike;
    public CarRaycast bikeScript;
    private float input;

    void Start()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Bike"))
        {
            deathCanvas.SetActive(true);
            dead = true;
            currentLives.text = string.Format("Lives Remaining: {0}", getCheckpoint.lifeCounter);
            currentScore.text = string.Format("Current Score: {0}", getCheckpoint.score);
            bike.SetActive(false);
            if (dead)
            {
                if(input > 0)
                {
                    bike.SetActive(true);
                    respawn.RespawnBike();
                    dead = false;
                    deathCanvas.SetActive(false);
                }
            }
            //RespawnBike();
        }
    }

    private void OnContinue(InputValue value)
    {
        input = value.Get<float>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
