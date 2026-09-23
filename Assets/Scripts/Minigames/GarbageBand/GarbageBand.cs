using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.GarbageBand
{
    public class GarbageBand : Minigame
    {

    }

    public class LetsGo : RhythmEvent
    {
        public LetsGo()
        {
            actions = new List<CallForAction>
            {
                new CallForAction(()=>{}, 1f),
                new CallForAction(()=>{}, 2f),
                new CallForAction(()=>{}, 3f, RhythmInputs.A, 0.5f, 0.5f),
            };
        }
    }

    public class DropIt : RhythmEvent
    {
        public DropIt()
        {
            actions = new List<CallForAction>
            {
                new CallForAction(()=>{}, 1f),
                new CallForAction(()=>{}, 1.5f),
                new CallForAction(()=>{}, 2f, RhythmInputs.A, 0.5f, 0.5f),
            };
        }
    }

    public class WaitNow : RhythmEvent
    {
        public WaitNow()
        {
            actions = new List<CallForAction>
            {
                new CallForAction(()=>{}, 1f),
                new CallForAction(()=>{}, 1.5f),
                new CallForAction(()=>{}, 2.5f, RhythmInputs.A, 0.5f, 0.5f),
            };
        }
    }
}

