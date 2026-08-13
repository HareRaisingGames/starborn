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

        const float inputEnableBuffer = 0.05f;

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
                UpdateInputCallbacks();
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
        bool callbacksRegistered;
        bool disposed;
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
            enabled = false;
            Generate();
        }

        public void onInputHit(InputAction.CallbackContext context)
        {
            if(!enabled) return;

            curHit = Conductor.instance.songPosition;
            checkForAccuracy = (curHit >= startPoint) && (curHit <= endPoint);

            float accurary = 0;
            if (checkForAccuracy && mustHit && !hasHit && !autoplay)
            {
                bool early = false;
                if(curHit == desHit)
                {
                    accurary = 1.0f;
                }
                else if(curHit >= startPoint && curHit < desHit)
                {
                    accurary = MathUtils.Normalize(curHit, startPoint, desHit);
                    Debug.Log(accurary);
                    early = true;
                }
                else if(curHit <= endPoint && curHit > desHit)
                {
                    accurary = MathUtils.ReverseNormalize(curHit, desHit, endPoint);
                    Debug.Log(accurary);
                }

                if (accurary >= 0.8)
                {
                    onHit?.Invoke();
                    if (accurary >= 0.95)
                        accurary = 1;
                    MinigameManager.instance.accuracies.Add(accurary);
                    success = true;
                    hasHit = true;
                    Disable();
                }
                else if(accurary < 0.8 && accurary >= 0.6)
                {
                    onHalfHit?.Invoke(early);
                    MinigameManager.instance.accuracies.Add(accurary);
                    success = true;
                    hasHit = true;
                    Disable();
                }
                else
                {
                    onMiss?.Invoke();
                    MinigameManager.instance.accuracies.Add(0f);
                    success = true;
                    hasHit = true;
                    Disable();
                }

                MinigameManager.instance.displayAccuracy = 0;
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
            UpdateInputCallbacks();
                 

            //desHit = destination;
        }

        void UpdateInputCallbacks()
        {
            if (disposed || InputAction == null || !mustHit)
            {
                RemoveInputCallbacks();
                return;
            }

            if (autoplay)
            {
                RemoveInputCallbacks();
                return;
            }

            if (!callbacksRegistered)
            {
                InputAction.performed += onInputHit;
                InputAction.canceled += onInputRelease;
                callbacksRegistered = true;
            }
        }

        void RemoveInputCallbacks()
        {
            if (InputAction != null && callbacksRegistered)
            {
                InputAction.performed -= onInputHit;
                InputAction.canceled -= onInputRelease;
                callbacksRegistered = false;
            }
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
            if (disposed || InputAction == null)
                return;

            InputAction.Enable();
            enabled = true;
        }

        public void Disable()
        {
            if (InputAction == null)
                return;

            InputAction.Disable();
            enabled = false;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            RemoveInputCallbacks();
            Disable();
            m_inputSystem.Dispose();
            disposed = true;
        }

        bool enabled = false;

        bool found;
        public void Update(float time)
        {
            if(mustHit)
            {
                curHit = time;
                checkForAccuracy = (curHit >= startPoint) && (curHit <= endPoint);

                if(!enabled && !success && curHit >= startPoint - inputEnableBuffer && curHit <= endPoint)
                {
                    Enable();
                }

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
                    onMiss?.Invoke();
                    MinigameManager.instance.accuracies.Add(0f);
                    hasHit = true;
                    success = true;
                    Disable();
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
