using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeParticleHandler : MonoBehaviour
{
    [SerializeField] private CarRaycast bike;
    [SerializeField] private ParticleSystem driftDustFront;
    [SerializeField] private ParticleSystem driftDustBack;

    private ParticleSystem.EmissionModule emissionFront;
    private ParticleSystem.EmissionModule emissionBack;

    private void Start()
    {
        emissionFront = driftDustFront.emission;
        emissionBack = driftDustBack.emission;
    }

    private void Update()
    {
        if (bike.input.gas > 0 && (bike.input.reverse > 0 || bike.input.brake > 0) && bike.input.grounded > 1)
        {
            if (!driftDustBack.isEmitting)
            {
                var emissionBack = driftDustBack.emission;
                emissionBack.enabled = true;  // Enable instantly
                driftDustBack.Play();
            }

            if (!driftDustFront.isEmitting)
            {
                var emissionFront = driftDustFront.emission;
                emissionFront.enabled = true;
                driftDustFront.Play();
            }
        }
        else
        {
            if (driftDustBack.isEmitting)
            {
                driftDustBack.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                StartCoroutine(DisableEmissionAfterDelay(driftDustBack, 0.05f));  // Short delay before disabling emission
            }

            if (driftDustFront.isEmitting)
            {
                driftDustFront.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                StartCoroutine(DisableEmissionAfterDelay(driftDustFront, 0.05f));
            }
        }
    }

    private IEnumerator DisableEmissionAfterDelay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        var emission = ps.emission;
        emission.enabled = false;
    }
}