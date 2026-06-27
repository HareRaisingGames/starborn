using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using Starborn.BopNLock;

namespace Starborn.BopNLock
{
    public class BopNLock : Minigame
    {
        [Header("Sounds")]
        public AudioSource clapping;
    }
}

public class FourClap : RhythmEvent
{
    public BopNLock game;
    public override void SetUp()
    {
        base.SetUp();
        game = Object.FindObjectOfType<BopNLock>();
    }

    public FourClap()
    {
        actions = new List<CallForAction>()
        {
            new CallForAction(() => { game.clapping.Play(); }, 1),
            new CallForAction(() => { game.clapping.Play(); }, 2),
            new CallForAction(() => { game.clapping.Play(); }, 3),
            new CallForAction(() => {}, 4),
            new CallForAction(() => {}, 5, RhythmInputs.A, 0.5f, 0.5f),
            new CallForAction(() => {}, 6, RhythmInputs.A, 0.5f, 0.5f),
            new CallForAction(() => {}, 7, RhythmInputs.A, 0.5f, 0.5f),
            new CallForAction(() => {}, 8, RhythmInputs.A, 0.5f, 0.5f),
        };
    }
}
