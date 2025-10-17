using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Starborn.InputSystem
{
    [System.Serializable]
    ///<summary>
    /// How will the events work?
    ///
    /// The start point is the song position in which the event has been placed
    ///
    /// Each function will be called based on the song position's plus the crochet (seconds per beat) times the beat placement
    /// startPoint + Conductor.instance.crochet* beat
    ///
    /// If the song's position matches the placement of the event's beat, Invoke the Action
    /// </summary>
    public class RhythmEvent : IREvent
    {
        public virtual void SetUp()
        {

        }

        //public SetUp setUp;

        //Where to start the event
        [HideInInspector] public float startPoint;
        public List<Attribute> attributes;
        public void AddAttribute(string name, Func<dynamic> property, Type type, dynamic value = null)
        {
            foreach (Attribute attribute in attributes)
            {
                if (name == attribute.name)
                    return;
            }

            attributes.Add(new Attribute(name, property, type, value));
        }

        public string minigame;

        [System.Serializable]
        public class CallForAction
        {
            private Action _action;
            private float _beat = 0; //Where each part of the event will be played per beat
            public RhythmInput input;
            public RhythmInputs inputMarker; //The input enum 
            private float _start;
            private float _end;
            private Action _onHit;

            private bool _hasInput = false;
            public bool hasInput => _hasInput;
            public Action action =>_action;

            public float beat => _beat;
            public float startPoint => _start;
            public float endPoint => _end;
            public Action onHit => _onHit;

            public CallForAction(Action action, float beat, RhythmInputs input = RhythmInputs.None, float start = 1, float end = 1, Action onHit = null)
            {
                _action = action;
                _beat = beat;
                inputMarker = input;
                _start = start;
                _end = end;
                _onHit = onHit;

                _hasInput = input != RhythmInputs.None;

                //this.input = new RhythmInput(input).SetDestination(beat).SetRange(_start,_end);

                //Debug.Log(startPoint);
                //Debug.Log(endPoint);

                //if (this.input.action != RhythmInputs.None && enable) this.input.Enable();
            }

            public void AddAction(Action action)
            {
                _action = action;
            }

            public CallForAction AddInput(float length, bool enable = false)
            {
                input = new RhythmInput(inputMarker).SetDestination(beat).SetRange(_start * length, _end * length).SetOnHit(_onHit);
                if (input.action != RhythmInputs.None && enable) input.Enable();
                return this;
            }

            public void UpdateInput(float time)
            {
                if(input != null)
                    input.Update(time);
            }
        }

        //This is the standard event diagram
        public List<CallForAction> actions = new List<CallForAction>();

        //This is what will be placed in the actual chart
        private List<CallForAction> actions_in_chart = new List<CallForAction>();
        //This is where all the inputs will be recorded
        private List<RhythmInput> inputs = new List<RhythmInput>();


        public RhythmEvent()
        {
            SetUp();

            if (Minigame.instance != null)
                Minigame.instance.events.Add(this);

            MinigameManager.instance.events.Add(this);
        }

        public void AddToChart(float time, float crochet = 1)
        {
            startPoint = time;
            foreach(CallForAction action in actions)
            {
                CallForAction newCFA = new CallForAction(action.action, startPoint + Conductor.instance.crochet * (action.beat - 1), action.inputMarker, action.startPoint, action.endPoint, action.onHit);
                if (action.hasInput)
                {
                    newCFA = newCFA.AddInput(crochet, true);
                    inputs.Add(newCFA.input);
                }
                actions_in_chart.Add(newCFA);
            }
        }

        //Call if the time of the song has been played for each event
        public void CheckForInvoke(float time)
        {
            for(int i = actions_in_chart.Count - 1; i > -1; i--)
            {
                if(time >= actions_in_chart[i].beat)
                {
                    actions_in_chart[i].action?.Invoke();
                    actions_in_chart[i] = null;
                    actions_in_chart.RemoveAt(i);
                }
            }

            foreach(RhythmInput input in inputs)
            {
                input.Update(time);
            }
        }
    }

    public class Attribute
    {
        ///string name
        ///Type type
        ///dynamic value
        ///dynamic property
        
        public string name;
        public Type type;
        public dynamic value;
        public Func<dynamic> property;

        public Attribute(string name, Func<dynamic> property, Type type, dynamic value = null)
        {
            this.name = name;
            this.property = property;
            this.value = value;
            this.type = type;

            if (this.value == null)
                this.value = property.Invoke();
        }
    }
}

