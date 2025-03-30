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

    void Update()
    {
        if (bike.input.gas > 0 && (bike.input.reverse > 0 || bike.input.brake > 0) && bike.input.grounded > 1)
        {
            emissionFront.rateOverTime = 10f;
            emissionFront.rateOverDistance = 1.2f;
            emissionBack.rateOverTime = 10f;
            emissionBack.rateOverDistance = 1.2f;
            if (!driftDustBack.isPlaying)
            {
                driftDustBack.Clear();
                driftDustBack.Emit(1);
                driftDustBack.Play();
            }

            if (!driftDustFront.isPlaying)
            {
                driftDustFront.Clear();
                driftDustFront.Emit(1);
                driftDustFront.Play();
            }
        }
        else
        {
            emissionFront.rateOverTime = 0f;
            emissionFront.rateOverDistance = 0f;
            emissionBack.rateOverTime = 0f;
            emissionBack.rateOverDistance = 0f;
            driftDustBack.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            driftDustFront.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
