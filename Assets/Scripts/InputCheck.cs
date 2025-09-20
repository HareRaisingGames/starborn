using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputCheck : MonoBehaviour
{
    protected string controlType;
    public string controller => controlType;

    Vector2 mousePosition;
    void Awake()
    {
        InputCheck[] objs = FindObjectsOfType<InputCheck>();

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(Gamepad.current.displayName);
        if(Mouse.current != null)
        {
            mousePosition = Mouse.current.position.ReadValue();
        }

    }

    // Update is called once per frame
    void Update()
    {
        foreach(Gamepad controllerType in Gamepad.all)
        {
            foreach(InputControl control in controllerType.allControls)
            {
                if (control is ButtonControl button && button.isPressed)
                {
                    controlType = controllerType.displayName;
                }
            }
        }

        foreach(Keyboard keyboard in InputSystem.devices)
        {
            if(keyboard.anyKey.isPressed)
            {
                controlType = "PC";
            }
        }

        foreach(Mouse mouse in InputSystem.devices)
        {
            Vector2 newPosition = mouse.position.ReadValue();
            bool isPressed = mouse.IsPressed(0) || mouse.IsPressed(1) || mouse.IsPressed(2);

            if (isPressed && !mousePosition.Equals(newPosition))
            {
                controlType = "PC";
            }

            mousePosition = newPosition;
        }
    }
}
