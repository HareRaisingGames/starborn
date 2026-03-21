using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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
        IEnumerable<AudioClip> filtered = allSounds.Where(sound => sound.name.Contains(baseName));
        AudioClip[] filteredSFXs = filtered.ToArray();

        if(filteredSFXs.Length != 0)
        {
            System.Random random = new System.Random();
            int r = random.Next(0, filteredSFXs.Length);
            return filteredSFXs[r];
        }

        return null;
    }
}