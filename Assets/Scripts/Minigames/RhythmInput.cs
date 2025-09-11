using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Starborn.InputSystem
{
    public interface IInput
    {
        //void Action();
    }

    public class RhythmInput : IInput
    {
        public delegate void EventCallback(RhythmInput call);

        public Action onHit;
        public Action onMiss;
        public Action<bool> onHalfHit;
        //public EventCallback OnMiss;

        public float curHit; //The Conductor's current position
        public float desHit; //The song position that the player's suppose to hit

        public float range; //The song's seconds per beat (spb)
        public float startPoint;
        public float endPoint;

        public bool checkForAccuracy;
        public bool success;
        private bool mustHit;
        public bool MustHit => mustHit;
        private bool hasHit;

        private InputAction InputAction;

        private int id;

        public int state = 0;

        private StarbornInputSystem m_inputSystem = new StarbornInputSystem();
        private RhythmInputs _action;
        public RhythmInputs action
        {
            get
            {
                return _action;
            }
        }

        public RhythmInput(RhythmInputs action)
        {
            _action = action;
            id = (int)UnityEngine.Random.Range(1, 1000);
            MinigameManager.instance.inputs.Add(this);
            Generate();
        }

        public void onInputHit(InputAction.CallbackContext context)
        {
            Action();
            float accurary = 0;
            if(checkForAccuracy && mustHit && !hasHit)
            {
                bool early = false;
                if(curHit == desHit)
                {
                    Debug.Log(id + ": " + 1.0f);
                    accurary = 1.0f;
                }
                else if(curHit >= startPoint && curHit < desHit)
                {
                    Debug.Log(id + ": " + MathUtils.Normalize(curHit, startPoint, desHit));
                    accurary = MathUtils.Normalize(curHit, startPoint, desHit);
                    early = true;
                }
                else if(curHit <= endPoint && curHit > desHit)
                {
                    Debug.Log(id + ": " + MathUtils.ReverseNormalize(curHit, desHit, endPoint));
                    accurary = MathUtils.ReverseNormalize(curHit, desHit, endPoint);
                }

                if (accurary >= 0.5)
                    onHit?.Invoke();
                else
                    onHalfHit?.Invoke(early);

                hasHit = true;
            }


        }

        public void Action()
        {

        }

        void Generate()
        {
            //Debug.Log(m_inputSystem.Rhythm.A);
            //m_inputSystem = new StarbornInputSystem();
            InputAction[] actionList = 
            { 
                m_inputSystem.Rhythm.A, 
                m_inputSystem.Rhythm.Left, 
                m_inputSystem.Rhythm.Right, 
                m_inputSystem.Rhythm.Up, 
                m_inputSystem.Rhythm.Down
            };

            switch (_action.ToString())
            {
                case "A":
                    InputAction = m_inputSystem.Rhythm.A;
                    break;
                case "Left":
                    InputAction = m_inputSystem.Rhythm.Left;
                    break;
                case "Down":
                    InputAction = m_inputSystem.Rhythm.Right;
                    break;
                case "Up":
                    InputAction = m_inputSystem.Rhythm.Up;
                    break;
                case "Right":
                    InputAction = m_inputSystem.Rhythm.Down;
                    break;
                case "Pad":
                    InputAction = m_inputSystem.Rhythm.Pad;
                    break;
                case "Random":
                    InputAction = actionList[(int)UnityEngine.Random.Range(1, actionList.Length - 1)];
                    break;
                default:
                    InputAction = null;
                    break;
            }

            mustHit = _action != RhythmInputs.None;
            //Debug.Log(InputAction);
            if (mustHit)
                InputAction.performed += onInputHit;

            //desHit = destination;
        }
        public RhythmInput SetOnHit(Action action)
        {
            onHit = action;
            return this;
        }

        public RhythmInput SetOnHalfHit(Action<bool> action)
        {
            onHalfHit = action;
            return this;
        }
        public RhythmInput SetDestination(float destination)
        {
            desHit = destination;
            return this;
        }

        public RhythmInput SetRange(float start = 0, float end = 0)
        {
            startPoint = desHit - start;
            endPoint = desHit + end;
            return this;
        }

        public void Enable()
        {
            InputAction.Enable();
        }

        public void Disable()
        {
            InputAction.Disable();
        }

        public void Update(float time)
        {
            if(mustHit)
            {
                curHit = time;
                checkForAccuracy = (curHit >= startPoint) && (curHit <= endPoint);

                if(curHit > endPoint && !hasHit)
                {
                    onMiss?.Invoke();
                    hasHit = true;
                }
            }
        }
    }

}

public enum RhythmInputs
{
    None,
    A,
    Left,
    Right,
    Up,
    Down,
    Pad,
    Random
}

