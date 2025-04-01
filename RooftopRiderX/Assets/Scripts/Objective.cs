using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public GetCheckpoint objectiveManager;  

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Bike"))  
        {
            objectiveManager.GotObjective(this);
        }
    }
}
