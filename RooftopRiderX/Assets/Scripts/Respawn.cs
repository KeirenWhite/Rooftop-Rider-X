using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Respawn : MonoBehaviour
{
    public GameObject bike;
    public Camera mainCamera;
    public Rigidbody bikeRb;
    public GameObject respawnPoint;
    /*public AudioSource fallSound;*/
    public GetCheckpoint getCheckpoint;
    //public TMP_Text lives;
    

    /*private void OnRespawn(InputValue value)
    {
        RespawnBike();
    }*/

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Bike"))
        {
            //RespawnBike();
            getCheckpoint.lifeCounter--;
            getCheckpoint.lives.text = string.Format("Lives: {0}", getCheckpoint.lifeCounter);
        }
    }
    
    public void RespawnBike()
    {
        bikeRb.velocity = Vector3.zero;
        bikeRb.angularVelocity = Vector3.zero;
        bike.transform.position = respawnPoint.transform.position;
        bike.transform.eulerAngles = respawnPoint.transform.eulerAngles;
        mainCamera.transform.position = respawnPoint.transform.position;
        //fallSound.Play();
        bike.GetComponent<Boost>().SetBoostToMax();
    }

    /*private void GameOver()
    {
        if (lifeCounter <= 0)
        {
            bike.SetActive(false);
        }
    }*/
    
}
