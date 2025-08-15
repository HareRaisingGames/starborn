using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using System.Reflection;
using Starborn;
using System.Linq;
using UnityEditor;

public abstract class Minigame : MonoBehaviour
{
    StarbornInputSystem m_inputSystem;

    public string minigameName;
    public Conductor conductor;

    protected bool startGame;

    //public List<RhythmInput> inputs = new List<RhythmInput>();
    [HideInInspector]
    public List<string> eventsList = new List<string>();
    public List<Charting> chartings = new List<Charting>();
    protected Charting selectedCharting;

    public static double ngEarlyTimeBase = 0.1, justEarlyTimeBase = 0.05, aceEarlyTimeBase = 0.01, aceLateTimeBase = 0.01, justLateTimeBase = 0.05, ngLateTimeBase = 0.1;

    //public static double ngEarlyTime => ngEarlyTimeBase * Conductor.instance?.SongPitch ?? 1;
    //public static double justEarlyTime => justEarlyTimeBase * Conductor.instance?.SongPitch ?? 1;
    //public static double aceEarlyTime => aceEarlyTimeBase * Conductor.instance?.SongPitch ?? 1;
    //public static double aceLateTime => aceLateTimeBase * Conductor.instance?.SongPitch ?? 1;
    //public static double justLateTime => justLateTimeBase * Conductor.instance?.SongPitch ?? 1;
    //public static double ngLateTime => ngLateTimeBase * Conductor.instance?.SongPitch ?? 1;

    public virtual void OnValidate()
    {
        eventsList.Clear();
        System.Type myType = GetType();
        string nameSpaceName = myType.Namespace;
        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            System.Type[] classes = ReflectionUtils.GetTypesInNamespace(assembly, nameSpaceName);
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

        foreach(Charting chart in chartings)
        {
            foreach(Section section in chart.sections)
            {
                foreach(Inputs input in section.inputList)
                {
                    input.events = eventsList;
                }
            }
        }
    }

    public virtual void Awake()
    {
        //Debug.Log(ReflectionUtils.GetTypesInNamespace(System.AppDomain.CurrentDomain.GetAssemblies()[1],"Starborn." + minigameName).Length);
        m_inputSystem = new StarbornInputSystem();
        m_inputSystem.Rhythm.A.performed += onA;
        m_inputSystem.Rhythm.Left.performed += onLeft;
        m_inputSystem.Rhythm.Down.performed += onDown;
        m_inputSystem.Rhythm.Up.performed += onUp;
        m_inputSystem.Rhythm.Right.performed += onRight;

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
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

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
}
