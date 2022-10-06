using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fogControl : MonoBehaviour
{
    public Transform player;
    const float fullSize = 480;
    const float originDensity = 0.001f;
    const float coefficiency = 0.005f;
    // Start is called before the first frame update
    void Start()
    {   
        RenderSettings.fog = true;
        RenderSettings.fogDensity = originDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        StartCoroutine(UpdateFog());
    }

    IEnumerator UpdateFog()
    {
        while (true)
        {
            if (player.position.z < fullSize / 3)
            {
                PiorDensity();
            }
            else if (player.position.z >= fullSize / 3 && player.position.z < fullSize * 0.66f)
            {
                LateDensity();
            }
            else
            {
                RenderSettings.fogDensity = 0;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    void PiorDensity()
    {
        RenderSettings.fogDensity = player.position.z*Time.deltaTime*coefficiency+originDensity;
    }
    void LateDensity()
    {
        RenderSettings.fogDensity =  (fullSize*0.66f -player.position.z)*Time.deltaTime*coefficiency+originDensity;
    }
}
