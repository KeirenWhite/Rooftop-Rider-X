using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class GrindRail : MonoBehaviour
{
    public Rigidbody rb;
    private GameObject endPoint;
    public float fixedRotationX;
    public float fixedRotationY;
    public float fixedRotationZ;
    private bool grinding;
    public float launchStrength = 100f;
    public float grindSpeed = 100f;
    public CarRaycast carRaycast;
    private Vector3 grindTowards;

    private Boost boostScript;

    private string endPointTag = "EndGrindA";

    void Start()
    {
        boostScript = carRaycast.gameObject.GetComponent<Boost>();
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

            DetermineEndPoint(col);
            grinding = true;

            boostScript.GrindBoostEnabled(true);
        }
        if (col.CompareTag(endPointTag))
        {
            grinding = false;
            boostScript.GrindBoostEnabled(false);
        }
    }
    private void Grind()
    {
        if (endPoint == null)
            return;

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
           if (carRaycast.input.jump > 0)
           {
               grinding = false;
               rb.freezeRotation = false;
               rb.AddForce(Vector3.up * launchStrength * carRaycast.input.jump);
               boostScript.GrindBoostEnabled(false);
           }
        }  
    }

    private void DetermineEndPoint(Collider col)
    {
        if (grinding)
            return;

        Vector3 aPointAngle = col.GetComponentInParent<Transform>().transform.rotation * Vector3.up;
        Vector3 bikeVel = carRaycast.worldVelocity;

        float dot = Vector2.Dot(aPointAngle.normalized, bikeVel.normalized);

        Debug.Log("point A angle: " + aPointAngle.normalized + "      bike vel: " + bikeVel.normalized + "      dot: " + dot);

        if (dot >= 0)
        {
            endPoint = col.gameObject.GetComponentInParent<EndPointHolder>().endPointA;
            endPointTag = "EndGrindA";
            return;
        }
        else if (dot < 0)
        {
            endPoint = col.gameObject.GetComponentInParent<EndPointHolder>().endPointB;
            endPointTag = "EndGrindB";
            return;
        }
    }
}
