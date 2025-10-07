using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Countdown : MonoBehaviour
{
    static Countdown _instance;

    AudioSource three;
    AudioSource two;
    AudioSource one;
    AudioSource lets;
    AudioSource go;

    static string _folder = "base";

    public static Countdown instance
    {
        get
        {
            if(_instance == null)
            {
                if(FindObjectOfType<Countdown>() == null)
                {
                    GameObject countdown = new GameObject("Countdown");
                    _instance = countdown.AddComponent<Countdown>();

                    _instance.three = SetAudioSource("Three", countdown.transform);
                    _instance.two = SetAudioSource("Two", countdown.transform);
                    _instance.one = SetAudioSource("One", countdown.transform);
                    _instance.lets = SetAudioSource("Lets", countdown.transform);
                    _instance.go = SetAudioSource("Go", countdown.transform);

                    _instance.AssignAudioClip(_folder);

                }
                else
                {
                    _instance = FindObjectOfType<Countdown>();
                    GameObject countdown = _instance.gameObject;
                    for (int i = countdown.transform.childCount - 1; i >= 0; i--)
                    {
                        GameObject child = countdown.transform.GetChild(i).gameObject;

                        Destroy(child);
                    }

                    _instance.three = SetAudioSource("Three", countdown.transform);
                    _instance.two = SetAudioSource("Two", countdown.transform);
                    _instance.one = SetAudioSource("One", countdown.transform);
                    _instance.lets = SetAudioSource("Lets", countdown.transform);
                    _instance.go = SetAudioSource("Go", countdown.transform);

                    _instance.AssignAudioClip(_folder);
                }
            }
            return _instance;
        }
    }

    public static string folder
    {
        set
        {
            _folder = value;
            instance.AssignAudioClip(value);
        }
    }

    void AssignAudioClip(string foldername)
    {
        if(three != null)
        {
            three.clip = Resources.Load<AudioClip>($"Countdown/{foldername}/three");
        }
        if (two != null)
        {
            two.clip = Resources.Load<AudioClip>($"Countdown/{foldername}/two");
        }
        if (one != null)
        {
            one.clip = Resources.Load<AudioClip>($"Countdown/{foldername}/one");
        }
        if (lets != null)
        {
            lets.clip = Resources.Load<AudioClip>($"Countdown/{foldername}/lets");
        }
        if (go != null)
        {
            go.clip = Resources.Load<AudioClip>($"Countdown/{foldername}/go");
        }
    }

    public async static void StartCountdown(float time, Action callback = null, int i = 0)
    {
        instance.Start();

        switch(i)
        {
            case 0:
                instance.three.Play();
                break;
            case 2:
                instance.two.Play();
                break;
            case 4:
                instance.one.Play();
                break;
            case 5:
                instance.lets.Play();
                break;
            case 6:
                instance.go.Play();
                break;
            case 8:
                callback?.Invoke();
                return;
        }

        int milliseconds = (int)(time * 1000 / 2);
        await Task.Delay(milliseconds);
        i++;
        StartCountdown(time, callback, i);
    }

    static AudioSource SetAudioSource(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        obj.transform.parent = parent;
        return source;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
