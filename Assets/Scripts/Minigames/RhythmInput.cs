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
        public float[] margin = new float[2];

        public bool checkForAccuracy;
        public bool success;
        private bool mustHit;
        public bool MustHit => mustHit;
        private bool hasHit;

        bool autoplay = false;

        public bool AUTOPLAY
        {
            set
            {
                autoplay = value;
                if(autoplay)
                    InputAction.performed -= onInputHit;
                else
                    InputAction.performed += onInputHit;
            }
        }

        public bool HasHit
        {
            get
            {
                return hasHit;
            }
            set
            {
                hasHit = value;
            }
        }

        bool _canPlay = true;
        public bool canPlay
        {
            set
            {
                _canPlay = value;
            }
        }

        private InputAction InputAction;
        public InputAction input => InputAction;

        private int id;

        public int state = 0;

        private StarbornInputSystem m_inputSystem = new StarbornInputSystem();
        private RhythmInputs _action;

        private float spb;
        public float secPerBeat => spb;
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
            spb = Conductor.instance.crochet;
            enabled = true;
            Generate();
        }

        public void onInputHit(InputAction.CallbackContext context)
        {
            if(!enabled) return;

            float accurary = 0;
            // Debug.Log(curHit);
            if (checkForAccuracy && mustHit && !hasHit && !autoplay)
            {
                //TimeToAccuracy(curHit);
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

                if (accurary >= 0.8)
                {
                    onHit?.Invoke();
                    if (accurary >= 0.95) //Super close means it's a perfect hit
                        accurary = 1;
                    success = true;
                }
                else if(accurary < 0.8 && accurary >= 0.6)
                {
                    onHalfHit?.Invoke(early);
                    success = true;
                }

                MinigameManager.instance.accuracies.Add(accurary);
                MinigameManager.instance.displayAccuracy = 0;

                hasHit = true;
            }


        }

        public void onInputRelease (InputAction.CallbackContext context)
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
                    InputAction = m_inputSystem.Rhythm.Down;
                    break;
                case "Up":
                    InputAction = m_inputSystem.Rhythm.Up;
                    break;
                case "Right":
                    InputAction = m_inputSystem.Rhythm.Right;
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
            if (mustHit && !autoplay)
            {
                InputAction.performed += onInputHit;
                InputAction.canceled += onInputRelease;
            }
                

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

        public RhythmInput SetOnMiss(Action action)
        {
            onMiss = action;
            return this;
        }

        public RhythmInput SetDestination(float destination)
        {
            desHit = destination;
            return this;
        }

        public RhythmInput SetRange(float start = 0, float end = 0)
        {
            margin[0] = start;
            margin[1] = end;
            startPoint = desHit - start * spb;
            endPoint = desHit + end * spb;
            return this;
        }

        public void Enable()
        {
            InputAction.Enable();
            enabled = true;
        }

        public void Disable()
        {
            InputAction.Disable();
            enabled = false;
        }

        bool enabled = false;

        bool found;
        public void Update(float time)
        {
            if(mustHit)
            {
                curHit = time;
                checkForAccuracy = (curHit >= startPoint) && (curHit <= endPoint);

                /*if(checkForAccuracy)
                {
                    //Debug.Log("Hit!");
                    if(curHit >= startPoint && curHit < desHit)
                    {
                        if (MathUtils.Normalize(curHit, startPoint, desHit) >= 0.8)
                        {
                            Debug.Log("Good!");
                        }
                        //Debug.Log(id + ": " + MathUtils.Normalize(curHit, startPoint, desHit));
                    }
                    else if(curHit <= endPoint && curHit > desHit)
                    {
                        if (MathUtils.ReverseNormalize(curHit, desHit, endPoint) >= 0.8)
                        {
                            Debug.Log("Good!");
                        }
                        //Debug.Log(id + ": " + MathUtils.ReverseNormalize(curHit, desHit, endPoint));
                    }
                }*/

                if(autoplay)
                {
                    if(curHit >= desHit && !found)
                    {
                        found = true;
                        onHit?.Invoke();
                        success = true;
                        hasHit = true;
                        //Debug.Log("Brrrap!");
                    }
                }

                if(curHit > endPoint && !success)
                {
                    //Debug.Log("Ack!");
                    onMiss?.Invoke();
                    hasHit = true;
                    success = true;
                }
            }
        }

        public void OnMiss()
        {
            Debug.Log("Ack!");
            if (_canPlay)
            {
                Debug.Log("Ack!");
                onMiss?.Invoke();
                //Debug.Log("Oh no!");
                //MinigameManager.instance.LoseALife();
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

