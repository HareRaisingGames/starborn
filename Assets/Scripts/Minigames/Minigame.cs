using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using Starborn;

public abstract class Minigame : MonoBehaviour
{
    StarbornInputSystem m_inputSystem;

    public string minigameName;
    public Conductor conductor;

    public List<Starborn.InputSystem.Event> inputs = new List<Starborn.InputSystem.Event>();

    private void Awake()
    {
        m_inputSystem = new StarbornInputSystem();
        m_inputSystem.Rhythm.A.performed += onA;
        m_inputSystem.Rhythm.Left.performed += onLeft;
        m_inputSystem.Rhythm.Down.performed += onDown;
        m_inputSystem.Rhythm.Up.performed += onUp;
        m_inputSystem.Rhythm.Right.performed += onRight;
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
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void onA(InputAction.CallbackContext context)
    {
        //If the A button has been pressed
    }

    public virtual void onLeft(InputAction.CallbackContext context)
    {
        //If the Left button has been pressed
    }

    public virtual void onDown(InputAction.CallbackContext context)
    {
        //If the Down button has been pressed
    }

    public virtual void onUp(InputAction.CallbackContext context)
    {
        //If the Up button has been pressed
    }

    public virtual void onRight(InputAction.CallbackContext context)
    {
        //If the Right button has been pressed
    }
}
