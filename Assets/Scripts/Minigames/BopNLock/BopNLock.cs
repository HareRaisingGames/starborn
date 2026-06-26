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
}
