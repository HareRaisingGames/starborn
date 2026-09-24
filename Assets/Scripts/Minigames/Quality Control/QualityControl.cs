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
                new CallForAction(()=>{ /*Input cue*/ }, 5f, RhythmInputs.Pad),
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
                new CallForAction(()=>{ /*Input cue*/ }, 5f, RhythmInputs.A),
            };
        }
    }
}
