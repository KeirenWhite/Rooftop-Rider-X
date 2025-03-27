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

    [SerializeField] private Material stripeMat;

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
    }

    void FixedUpdate()
    {
        //DynamicMusic();

        DynamicMusicStateMachine();

        Debug.Log(currentSpeed);
    }

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

    private void ColorChange(int colorState)
    {
        switch (colorState)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            default:
            case 0:
                break;
        }
    }

    private void InitializeState()
    {
        // reset timer if new state
        if (deltaTransition != (int)state)
        {
            currentTime = 0f;
            deltaTransition = (int)state;
        }

        Debug.Log(state);
    }
}
