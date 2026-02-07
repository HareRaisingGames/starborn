using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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
        inputActions.Menu.Navigate.canceled += delegate (InputAction.CallbackContext context) {
            release = 0;
        };
        inputActions.Menu.Select.performed += OnSelect;
        inputActions.Menu.Back.performed += OnBack;
    }

    bool mouseMovement;
    protected float release = 0;

    [HideInInspector]
    public bool hasSelected = false;

    public UnityEvent onBackEvent;

    [Header("AudioClips")]
    public AudioClip select;
    public AudioClip up;
    public AudioClip down;

    protected AudioSource selectSource;
    protected AudioSource navigateSource;
    protected AudioSource navigateSource2;

    // Start is called before the first frame update
    public virtual void Start()
    {
        if(options.Count != 0)
            ChangeSelection();

        GameObject obj = new GameObject("Select");
        selectSource = obj.AddComponent<AudioSource>();
        selectSource.playOnAwake = false;
        MixerSettings.SetAudioGroup(selectSource, "SFX");
        if (select != null) selectSource.clip = select;
        obj.transform.parent = transform;

        obj = new GameObject("Navigate");
        navigateSource = obj.AddComponent<AudioSource>();
        navigateSource.playOnAwake = false;
        MixerSettings.SetAudioGroup(navigateSource, "SFX");
        if (up != null) navigateSource.clip = up;
        obj.transform.parent = transform;

        if(down != null)
        {
            obj.name = "Up";
            obj = new GameObject("Down");
            navigateSource2 = obj.AddComponent<AudioSource>();
            navigateSource2.playOnAwake = false;
            navigateSource2.clip = down;
            MixerSettings.SetAudioGroup(navigateSource2, "SFX");
            obj.transform.parent = transform;
        }

        foreach(Option option in options)
        {
            if(option.item.GetComponent<Button>())
            {
                Button button = option.item.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(delegate() 
                { 
                    selectSource.Play();
                    Invoke("OnAudioFinished", selectSource.clip.length);
                });
                button.onClick.AddListener(Select);
            }
        }

    }

    protected virtual void OnAudioFinished()
    {

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
                    {
                        Hover(options.IndexOf(option));
                        curItem = option;
                    }
                        
                }
            }

            if (startScroll)
            {
                scrollFactor += Time.unscaledDeltaTime * 2;
                if (scrollFactor >= 1)
                {
                    changeFactor += Time.unscaledDeltaTime * 7.5f * Mathf.Abs(scrollSpeed);
                    //Debug.Log(Mathf.Round(changeFactor));

                    if (setNum != Mathf.Round(changeFactor))
                        ChangeSelection(scrollDirection);

                    setNum = Mathf.Round(changeFactor);
                }
            }

            if (curItem != null && options.IndexOf(curItem) != _curOption && !startScroll)
            {
                curOption = options.IndexOf(curItem);
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

        scrollSpeed = motion.y;

        scrollDirection = c;

        if(context.action.IsPressed())
        {
            if(!justPressed)
            {
                startScroll = true;
                if (c != 0 && release != c)
                    ChangeSelection(c);
                justPressed = true;
            }

        }
        else
        {
            startScroll = false;
            scrollFactor = 0;
            setNum = -1;
            justPressed = false;
            //Debug.Log("I pressed");
        }

        release = c;
            
    }

    protected bool justPressed;
    bool startScroll; //Determines when to start the scroll procedure
    float scrollFactor = 0; //The factor of the scroll for starting
    float changeFactor = 0;
    int scrollDirection = 0; //The direction
    float scrollSpeed;
    float setNum = -1; //Check to see if numbers are different before performing action

    public virtual void OnSelect(InputAction.CallbackContext context)
    {
        Select();
    }

    void Select()
    {
        if (hasSelected)
            return;

        if (options.Count == 0) return;

        hasSelected = true;

        if (options[_curOption].Select(selectSource))
            hasSelected = false;
    }

    protected virtual void Hover(int id)
    {

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

        if (change != 0)
        {
            if(navigateSource2 != null)
                if(change > 0)
                    navigateSource.Play();
                else
                    navigateSource2.Play();
            else
                navigateSource.Play();
        }

        var opt = _curOption - change;
        //_curOption -= change;
        if (opt >= options.Count) opt = 0;
        else if (opt < 0) opt = options.Count - 1;

        curOption = opt;
    }

    protected virtual void SetSelection(int s)
    {
        float itemWidth = Mathf.Abs(options[s].item.sizeDelta.x / 2);
        if(cursor != null)
        {
            float cursorWidth = Mathf.Abs(cursor.rectTransform.sizeDelta.x / 2);
            cursor.rectTransform.anchoredPosition
                = new Vector2(options[s].item.anchoredPosition.x - itemWidth - cursorWidth - offset, options[s].item.anchoredPosition.y);
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
