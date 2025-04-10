using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Music : MonoBehaviour
{
    //public CarRaycast CarRaycast;
    public Rigidbody rb;
    public AudioSource audioSourceZero;
    public AudioSource audioSourceLow;
    public AudioSource audioSourceMid;
    public AudioSource audioSourceMidSolo;
    public AudioSource audioSourceTop;
    private float currentSpeed;
    private float currentTime = 0;
    public float duration = 2f;

    /*[SerializeField]*/ private float zeroTransition = 4f;
    /*[SerializeField]*/ private float lowerTransition = 56f;
    /*[SerializeField]*/ private float upperTransition = 90f;

    [SerializeField] private float zeroToLow = 5f;
    [SerializeField] private float lowToZero = 1f;
    [SerializeField] private float lowToMid = 56f;
    [SerializeField] private float midToLow = 38f;
    [SerializeField] private float midToTop = 88f;
    [SerializeField] private float topToMid = 72f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem sonicBoom;
    [SerializeField] private ParticleSystem speedLines;
    private GameObject speedlineObject;
    [SerializeField] private float lineSpeedMid = 20f;
    [SerializeField] private float lineSpeedTop = 40f;
    private ParticleSystem.EmissionModule speedlineEmission;

    [Header("Color")]
    [SerializeField] private GameObject stripe;
    private Material stripeMat;
    [SerializeField] private float colorChangeSpeed = 0.08f;

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float changeFOVSpeed = 0.2f;
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float midSpeedFOV = 75f;
    [SerializeField] private float topSpeedFOV = 90f;

    private int deltaTransition = 0;

    private enum MusicState
    {
        zeroSpeed,
        lowSpeed,
        midSpeed,
        topSpeed
    }

    private MusicState state = MusicState.zeroSpeed;

    void Start()
    {
        audioSourceZero.Play();
        audioSourceLow.Play();
        audioSourceMid.Play();
        audioSourceMidSolo.Play();
        audioSourceTop.Play();

        audioSourceLow.volume = 0f;
        audioSourceMid.volume = 0f;
        audioSourceMidSolo.volume = 0f;
        audioSourceTop.volume = 0f;

        stripeMat = stripe.GetComponent<MeshRenderer>().material;

        speedlineEmission = speedLines.emission;
        speedlineObject = speedLines.gameObject;
    }

    void FixedUpdate()
    {
        //DynamicMusic();
        DynamicMusicStateMachine();

        PositionSpeedLines();

        AlignSonicBoom();

        ColorChange();

        CamFOVChange();

        //Debug.Log(currentSpeed);
    }

    /*
    private void DynamicMusic()
    {
        currentSpeed = rb.velocity.magnitude;
        if (currentTime < 1)
            currentTime += Time.deltaTime * duration;
        audioSourceZero.volume = .5f;
        audioSourceLow.volume = 0;
        audioSourceMid.volume = 0;
        audioSourceMidSolo.volume = 0;
        audioSourceTop.volume = 0;
        float startZero = audioSourceZero.volume;
        float startLow = audioSourceLow.volume;
        float startMid = audioSourceMid.volume;
        float startMidSolo = audioSourceMidSolo.volume;
        float startTop = audioSourceTop.volume;
        //Debug.Log(currentSpeed);

        int currentTransition = 0;

        if(currentSpeed >= 0 && currentSpeed <= zeroTransition) //zero speed
        {
            currentTransition = 0;
            // reset timer if new transition
            if (currentTransition != deltaTransition)
                currentTime = 0f;

            audioSourceZero.volume = 1;
            // set all audio sources to correct volume
            audioSourceLow.volume = Mathf.Lerp(startLow, 0, currentTime);
            audioSourceMid.volume = Mathf.Lerp(startMid, 0, currentTime);
            audioSourceMidSolo.volume = Mathf.Lerp(startMidSolo, 0, currentTime);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime);

            // set deltaTransition so state isn't repeated
            deltaTransition = 0;
        }
        if(currentSpeed > zeroTransition && currentSpeed <= lowerTransition) //low speed
        {
            currentTransition = 1;
            if (currentTransition != deltaTransition)
                currentTime = 0f;
            audioSourceZero.volume = 1; 
            audioSourceLow.volume = Mathf.Lerp(startLow, 1, currentTime); 
            audioSourceMid.volume = Mathf.Lerp(startMid, 0, currentTime);
            audioSourceMidSolo.volume = Mathf.Lerp(startMidSolo, 0, currentTime);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime);
            deltaTransition = 1;
        }
        if(currentSpeed > lowerTransition && currentSpeed <= upperTransition) //moderate speed
        {
            currentTransition = 2;
            if (currentTransition != deltaTransition)
                currentTime = 0f;
            audioSourceZero.volume = 1;
            audioSourceLow.volume = 1;
            audioSourceMid.volume = Mathf.Lerp(startMid, 1, currentTime);
            audioSourceMidSolo.volume = Mathf.Lerp(startMidSolo, 1, currentTime);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime);
            deltaTransition = 2;
        }
        if(currentSpeed > upperTransition) //top speed
        {
            currentTransition = 3;
            if (currentTransition != deltaTransition)
                currentTime = 0f;
            audioSourceZero.volume = 1;
            audioSourceLow.volume = 1;
            audioSourceMid.volume = 1;
            audioSourceMidSolo.volume = 0;
            audioSourceTop.volume = Mathf.Lerp(startTop, 1, currentTime);
            deltaTransition = 3;
        }

        //Debug.Log(currentTime);

    }
    */

    private void AlignSonicBoom()
    {
        if (!sonicBoom.isPlaying) return;

        sonicBoom.gameObject.transform.LookAt(rb.velocity.normalized + sonicBoom.gameObject.transform.position);
    }

    private void DynamicMusicStateMachine()
    {
        currentSpeed = rb.velocity.magnitude;

        currentTime = 0.02f;

        //  current volume for each audiosource to ensure smooth lerping
        float startLow = audioSourceLow.volume;
        float startMid = audioSourceMid.volume;
        float startMidSolo = audioSourceMidSolo.volume;
        float startTop = audioSourceTop.volume;
        //Debug.Log(currentSpeed);

        switch (state)
        {
            case MusicState.lowSpeed:
                InitializeState();

                //  set relevant audio sources to correct volume
                audioSourceLow.volume = Mathf.MoveTowards(startLow, 1, currentTime);
                audioSourceMid.volume = Mathf.MoveTowards(startMid, 0, currentTime);
                audioSourceMidSolo.volume = Mathf.MoveTowards(startMidSolo, 0, currentTime);
                audioSourceTop.volume = Mathf.MoveTowards(startTop, 0, currentTime);

                //  change state
                if (currentSpeed >= lowToMid)
                    state = MusicState.midSpeed;
                else if (currentSpeed <= lowToZero)
                    state = MusicState.zeroSpeed;

                break;

            case MusicState.midSpeed:
                InitializeState();

                //  set relevant audio sources to correct volume
                audioSourceMid.volume = Mathf.MoveTowards(startMid, 1, currentTime);
                audioSourceMidSolo.volume = Mathf.MoveTowards(startMidSolo, 1, currentTime);
                audioSourceTop.volume = Mathf.MoveTowards(startTop, 0, currentTime);

                //  change state
                if (currentSpeed >= midToTop)
                    state = MusicState.topSpeed;
                else if (currentSpeed <= midToLow)
                    state = MusicState.lowSpeed;

                break;

            case MusicState.topSpeed:
                InitializeState();

                //  set relevant audio sources to correct volume
                audioSourceMidSolo.volume = Mathf.MoveTowards(startMidSolo, 0, currentTime);
                audioSourceTop.volume = Mathf.MoveTowards(startTop, 1, currentTime);

                //  change state
                if (currentSpeed <= topToMid)
                    state = MusicState.midSpeed;

                break;

            default:
            case MusicState.zeroSpeed:
                InitializeState();

                //  set relevant audio sources to correct volume
                audioSourceLow.volume = Mathf.MoveTowards(startLow, 0, currentTime);
                audioSourceMid.volume = Mathf.MoveTowards(startMid, 0, currentTime);
                audioSourceMidSolo.volume = Mathf.MoveTowards(startMidSolo, 0, currentTime);
                audioSourceTop.volume = Mathf.MoveTowards(startTop, 0, currentTime);

                //  change state
                if (currentSpeed >= zeroToLow)
                    state = MusicState.lowSpeed;

                break;
        }
    }

    [Header(" ----- Speed Colors ----- ")]
    [Header("Zero Speed")]
    [SerializeField] private Color zeroSpeedDark = Color.white;
    [SerializeField] private Color zeroSpeedLight = Color.black;
    [Header("Low Speed")]
    [SerializeField] private Color lowSpeedDark = Color.white;
    [SerializeField] private Color lowSpeedLight = Color.black;
    [Header("Mid Speed")]
    [SerializeField] private Color midSpeedDark = Color.white;
    [SerializeField] private Color midSpeedLight = Color.black;
    [Header("Top Speed")]
    [SerializeField] private Color topSpeedDark = Color.white;
    [SerializeField] private Color topSpeedLight = Color.black;

    private Color darkColor = Color.white;
    private Color lightColor = Color.white;
    private void ColorChange()
    {
        switch (state)
        {
            case MusicState.lowSpeed:
                darkColor = Vector4.MoveTowards(darkColor, lowSpeedDark, colorChangeSpeed);
                lightColor = Vector4.MoveTowards(lightColor, lowSpeedLight, colorChangeSpeed);
                break;
            case MusicState.midSpeed:
                darkColor = Vector4.MoveTowards(darkColor, midSpeedDark, colorChangeSpeed);
                lightColor = Vector4.MoveTowards(lightColor, midSpeedLight, colorChangeSpeed);
                break;
            case MusicState.topSpeed:
                darkColor = Vector4.MoveTowards(darkColor, topSpeedDark, colorChangeSpeed);
                lightColor = Vector4.MoveTowards(lightColor, topSpeedLight, colorChangeSpeed);
                break;
            default:
            case MusicState.zeroSpeed:
                darkColor = Vector4.MoveTowards(darkColor, zeroSpeedDark, colorChangeSpeed);
                lightColor = Vector4.MoveTowards(lightColor, zeroSpeedLight, colorChangeSpeed);
                break;
        }

        stripeMat.SetColor("_DarkColor", darkColor);
        stripeMat.SetColor("_BrightColor", lightColor);
    }

    private void CamFOVChange()
    {
        switch (state)
        {
            case MusicState.lowSpeed:
                cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, defaultFOV, changeFOVSpeed);
                break;
            case MusicState.midSpeed:
                cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, midSpeedFOV, changeFOVSpeed);
                break;
            case MusicState.topSpeed:
                cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, topSpeedFOV, changeFOVSpeed);
                break;
            default:
            case MusicState.zeroSpeed:
                cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, defaultFOV, changeFOVSpeed);
                break;
        }

    }

    private float SmoothMoveTowardsDelta(float current, float target, float maxDelta)
    {
        float newMaxDelta = maxDelta;

        if (Mathf.Abs(target) / Mathf.Abs(current) > 0.8f)
        {
            newMaxDelta = maxDelta * (5 * (1 - (Mathf.Abs(target) / Mathf.Abs(current))));
        }

        return Mathf.MoveTowards(current, target, newMaxDelta);
    }


    private void InitializeState()
    {
        // reset timer if new state
        if (deltaTransition != (int)state)
        {
            switch ((int)state)
            {
                case 1:
                    speedLines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    StartCoroutine(DisableEmissionAfterDelay(speedLines, 0.05f));
                    break;
                case 2:
                    speedlineEmission.rateOverTime = lineSpeedMid;
                    speedlineEmission.enabled = true;
                    speedLines.Play();
                    break;
                case 3:
                    speedlineEmission.rateOverTime = lineSpeedTop;
                    speedlineEmission.enabled = true;
                    speedLines.Play();
                    break;
                default:
                case 0:
                    //speedLines.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    //StartCoroutine(DisableEmissionAfterDelay(speedLines, 0.05f));
                    break;
            }

            currentTime = 0f;
            deltaTransition = (int)state;

            if (state == MusicState.topSpeed)
                sonicBoom.Play();
        }
        //Debug.Log(state);
    }

    private void PositionSpeedLines()
    {
        if (state == MusicState.lowSpeed || state == MusicState.zeroSpeed) return;

        speedlineObject.transform.localPosition = new Vector3(0f, 0f, Mathf.Clamp(rb.velocity.magnitude, 65f, 130f));

    }

    private IEnumerator DisableEmissionAfterDelay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        var emission = ps.emission;
        emission.enabled = false;
    }
}
