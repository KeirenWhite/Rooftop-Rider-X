using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TrickScriptableObject")]
public class TrickScriptableObject : ScriptableObject
{
    /*** Object Variables ***/
    bool inAir;
    bool holdOK;

    /*** Tracked Variables ***/
    /* 
    * Core variables:
    * - Velocity
    *      - float minimumMagnitude;
    *      - bool trackMoveDirection;
    *           - Vector3 targetMoveDirection;
    * - Face direction
    *      - bool trackFaceDirection
    *           - Vector3 targetFaceDirection;
    *           - bool relativeToVelocity;
    * 
    * Air variables:
    * - Rotation
    *      - bool trackRotation;
    *           - Vector3 rotationDifference;
    * - Boost
    *       - bool trackBoost
    *           - bool bonusForBoost;
    *               - float boostMultiplier;
    *           - bool boostRequired;
    *               - float boostAmount;
    * 
    * Ground variables:
    * - Drift
    *       - bool trackDrift
    *           - float targetDriftTime;
    *           - float minimumDriftSharpness;
    * - Ground normal
    * - Accelerate/Reverse
    * - Grounded wheels
    * 
    */

    //Note: Hangtime points are applied on bike landing




}