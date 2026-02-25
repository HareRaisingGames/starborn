using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public Slider _slider;
    protected float minValue = 0;
    protected float maxValue = 1;
    public TMP_Text text;
    public Func<float> value;
    [HideInInspector]
    public bool activate;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        if (_slider != null)
        {
            _slider.maxValue = maxValue;
            _slider.minValue = minValue;
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (!activate) return;
        _slider.value = Mathf.Clamp01(value != null ? value.Invoke() : 0);
        if(text != null)
        {
            text.text = $"{Mathf.Round(_slider.value * 100)}%";
        }
    }
}
