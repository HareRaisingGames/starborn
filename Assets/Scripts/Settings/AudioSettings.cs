using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class MixerSettings
{
    private static AudioMixer master;

    static MixerSettings()
    {
        master = Resources.Load<AudioMixer>("Audio/MainMixer");
    }

    public static void SetAudioGroup(AudioSource source, string group, int id = 0)
    {
        if (master != null && source != null)
            source.outputAudioMixerGroup = master.FindMatchingGroups(group)[id];
    }
    public static void SetVolume(string group, float volume) => master.SetFloat($"{group}Volume", Mathf.Log10(volume)*20);
}
