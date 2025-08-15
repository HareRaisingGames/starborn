using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Starborn.InputSystem;

public class AudioAdjuster : MonoBehaviour
{
    AudioSource test;
    public Slider slider;
    public Charting charting = new Charting();

    //public RhythmInput rhythm = new RhythmInput(RhythmInputs.Down);
    // Start is called before the first frame update

    private void Awake()
    {
        StarbornInputSystem inputSystem = new StarbornInputSystem();
        //charting.Generate();
    }

    void Start()
    {
        test = GetComponent<AudioSource>();
        test.volume = 0;

        //AssetsManager.nun();
        //Debug.Log(rhythm.action);
        
    }

    private void OnEnable()
    {
        //rhythm.Enable();   
    }

    private void OnDisable()
    {
        //rhythm.Disable();
    }

    public void SetVolume()
    {
        test.volume = slider.value;
    }
}
