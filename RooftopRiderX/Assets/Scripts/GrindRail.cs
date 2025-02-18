using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class GrindRail : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject endPoint;
    public float fixedRotationX;
    public float fixedRotationY;
    public float fixedRotationZ;
    private bool grinding;
    public float launchStrength = 100f;
    public float grindSpeed = 100f;
    public CarRaycast CarRaycast;
    private Vector3 grindTowards;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FreezeRotation();
        Grind();
        ExitGrind();
    }

    private void FreezeRotation()
    {
        if (grinding)
        {
            //Vector3 eulerAngles = transform.eulerAngles;
            //rb.transform.eulerAngles = new Vector3(fixedRotationX, fixedRotationY, fixedRotationZ);
            rb.freezeRotation = true;
        }
        else
        {
            rb.freezeRotation = false;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Grind"))
        {
            Debug.Log("Grinding");
            col.gameObject.GetComponentInChildren<Collider>();
            grinding = true;
        }
        if (col.CompareTag("EndGrind"))
        {
            grinding = false;
        }
    }
    private void Grind()
    {
        Vector3 grindTowards = (endPoint.transform.position - transform.position).normalized;
        if (grinding)
        {
            rb.AddForce(grindTowards * grindSpeed, ForceMode.Acceleration);
        }

    }

    private void ExitGrind()
    {
        if (grinding)
        {
           if (CarRaycast.input.jump > 0)
           {
               grinding = false;
               rb.freezeRotation = false;
               rb.AddForce(Vector3.up * launchStrength * CarRaycast.input.jump);
           }
        }  
    }
}
