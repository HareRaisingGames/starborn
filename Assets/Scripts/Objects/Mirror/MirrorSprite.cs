using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MirrorSprite : MonoBehaviour
{
    SpriteRenderer baseRenderer;
    public SpriteRenderer target;
    #region Sprite Overrides
    [System.Serializable]
    public class SpriteOverride
    {
        public Sprite original;
        public Sprite replacement;
    }
    public List<SpriteOverride> overrides;
    public Dictionary<Sprite, Sprite> overrideDictionary = new Dictionary<Sprite, Sprite>();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        baseRenderer = GetComponent<SpriteRenderer>();
        if (target != null)
        {
            transform.localPosition = target.transform.localPosition;
            transform.localRotation = target.transform.localRotation;
            //if (scale) transform.localScale = original.transform.localScale;
        }

        foreach(SpriteOverride over in overrides)
        {
            overrideDictionary.Add(over.original, over.replacement);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            baseRenderer.sprite = GetOverrideSprite(target.sprite);
            transform.localPosition = target.transform.localPosition;
            transform.localRotation = target.transform.localRotation;
            //if (scale) transform.localScale = original.transform.localScale;
        }
    }

    Sprite GetOverrideSprite(Sprite sprite)
    {
        if (overrideDictionary.ContainsKey(sprite))
            return overrideDictionary[sprite];
        return sprite;
    }
}
