using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Starborn.InputSystem;
using System.Reflection;
using Starborn;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public abstract class Minigame : MonoBehaviour
{
    StarbornInputSystem m_inputSystem;

    public StarbornInputSystem inputSystem => m_inputSystem;

    [HideInInspector]
    public string minigameName;
    public Conductor conductor;
    public bool autoPlay;
    public bool isTutorial;
    protected bool _startGame;
    public bool startGame
    {
        set
        {
            _startGame = value;
        }
    }
    protected bool completed
    {
        get
        {
            if (hasCompleted != null)
                return hasCompleted.Invoke();
            return false;
        }
    }

    public AudioClip song;
    public AudioClip tutorialSong;
    public string caller = "bugz";
    protected bool inTutorial;

    [HideInInspector]
    public bool paused;

    //public List<RhythmInput> inputs = new List<RhythmInput>();
    [HideInInspector]
    public List<string> eventsList = new List<string>();
    public string countdown;
    [SerializeField]
    public List<Charting> chartings = new List<Charting>();
    [SerializeField]
    [HideInInspector]
    public List<Charting> checkChartings = new List<Charting>();

    public Tutorial tutorial;
    public string tutorialCountdown;
    public List<Charting> endlessChartings = new List<Charting>();
    [HideInInspector] public List<RhythmEvent> events = new List<RhythmEvent>();
    protected Charting selectedCharting;

    public static double ngEarlyTimeBase = 0.1, justEarlyTimeBase = 0.05, aceEarlyTimeBase = 0.01, aceLateTimeBase = 0.01, justLateTimeBase = 0.05, ngLateTimeBase = 0.1;

    public static double ngEarlyTime => ngEarlyTimeBase * Conductor.instance?.music.pitch ?? 1;
    public static double justEarlyTime => justEarlyTimeBase * Conductor.instance?.music.pitch ?? 1;
    public static double aceEarlyTime => aceEarlyTimeBase * Conductor.instance?.music.pitch ?? 1;
    public static double aceLateTime => aceLateTimeBase * Conductor.instance?.music.pitch ?? 1;
    public static double justLateTime => justLateTimeBase * Conductor.instance?.music.pitch ?? 1;
    public static double ngLateTime => ngLateTimeBase * Conductor.instance?.music.pitch ?? 1;

    [Header("Camera")]
    public Camera defaultCamera;
    public float zoom = 5f;
    public Color bgColor = new Color(0.19215686274f, 0.30196078431f, 0.47450980392f);
    public Vector3 camPosition = new Vector3(0,0,-10);

    [HideInInspector]
    public EventSystem eventSystem;

    [Space]
    [Header("Minigame")]
    public System.Func<bool> hasCompleted;

    [HideInInspector]
    public System.Func<int> amountJudger;

    public static double NgEarlyTime(float pitch = -1, double margin = 0)
    {
        if (pitch < 0)
            return 1 - (ngEarlyTime + margin);
        return 1 - ((ngEarlyTime + margin) * pitch);
    }

    public static double NgLateTime(float pitch = -1, double margin = 0)
    {
        if (pitch < 0)
            return 1 + (ngLateTime + margin);
        return 1 + ((ngLateTime + margin) * pitch);
    }

    public static double JustEarlyTime(float pitch = -1, double margin = 0)
    {
        if (pitch < 0)
            return 1 - (justEarlyTime + margin);
        return 1 - ((justEarlyTime + margin) * pitch);
    }

    public static double JustLateTime(float pitch = -1, double margin = 0)
    {
        if (pitch < 0)
            return 1 + (justLateTime + margin);
        return 1 + ((justLateTime + margin) * pitch);
    }

    public static double AceEarlyTime(float pitch = -1, double margin = 0)
    {
        if (pitch < 0)
            return 1 - (aceEarlyTime + margin);
        return 1 - ((aceEarlyTime + margin) * pitch);
    }

    public static double AceLateTime(float pitch = -1, double margin = 0)
    {
        if (pitch < 0)
            return 1 + (aceLateTime + margin);
        return 1 + ((aceLateTime + margin) * pitch);
    }

    public static Minigame instance;

    bool minigameListRecieved;
    public static bool gotGameOver;

#if UNITY_EDITOR
    public virtual void OnValidate()
    {
        System.Type myType = GetType();
        minigameName = myType.Namespace;
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        if(GetType().GetField("chartings") != null)
        {

        }

        eventsList.Clear();
        foreach (Assembly assembly in assemblies)
        {
            System.Type[] classes = ReflectionUtils.GetTypesInNamespace(assembly, minigameName);
            if (classes.Length != 0)
            {
                for (int i = 0; i < classes.Length; i++)
                {
                    if (classes[i].IsSubclassOf(typeof(RhythmEvent)))
                    {
                        eventsList.Add(classes[i].Name);
                    }
                }
            }
        }

        foreach (Charting chart in chartings)
        {
            foreach (Section section in chart.sections)
            {
                foreach (Inputs input in section.inputList)
                {
                    input.events.Clear();
                    for (int i = 0; i < eventsList.Count; i++)
                    {
                        input.events.Add(eventsList[i]);
                    }
                }
            }
        }
    }
#endif
    public virtual void Awake()
    {
        instance = this;
        if(defaultCamera != null)
        {
            zoom = defaultCamera.orthographicSize;
            bgColor = defaultCamera.backgroundColor;
            camPosition = defaultCamera.transform.position;
        }
        eventSystem = FindFirstObjectByType<EventSystem>();
        //Debug.Log(ReflectionUtils.GetTypesInNamespace(System.AppDomain.CurrentDomain.GetAssemblies()[1],"Starborn." + minigameName).Length);
        m_inputSystem = new StarbornInputSystem();
        m_inputSystem.Rhythm.A.performed += onA;
        m_inputSystem.Rhythm.A.canceled += onReleaseA;
        m_inputSystem.Rhythm.Left.performed += onLeft;
        m_inputSystem.Rhythm.Down.performed += onDown;
        m_inputSystem.Rhythm.Up.performed += onUp;
        m_inputSystem.Rhythm.Right.performed += onRight;
        m_inputSystem.Rhythm.Pad.performed += onPad;
        m_inputSystem.Rhythm.Pad.canceled += onReleasePad;

        if (chartings.Count != 0) selectedCharting = chartings[Random.Range(0, chartings.Count - 1)];

        MinigameManager.managerType = "";
        Resources.Load<GameObject>($"Prefabs/Manager");
    }

    private void OnEnable()
    {
        m_inputSystem.Rhythm.Enable();
    }

    private void OnDisable()
    {
        m_inputSystem.Rhythm.Disable();
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        if(gotGameOver)
        {
            StartCoroutine(SongSetUp());
            IEnumerator SongSetUp()
            {
                yield return new WaitForSeconds(2.1f);
                setUpSong = true;
                SetUpSong();
                gotGameOver = false;
            }
        }

        //SetUpSong();
    }

    [HideInInspector]
    public bool setUpSong = false;
    public virtual void SetUpSong(string component = "")
    {
        TweenManager.instance.AddManager();
        if (isTutorial)
        {
            MinigameManager.instance.StartTutorial();
            inTutorial = true;
        }
        else
        {
            StartSong();
        }
    }
    [HideInInspector]
    public bool eligibleForClear = false;
    public virtual void StartSong()
    {
        if (song != null) Conductor.instance.music.clip = song;
        if (selectedCharting.setBPM && selectedCharting.bpm > 0)
            Conductor.instance.manualBpm = selectedCharting.bpm;
        Conductor.instance.SetUpBPM();
        inTutorial = false;
        selectedCharting.AddCharting(Conductor.instance.crochet, minigameName);
        MinigameManager.SetCongratsAudio(caller);
        AdditionalSongSetup();
        OnCountdown();
        if (selectedCharting.skipCountdown)
        {
            Conductor.instance.PlayMusic();
            eligibleForClear = true;
        }
        else
            Countdown.StartCountdown(Conductor.instance.crochet, delegate()
            {
                Conductor.instance.PlayMusic();
                eligibleForClear = true;
            });
        if(!isTutorial && MinigameManager.instance.backdrop != null)
            MinigameManager.instance.backdrop.SetActive(false);
        //MinigameManager.instance.hearts.gameObject.SetActive(true);
        MinigameManager.instance.hearts.gameObject.SetActive(true);
        foreach (Transform child in MinigameManager.instance.hearts.gameObject.transform)
        {
            if (child.gameObject.GetComponent<Image>() || child.gameObject.GetComponent<SpriteRenderer>())
            {
                ColorUtils.SetAlpha(child.gameObject, 0);
                TweenManager.AlphaTween(child.gameObject, 0, 1, 1f, Eases.EaseInOutCubic).SetStartDelay(0.25f);
            }
        }
        MinigameManager.instance.accuracyObj.SetActive(true);
        foreach (Transform child in MinigameManager.instance.accuracyObj.transform)
        {
            if (child.gameObject.GetComponent<Image>() || child.gameObject.GetComponent<TMP_Text>())
            {
                ColorUtils.SetAlpha(child.gameObject, 0);
                TweenManager.AlphaTween(child.gameObject, 0, 1, 1f, Eases.EaseInOutCubic).SetStartDelay(0.25f);
            }
        }

        _startGame = true;

        //Debug.Log(MinigameManager.instance.inputs.Count);
            
        //Debug.Log(MinigameManager.instance.inputs.Count);

        //StartCoroutine(PlayMusic());
        //IEnumerator PlayMusic()
        //{
        //yield return new WaitForSeconds(1);
        //Countdown.StartCountdown(Conductor.instance.crochet, Conductor.instance.music.Play);
        //Conductor.instance.music.Play();
        //}
    }

    int _curBeat = 0;
    int prevBeat = 0;
    public int curBeat => _curBeat;

    int _curStep = 0;
    int prevStep = 0;

    public int curStep => _curStep;

    // Update is called once per frame
    public virtual void Update()
    {
        if (Conductor.instance.isPlaying)
        {
            _curBeat = Conductor.instance.curBeat;
            _curStep = Conductor.instance.curStep;
            BeatUpdate();
            StepUpdate();

        }
    }

    public virtual void onDestroy()
    {
        Resources.UnloadUnusedAssets();
    }

    public System.Action<int> OnBeatChange;
    public System.Action<int> OnBeatTutorial;

    public System.Action<int> OnStepChange;
    public System.Action<int> OnStepTutorial;

    public System.Action OnSongStart;
    public System.Action OnGameOver;

    public void BeatUpdate()
    {
        if (prevBeat != _curBeat)
        {
            OnBeatChange?.Invoke(_curBeat);
            OnBeatTutorial?.Invoke(_curBeat);
        }
        prevBeat = _curBeat;
    }

    public void StepUpdate()
    {
        if (prevStep != _curStep)
        {
            OnStepChange?.Invoke(_curStep);
            OnStepTutorial?.Invoke(_curStep);
        }
        prevStep = _curStep;
    }

    public virtual void AdditionalSongSetup(string tag = "")
    {

    }

    public virtual void TutorialAdditionals()
    {

    }

    public virtual void PostTutorialAdditionals()
    {

    }

    public virtual void OnCountdown()
    {

    }

    //If you're in a tutorial and you need to do a reset of something
    public virtual void TutorialReset()
    {
        Countdown.ResetCountdown();
    }

    public virtual void TutorialOnComplete(int amount, string tag = "")
    {
        Countdown.ResetCountdown();
    }

    public virtual void TutorialCallback(string tag = "")
    {

    }

    public virtual void OnTutorialStop()
    {

    }

    public virtual void onA(InputAction.CallbackContext context)
    {
        //If the A button has been pressed
    }

    public virtual void onReleaseA(InputAction.CallbackContext context)
    {
        //If the A button has been released
    }

    public virtual void onLeft(InputAction.CallbackContext context)
    {
        //If the Left button has been pressed
    }

    public virtual void onDown(InputAction.CallbackContext context)
    {
        //If the Down button has been pressed
    }

    public virtual void onUp(InputAction.CallbackContext context)
    {
        //If the Up button has been pressed
    }

    public virtual void onRight(InputAction.CallbackContext context)
    {
        //If the Right button has been pressed
    }

    public virtual void onPad(InputAction.CallbackContext context)
    {
        //If the arrow pad has been pressed
    }


    public virtual void onReleasePad(InputAction.CallbackContext context)
    {
        //If the arrow pad has been released
    }
}

//This is specifically for the minigames
public class GamePopupAttribute : PropertyAttribute
{

}

public class ParamAttribute : PropertyAttribute
{
    //public string eventName;

    /*public ParameterAttribute(string name)
    {
        eventName = name;
    }*/
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GamePopupAttribute))]
public class MinigameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<string> events = null;
        //List<List<Attribute>> attributes = new List<List<Attribute>>();
        if(property.serializedObject.targetObject.GetType() == typeof(Minigame) || property.serializedObject.targetObject.GetType().IsSubclassOf(typeof(Minigame)))
        {
            System.Type t = property.serializedObject.targetObject.GetType();
            List<Charting> chartings = t.GetField("chartings").GetValue(property.serializedObject.targetObject) as List<Charting>;
            foreach(Charting charting in chartings)
            {
                List<Section> sections = charting.GetType().GetField("sections").GetValue(charting) as List<Section>;
                foreach(Section section in sections)
                {
                    List<Inputs> inputs = section.GetType().GetField("inputList").GetValue(section) as List<Inputs>;
                    foreach(Inputs input in inputs)
                    {
                        events = input.GetType().GetField("events").GetValue(input) as List<string>;
                    }
                }
            }

            if(events != null && events.Count != 0)
            {
                int selectedIndex = Mathf.Max(events.IndexOf(property.stringValue), 0);
                selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, events.ToArray());
                property.stringValue = events[selectedIndex];
            }
            else
                EditorGUI.PropertyField(position, property, label);
        }
        else
            EditorGUI.PropertyField(position, property, label);
    }
}

/*[CustomPropertyDrawer(typeof(ParamAttribute))]
public class ParameterArea : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<Parameter> parameters = null;
        //ParameterAttribute paramAttribute = (ParameterAttribute)attribute;
        if (property.serializedObject.targetObject.GetType() == typeof(Minigame) || property.serializedObject.targetObject.GetType().IsSubclassOf(typeof(Minigame)))
        {
            System.Type t = property.serializedObject.targetObject.GetType();
            List<Charting> chartings = t.GetField("chartings").GetValue(property.serializedObject.targetObject) as List<Charting>;
            foreach (Charting charting in chartings)
            {
                List<Section> sections = charting.GetType().GetField("sections").GetValue(charting) as List<Section>;
                foreach (Section section in sections)
                {
                    List<Inputs> inputs = section.GetType().GetField("inputList").GetValue(section) as List<Inputs>;
                    foreach (Inputs input in inputs)
                    {
                        System.Type classType = ReflectionUtils.FindClassAcrossAllNamespaces(input.Event);
                        if (classType != null)
                        {
                            FieldInfo parameterProperty = classType.GetField("parameters");
                            object instance = System.Activator.CreateInstance(classType);
                            parameters = (List<Parameter>)parameterProperty.GetValue(instance);
                        }
                    }
                }
            }

            //Debug.Log(parameters.Count);

            if (parameters != null && parameters.Count != 0)
            {
                foreach(Parameter parameter in parameters)
                {
                    //if(parameter.parameter.GetType() == typeof(bool))
                    //{
                        //EditorGUI.Toggle(position, parameter.caption, (bool)parameter.parameter);
                        //property.
                    //}
                }
                //int selectedIndex = Mathf.Max(events.IndexOf(property.stringValue), 0);
                //selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, events.ToArray());
                //property.stringValue = events[selectedIndex];
            }
            else
                EditorGUI.PropertyField(position, property, label);

        }
        else
            EditorGUI.PropertyField(position, property, label);
    }
}*/
#endif