using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeParticleHandler : MonoBehaviour
{
    [SerializeField] private CarRaycast bike;

    [SerializeField] private ParticleSystem driftDustFront;
    [SerializeField] private ParticleSystem driftDustBack;
    [SerializeField] private ParticleSystem trickParticles;

    private void Update()
    {
        if (bike.input.gas > 0 && (bike.input.reverse > 0 || bike.input.brake > 0) && bike.input.grounded > 1)
        {
            if (!trickParticles.isEmitting)
            {
                var emissionTrick = trickParticles.emission;
                emissionTrick.enabled = true;
                trickParticles.Play();
            }

            if (!driftDustBack.isEmitting)
            {
                var emissionBack = driftDustBack.emission;
                emissionBack.enabled = true;
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
            if (trickParticles.isEmitting)
            {
                trickParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                StartCoroutine(DisableEmissionAfterDelay(trickParticles, 0.05f));
            }

            if (driftDustBack.isEmitting)
            {
                driftDustBack.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                StartCoroutine(DisableEmissionAfterDelay(driftDustBack, 0.05f));
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