using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using UnityEngine.Serialization;
using TMPro;
public class PopupMenu : OptionMenu
{
    public static PopupMenu instance;
    public TMP_Text message;
    public override void Awake()
    {
        base.Awake();
        instance = FindObjectOfType<PopupMenu>();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void OnNavigate(InputAction.CallbackContext context)
    {
        if (hasSelected)
            return;

        Vector2 motion = context.ReadValue<Vector2>();
        int c = 0;
        if (motion.x > 0) c = -1;
        else if (motion.x < 0) c = 1;

        if (context.action.IsPressed())
        {
            if (!justPressed)
            {
                if (c != 0 && release != c)
                    ChangeSelection(c);
                justPressed = true;
            }

        }
        else
        {
            justPressed = false;
            //Debug.Log("I pressed");
        }

        release = c;

    }

    public static void SetPopUp()
    {

    }
}
