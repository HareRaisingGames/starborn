using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public static partial class MusicUtils
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

public static partial class MusicUtils
{
    public static AudioClip GetRandomClip(string directory, string baseName)
    {
        AudioClip[] allSounds = Resources.LoadAll<AudioClip>(directory);

        int count = 0;
        foreach (AudioClip sound in allSounds)
        {
            if (sound.name.Contains(baseName))
                count++;
        }

        if (count == 0)
            return null;

        int target = Random.Range(0, count);
        int index = 0;
        foreach (AudioClip sound in allSounds)
        {
            if (sound.name.Contains(baseName))
            {
                if (index == target)
                    return sound;
                index++;
            }
        }

        return null;
    }
}