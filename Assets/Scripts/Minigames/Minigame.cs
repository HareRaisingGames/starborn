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

public abstract class Minigame : MonoBehaviour
{
    StarbornInputSystem m_inputSystem;

    [HideInInspector]
    public string minigameName;
    public Conductor conductor;
    public bool autoPlay;
    public bool isTutorial;
    protected bool startGame;
    protected bool completed;

    public AudioClip song;
    public AudioClip tutorialSong;

    [HideInInspector]
    public bool paused;

    //public List<RhythmInput> inputs = new List<RhythmInput>();
    [HideInInspector]
    public List<string> eventsList = new List<string>();
    public List<Charting> chartings = new List<Charting>();
    public Tutorial tutorial;
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

    public virtual void OnValidate()
    {
        foreach (Charting chart in chartings)
        {
            foreach (Section section in chart.sections)
            {
                foreach (Inputs input in section.inputList)
                {
                    input.events.Clear();
                }
            }
        }

        if(GetType().GetField("chartings") != null)
        {

        }

        eventsList.Clear();
        System.Type myType = GetType();
        minigameName = myType.Namespace;
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
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
                    /*input.events = eventsList;*/
                    foreach (Assembly assembly in assemblies)
                    {
                        System.Type[] classes = ReflectionUtils.GetTypesInNamespace(assembly, minigameName);
                        if (classes.Length != 0)
                        {
                            for (int i = 0; i < classes.Length; i++)
                            {
                                if (classes[i].IsSubclassOf(typeof(RhythmEvent)))
                                {
                                    input.events.Add(classes[i].Name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

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
        m_inputSystem.Rhythm.Left.performed += onLeft;
        m_inputSystem.Rhythm.Down.performed += onDown;
        m_inputSystem.Rhythm.Up.performed += onUp;
        m_inputSystem.Rhythm.Right.performed += onRight;
        m_inputSystem.Rhythm.Pad.performed += onPad;

        if(chartings.Count != 0) selectedCharting = chartings[Random.Range(0, chartings.Count - 1)];
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
        //Debug.Log(events.Count);
        TweenManager.instance.AddManager();
        Conductor.instance.SetUpBPM();
        if(isTutorial)
        {
            MinigameManager.instance.StartTutorial();
        }
        else
        {
            //StartSong();
        }
        //selectedCharting.AddCharting(Conductor.instance.crochet, minigameName);
    }

    public void StartSong()
    {
        MinigameManager.Clear();
        if (song != null) Conductor.instance.music.clip = song;
        Conductor.instance.SetUpBPM();
        selectedCharting.AddCharting(Conductor.instance.crochet, minigameName);
        Countdown.StartCountdown(Conductor.instance.crochet, Conductor.instance.music.Play);

        //StartCoroutine(PlayMusic());
        //IEnumerator PlayMusic()
        //{
        //yield return new WaitForSeconds(1);
        //Countdown.StartCountdown(Conductor.instance.crochet, Conductor.instance.music.Play);
            //Conductor.instance.music.Play();
        //}
    }

    protected int curBeat = 0;
    int prevBeat = 0;

    // Update is called once per frame
    public virtual void Update()
    {
        if (Conductor.instance.isPlaying)
        {
            curBeat = Conductor.instance.curBeat;
            foreach(RhythmEvent eventT in events)
            {
                eventT.CheckForInvoke(Conductor.instance.songPosition);
            }
        }
            

        if (prevBeat != curBeat) OnBeatChange?.Invoke();
        prevBeat = curBeat;
    }

    public System.Action OnBeatChange;

    public virtual void onA(InputAction.CallbackContext context)
    {
        //If the A button has been pressed
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
}

//This is specifically for the minigames
public class GamePopupAttribute : PropertyAttribute
{

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
#endif