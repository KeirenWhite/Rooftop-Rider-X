using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Boost : MonoBehaviour
{
    [SerializeField] private float forwardForce = 100f;

    private float input;

    private Rigidbody rb;

    [SerializeField] private Transform forcePos;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (input > 0)
        {
            rb.AddForceAtPosition((transform.forward * forwardForce), forcePos.position, ForceMode.Force);
        }
    }

    private void OnBoost(InputValue value)
    {
        input = value.Get<float>();
    }
}
