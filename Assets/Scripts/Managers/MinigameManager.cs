using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
    bool finishedGame = false;
    bool inTutorialMinigame = false;

    public bool canPlay
    {
        set
        {
            _canPlay = value;
        }
    }

    Tween<float> pauseTween;

    public GameObject accuracyObj;
    public TMP_Text accuracyText;
    public TMP_Text livesText;
    public TMP_Text beatsText;
    public TMP_Text remainingText;
    public GameObject pauseGameOver;

    public GameObject cleared;

    AudioSource applause;
    AudioSource pressSource;
    AudioSource releaseSource;

    public AudioClip press;
    public AudioClip release;
    
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

    public GameObject backdrop;
    public TMP_Text text;

    bool paused;

    public bool consequences
    {
        set
        {
            _consequences = value;
        }
    }

    Scene scene;

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
        m_inputSystem.Dialogue.A.performed += OnA;
        m_inputSystem.Dialogue.A.canceled += OnReleaseA;
        m_inputSystem.Dialogue.Skip.performed += OnSkip;

        scene = SceneManager.GetActiveScene();

    }

    private void OnEnable()
    {
        m_inputSystem.Dialogue.Enable();
    }

    private void OnDisable()
    {
        m_inputSystem.Dialogue.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        _lives = maxLives;
        minigame = FindObjectOfType<Minigame>();
        if (cleared != null)
            cleared.SetActive(false);

        if(Resources.Load<AudioClip>("Audio/applause") != null)
        {
            GameObject applauseObj = new GameObject("Applause");
            applause = applauseObj.AddComponent<AudioSource>();
            applause.playOnAwake = false;
            applause.clip = Resources.Load<AudioClip>("Audio/applause");
        }

        GameObject pressObj = new GameObject("Press");
        pressSource = pressObj.AddComponent<AudioSource>();
        pressSource.playOnAwake = false;
        if (press != null)
            pressSource.clip = press;

        GameObject releaseObj = new GameObject("Release");
        releaseSource = releaseObj.AddComponent<AudioSource>();
        releaseSource.playOnAwake = false;
        if (release != null)
            releaseSource.clip = release;

        if (remainingText != null) remainingText.text = "";
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
            return Mathf.Clamp(Mathf.Round(totalAccuracy * 100), 0, 100);
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

            CheckForAdditionalAssets();
        }

        if(_lives <= 0 && !inTutorial && !_gameOver)
        {
            _gameOver = true;
            MusicUtils.SlowDownMusic(Conductor.instance.music, 2.5f, delegate() {
                Conductor.instance.music.Stop();
                if(FindObjectOfType<DialogueManager>(true) != null)
                {
                    FindObjectOfType<DialogueManager>(true).GameOver();
                    if(GetComponent<Canvas>()) GetComponent<Canvas>().sortingOrder = -2;
                }
                //Debug.Log();
            });
        }

        if(accuracyText != null) accuracyText.text = Mathf.Round(displayAccuracy * 100) + "%";
        if (livesText != null) livesText.text = Mathf.Clamp(lives, 0, Mathf.Infinity).ToString();
        if (beatsText != null) beatsText.text = conductor.curBeat.ToString();
        if (backdrop != null && text != null) backdrop.SetActive(text.text != "");

        if (hearts != null)
            hearts.SetLives(lives);

        displayAccuracy = Mathf.Lerp(displayAccuracy, totalAccuracy, Time.deltaTime * 10);

        foreach (RhythmInput input in inputs) input.canPlay = _canPlay;

        if(minigame != null)
        {
            bool? done = minigame.hasCompleted?.Invoke();
            //Debug.Log(done);
            //The requirement for when a minigame is completed
            if(!inTutorialMinigame)
            {
                if (minigame.hasCompleted != null && minigame.hasCompleted.Invoke() && !finishedGame && !_gameOver)
                {
                    finishedGame = true;
                    if (cleared != null)
                        cleared.SetActive(true);
                    StaticProperties.canPause = false;
                }
            }
            else
            {
                if(Conductor.instance.music.isPlaying)
                {
                    minigame.TutorialAdditionals();
                    if (Conductor.instance.music.timeSamples < previousTimeSamples && previousTimeSamples > 0)
                    {
                        Clear();
                        lines[t].section.AddCharting(Conductor.instance.crochet, minigame.minigameName);
                    }

                    previousTimeSamples = Conductor.instance.music.timeSamples;
                    //Debug.Log(previousTimeSamples);
                }

            }

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

    public void CheckForAdditionalAssets()
    {
        if(FindObjectOfType<DialogueManager>(true) != null)
        {
            DialogueManager manager = FindObjectOfType<DialogueManager>(true);
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                if(!manager.sceneVisibilities[scene.name].ContainsKey(obj))
                    manager.sceneVisibilities[scene.name].Add(obj, obj.activeInHierarchy);
            }

        }
    }

    bool inTutorial;
    bool skipTutorial;
    List<TutorialLine> lines = new List<TutorialLine>();
    public void StartTutorial()
    {
        if(minigame != null)
        {
            if(minigame.isTutorial && minigame.tutorial.lines.Count != 0)
            {
                lines = minigame.tutorial.lines;
                _consequences = false;
                inTutorial = true;
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
        if (minigame != null)
        {
            if (lines.Count <= t)
            {
                text.text = "";
                _consequences = true;
                inTutorial = false;
                StartCoroutine(SetUp());
                minigame.hasCompleted = delegate () { return false; };
                minigame.OnBeatTutorial = null;
                minigame.PostTutorialAdditionals();
                IEnumerator SetUp()
                {
                    yield return new WaitForSeconds(1f);
                    minigame.StartSong();
                }
                return;
            }
            text.text = lines[t].dialogue;
            if (lines[t].section.sections.Count != 0)
            {
                if (minigame.tutorialSong != null) Conductor.instance.music.clip = minigame.tutorialSong;
                Conductor.instance.music.loop = true;
                previousTimeSamples = Conductor.instance.music.timeSamples;
                if (lines[t].section.setBPM && lines[t].section.bpm > 0)
                    Conductor.instance.manualBpm = lines[t].section.bpm;
                inTutorialMinigame = true;
                Conductor.instance.SetUpBPM();
                minigame.TutorialOnComplete(lines[t].amount);
                requiredAmount = lines[t].amount;
                minigame.OnBeatTutorial = OnCompleteTutorialSection;
                lines[t].section.AddCharting(Conductor.instance.crochet, minigame.minigameName);
                StartCoroutine(SetUp());
                IEnumerator SetUp()
                {
                    yield return new WaitForSeconds(0.5f);
                    minigame.AdditionalSongSetup();
                    if (lines[t].section.skipCountdown)
                        Conductor.instance.PlayMusicWithoutCallback();
                    else
                        Countdown.StartCountdown(Conductor.instance.crochet, Conductor.instance.PlayMusicWithoutCallback);

                    if (remainingText != null && minigame.amountJudger != null)
                        remainingText.text = $"{requiredAmount - minigame.amountJudger.Invoke()}\n<size=25>left</size>";
                }

                return;
                //Debug.Log(events.Count);
            }
            t++;
        }
    }

    int requiredAmount = 0;

    private void OnCompleteTutorialSection(int b)
    {
        if(b % 4 == 0 || b == 0)
        {
            if (minigame == null)
                return;

            if(remainingText != null && minigame.amountJudger != null)
            {
                remainingText.text = $"{requiredAmount - minigame.amountJudger.Invoke()}\n<size=25>left</size>";
            }

            if(minigame.hasCompleted != null && minigame.hasCompleted.Invoke())
            {
                Conductor.instance.music.Stop();
                minigame.hasCompleted = null;
                t++;
                text.text = "";
                Conductor.instance.music.loop = false;
                if (remainingText != null) remainingText.text = "";
                if (applause != null)
                    applause.Play();
                Conductor.startSong = false;
                minigame.TutorialReset();
                Clear();
                StartCoroutine(Next());
                IEnumerator Next()
                {
                    yield return new WaitForSeconds(1.5f);
                    inTutorialMinigame = false;
                    NextLine();
                }
            }
        }
    }

    private int previousTimeSamples;
    public void OnPause(InputAction.CallbackContext context)
    {
        if(StaticProperties.canPause && !PauseMenu.pauseTrans)
        {
            paused = !paused;
            StaticProperties.canPause = false;
            if (paused)
            {
                PauseMenu.Open(OpenCallback, Unpause, delegate() {
                    StaticProperties.canPause = true;
                }, delegate ()
                {
                    paused = false;
                }, RestartMinigame
                );
                PauseMenu.restartMessage = 
                    $"{(minigame != null ? " " + minigame.minigameName.Split(".")[minigame.minigameName.Split(".").Length - 1] : "")}";
            }
            else
            {
                PauseMenu.Close(Unpause, true);
            }
        }

    }

    void RestartMinigame()
    {
        if (FindObjectOfType<DialogueManager>(true) != null)
        {
            DialogueManager manager = FindObjectOfType<DialogueManager>(true);
            manager.gameObject.SetActive(true);
            Clear();
            if (manager.transitionCanvas != null) manager.transitionCanvas.SetActive(true);
            if (manager.fade != null)
            {
                manager.fade.SetActive(true);
                ColorUtils.SetAlpha(manager.fade, 0);
            }
            manager.TryAgain(delegate () { Time.timeScale = 1; });
            PauseMenu.instance.gameObject.SetActive(false);
            PopupMenu.instance.SolidDestroy();
            Countdown.CancelCountdown();
        }

        Minigame.gotGameOver = true;
        Conductor.startSong = false;
    }

    void OpenCallback()
    {
        if(minigame != null)
            minigame.paused = true;
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

        if (Countdown.activatedCountdown)
            Countdown.PauseCountdown();
    }

    void Unpause()
    {
        if (minigame != null)
            minigame.paused = true;
        StaticProperties.canPause = true;
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
        if (Countdown.activatedCountdown)
            Countdown.ResumeCountdown();
    }

    public void OnA(InputAction.CallbackContext context)
    {
        if(!paused)
        {
            if (inTutorial && !inTutorialMinigame || finishedGame)
            {
                hasPressed = true;
                pressSource.Play();
            }
        }
    }
    bool hasPressed;
    public void OnReleaseA(InputAction.CallbackContext context)
    {
        if(hasPressed)
        {
            if (inTutorial && !inTutorialMinigame)
            {
                NextLine();
            }
            else if (finishedGame)
            {
                if (FindObjectOfType<DialogueManager>(true) != null)
                {
                    totalAccuracies.Add(totalAccuracy);
                    Clear();
                    FindObjectOfType<DialogueManager>(true).FromGame();
                }
            }
            hasPressed = false;
            releaseSource.Play();
        }
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        if(inTutorial && !skipTutorial)
        {
            //
            skipTutorial = true;
            lines = minigame.tutorial.skipTutorial;
            t = 0;
            //Stop whatever music/minigame is happening if there is any happening
            if(Conductor.instance.music.isPlaying)
            {
                Conductor.instance.music.Stop();
                Conductor.instance.ManualSetUpBPM(0);
                inTutorialMinigame = false;
                Conductor.instance.music.loop = false;
            }
            if (remainingText != null) remainingText.text = "";
            minigame.OnBeatTutorial = null;
            minigame.hasCompleted = delegate () { return false; };
            Clear();
            NextLine();
        }

    }

    public static void Clear()
    {
        instance.events.Clear();
        instance.inputs.Clear();
        instance.accuracies.Clear();
    }

    public static void EndChapter()
    {
        ClearAccuracies();
    }
}
