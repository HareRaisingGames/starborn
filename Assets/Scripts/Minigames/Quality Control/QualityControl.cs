using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.QualityControl
{
    public class Keep : RhythmEvent
    {
        public Keep()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 5f, RhythmInputs.Pad, 1f, 1f),
            };
        }
    }

    public class Melt : RhythmEvent
    {
        public Melt()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 5f, RhythmInputs.A, 1f, 1f),
            };
        }
    }

    public class Dud : RhythmEvent
    {
        public Dud()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 5f, RhythmInputs.None, 1f, 1f), //Needs to be updated so that it can implement inputs you're not suppose to hit
            };
        }
    }
}
