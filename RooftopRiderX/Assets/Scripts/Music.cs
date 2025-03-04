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

    [SerializeField] private float zeroTransition = 4f;
    [SerializeField] private float lowerTransition = 50f;
    [SerializeField] private float upperTransition = 130f;

    private int deltaTransition = 0;

    void Start()
    {
        audioSourceZero.Play();
        audioSourceLow.Play();
        audioSourceMid.Play();
        audioSourceMidSolo.Play();
        audioSourceTop.Play();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DynamicMusic();
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
            if (currentTransition != deltaTransition)
                currentTime = 0f;
            audioSourceZero.volume = 1;
            audioSourceLow.volume = Mathf.Lerp(startLow, 0, currentTime);
            audioSourceMid.volume = Mathf.Lerp(startMid, 0, currentTime);
            audioSourceMidSolo.volume = Mathf.Lerp(startMidSolo, 0, currentTime);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime);
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
}
