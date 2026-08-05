using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundByte : MonoBehaviour
{
    [HideInInspector] public float timeSamples;
    public string type
    {
        set
        {
            MixerSettings.SetAudioGroup(GetComponent<AudioSource>(), value);
        }
    }

    private void Awake()
    {
        type = "SFX";
    }

    void Update()
    {
        AudioSource src = GetComponent<AudioSource>();
        if (src.isPlaying)
        {
            timeSamples = src.timeSamples;
        }
        else if (timeSamples > 0 && !src.loop)
        {
            Destroy(gameObject);
            return;
        }

        if (src.timeSamples < timeSamples && timeSamples > 0)
        {
            Destroy(gameObject);
        }
    }
}
