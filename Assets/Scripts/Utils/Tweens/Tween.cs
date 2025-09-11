using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

public class Tween<T>: ITween
{
    /**
     * List of values to look for in tweens
     * x
     * y
     * z
     * alpha
     * color
     */

    /*public static object getProperty(object obj, string name)
    {
        foreach (string value in name.Split("."))
        {
            obj = obj.GetType().GetProperty(value).GetValue(obj);
        }
        return obj;
    }*/

    private T _startValue;
    private T _endValue;
    private float _duration;
    private Action<T> _onTweenUpdate;
    private float _elapsedTime = 0f;
    private Eases _easeType = Eases.Linear;

    private float _delayElapsedTime = 0f;
    private int _loopsCompleted = 0;
    private bool _reverse = false;
    private bool _tableTennis = false;
    private int _loopCount = 1;

    private float _percentThreshold = -1f;

    private Action _onUpdate;
    private Action _onPercentCompleted;

    public T endValue => _endValue;


    public Tween(object target, string identifier, T startValue, T endValue, float duration, Action<T> onTweenUpdate, Eases type = default(Eases), Action onComplete = default(Action))
    {
        Target = target;
        Identifier = identifier;
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _onTweenUpdate = onTweenUpdate;
        _easeType = type;
        this.onComplete = null ?? onComplete;

        TweenManager.instance.AddTween(this);
    }

    public Tween(object target, string identifier, T startValue, T endValue, float duration, Action<T> onTweenUpdate, Action onComplete = default(Action))
    {
        Target = target;
        Identifier = identifier;
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _onTweenUpdate = onTweenUpdate;
        this.onComplete = null ?? onComplete;

        TweenManager.instance.AddTween(this);
    }

    public object Target { get; private set; }
    public bool isComplete { get; private set; }
    public bool WasKilled { get; private set; }
    public bool isPaused { get; private set; }
    public bool IgnoreTimeScale { get; private set;  }
    public string Identifier { get; private set;  }
    public float DelayTime { get; private set; }
    public Action onComplete { get; set; }

    public void Update()
    {
        _delayElapsedTime += IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

        if (_delayElapsedTime >= DelayTime)
        {
            if (isComplete)
                return;

            if (IsTargetDestroyed())
            {
                FullKill();
                return;
            }

            _elapsedTime += IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = _elapsedTime / _duration;
            float easedT = Ease(_easeType, t);
            T currentValue;

            currentValue = _reverse ? Interpolate(_endValue, _startValue, easedT) : Interpolate(_startValue, _endValue, easedT);
            
            _onUpdate?.Invoke();
            _onTweenUpdate?.Invoke(currentValue);

            if (_percentThreshold >= 0 && t >= _percentThreshold)
            {
                _onPercentCompleted?.Invoke();
                _percentThreshold = -1f;
            }

            if (_elapsedTime >= _duration)
            {
                _loopsCompleted++;
                _elapsedTime = 0f;

                if (_tableTennis)
                    _reverse = !_reverse;

                if (_loopCount > 0 && _loopsCompleted >= _loopCount)
                {
                    isComplete = true;
                    OnCompleteKill();
                }
            }

        }
            
    }

    public void OnCompleteKill()
    {
        isComplete = true;
        _onUpdate = null;
        _onTweenUpdate = null;
        _onPercentCompleted = null;

    }
    public void FullKill()
    {
        OnCompleteKill();
        WasKilled = true;
        onComplete = null;
    }
    public bool IsTargetDestroyed()
    {
        if(Target is MonoBehaviour mono && mono == null)
            return true;
        if (Target is GameObject game && game == null)
            return true;
        if (Target is Delegate del && del.Target == null)
            return true;

        return false;
    }
    public void Pause()
    {
        isPaused = true;
    }
    public void Resume()
    {
        isPaused = false;
    }
    public T Interpolate(T start, T end, float t)
    {
        if (start is float startFloat && end is float endFloat)
            return (T)(object)Mathf.LerpUnclamped(startFloat, endFloat, t);

        if (start is Vector3 startVector && end is Vector3 endVector)
            return (T)(object)Vector3.LerpUnclamped(startVector, endVector, t);

        if (start is Vector2 startVector2 && end is Vector2 endVector2)
            return (T)(object)Vector2.LerpUnclamped(startVector2, endVector2, t);

        if (start is Color startColor && end is Color endColor)
            return (T)(object)Color.LerpUnclamped(startColor, endColor, t);

        throw new NotImplementedException($"Interpolation for type {typeof(T)} has not yet been implemented");
    }

    public Tween<T> SetLoop(int loopCounted = 1)
    {
        _loopCount = loopCounted;
        return this;
    }

    public Tween<T> SetPingPong(int loopCounted = 1)
    {
        _loopCount = loopCounted;
        _tableTennis = true;
        return this;
    }

    public Tween<T> SetReverse()
    {
        _reverse = true;
        return this;
    }

    public Tween<T> SetIgnoreTimeScale()
    {
        IgnoreTimeScale = true;
        return this;
    }

    public Tween<T> SetOnUpdate(Action onUpdate)
    {
        _onUpdate = onUpdate;
        return this;
    }

    public Tween<T> SetOnPercentCompleted(float percentCompleted, Action onPercentCompleted)
    {
        _percentThreshold = Mathf.Clamp01(percentCompleted);
        _onPercentCompleted = onPercentCompleted;
        return this;
    }

    public Tween<T> SetStartDelay(float delayTime)
    {
        DelayTime = delayTime;
        return this;
    }

    #region Eases

    #region Tween Functions
    public static float Linear(float t) => t;

    public static float InQuad(float t) => t * t;
    public static float OutQuad(float t) => 1 - InQuad(1 - t);
    public static float InOutQuad(float t)
    {
        if (t < 0.5) return InQuad(t * 2) / 2;
        return 1 - InQuad((1 - t) * 2) / 2;
    }

    public static float InCubic(float t) => t * t * t;
    public static float OutCubic(float t) => 1 - InCubic(1 - t);
    public static float InOutCubic(float t)
    {
        if (t < 0.5) return InCubic(t * 2) / 2;
        return 1 - InCubic((1 - t) * 2) / 2;
    }

    public static float InQuart(float t) => t * t * t * t;
    public static float OutQuart(float t) => 1 - InQuart(1 - t);
    public static float InOutQuart(float t)
    {
        if (t < 0.5) return InQuart(t * 2) / 2;
        return 1 - InQuart((1 - t) * 2) / 2;
    }

    public static float InQuint(float t) => t * t * t * t * t;
    public static float OutQuint(float t) => 1 - InQuint(1 - t);
    public static float InOutQuint(float t)
    {
        if (t < 0.5) return InQuint(t * 2) / 2;
        return 1 - InQuint((1 - t) * 2) / 2;
    }

    public static float InSine(float t) => 1 - (float)Math.Cos(t * Math.PI / 2);
    public static float OutSine(float t) => (float)Math.Sin(t * Math.PI / 2);
    public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

    public static float InExpo(float t) => (float)Math.Pow(2, 10 * (t - 1));
    public static float OutExpo(float t) => 1 - InExpo(1 - t);
    public static float InOutExpo(float t)
    {
        if (t < 0.5) return InExpo(t * 2) / 2;
        return 1 - InExpo((1 - t) * 2) / 2;
    }

    public static float InCirc(float t) => -((float)Math.Sqrt(1 - t * t) - 1);
    public static float OutCirc(float t) => 1 - InCirc(1 - t);
    public static float InOutCirc(float t)
    {
        if (t < 0.5) return InCirc(t * 2) / 2;
        return 1 - InCirc((1 - t) * 2) / 2;
    }

    public static float InElastic(float t) => 1 - OutElastic(1 - t);
    public static float OutElastic(float t)
    {
        float p = 0.3f;
        return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - p / 4) * (2 * Math.PI) / p) + 1;
    }
    public static float InOutElastic(float t)
    {
        if (t < 0.5) return InElastic(t * 2) / 2;
        return 1 - InElastic((1 - t) * 2) / 2;
    }

    public static float InBack(float t)
    {
        float s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }
    public static float OutBack(float t) => 1 - InBack(1 - t);
    public static float InOutBack(float t)
    {
        if (t < 0.5) return InBack(t * 2) / 2;
        return 1 - InBack((1 - t) * 2) / 2;
    }

    public static float InBounce(float t) => 1 - OutBounce(1 - t);
    public static float OutBounce(float t)
    {
        float div = 2.75f;
        float mult = 7.5625f;

        if (t < 1 / div)
        {
            return mult * t * t;
        }
        else if (t < 2 / div)
        {
            t -= 1.5f / div;
            return mult * t * t + 0.75f;
        }
        else if (t < 2.5 / div)
        {
            t -= 2.25f / div;
            return mult * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / div;
            return mult * t * t + 0.984375f;
        }
    }
    public static float InOutBounce(float t)
    {
        if (t < 0.5) return InBounce(t * 2) / 2;
        return 1 - InBounce((1 - t) * 2) / 2;
    }
    #endregion

    public static float Ease(Eases type, float t)
    {
        switch(type)
        {
            case Eases.Linear:
                return Linear(t);
            case Eases.EaseInBack:
                return InBack(t);
            case Eases.EaseOutBack:
                return OutBack(t);
            case Eases.EaseInOutBack:
                return InOutBack(t);
            case Eases.EaseInBounce:
                return InBounce(t);
            case Eases.EaseOutBounce:
                return OutBounce(t);
            case Eases.EaseInOutBounce:
                return InOutBounce(t);
            case Eases.EaseInCirc:
                return InCirc(t);
            case Eases.EaseOutCirc:
                return OutCirc(t);
            case Eases.EaseInOutCirc:
                return InOutCirc(t);
            case Eases.EaseInCubic:
                return InCubic(t);
            case Eases.EaseOutCubic:
                return OutCubic(t);
            case Eases.EaseInOutCubic:
                return InOutCubic(t);
            case Eases.EaseInElastic:
                return InElastic(t);
            case Eases.EaseOutElastic:
                return OutElastic(t);
            case Eases.EaseInOutElastic:
                return InOutElastic(t);
            case Eases.EaseInExpo:
                return InExpo(t);
            case Eases.EaseOutExpo:
                return OutExpo(t);
            case Eases.EaseInOutExpo:
                return InOutExpo(t);
            case Eases.EaseInQuad:
                return InQuad(t);
            case Eases.EaseOutQuad:
                return OutQuad(t);
            case Eases.EaseInOutQuad:
                return InOutQuad(t);
            case Eases.EaseInQuart:
                return InQuart(t);
            case Eases.EaseOutQuart:
                return OutQuart(t);
            case Eases.EaseInOutQuart:
                return InOutQuart(t);
            case Eases.EaseInQuint:
                return InQuint(t);
            case Eases.EaseOutQuint:
                return OutQuint(t);
            case Eases.EaseInOutQuint:
                return InOutQuint(t);
            case Eases.EaseInSine:
                return InSine(t);
            case Eases.EaseOutSine:
                return OutSine(t);
            case Eases.EaseInOutSine:
                return InOutSine(t);
        }

        return t;
    }

    #endregion


}

public enum Eases
{
    Linear = 0,
    EaseInQuad = 2,
    EaseOutQuad = 3,
    EaseInOutQuad = 4,
    EaseOutInQuad = 33,
    EaseInCubic = 5,
    EaseOutCubic = 6,
    EaseInOutCubic = 7,
    EaseOutInCubic = 34,
    EaseInQuart = 8,
    EaseOutQuart = 9,
    EaseInOutQuart = 10,
    EaseOutInQuart = 35,
    EaseInQuint = 11,
    EaseOutQuint = 12,
    EaseInOutQuint = 13,
    EaseOutInQuint = 36,
    EaseInSine = 14,
    EaseOutSine = 15,
    EaseInOutSine = 16,
    EaseOutInSine = 37,
    EaseInExpo = 17,
    EaseOutExpo = 18,
    EaseInOutExpo = 19,
    EaseOutInExpo = 38,
    EaseInCirc = 20,
    EaseOutCirc = 21,
    EaseInOutCirc = 22,
    EaseOutInCirc = 39,
    EaseInBounce = 23,
    EaseOutBounce = 24,
    EaseInOutBounce = 25,
    EaseOutInBounce = 40,
    EaseInBack = 26,
    EaseOutBack = 27,
    EaseInOutBack = 28,
    EaseOutInBack = 41,
    EaseInElastic = 29,
    EaseOutElastic = 30,
    EaseInOutElastic = 31,
    EaseOutInElastic = 42,
    Spring = 32,

    //EaseOutInQuad,
    //EaseOutInCubic,
    //EaseOutInQua
}
