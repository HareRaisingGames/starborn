using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;


public class Countdown : MonoBehaviour
{
    static Countdown _instance;

    AudioSource three;
    AudioSource two;
    AudioSource one;
    AudioSource lets;
    AudioSource go;

    static string _folder = "base";

    static PauseToken pauseToken;
    public static PauseTokenSource pauseTokenSource;

    static CancellationTokenSource cancellationTokenSource;
    static CancellationToken cancellationToken;

    static bool _activatedCountdown;
    public static bool activatedCountdown => _activatedCountdown;

    static bool stop;

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

        /*switch(i)
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
        StartCountdown(time, callback, i);*/

        _activatedCountdown = true;
        pauseTokenSource = new PauseTokenSource();
        pauseToken = pauseTokenSource.Token;
        pauseTokenSource.Resume();
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
        stop = false;

        await CountdownTask(time, callback);
    }

    static AudioSource SetAudioSource(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        obj.transform.parent = parent;
        return source;
    }

    static async Task CountdownTask(float time, Action callback = null, int i = 0)
    {
        if (stop) return;

        if (pauseToken != null)
            await pauseToken.WaitWhilePaused();

        switch (i)
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
                _activatedCountdown = false;
                return;
        }
        int milliseconds = (int)(time * 1000 / 2);
        await Task.Delay(milliseconds);
        i++;
        await CountdownTask(time, callback, i);

    }

    public static void PauseCountdown()
    {
        pauseTokenSource.Pause();
    }

    public static void ResumeCountdown()
    {
        pauseTokenSource.Resume();
    }

    public static void CancelCountdown()
    {
        if(cancellationTokenSource != null)
            cancellationTokenSource.Cancel();
        instance.three.Stop();
        instance.two.Stop();
        instance.one.Stop();
        instance.lets.Stop();
        instance.go.Stop();
        stop = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Editor Properties
#if UNITY_EDITOR
    /// This method is called when the script is loaded or a recompile occurs
    [InitializeOnLoadMethod]
    private static void OnInitialize()
    {
        // Subscribe to the 'quitting' event
        EditorApplication.quitting += OnEditorQuitting;
    }

    // This method will be called when the Editor is quitting
    private static void OnEditorQuitting()
    {
        CancelCountdown();
        // Add any specific actions or cleanup logic here,
        // such as saving data, releasing resources, etc.
    }
#endif
    #endregion

    #region Runtime Properties
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInitialize()
    {
        Application.quitting += onRuntimeQutting;
    }

    private static void onRuntimeQutting()
    {
        CancelCountdown();
    }
    #endregion
}
