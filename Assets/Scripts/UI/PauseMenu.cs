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

    public void Resume()
    {
        Close();
    }
}
