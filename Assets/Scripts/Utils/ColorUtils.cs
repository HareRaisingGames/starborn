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
}
