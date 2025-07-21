using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Starborn.InputSystem
{
    public class Event : MonoBehaviour
    {
        public delegate void EventCallback(Event call);
        public delegate void EventCallbackState(Event caller, float state);


        public EventCallbackState onHit;
        public EventCallback OnMiss;

        private InputAction InputAction;

        [Space][SerializeField] private StarbornInputSystem m_inputSystem;

        public enum RhythmInputs
        {
            A,
            Left,
            Right,
            Up,
            Down
        }

        public RhythmInputs action;

        private void Awake()
        {
            m_inputSystem = new StarbornInputSystem();
            //Debug.Log(m_inputSystem.Rhythm.A);
            switch (action.ToString())
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
            }

            InputAction.performed += onInputHit;
        }

        public void OnEnable()
        {
            InputAction.Enable();
        }

        public void OnDisable()
        {
            InputAction.Disable();
        }

        public void onInputHit(InputAction.CallbackContext context)
        {
            //Debug.Log("Hello!");
        }
    }
}
