using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.SecuritySmash
{
    public class SecuritySmash : Minigame
    {

    }

    public class SmallDrone : RhythmEvent
    {
        public SmallDrone()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 2f, RhythmInputs.A, 1f, 1f),
            };
        }
    }

    public class QuickDrone : RhythmEvent
    {
        public QuickDrone()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Enter cue*/ }, 0f),
                new CallForAction(()=>{ /*Sound cue*/ }, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 1.5f, RhythmInputs.A, 0.5f, 0.5f),
                new CallForAction(()=>{ /*Input cue*/ }, 2f, RhythmInputs.A, 0.5f, 0.5f),
            };
        }
    }

    public class BigDrone : RhythmEvent
    {
        public BigDrone()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{ /*Sound cue*/ }, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 2f, RhythmInputs.A, 1f, 1f),
                new CallForAction(()=>{ /*Input cue*/ }, 3f, RhythmInputs.A, 1f, 1f),
            };
        }
    }
}
