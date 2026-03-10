using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using System.IO;

public class Screenshot : MonoBehaviour
{
    public bool activate;
    private StarbornInputSystem m_inputSystem;
    public string filename;
    int i = 0;
    void Awake()
    {
        m_inputSystem = new StarbornInputSystem();
        if (activate)
        {
            m_inputSystem.Rhythm.Down.performed += Snap;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        m_inputSystem.Enable();
    }

    private void OnDisable()
    {
        m_inputSystem.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Snap(InputAction.CallbackContext context)
    {
        while (File.Exists($"{Application.dataPath}/{filename}_{i}.png"))
            i++;
        ScreenCapture.CaptureScreenshot($"{Application.dataPath}/{filename}_{i}.png");
        i++;
    }
}
