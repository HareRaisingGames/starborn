using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Starborn.InputSystem;

public class InputSprite : MonoBehaviour
{
    private SpriteRenderer render;
    private Image image;

    public ActionType input;
    public bool whiteTexture;
    InputAction action;
    private StarbornInputSystem inputSystem;
    private bool isImage => image != null;
    private bool isRender => render != null;

    private static Sprite[] cachedSprites;

    void Awake()
    {
        if (cachedSprites == null)
            cachedSprites = Resources.LoadAll<Sprite>("Sprites/Icons/game_icons_assets");
    }

    // Start is called before the first frame update
    void Start()
    {
        inputSystem = new StarbornInputSystem();
        if (GetComponent<RectTransform>() != null)
            image = GetComponent<Image>();
        else
            render = GetComponent<SpriteRenderer>();

        if(FindObjectOfType<InputCheck>() == null)
        {
            GameObject obj = new GameObject("Inputs");
            obj.AddComponent<InputCheck>();
        }

        switch(input)
        {
            case ActionType.DialogueEnter:
                action = inputSystem.Dialogue.A;
                break;
            case ActionType.DialoguePause:
                action = inputSystem.Dialogue.Pause;
                break;
            case ActionType.DialogueSkip:
                action = inputSystem.Dialogue.Skip;
                break;
            case ActionType.DialougeLog:
                action = inputSystem.Dialogue.Log;
                break;
            case ActionType.RhythmA:
                action = inputSystem.Rhythm.A;
                break;
            case ActionType.RhythmDpad:
                action = inputSystem.Rhythm.Pad;
                break;
            case ActionType.RhythmLeft:
                action = inputSystem.Rhythm.Left;
                break;
            case ActionType.RhythmDown:
                action = inputSystem.Rhythm.Down;
                break;
            case ActionType.RhythmUp:
                action = inputSystem.Rhythm.Up;
                break;
            case ActionType.RhythmRight:
                action = inputSystem.Rhythm.Right;
                break;
        }

        UpdateIcon();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        string inputType = InputCheck.GetBindFromAction(action);

        if (cachedSprites == null)
            return;

        foreach(Sprite sprite in cachedSprites)
        {
            if(sprite.name == inputType)
            {
                if (isImage)
                    image.sprite = sprite;
                else if (isRender)
                    render.sprite = sprite;
                break;
            }
        }
    }
}
