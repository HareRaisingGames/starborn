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

        private List<InputAction> misInputs = new List<InputAction>();

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

        public float savedAccuracy;

        public RhythmInput(RhythmInputs action, List<RhythmMisinputs> misinputs = null)
        {
            _action = action;
            id = (int)UnityEngine.Random.Range(1, 1000);
            MinigameManager.instance.inputs.Add(this);
            spb = Conductor.instance.crochet;
            enabled = false;
            SetMisinputs(misinputs);
            Generate();
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

        private Dictionary<InputAction, RhythmMisinputs> misinputCallbacks = new Dictionary<InputAction, RhythmMisinputs>();


        public void onInputHit(InputAction.CallbackContext context)
        {
            if (!enabled) return;

            curHit = Conductor.instance.songPosition;
            checkForAccuracy = (curHit >= startPoint) && (curHit <= endPoint);

            float accurary = 0;
            if (checkForAccuracy && mustHit && !hasHit && !autoplay)
            {
                // Debug.Log(MinigameManager.instance.curInputs.Count);
                bool early = false;
                if (curHit == desHit)
                {
                    accurary = 1.0f;
                }
                else if (curHit >= startPoint && curHit < desHit)
                {
                    accurary = MathUtils.Normalize(curHit, startPoint, desHit);
                    early = true;
                }
                else if (curHit <= endPoint && curHit > desHit)
                {
                    accurary = MathUtils.ReverseNormalize(curHit, desHit, endPoint);
                }
                savedAccuracy = accurary;

                if (MinigameManager.instance.curInputs.Count > 1)
                {
                    // Debug.Log(MinigameManager.FindHighestAccuracy());
                    // Debug.Log(accurary == MinigameManager.FindHighestAccuracy());
                    if (accurary != MinigameManager.FindHighestAccuracy())
                        return;
                }

                Debug.Log($"{id}: {accurary}");

                if (accurary >= 0.8)
                {
                    //Checks if it's the correct input
                    if (context.action.name == InputAction.name)
                    {
                        onHit?.Invoke();
                        if (accurary >= 0.95)
                            accurary = 1;
                        MinigameManager.instance.accuracies.Add(accurary);
                    }
                    else
                    {
                        if (misinputCallbacks.ContainsKey(context.action))
                        {
                            misinputCallbacks[context.action].InvokeHit();
                        }
                        MinigameManager.instance.accuracies.Add(0f);
                    }
                    success = true;
                    hasHit = true;
                    Disable();
                }
                else if (accurary < 0.8 && accurary >= 0.6)
                {
                    //Checks if it's the correct input
                    if (context.action.name == InputAction.name)
                    {
                        onHalfHit?.Invoke(early);
                        MinigameManager.instance.accuracies.Add(accurary);
                    }
                    else
                    {
                        if (misinputCallbacks.ContainsKey(context.action))
                        {
                            misinputCallbacks[context.action].InvokeHalfHit(early);
                        }
                        MinigameManager.instance.accuracies.Add(0f);
                    }
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

        public void onInputRelease(InputAction.CallbackContext context)
        {

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
                foreach(InputAction misinput in misInputs)
                {
                    misinput.performed += onInputHit;
                    misinput.canceled += onInputRelease;
                }
                callbacksRegistered = true;
            }
        }

        void RemoveInputCallbacks()
        {
            if (InputAction != null && callbacksRegistered)
            {
                InputAction.performed -= onInputHit;
                InputAction.canceled -= onInputRelease;
                foreach(InputAction misinput in misInputs)
                {
                    misinput.performed -= onInputHit;
                    misinput.canceled -= onInputRelease;
                }
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

        //This is in case of a minigame with two different inputs (A and Down for example) and the player hits the opposing input
        public void SetMisinputs(List<RhythmMisinputs> misinputs = null)
        {
            if (misinputs != null && misinputs.Count > 0)
            {
                // Debug.Log(misinputs.Count);
                foreach (RhythmMisinputs misinput in misinputs)
                {
                    InputAction inputAction;
                    if(misinput.GetInputName() == _action.ToString())
                        continue;
                    
                    switch (misinput.GetInputName())
                    {
                        case "A":
                            inputAction = m_inputSystem.Rhythm.A;
                            break;
                        case "Left":
                            inputAction = m_inputSystem.Rhythm.Left;
                            break;
                        case "Down":
                            inputAction = m_inputSystem.Rhythm.Down;
                            break;
                        case "Up":
                            inputAction = m_inputSystem.Rhythm.Up;
                            break;
                        case "Right":
                            inputAction = m_inputSystem.Rhythm.Right;
                            break;
                        case "Pad":
                            inputAction = m_inputSystem.Rhythm.Pad;
                            break;
                        default:
                            inputAction = null;
                            break;
                    }

                    if(inputAction != null && !misinputCallbacks.ContainsKey(inputAction))
                    {
                        misinputCallbacks.Add(inputAction, misinput);
                        misInputs.Add(inputAction);
                    }

                }
            }
        }

        public void Enable()
        {
            if (disposed || InputAction == null)
                return;

            InputAction.Enable();
            foreach(InputAction misinput in misInputs)
                misinput.Enable();

            enabled = true;
        }

        public void Disable()
        {
            if (InputAction == null)
                return;

            InputAction.Disable();
            foreach(InputAction misinput in misInputs)
                misinput.Disable();
            
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
            if (mustHit)
            {
                curHit = time;
                checkForAccuracy = (curHit >= startPoint) && (curHit <= endPoint);

                if (!enabled && !success && curHit >= startPoint - inputEnableBuffer && curHit <= endPoint)
                {
                    Enable();
                }

                if (autoplay)
                {
                    if (curHit >= desHit && !found)
                    {
                        found = true;
                        onHit?.Invoke();
                        success = true;
                        hasHit = true;
                    }
                }

                if (curHit > endPoint && !success)
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

public struct RhythmMisinputs
{
    private RhythmInputs input;
    private Action onHit;
    private Action<bool> onHalfHit;

    public RhythmMisinputs(RhythmInputs input, Action onHit, Action<bool> onHalfHit)
    {
        this.input = input;
        this.onHit = onHit;
        this.onHalfHit = onHalfHit;
    }

    public RhythmMisinputs(RhythmInputs input, Action onHit)
    {
        this.input = input;
        this.onHit = onHit;
        onHalfHit = null;
    }

    public RhythmMisinputs(RhythmInputs input, Action<bool> onHalfHit)
    {
        this.input = input;
        onHit = null;
        this.onHalfHit = onHalfHit;
    }

    public string GetInputName()
    {
        return input.ToString();
    }

    public void InvokeHit()
    {
        onHit?.Invoke();
    }

    public void InvokeHalfHit(bool isGood)
    {
        onHalfHit?.Invoke(isGood);
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
