using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.AssembleyLine
{
    public class Robot : RhythmEvent
    {
        public Robot()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 2f),
                new CallForAction(()=>{ /*Input cue*/ }, 4f, RhythmInputs.A),
            };
        }
    }

    public class BigRobot : RhythmEvent
    {
        public BigRobot()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 2f),
                new CallForAction(()=>{ /*Sound cue*/ }, 2.5f),
                new CallForAction(()=>{ /*Input cue*/ }, 4f, RhythmInputs.A),
                new CallForAction(()=>{ /*Input cue*/ }, 4.5f, RhythmInputs.A),
            };
        }
    }
}
