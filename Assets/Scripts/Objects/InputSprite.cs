using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSprite : MonoBehaviour
{
    private SpriteRenderer render;
    private Image image;

    public RhythmInputs input;

    private bool isImage => image != null;
    private bool isRender => render != null;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<RectTransform>() != null)
            image = GetComponent<Image>();
        else
            render = GetComponent<SpriteRenderer>();

        if(FindObjectOfType<InputCheck>() == null)
        {
            GameObject obj = new GameObject("Inputs");
            obj.AddComponent<InputCheck>();
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
        string inputType = $"{InputCheck.controller}_{input.ToString()}";

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Icons/game_icons");
        foreach(Sprite sprite in sprites)
        {
            if(sprite.name == inputType)
            {
                if (isImage)
                    image.sprite = sprite;
                else if (isRender)
                    render.sprite = sprite;
            }
        }
    }
}
