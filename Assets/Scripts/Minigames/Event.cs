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

        public InputAction InputAction;
    }
}
