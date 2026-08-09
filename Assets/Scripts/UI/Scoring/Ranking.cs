using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Ranking : MonoBehaviour
{
    public float defaultWidth = 75f;
    Image image;
    void Awake()
    {
        image = GetComponent<Image>();
        gameObject.SetActive(false);
    }
    public void DisplayRanking(float accuracy)
    {
        if(!gameObject.activeInHierarchy) gameObject.SetActive(true);
        image.sprite = RankingClass.GetRanking(accuracy);
        image.SetNativeSize();
        image.GetComponent<RectTransform>().sizeDelta = 
        new Vector2(image.GetComponent<RectTransform>().sizeDelta.x/2f,image.GetComponent<RectTransform>().sizeDelta.y/2f);
    }
}

public static class RankingClass
{
    static Sprite[] spritesheet;
    static Dictionary<string, Sprite> rankings = new Dictionary<string, Sprite>();
    static RankingClass()
    {
        if(spritesheet == null)
        {
            spritesheet = Resources.LoadAll<Sprite>("Sprites/UI/starborn_rankings");
            foreach(Sprite sprite in spritesheet)
            {
                rankings.Add(StringUtils.Replace(sprite.name, "Starborn_", ""), sprite);
            }
        }
    }

    public static Sprite GetRanking(float accuracy)
    {
        if(accuracy > 1f || accuracy < 0f) return null;
        if(accuracy == 1f) return rankings["P"];
        else if(accuracy < 1f && accuracy >= 0.95f) return rankings["S"];
        else if(accuracy < 0.95f && accuracy >= 0.9f) return rankings["A"];
        else if(accuracy < 0.9f && accuracy >= 0.8f) return rankings["B"];
        else if(accuracy < 0.8f && accuracy >= 0.7f) return rankings["C"];
        else if(accuracy < 0.7f && accuracy >= 0.6f) return rankings["D"];
        else return rankings["F"];
    }
}
