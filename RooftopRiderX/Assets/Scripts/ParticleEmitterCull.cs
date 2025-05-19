using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitterCull : MonoBehaviour
{
    private ParticleSystem particles;

    private float timeElapsed;
    [SerializeField] private float destroyTimer = 0.5f;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > destroyTimer)
            Destroy(gameObject);
    }
}
