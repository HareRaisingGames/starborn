using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starborn.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : OptionMenu
{
    StarbornInputSystem m_inputSystem;
    public override void Awake()
    {
        base.Awake();
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Action exitCallback;

    #region GameObjects
    public GameObject blackBG;
    public GameObject menu;
    #endregion

    public static PauseMenu instance;
    public static bool inMainMenu;
    public static bool pauseTrans = false;

    public static Action additionalClose;

    public bool glow;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public static void Open(Action enter = null, Action exit = null, Action transition = null, Action onBack = null)
    {
        pauseTrans = true;
        instance.gameObject.SetActive(true);
        instance.exitCallback = exit;
        inMainMenu = true;
        enter?.Invoke();
        instance.curOption = 0;
        if (instance.blackBG != null)
        {
            ColorUtils.SetAlpha(instance.blackBG, 0);
            TweenManager.XTween(instance.blackBG, -800, 0, 0.5f, Eases.EaseInOutQuad).SetIgnoreTimeScale().SetStartDelay(0.1f);
            TweenManager.AlphaTween(instance.blackBG, 0, 0.75f, 0.5f, Eases.EaseInOutQuad, delegate() {
                transition?.Invoke();
                pauseTrans = false;
            }).SetIgnoreTimeScale().SetStartDelay(0.1f);
        }

        TweenManager.YTween(instance.menu, -100, 0, 0.25f, Eases.EaseInOutQuad).SetIgnoreTimeScale().SetStartDelay(0.1f);
        foreach (Transform child in instance.menu.transform)
        {
            if (child.gameObject.GetComponent<Image>()
                || child.gameObject.GetComponent<SpriteRenderer>()
                || child.gameObject.GetComponent<TMP_Text>())
            {
                ColorUtils.SetAlpha(child.gameObject, 0);
                TweenManager.AlphaTween(child.gameObject, 0, 1, 0.25f, Eases.EaseInOutCubic, delegate () {

                }).SetIgnoreTimeScale().SetStartDelay(0.1f);
            }

        }
    }

    public static void Close(Action exit = null)
    {
        if (exit != null)
            instance.exitCallback = exit;
        pauseTrans = true;
        instance.hasSelected = true;
        if (instance.blackBG != null)
        {
            TweenManager.XTween(instance.blackBG, 0, -800, 0.5f, Eases.EaseInOutQuad).SetIgnoreTimeScale().SetStartDelay(0.1f);
            TweenManager.AlphaTween(instance.blackBG, 0.75f, 0, 0.5f, Eases.EaseInOutQuad, delegate () {
                additionalClose?.Invoke();
                additionalClose = null;
                instance.exitCallback?.Invoke();
                instance.gameObject.SetActive(false);
                pauseTrans = false;
            }).SetIgnoreTimeScale().SetStartDelay(0.1f);

            TweenManager.YTween(instance.menu, 0, -100, 0.25f, Eases.EaseInOutQuad).SetIgnoreTimeScale().SetStartDelay(0.1f);
            foreach (Transform child in instance.menu.transform)
            {
                if (child.gameObject.GetComponent<Image>()
                    || child.gameObject.GetComponent<SpriteRenderer>()
                    || child.gameObject.GetComponent<TMP_Text>())
                {
                    ColorUtils.SetAlpha(child.gameObject, 1);
                    TweenManager.AlphaTween(child.gameObject, 1, 0, 0.25f, Eases.EaseInOutCubic, delegate () {

                    }).SetIgnoreTimeScale().SetStartDelay(0.1f);
                }

            }
        }

    }

    Tween<float> glowTween;
    protected override void SetSelection(int s)
    {
        base.SetSelection(s);

        if(glow)
        {
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
    public void Resume()
    {
        Close();
    }
}
