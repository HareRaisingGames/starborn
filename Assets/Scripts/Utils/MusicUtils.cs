using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class MusicUtils
{
    public static void SlowDownMusic(AudioSource audio, float duration, Action onComplete = null)
    {
        TweenManager.NumTween(() => audio.pitch, (value) => { audio.pitch = value; }, 0, duration, Eases.EaseOutInCubic, onComplete);
    }

    public static void MusicFadeIn(AudioSource audio, float duration, Action onComplete = null)
    {
        TweenManager.NumTween(() => audio.volume, (value) => { audio.volume = value; }, 1, duration, Eases.EaseOutInCubic, onComplete);
    }

    public static void MusicFadeOut(AudioSource audio, float duration, Action onComplete = null)
    {
        TweenManager.NumTween(() => audio.volume, (value) => { audio.volume = value; }, 0, duration, Eases.EaseOutInCubic, onComplete);
    }

    public static void MusicFade(AudioSource audio, float end, float duration, Action onComplete = null, float? start = null)
    {
        if(start != null)
        {
            audio.volume = Mathf.Clamp01(start.Value);
        }
        TweenManager.NumTween(() => audio.volume, (value) => { audio.volume = value; }, end, duration, Eases.EaseOutInCubic, onComplete);
    }
}
