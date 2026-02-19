using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Slider _slider;
    public float minValue = 0;
    public float maxValue;
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
        
    }
}
