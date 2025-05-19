using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Boost : MonoBehaviour
{
    [SerializeField] private float forwardForce = 100f;
    [SerializeField] private Transform forcePos;
    public float input;
    private Rigidbody rb;
    public float boostVal = 100f;
    [SerializeField] private Slider boostSlider;
    [SerializeField] private RectTransform fill;
    [SerializeField] private float boostShakeAmount = 4f;
    private Vector3 fillStartPos;
    [SerializeField] private float groundedBoostUseMult = 10f;
    [SerializeField] private float aerialBoostUseMult = 10f;
    [SerializeField] private float boostRefillMult = 1f;
    [SerializeField] private float trickMultiplier = 0.25f;
    [SerializeField] private float driftMultSensitivity = 3f;
    [SerializeField] private float driftMultiplier = 3f;
    public GetCheckpoint getCheckpoint;

    [SerializeField] private CarRaycast bikeScript;

    [SerializeField] private ParticleSystem stars;
    private ParticleSystem.EmissionModule starEmission;

    private float driftTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        fillStartPos = fill.transform.position;

        rb = gameObject.GetComponent<Rigidbody>();
        starEmission = stars.emission;
    }

    private void FixedUpdate()
    {
        if (input > 0 && boostVal > 0 && !bikeScript.input.downed)
        {
            rb.AddForceAtPosition((transform.forward * forwardForce), forcePos.position, ForceMode.Force);
            boostVal -= Time.deltaTime * (bikeScript.input.grounded == 2 ? groundedBoostUseMult : aerialBoostUseMult);
            boostSlider.value = boostVal;

            BoostShake();
        }
        else
        {
            fill.transform.position = fillStartPos;
        }
        
        if (input == 0 && boostVal < 100 && bikeScript.input.grounded > 0)
        {
            boostVal += (Time.deltaTime * boostRefillMult) * DriftMultiplier();
            boostSlider.value = boostVal;
        }
    }

    private void BoostShake()
    {
        Vector3 shake = new Vector3(Random.Range(-boostShakeAmount, boostShakeAmount), Random.Range(-boostShakeAmount, boostShakeAmount), 0);

        fill.position = fillStartPos + shake;
    }

    private void OnBoost(InputValue value)
    {
        input = value.Get<float>();
    }

    private float DriftMultiplier()
    {
        if (bikeScript.input.gas > 0 && (bikeScript.input.reverse > 0 || bikeScript.input.brake > 0) && bikeScript.input.grounded > 1)
        {
            if (input > 0)
            {
                driftTimer = 0f;
            }

            driftTimer += Time.deltaTime;
            return  2 + Mathf.Pow(driftMultSensitivity, driftTimer / driftMultiplier);
        }
        else
        {
            driftTimer = 0f;
            return 1f;
        }

    }

   

    public void RefillBoost(float addBoost, bool overrideTrick = false)
    {
        //Debug.Log(addBoost);

        boostVal += addBoost * (overrideTrick ? 1f : trickMultiplier);
        if (boostVal > 100)
            boostVal = 100;

        starEmission.rateOverTime = 5f * addBoost;
        stars.Play();

        boostSlider.value = boostVal;
    }

    public void SetBoostToMax()
    {
        boostVal = 100;
        boostSlider.value = boostVal;
    }
}
