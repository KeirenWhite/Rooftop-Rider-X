using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrailHandler : MonoBehaviour
{
    [Header("Needed Scripts")]
    [SerializeField] private CarRaycast bike;
    [SerializeField] private Boost boost;

    [Header("Trails")]
    [SerializeField] private GameObject driveTrail;
    [SerializeField] private GameObject boostTrail;

    [Header("Fields")]
    [SerializeField] private float drivePositionVariation = 0.1f;
    [SerializeField] private float driveScaleVariation = 0.2f;
    [SerializeField] private float boostPositionVariation = 0.1f;
    [SerializeField] private float boostScaleVariation = 0.2f;

    // variables
    private Vector3 driveStartPos = Vector3.zero;
    private Vector3 driveStartScale = Vector3.one;
    private Vector3 boostStartPos = Vector3.zero;
    private Vector3 boostStartScale = Vector3.one;
    private int trailID = 0; // 0 + none, 1 = drive, 2 = boost

    private bool isBoosting = true;

    private void Start()
    {
        driveStartPos = driveTrail.transform.localPosition;
        driveStartScale = driveTrail.transform.localScale;
        boostStartPos = boostTrail.transform.localPosition;
        boostStartScale = boostTrail.transform.localScale;
    }

    void Update()
    {
        DetermineTrail();

        if (trailID == 1)
            TrailJitter(driveTrail, driveStartPos, driveStartScale, false);
        else if (trailID == 2)
            TrailJitter(boostTrail, boostStartPos, boostStartScale, true);
    }

    private void DetermineTrail()
    {
        if (bike.input.gas > 0) 
        { 
            driveTrail.SetActive(true);

            trailID = 1;
        }
        else driveTrail.SetActive(false);

        if (boost.input > 0 && boost.boostVal > 0)
        {
            isBoosting = true;
        }
        else
        {
            isBoosting = false;
        }

        if (isBoosting)
        {
            boostTrail.SetActive(true);
            driveTrail.SetActive(false);

            trailID = 2;
        }
        else boostTrail.SetActive(false);

        if (bike.input.gas == 0 && !isBoosting)
        {
            trailID = 0;
        }
    }

    private void TrailJitter(GameObject trail, Vector3 trailPos, Vector3 trailScale, bool useBoost)
    {
        if (!useBoost)
        {
            trail.transform.localPosition = trailPos + new Vector3(Random.Range(-drivePositionVariation, drivePositionVariation), Random.Range(-drivePositionVariation, drivePositionVariation), Random.Range(-drivePositionVariation, drivePositionVariation));
            trail.transform.localScale = trailScale + new Vector3(Random.Range(-driveScaleVariation, driveScaleVariation), Random.Range(-driveScaleVariation - 0.0015f, driveScaleVariation + 0.0015f), Random.Range(-driveScaleVariation, driveScaleVariation));
        }
        else
        {
            trail.transform.localPosition = trailPos + new Vector3(Random.Range(-boostPositionVariation, boostPositionVariation), Random.Range(-boostPositionVariation, boostPositionVariation), Random.Range(-boostPositionVariation, boostPositionVariation));
            trail.transform.localScale = trailScale + new Vector3(Random.Range(-boostScaleVariation, boostScaleVariation), Random.Range(-boostScaleVariation, boostScaleVariation), Random.Range(-boostScaleVariation, boostScaleVariation));
        }
    }
}
