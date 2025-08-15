using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEditor;

namespace Starborn.InputSystem
{
    [System.Serializable]
    public class Charting
    {
        public List<Section> sections = new List<Section>();
    }

    [System.Serializable]
    public class Section
    {
        public List<Inputs> inputList = new List<Inputs>();
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
        public List<string> events = new List<string>();
        public float mark;
    }
}


