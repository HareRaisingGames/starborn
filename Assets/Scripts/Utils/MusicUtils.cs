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
}
