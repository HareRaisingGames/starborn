using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ColorUtils
{
    public static void SetAlpha(GameObject obj, float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        if(obj.GetComponent<Image>() != null)
        {
            Color color = obj.GetComponent<Image>().color;
            obj.GetComponent<Image>().color = new Color(color.r, color.g, color.b, value);
        }
        else if (obj.GetComponent<TMP_Text>() != null)
        {
            Color color = obj.GetComponent<TMP_Text>().color;
            obj.GetComponent<TMP_Text>().color = new Color(color.r, color.g, color.b, value);
        }
        else if (obj.GetComponent<SpriteRenderer>() != null)
        {
            Color color = obj.GetComponent<SpriteRenderer>().color;
            obj.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, value);
        }
    }

    public static void SetColorByString(GameObject obj, string color)
    {
        Color selectedColor;
        switch(color.ToLower())
        {
            case "blue":
                selectedColor = Color.blue;
                break;
            case "black":
                selectedColor = Color.black;
                break;
            case "cyan":
                selectedColor = Color.cyan;
                break;
            case "gray":
                selectedColor = Color.gray;
                break;
            case "green":
                selectedColor = Color.green;
                break;
            case "grey":
                selectedColor = Color.grey;
                break;
            case "magenta":
                selectedColor = Color.magenta;
                break;
            case "red":
                selectedColor = Color.red;
                break;
            case "white":
                selectedColor = Color.white;
                break;
            case "yellow":
                selectedColor = Color.yellow;
                break;
            default:
                Color newColor;
                if(ColorUtility.TryParseHtmlString(color, out newColor))
                {
                    selectedColor = newColor;
                }
                else
                    selectedColor = Color.white;

                break;
        }

        if (obj.GetComponent<Image>() != null)
        {
            Color baseColor = obj.GetComponent<Image>().color;
            obj.GetComponent<Image>().color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, baseColor.a);
        }
        else if (obj.GetComponent<TMP_Text>() != null)
        {
            Color baseColor = obj.GetComponent<TMP_Text>().color;
            obj.GetComponent<TMP_Text>().color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, baseColor.a);
        }
        else if (obj.GetComponent<SpriteRenderer>() != null)
        {
            Color baseColor = obj.GetComponent<SpriteRenderer>().color;
            obj.GetComponent<SpriteRenderer>().color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, baseColor.a);
        }
    }
}
