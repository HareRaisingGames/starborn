using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundByte : MonoBehaviour
{
    [HideInInspector] public float timeSamples;

    // Update is called once per frame
    void Update()
    {
        if(GetComponent<AudioSource>().isPlaying)
        {
            timeSamples = GetComponent<AudioSource>().timeSamples;
        }

        if (GetComponent<AudioSource>().timeSamples < timeSamples && timeSamples > 0)
        {
            Destroy(gameObject);
        }
    }
}
