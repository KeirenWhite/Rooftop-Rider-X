using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public GetCheckpoint objectiveManager;
    public ArrowPoint target;

    private void Start()
    {
        //target.ChangeTarget(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z));
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Bike"))  
        {
            objectiveManager.GotObjective(this);
        }
    }
}
