using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputCheck : MonoBehaviour
{
    protected static string controlType;
    public static string controller => controlType;

    protected static bool startUp = false;

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

    //List<string> pcDevices = new List<string>(){ "Keyboard", "Mouse", "Touchscreen", "Tablet Monitor" };

    // Start is called before the first frame update
    void Start()
    {
        if (!startUp)
        {
            GetStartUpDevice();
            if (controlType == "PC")
                foreach (Gamepad controllerType in Gamepad.all)
                {
                    if (controllerType.displayName.Contains("XInput") || controllerType.displayName.Contains("Xbox"))
                        controlType = "Xbox";
                    else if (controllerType.displayName.Contains("DualShock") || controllerType.displayName.Contains("PS"))
                        controlType = "PlayStation";
                    else if (controllerType.displayName.Contains("Switch"))
                        controlType = "Switch";
                    else
                        controlType = controllerType.displayName;
                    break;
                }
            startUp = true;
        }

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

                    if (controllerType.displayName.Contains("XInput") || controllerType.displayName.Contains("Xbox"))
                        controlType = "Xbox";
                    else if (controllerType.displayName.Contains("DualShock") || controllerType.displayName.Contains("PS"))
                        controlType = "PlayStation";
                    else if (controllerType.displayName.Contains("Switch"))
                        controlType = "Switch";
                    else
                        controlType = controllerType.displayName;
                }
            }
        }

        foreach (InputDevice device in InputSystem.devices)
        {
            if(device is Keyboard)
            {
                Keyboard keyboard = device as Keyboard;
                if (keyboard.anyKey.isPressed)
                {
                    controlType = "PC";
                }
            }

            if(device is Mouse)
            {
                Mouse mouse = device as Mouse;
                Vector2 newPosition = mouse.position.ReadValue();
                bool isPressed = mouse.IsPressed(0) || mouse.IsPressed(1) || mouse.IsPressed(2);

                if (isPressed && !mousePosition.Equals(newPosition))
                {
                    controlType = "PC";
                    mousePosition = newPosition;
                }
            }
        }
    }

    void GetStartUpDevice()
    {
        switch(Application.platform)
        {
            case RuntimePlatform.XboxOne:
                controlType = "Xbox";
                break;
            case RuntimePlatform.PS4:
            case RuntimePlatform.PS5:
                controlType = "PlayStation";
                break;
            case RuntimePlatform.Switch:
                controlType = "Switch";
                break;
            default:
                controlType = "PC";
                break;
        }
    }
}
