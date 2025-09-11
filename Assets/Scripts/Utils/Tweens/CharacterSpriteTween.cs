using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rabbyte;
using System;

public static class CharacterSpriteTween
{
    #region Trans In
    public static void FadeIn(CharacterSprite sprite, float duration, Eases ease = Eases.Linear, Action onComplete = null)
    {
        TweenManager.AlphaTween(sprite.gameObject, 0, 1, duration, ease, onComplete);
    }

    public static void SlideIn(CharacterSprite sprite, float offset, float duration, Eases ease = Eases.Linear, Action onComplete = null)
    {
        float startX = sprite.position.x;
        /*TweenManager.NumTween(() => {
            float off = Mathf.Abs(offset);
            return startX + offset; 
        });*/
    }
    #endregion

    #region Trans Out
    public static void FadeOut(CharacterSprite sprite, float duration, Eases ease = Eases.Linear, Action onComplete = null)
    {
        TweenManager.AlphaTween(sprite.gameObject, 1, 0, duration, ease, onComplete);
    }
    #endregion

    #region Other
    public static void KillAllTweensOfSprite(CharacterSprite sprite, bool setToEnd = true)
    {
        List<ITween> tweens = new List<ITween>();
        foreach(KeyValuePair<string, ITween> tween in TweenManager.instance.activeTweens)
        {
            if (tween.Key.Contains(sprite.GetInstanceID().ToString()))
                tweens.Add(tween.Value);
        }
    }
    #endregion
}
