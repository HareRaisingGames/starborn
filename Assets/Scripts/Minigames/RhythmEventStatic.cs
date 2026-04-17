using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using Starborn;

//A global list for all the simon says style minigames, will reset upon scene exit
public class SimonSays
{
    public static List<Interval> intervals = new List<Interval>();
    public static Interval currentInvertal;
    public static Interval latestInterval
    {
        get
        {
            if (intervals.Count == 0) return null;
            return intervals[intervals.Count - 1];
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Clear() => intervals.Clear();
}

public class KeepTheBeat
{
    public static List<KeepTheBeat> list = new List<KeepTheBeat>();
}

public abstract class SetInterval : RhythmEvent
{
    Interval interval;
    public override void SetUp()
    {
        base.SetUp();
        timeCallback = delegate (float value)
        {
            foreach (Interval inter in SimonSays.intervals)
            {
                if (!inter.isClosed)
                {
                    interval = inter;
                    break;
                }
            }

            if (interval == null)
            {
                interval = new Interval(value / Conductor.instance.crochet);
                SimonSays.intervals.Add(interval);
            }
            else
                interval.CloseInterval(value / Conductor.instance.crochet);
        };


    }
    public SetInterval()
    {
        actions = new List<CallForAction>()
        {
            new CallForAction(()=>{
                SimonSays.currentInvertal = interval;
            }, 1f )
        };
    }
}

public abstract class PlaybackInterval : RhythmEvent
{
    Dictionary<float, IntervalInput> inputs = new Dictionary<float, IntervalInput>();
    Interval interval;
    public override void SetUp()
    {
        base.SetUp();
        timeCallback = delegate (float value)
        {
            interval = SimonSays.latestInterval;
            foreach(KeyValuePair<float, IntervalInput> input in interval.inputs)
            {
                IntervalInput newIntervalInput = new IntervalInput(input.Value);
                inputs.Add(input.Key, newIntervalInput);
            }
        };
    }

    public PlaybackInterval(System.Action callback = null)
    {
        actions = new List<CallForAction>()
        {
            new CallForAction(()=>{
                callback?.Invoke();
                AddInInputs(Conductor.instance.crochet);
            }, 1f)
        };
    }

    void AddInInputs(float time)
    {
        foreach(KeyValuePair<float, IntervalInput> input in inputs)
        {
            input.Value.AddToChart(startPoint + (input.Key * time), time);
        }
    }
}

public abstract class StartBeat : RhythmEvent
{
    public override void SetUp()
    {
        base.SetUp();
        timeCallback = delegate (float value)
        {
            if (actions != null)
            {

            }
        };
    }
}

public abstract class StopBeat : RhythmEvent
{
    public override void SetUp()
    {
        base.SetUp();
    }
}

public class Interval
{
    bool _isClosed;
    public float startPoint => 0;
    public float endPoint;

    float baseStart;
    float baseEnd;
    public bool isClosed
    {
        get
        {
            return _isClosed;
        }
    }
    public Dictionary<float, IntervalInput> realtimeInputs = new Dictionary<float, IntervalInput>();
    public Dictionary<float, IntervalInput> inputs = new Dictionary<float, IntervalInput>();
    public Interval(float start = 0)
    {
        _isClosed = false;
        baseStart = start;
    }

    public void CloseInterval(float end = 0)
    {
        _isClosed = true;
        baseEnd = end;
        endPoint = baseEnd - baseStart;

        foreach(KeyValuePair<float, IntervalInput> input in realtimeInputs)
        {
            inputs.Add(baseEnd - input.Key, input.Value);
        }
    }
}

public class IntervalInput : RhythmEvent
{
    CallForAction _callerAction;
    CallForAction _copyAction;

    public Interval interval;

    public CallForAction callerAction => _callerAction;
    public CallForAction copyAction => _copyAction;

    public bool spawn;

    public override void SetUp()
    {
        base.SetUp();
        timeCallback = delegate (float value)
        {
            if(!spawn)
            {
                foreach (Interval inter in SimonSays.intervals)
                {
                    if (!inter.isClosed)
                    {
                        interval = inter;
                        break;
                    }
                }

                if (interval != null)
                {
                    interval.realtimeInputs.Add(value / Conductor.instance.crochet, this);
                }
            }
        };
    }

    public IntervalInput(RhythmInputs input, System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null)
    {
        spawn = false;
        _callerAction = new CallForAction(caller, 1f);
        _copyAction = new CallForAction(copy, 1f, input, 0.5f, 0.5f, onHit, onHalfHit, onMiss);

        actions = new List<CallForAction>()
        {
            _callerAction
        };
    }

    public IntervalInput(IntervalInput copy)
    {
        spawn = true;
        _callerAction = copy.callerAction;
        _copyAction = copy.copyAction;

        actions = new List<CallForAction>()
        {
            _copyAction
        };
    }
}

#region Input Types
public abstract class IntervalA : IntervalInput
{
    //public abstract void Caller();
    public IntervalA(System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null) : base(RhythmInputs.A, caller, copy, onHit, onHalfHit, onMiss)
    {
        
    }
}

public abstract class IntervalPad : IntervalInput
{
    public IntervalPad(System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null) : base(RhythmInputs.Pad, caller, copy, onHit, onHalfHit, onMiss)
    {

    }
}

public abstract class IntervalUp : IntervalInput
{
    public IntervalUp(System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null) : base(RhythmInputs.Up, caller, copy, onHit, onHalfHit, onMiss)
    {

    }
}

public abstract class IntervalDown : IntervalInput
{
    public IntervalDown(System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null) : base(RhythmInputs.Down, caller, copy, onHit, onHalfHit, onMiss)
    {

    }
}

public abstract class IntervalLeft : IntervalInput
{
    public IntervalLeft(System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null) : base(RhythmInputs.Left, caller, copy, onHit, onHalfHit, onMiss)
    {

    }
}

public abstract class IntervalRight : IntervalInput
{
    public IntervalRight(System.Action caller, System.Action copy, System.Action onHit = null, System.Action<bool> onHalfHit = null, System.Action onMiss = null) : base(RhythmInputs.Right, caller, copy, onHit, onHalfHit, onMiss)
    {

    }
}
#endregion