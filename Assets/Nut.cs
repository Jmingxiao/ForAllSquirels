using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nut : MonoBehaviour
{
    float bobSpeed = 2;
    float bobHeight = 0.25f;

    // degrees per second
    float spin = 45.0f;

    public ParticleSystem ps;
    public MeshRenderer[] renderers;

    float destroyTimer = 2.0f;
    bool destroy = false;

    private void Update()
    {
        if (destroy)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer < 0)
                Destroy(gameObject);
        }
        else
        {
            Vector3 newPos = transform.position;

            newPos.y += bobHeight * Time.deltaTime * Mathf.Sin(Time.time * bobSpeed);
            transform.position = newPos;

            transform.Rotate(transform.up, spin * Time.deltaTime);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ps.Stop();

            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }
}
