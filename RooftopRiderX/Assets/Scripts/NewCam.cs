using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewCam : MonoBehaviour
{
    [Header("Necessary Components")]
    [SerializeField] private Camera cam;
    [SerializeField] private CarRaycast bike;

    [Header("Properties")]
    [SerializeField] private float camOffset = 10f;

    // variables
    private Vector3 bikePos = Vector3.zero;
    private Vector3 bikeVel = Vector3.zero;

    private void Start()
    {
        
    }

    // initializes necessary variables at the beginning of each update frame
    private void FrameInitialize()
    {
        bikePos = bike.transform.position;
        bikeVel = bike.GetBikeVelocity();
        if (bikeVel.normalized == Vector3.zero)
            bikeVel = Vector3.forward;
    }

    void Update()
    {
        FrameInitialize();

        cam.transform.position = bikePos - (bikeVel.normalized * camOffset);
    }
}
