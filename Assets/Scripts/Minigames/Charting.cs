using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEditor;
using System;

namespace Starborn.InputSystem
{
    [System.Serializable]
    public class Charting
    {
        public bool skipCountdown = false;
        public List<Section> sections = new List<Section>();
        public bool setBPM = false;
        public float bpm;
        public Charting()
        {

        }

        public void AddCharting(float beat, string namespaceName = "")
        {
            var start = 0;
            foreach(Section section in sections)
            {
                section.startLength = start;
                for(int i = 0; i < section.loops; i++)
                {
                    foreach(Inputs input in section.inputList)
                    {
                        string className = (namespaceName != "" ? namespaceName + "." : "") + input.Event;
                        Type eventType = Type.GetType(className, false, false);
                        if (eventType != null)
                        {
                            MethodInfo method = eventType.GetMethod("AddToChart");
                            FieldInfo parameterProperty = eventType.GetField("parameters");
                            object newObject = Activator.CreateInstance(eventType, null);

                            //Debug.Log($"{input.parameters.Count}\n");
                           
                            if (method != null)
                            {
                                var parameters = new object[] { beat * (start + input.mark), beat };
                                var result = method.Invoke(newObject, parameters);
                            }
                        }
                    }
                    //Generate an event from string
                    /**
                     * Generate event from string
                     * Type event = Get Selected type
                     * Get AddToChart event with time * (start + event.beat)
                     */
                    start += section.length;
                }
                //start += section.length * section.loops;
            }
        }
    }

    [System.Serializable]
    public class Tutorial
    {
        public List<TutorialLine> lines = new List<TutorialLine>();
        public List<TutorialLine> skipTutorial = new List<TutorialLine>();
        public Tutorial()
        {

        }
    }

    [System.Serializable]
    public class TutorialLine
    {
        public string dialogue;
        public Charting section;
        public int amount = 1;
    }
    [System.Serializable]
    public class Section
    {
        public List<Inputs> inputList = new List<Inputs>();
        [HideInInspector]
        public int startLength = 0;
        public int length = 4;
        public int loops = 1;
        public Section()
        {
            length = 4;
            loops = 1;
        }
        //public int placement = 1;

        public void Generate()
        {
            //foreach (Inputs input in inputList)
            //{
            //inputs.Add(new RhythmInput(input.input));
            //}

            //foreach (RhythmInput input in inputs)
            //{
            //input.Generate();
            //input.Enable();
            //}
        }
    }

    [System.Serializable]
    public class Inputs
    {
        //public RhythmEvent Event;
        [HideInInspector]
        public List<string> events;
        //[ListToPopUp(typeof(Inputs), "events")]
        [GamePopup]
        public string Event;
        /*[HideInInspector]
        public List<Parameter> parameters
        {
            get
            {
                Type classType = ReflectionUtils.FindClassAcrossAllNamespaces(Event);
                if (classType != null)
                {
                    FieldInfo parameterProperty = classType.GetField("parameters");
                    object instance = Activator.CreateInstance(classType);
                    return (List<Parameter>)parameterProperty.GetValue(instance);
                }
                return new List<Parameter>();
            }
        }*/
        //[Parameter]
        [HideInInspector]
        public List<Parameter> displayParameters;
        public float mark;
    }
}
/*
{
    get
    {
        string eventName = $"Starborn.{Event}";
        Type classType = Type.GetType(eventName);
        if (classType != null)
        {
            FieldInfo parameterProperty = classType.GetField("parameters");
            object instance = Activator.CreateInstance(classType);
            return (List<Parameter>)parameterProperty.GetValue(instance);
        }
        return new List<Parameter>();
    }
}*/


