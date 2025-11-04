using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using UnityEngine.Serialization;

public class OptionMenu : MonoBehaviour
{
    int _curOption = 0;
    protected int curOption
    {
        get
        {
            return _curOption;
        }
        set
        {
            if (options.Count != 0)
                SetSelection(value);
            _curOption = value;
        }
    }
    public List<Option> options = new List<Option>();
    public Image cursor;
    public float offset = 2;
    StarbornInputSystem inputActions;
    public virtual void Awake()
    {
        inputActions = new StarbornInputSystem();
        inputActions.Menu.Navigate.performed += OnNavigate;
        inputActions.Menu.Navigate.canceled += OnNavigate;
        inputActions.Menu.Select.performed += OnSelect;
        inputActions.Menu.Back.performed += OnBack;
    }

    bool mouseMovement;

    [HideInInspector]
    public bool hasSelected = false;

    public UnityEvent onBackEvent;

    [Header("AudioClips")]
    public AudioClip select;
    public AudioClip navigate;

    AudioSource selectSource;
    AudioSource navigateSource;

    // Start is called before the first frame update
    public virtual void Start()
    {
        if(options.Count != 0)
            ChangeSelection();

        GameObject obj = new GameObject("Select");
        selectSource = obj.AddComponent<AudioSource>();
        selectSource.playOnAwake = false;
        if(select != null) selectSource.clip = select;
        obj.transform.parent = transform;

        obj = new GameObject("Navigate");
        navigateSource = obj.AddComponent<AudioSource>();
        navigateSource.playOnAwake = false;
        if (navigate != null) navigateSource.clip = navigate;
        obj.transform.parent = transform;

        foreach(Option option in options)
        {
            if(option.item.GetComponent<Button>())
            {
                Button button = option.item.GetComponent<Button>();
                /*for(int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    ]
                    for(int j = 0; j < option.action.GetPersistentEventCount(); j++)
                    {
                        if(option.ac)
                    }
                    //if(option.action.GetPersistentTarget(i))
                }*/

                button.onClick.AddListener(selectSource.Play);
            }
        }

    }

    // Update is called once per frame
    public virtual void Update()
    {
        if(!hasSelected)
        {
            Option curItem = null;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                //Debug.Log("Hit UI element: " + result.gameObject.name);

                //if(result.gameObject.GetComponent<RectTransform>)
                // You can add custom logic here based on the hit UI element
                foreach (Option option in options)
                {
                    if (option.HoverOver(result.gameObject.GetComponent<RectTransform>()))
                        curItem = option;
                }
            }

            if (curItem != null && options.IndexOf(curItem) != _curOption)
            {
                SetSelection(options.IndexOf(curItem));
                navigateSource.Play();
            }
        }

            
    }

    public virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if (hasSelected)
            return;
        Vector2 motion = context.ReadValue<Vector2>();
        int c = 0;
        if (motion.y > 0) c = 1;
        else if (motion.y < 0) c = -1;
        ChangeSelection(c);
    }

    public virtual void OnSelect(InputAction.CallbackContext context)
    {
        if (hasSelected)
            return;

        if (options.Count == 0) return;

        hasSelected = true;

        if (options[_curOption].Select(selectSource))
            hasSelected = false;
    }

    public virtual void OnBack(InputAction.CallbackContext context)
    {
        if (hasSelected)
            return;

        onBackEvent?.Invoke();
    }

    private void OnEnable()
    {
        inputActions.Menu.Enable();
        hasSelected = false;
    }

    private void OnDisable()
    {
        inputActions.Menu.Disable();
    }

    protected void ChangeSelection(int change = 0)
    {
        if (hasSelected)
            return;

        if (change != 0) navigateSource.Play();

        var opt = _curOption - change;
        //_curOption -= change;
        if (opt >= options.Count) opt = 0;
        else if (opt < 0) opt = options.Count - 1;

        curOption = opt;
    }

    void SetSelection(int s)
    {
        float itemWidth = Mathf.Abs(options[s].item.sizeDelta.x / 2);
        if(cursor != null)
        {
            float cursorWidth = Mathf.Abs(cursor.rectTransform.sizeDelta.x / 2);
            cursor.rectTransform.anchoredPosition
                = new Vector2(options[s].item.anchoredPosition.x - itemWidth - cursorWidth - Mathf.Abs(offset), options[s].item.anchoredPosition.y);
        }
        
    }

    public void Quit()
    {
        Application.Quit(0);
    }
}

[System.Serializable]
public class Option
{
    public RectTransform item;

    public UnityEvent action;

    public bool Select(AudioSource player = null)
    {
        action?.Invoke();
        if (player != null) player.Play();

        if (action == null) return true;

        int listeners = 0;

        if(action.GetPersistentEventCount() != 0)
        {
            for(int i = 0; i < action.GetPersistentEventCount(); i++)
            {
                if (action.GetPersistentTarget(i) == null)
                {
                    listeners++;
                }
            }
        }
        return action.GetPersistentEventCount() == listeners;
    }

    public bool HoverOver(RectTransform rectangle)
    {
        return rectangle == item;
    }
}
