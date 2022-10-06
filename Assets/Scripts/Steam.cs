using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steam : MonoBehaviour
{
    [SerializeField] float force = 20.0f;
    [SerializeField] float forceDuration = 1.0f;
    bool applyingForce = false;
    private float forceTimer = 0;

    [SerializeField] float steamCooldown = 2.0f;
    [SerializeField] float steamDuration = 2.0f;
    private float timer = 0;

    ParticleSystem steam;
    Collider coll;

    private Rigidbody collidingRigidbody;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource = null;

    private void Start()
    {
        steam = GetComponent<ParticleSystem>();
        coll = GetComponent<Collider>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (!steam.isPlaying && timer > steamCooldown)
        {
            steam.Play();
            audioSource.Play();
            coll.enabled = true;
            timer = 0;
        }
        else if (steam.isPlaying && timer > steamDuration)
        {
            steam.Stop();
            coll.enabled = false;
            timer = 0;
        }
    }

    private void FixedUpdate()
    {
        if (applyingForce)
        {
            collidingRigidbody.AddForce(transform.up * force, ForceMode.Acceleration);

            forceTimer += Time.fixedDeltaTime;
            if (forceTimer > forceDuration)
            {
                applyingForce = false;
                forceTimer = 0;
                collidingRigidbody = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            collidingRigidbody = other.attachedRigidbody;
            applyingForce = true;
        }
    }
}
