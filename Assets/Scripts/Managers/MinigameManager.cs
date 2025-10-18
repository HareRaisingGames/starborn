using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    Minigame minigame;
    Conductor conductor;

    [HideInInspector]
    public List<RhythmEvent> events = new List<RhythmEvent>();
    [HideInInspector]
    public List<RhythmInput> inputs = new List<RhythmInput>();

    [HideInInspector]
    public List<float> accuracies = new List<float>();

    public HeartGroup hearts;

    public int maxLives = 3;
    float _lives;
    bool _gameOver;
    bool _consequences = true;

    bool _canPlay = true;
    bool isTutorial = false;

    public bool canPlay
    {
        set
        {
            _canPlay = value;
        }
    }

    bool canPause = true;
    Tween<float> pauseTween;

    public TMP_Text accuracyText;
    public TMP_Text livesText;
    public TMP_Text beatsText;
    public GameObject pauseGameOver;
    public bool gameOver
    {
        get
        {
            return _gameOver;
        }
        set
        {
            _gameOver = value;
        }
    }
    public float lives
    {
        get
        {
            return _lives;
        }
        set
        {
            _lives = value;
        }
    }

    public TMP_Text text;

    bool paused;

    public bool consequences
    {
        set
        {
            _consequences = value;
        }
    }

    private static MinigameManager _instance;

    public static MinigameManager instance
    {
        get
        {
            if (_instance == null)
            {
                if(FindObjectOfType<MinigameManager>() == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/Manager");
                    if(prefab != null)
                    {
                        Instantiate(prefab, Vector3.zero, Quaternion.identity).name = "Manager";
                    }
                    else
                    {
                        GameObject manager = new GameObject("Minigame");
                        _instance = manager.AddComponent<MinigameManager>();
                    }
                }
                _instance = FindObjectOfType<MinigameManager>();
                
            }

            return _instance;
        }
    }

    public static List<float> totalAccuracies = new List<float>();
    public static void ClearAccuracies() => totalAccuracies.Clear();
    public static void AverageAccuracies(List<float> game) => totalAccuracies.Add(MathUtils.ListAverage(game));

    StarbornInputSystem m_inputSystem;

    private void Awake()
    {
        minigame = FindObjectOfType<Minigame>();
        conductor = FindObjectOfType<Conductor>();
        Conductor.instance.SetUpBPM();

        //text = GameObject.FindGameObjectWithTag("Tutorial").GetComponent<TMP_Text>();
        m_inputSystem = new StarbornInputSystem();
        m_inputSystem.Dialogue.Pause.performed += OnPause;

    }

    private void OnEnable()
    {
        m_inputSystem.Dialogue.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        _lives = maxLives;
        minigame = FindObjectOfType<Minigame>();
    }

    public void LoseALife(float amount = 1)
    {
        if(_consequences)
            _lives -= amount;
    }

    [HideInInspector]
    public float displayAccuracy = 0;
    
    public float totalAccuracy
    {
        get
        {
            if (accuracies.Count != 0)
            {
                return MathUtils.ListAverage(accuracies);
            }
            return 0;
        }
    }

    public float accuracy
    {
        get
        {
            return Mathf.Round(totalAccuracy * 100);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Conductor.instance.isPlaying)
        {
            foreach(RhythmEvent e in events)
            {
                e.CheckForInvoke(Conductor.instance.songPosition);
            }
            foreach (RhythmInput input in inputs)
            {
                input.Update(Conductor.instance.songPosition);

                if (minigame != null)
                {
                    input.AUTOPLAY = minigame.autoPlay;

                    /*if (minigame.autoPlay)
                    {
                        if(input.curHit >= input.desHit && !input.HasHit)
                        {
                            input.onHit?.Invoke();
                            input.HasHit = true;
                        }
                    }*/
                }

            }
        }

        if(_lives <= 0 && !isTutorial && !_gameOver)
        {
            _gameOver = true;
            MusicUtils.SlowDownMusic(Conductor.instance.music, 2.5f, delegate() {
                Conductor.instance.music.Stop();
                if(FindObjectOfType<DialogueManager>(true) != null)
                {
                    FindObjectOfType<DialogueManager>(true).GameOver();
                }
                //Debug.Log();
            });
        }

        if(accuracyText != null) accuracyText.text = Mathf.Round(displayAccuracy * 100) + "%";
        if (livesText != null) livesText.text = Mathf.Clamp(lives, 0, Mathf.Infinity).ToString();
        if (beatsText != null) beatsText.text = conductor.curBeat.ToString();

        if (hearts != null)
            hearts.SetLives(lives);

        displayAccuracy = Mathf.Lerp(displayAccuracy, totalAccuracy, Time.deltaTime * 10);

        foreach (RhythmInput input in inputs) input.canPlay = _canPlay;

        if(minigame != null)
        {
            //The requirement for when a minigame is completed
            /*if(minigame.hasCompleted?.Invoke())
            {

            }*/
        }

        /*if (Gamepad.current != null)
        {
            // Iterate through all buttons on the current gamepad
            Debug.Log(Gamepad.current.name);
            foreach (InputControl control in Gamepad.current.allControls)
            {
                // Check if the control is a button and if it's currently pressed
                if (control is ButtonControl button && button.isPressed)
                {
                    Debug.Log($"Any gamepad button pressed: {button.name}");
                    // You can add your desired action here, e.g., trigger an event
                    return; // Exit after finding the first pressed button
                }
            }
        }*/

        //inputs = Object.FindObjectsOfType<RhythmInput>();
        //inputs = new List<RhythmInput>(FindObjectsOfType<RhythmInput>());
    }

    public void StartTutorial()
    {
        if(minigame != null)
        {
            if(minigame.isTutorial && minigame.tutorial.lines.Count != 0)
            {
                if(text != null)
                {
                    NextLine();
                }
            }
        }
    }

    int t = 0;
    public void NextLine()
    {
        if(minigame.tutorial.lines.Count <= t)
        {
            return;
        }
        text.text = minigame.tutorial.lines[t].dialogue;
        t++;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        paused = !paused;
        minigame.paused = paused;

        if(canPause)
        {
            if (paused)
            {
                Time.timeScale = 0;
                if (Conductor.instance.isPlaying)
                {
                    Conductor.instance.music.Pause();
                    foreach (KeyValuePair<string, ITween> tween in TweenManager.instance.activeTweens)
                    {
                        tween.Value.Pause();
                    }

                    foreach (RhythmInput input in inputs)
                    {
                        input.Disable();
                    }
                }

                if (pauseGameOver != null)
                {
                    pauseTween = TweenManager.XTween(pauseGameOver, -800, 0, 0.5f, Eases.EaseInOutQuad).SetIgnoreTimeScale();
                }
            }
            else
            {
                if (pauseGameOver != null)
                {
                    if (pauseTween != null) pauseTween.OnCompleteKill();
                    canPause = false;
                    pauseTween = TweenManager.XTween(pauseGameOver, 0, -800, 0.5f, Eases.EaseInOutQuad, Unpause).SetIgnoreTimeScale();
                }
                else
                {
                    Unpause();
                }

                void Unpause()
                {
                    canPause = true;
                    Time.timeScale = 1;
                    if (Conductor.instance.isPaused)
                    {
                        Conductor.instance.music.UnPause();
                        foreach (KeyValuePair<string, ITween> tween in TweenManager.instance.activeTweens)
                        {
                            tween.Value.Resume();
                        }

                        foreach (RhythmInput input in inputs)
                        {
                            input.Enable();
                        }
                    }
                }
            }
        }

    }

    public static void Clear()
    {
        instance.events.Clear();
        instance.inputs.Clear();
        instance.accuracies.Clear();
    }
}
