using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public GetCheckpoint objectiveManager;
    public AudioSource audioSource;
    //public ArrowPoint target;

    private void Start()
    {
        //target.ChangeTarget(this.transform.position);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Bike"))  
        {
            audioSource.Play();
            objectiveManager.GotObjective(this);

            col.gameObject.GetComponent<Boost>().RefillBoost(20f, true);
        }
    }
}
