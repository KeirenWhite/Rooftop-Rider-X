using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    //public CarRaycast CarRaycast;
    public Rigidbody rb;
    public AudioSource audioSourceZero;
    public AudioSource audioSourceLow;
    public AudioSource audioSourceMid;
    public AudioSource audioSourceTop;
    /*public AudioClip lowSpeed;
    public AudioClip midSpeed;
    public AudioClip highSpeed;
    public AudioClip topSpeed;*/
    private float currentSpeed;
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

        if(currentSpeed <= 10f)
        {
            /*AudioListener.FindAnyObjectByType<lowSpeed>() = 10000;
            audioSourceLow.priority = 0;
            audioSourceMid.priority = 0;
            audioSourceTop.priority = 0;*/
        }
    }
}
