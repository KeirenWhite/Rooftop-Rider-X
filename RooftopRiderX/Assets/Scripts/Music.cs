using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Music : MonoBehaviour
{
    //public CarRaycast CarRaycast;
    public Rigidbody rb;
    public AudioSource audioSourceZero;
    public AudioSource audioSourceLow;
    public AudioSource audioSourceMid;
    public AudioSource audioSourceTop;
    private float currentSpeed;
    private float currentTime = 0;
    public float duration = 2f;

    [SerializeField] private float zeroTransition = 4f;
    [SerializeField] private float lowerTransition = 56f;
    [SerializeField] private float upperTransition = 90f;

    void Start()
    {
        audioSourceZero.Play();
        audioSourceLow.Play();
        audioSourceMid.Play();
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
        currentTime += Time.deltaTime;
        audioSourceZero.volume = .5f;
        audioSourceLow.volume = 0;
        audioSourceMid.volume = 0;
        audioSourceTop.volume = 0;
        float startZero = audioSourceZero.volume;
        float startLow = audioSourceLow.volume;
        float startMid = audioSourceMid.volume;
        float startTop = audioSourceTop.volume;
        Debug.Log(currentSpeed);

        if(currentSpeed >= 0 && currentSpeed <= zeroTransition) //zero speed
        {
            audioSourceZero.volume = 1;
            audioSourceLow.volume = Mathf.Lerp(startLow, 0, currentTime / duration);
            audioSourceMid.volume = Mathf.Lerp(startMid, 0, currentTime / duration);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime / duration); 
        }
        if(currentSpeed > zeroTransition && currentSpeed <= lowerTransition) //low speed
        {
            audioSourceZero.volume = 0; 
            audioSourceLow.volume = Mathf.Lerp(startLow, .9f, currentTime / 10f); 
            audioSourceMid.volume = Mathf.Lerp(startMid, 0, currentTime / duration);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime / duration);
        }
        if(currentSpeed > lowerTransition && currentSpeed <= upperTransition) //moderate speed
        {
            audioSourceZero.volume = 0;
            audioSourceLow.volume = Mathf.Lerp(startLow, 0, currentTime / duration);
            audioSourceMid.volume = Mathf.Lerp(startMid, .7f, currentTime / duration);
            audioSourceTop.volume = Mathf.Lerp(startTop, 0, currentTime / duration);
        }
        if(currentSpeed > upperTransition) //top speed
        {
            audioSourceZero.volume = 0;
            audioSourceLow.volume = Mathf.Lerp(startLow, 0, currentTime / duration);
            audioSourceMid.volume = Mathf.Lerp(startMid, 0, currentTime / duration);
            audioSourceTop.volume = Mathf.Lerp(startTop, .8f, currentTime / duration);
        }
    }
}
