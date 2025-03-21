using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public GameObject bike;
    public Camera mainCamera;
    public Rigidbody bikeRb;
    public GameObject respawnPoint;
    public AudioSource fallSound;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Bike"))
        {
            RespawnBike();
        }
    }
    
    private void RespawnBike()
    {
        bikeRb.velocity = Vector3.zero;
        bike.transform.position = respawnPoint.transform.position;
        bike.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        mainCamera.transform.position = respawnPoint.transform.position;
        fallSound.Play();
    }
    
}
