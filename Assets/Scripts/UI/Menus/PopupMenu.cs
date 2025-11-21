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
        instance = FindObjectOfType<PopupMenu>(true);
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

    public static void Open(string message = "", UnityAction openAction = null, UnityAction closeAction = null, Transform parent = null)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/PopUp");
        if (prefab != null)
        {
            GameObject popup = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            popup.name = "Are You Sure";
            popup.transform.parent = parent;
            popup.transform.localPosition = Vector3.zero;
            popup.transform.localScale = Vector3.one;
            instance = popup.GetComponent<PopupMenu>();
            popup.SetActive(true);
        }
        instance.options[0].action.AddListener(openAction);
        instance.message.text = message;
        instance.hasSelected = false;
        instance.options[0].action.AddListener(instance.CreateSound);
        instance.onBackEvent.AddListener(closeAction);

        foreach(Option option in instance.options)
        {
            if(option.item.GetComponent<Button>())
            {
                for(int i = 0; i < option.item.GetComponent<Button>().onClick.GetPersistentEventCount(); i++)
                {

                    for(int j = 0; j < option.action.GetPersistentEventCount(); j++)
                    {
                        

                    }
                }
            }
        }
    }

    void Yes()
    {

    }

    public void No()
    {
        options[0].action.RemoveAllListeners();
        onBackEvent.Invoke();
        onBackEvent.RemoveAllListeners();
        instance = null;
        Destroy(gameObject);
    }

    public void SolidDestroy()
    {
        options[0].action.RemoveAllListeners();
        onBackEvent.RemoveAllListeners();
        instance = null;
        Destroy(gameObject);
    }

    public void CreateSound()
    {
        if(select != null)
        {
            GameObject sound = new GameObject("Select");
            sound.AddComponent<SoundByte>();
            DontDestroyOnLoad(sound);
            sound.GetComponent<AudioSource>().playOnAwake = false;
            sound.GetComponent<AudioSource>().clip = select;
            sound.GetComponent<SoundByte>().timeSamples = sound.GetComponent<AudioSource>().timeSamples;
            sound.GetComponent<AudioSource>().Play();
        }
    }

    Tween<float> glowTween;
    protected override void SetSelection(int s)
    {
        base.SetSelection(s);

            if (glowTween != null)
                glowTween.FullKill();

            foreach (Option option in options)
            {
                if (option.item == null)
                    continue;

                TMP_Text text = option.item.GetComponent<TMP_Text>();

                if (text == null)
                    continue;

                text.fontMaterials[0].DisableKeyword("GLOW_ON");
                text.fontMaterials[0].SetFloat(ShaderUtilities.ID_GlowPower, 0);

                if (options.IndexOf(option) == s)
                {
                    text.fontMaterials[0].EnableKeyword("GLOW_ON");
                    text.fontMaterials[0].SetFloat(ShaderUtilities.ID_GlowOuter, 1);
                    text.fontMaterials[0].SetColor(ShaderUtilities.ID_GlowColor, Color.white);
                    text.fontMaterials[0].SetFloat(ShaderUtilities.ID_GlowPower, 1);
                    glowTween = TweenManager.NumTween(() => text.fontMaterials[0].GetFloat(ShaderUtilities.ID_GlowPower), (value) => {
                        text.fontMaterials[0].SetFloat(ShaderUtilities.ID_GlowPower, value);
                    }, 0.08f, 0.5f, Eases.EaseOutQuart).SetIgnoreTimeScale();
                    glowTween = TweenManager.NumTween(() => text.fontMaterials[0].GetFloat(ShaderUtilities.ID_GlowOuter), (value) => {
                        text.fontMaterials[0].SetFloat(ShaderUtilities.ID_GlowOuter, value);
                    }, 0.5f, 0.5f, Eases.EaseOutQuart).SetIgnoreTimeScale();
                }
            }
    }

    private void OnApplicationQuit()
    {
        foreach (Option option in options)
        {
            if (option.item == null)
                continue;

            TMP_Text text = option.item.GetComponent<TMP_Text>();

            if (text == null)
                continue;

            text.fontMaterials[0].DisableKeyword("GLOW_ON");
            text.fontMaterials[0].SetFloat(ShaderUtilities.ID_GlowPower, 0);
        }
    }
}
