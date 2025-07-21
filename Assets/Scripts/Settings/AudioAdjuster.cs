using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioAdjuster : MonoBehaviour
{
    AudioSource test;
    public Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        test = GetComponent<AudioSource>();
        test.volume = 0;
    }

    public void SetVolume()
    {
        test.volume = slider.value;
    }
}
