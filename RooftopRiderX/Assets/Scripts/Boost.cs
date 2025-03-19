using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Boost : MonoBehaviour
{
    [SerializeField] private float forwardForce = 100f;
    [SerializeField] private Transform forcePos;
    private float input;
    private Rigidbody rb;

    private float boostVal = 100f;
    [SerializeField] private Slider boostSlider;
    [SerializeField] private float groundedBoostUseMult = 10f;
    [SerializeField] private float aerialBoostUseMult = 10f;
    [SerializeField] private float boostRefillMult = 1f;
    [SerializeField] private float trickMultiplier = 0.25f;

    [SerializeField] private CarRaycast bikeScript;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (input > 0 && boostVal > 0 && !bikeScript.input.downed)
        {
            rb.AddForceAtPosition((transform.forward * forwardForce), forcePos.position, ForceMode.Force);
            boostVal -= Time.deltaTime * (bikeScript.input.grounded == 2 ? groundedBoostUseMult : aerialBoostUseMult);
            boostSlider.value = boostVal;
        }
        else if (input == 0 && boostVal < 100 && bikeScript.input.grounded > 0)
        {
            boostVal += Time.deltaTime * boostRefillMult;
            boostSlider.value = boostVal;
        }
    }

    private void OnBoost(InputValue value)
    {
        input = value.Get<float>();
    }

    public void RefillBoost(float addBoost)
    {
        //Debug.Log(addBoost);

        boostVal += addBoost * trickMultiplier;
        if (boostVal > 100)
            boostVal = 100;

        boostSlider.value = boostVal;
    }

}
