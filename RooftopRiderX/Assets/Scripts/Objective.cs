using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public GetCheckpoint objectiveManager;
    //public ArrowPoint target;

    private void Start()
    {
        //target.ChangeTarget(this.transform.position);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Bike"))  
        {
            objectiveManager.GotObjective(this);
        }
    }
}
