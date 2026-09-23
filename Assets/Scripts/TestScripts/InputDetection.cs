using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Starborn.InputSystem;

public class InputDetection : MonoBehaviour
{
    StarbornInputSystem m_inputSystem;
    public virtual void Awake()
    {
        m_inputSystem = new StarbornInputSystem();
        m_inputSystem.Rhythm.A.performed += InputCheck;
        m_inputSystem.Rhythm.Left.performed += InputCheck;
        m_inputSystem.Rhythm.Down.performed += InputCheck;
        m_inputSystem.Rhythm.Up.performed += InputCheck;
        m_inputSystem.Rhythm.Right.performed += InputCheck;
        m_inputSystem.Rhythm.Pad.performed += InputCheck;
    }

    private void OnEnable()
    {
        m_inputSystem.Rhythm.Enable();
    }

    private void OnDisable()
    {
        m_inputSystem.Rhythm.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void InputCheck(InputAction.CallbackContext context)
    {
        Debug.Log(context.action.name);
    }
}
