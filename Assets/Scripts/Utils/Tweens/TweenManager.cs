using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class TweenManager : MonoBehaviour
{
    private static TweenManager _instance;
    public static TweenManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject manager = new GameObject("Tween");
                _instance = manager.AddComponent<TweenManager>();
                UnityEngine.Random.InitState(1000);
            }

            return _instance;
        }
    }

    public void AddManager()
    {
        UnityEngine.Random.InitState(1000);
    }

    private Dictionary<string, ITween> _activeTweens = new Dictionary<string, ITween>();
    public Dictionary<string, ITween> activeTweens => _activeTweens;


    public void AddTween<T>(Tween<T> tween)
    {
        //Debug.Log(_activeTweens.Count);
        if (_activeTweens.ContainsKey(tween.Identifier)) _activeTweens[tween.Identifier].OnCompleteKill();
        _activeTweens[tween.Identifier] = tween;
    }

    public void RemoveTween(string identifier)
    {
        _activeTweens.Remove(identifier);
    }

    private void Update()
    {
        foreach(var pair in _activeTweens.ToList())
        {
            ITween tween = pair.Value;
            tween.Update();

            if(tween.isComplete && !tween.WasKilled)
            {
                if(tween.onComplete != null)
                {
                    tween.onComplete.Invoke();
                    tween.onComplete = null;
                }

                RemoveTween(pair.Key);
            }
            else if (tween.WasKilled)
                RemoveTween(pair.Key);
        }
    }

    public static Tween<float> XTween(GameObject gameObject, float startX, float endX, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        dynamic tranformation = null;
        bool is2D = false;
        if (gameObject.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObject.GetComponent<RectTransform>();
            is2D = true;
        }
        else
            tranformation = gameObject.GetComponent<Transform>();

        float value = UnityEngine.Random.value;
        string identifier = $"{tranformation.GetInstanceID()}_X_{value}";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startX, endX, duration, value =>
        {
            if (is2D)
            {
                Vector2 position = tranformation.anchoredPosition;
                position.x = value;
                tranformation.anchoredPosition = position;
            }
            else
            {
                Vector3 position = tranformation.position;
                position.x = value;
                tranformation.position = position;
            }
        }, type, onComplete);

        return tween;

    }

    public static Tween<float> YTween(GameObject gameObject, float startY, float endY, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        dynamic tranformation = null;
        bool is2D = false;
        if (gameObject.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObject.GetComponent<RectTransform>();
            is2D = true;
        }
        else
            tranformation = gameObject.GetComponent<Transform>();

        float value = UnityEngine.Random.value;
        string identifier = $"{tranformation.GetInstanceID()}_Y_{value}";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startY, endY, duration, value =>
        {
            if (is2D)
            {
                Vector2 position = tranformation.anchoredPosition;
                position.y = value;
                tranformation.anchoredPosition = position;
            }
            else
            {
                Vector3 position = tranformation.position;
                position.y = value;
                tranformation.position = position;
            }
        }, type, onComplete);

        return tween;

    }

    public static Tween<float> AlphaTween(GameObject gameObject, float startAlpha, float endAlpha, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        dynamic spriteRenderer = null;
        if (gameObject.GetComponent<Image>() != null)
            spriteRenderer = gameObject.GetComponent<Image>();
        else
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        float value = UnityEngine.Random.value;
        string identifier = $"{gameObject.GetInstanceID()}_Alpha_{value}";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startAlpha, endAlpha, duration, value =>
        {
            Color color = spriteRenderer.color;
            color.a = value;
            spriteRenderer.color = color;

        }, type, onComplete);

        return tween;

    }

    public static Tween<float> PitchTween(GameObject gameObject, float startAngle, float endAngle, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        dynamic tranformation = null;
        if (gameObject.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObject.GetComponent<RectTransform>();
        }
        else
            tranformation = gameObject.GetComponent<Transform>();

        float value = UnityEngine.Random.value;
        string identifier = $"{tranformation.GetInstanceID()}_Pitch_{value}";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startAngle, endAngle, duration, value =>
        {
            Vector3 rotation = tranformation.rotation.eulerAngles;
            rotation.x = value;
            tranformation.rotation = Quaternion.Euler(rotation);

        }, type, onComplete);

        return tween;


    }

    public static Tween<float> YawTween(GameObject gameObject, float startAngle, float endAngle, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        dynamic tranformation = null;
        if (gameObject.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObject.GetComponent<RectTransform>();
        }
        else
            tranformation = gameObject.GetComponent<Transform>();

        float value = UnityEngine.Random.value;
        string identifier = $"{tranformation.GetInstanceID()}_Yaw_{value}";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startAngle, endAngle, duration, value =>
        {
            Vector3 rotation = tranformation.rotation.eulerAngles;
            rotation.y = value;
            tranformation.rotation = Quaternion.Euler(rotation);

        }, type, onComplete);

        return tween;


    }

    public static Tween<float> RollTween(GameObject gameObject, float startAngle, float endAngle, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        dynamic tranformation = null;
        if (gameObject.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObject.GetComponent<RectTransform>();
        }
        else
            tranformation = gameObject.GetComponent<Transform>();

        float value = UnityEngine.Random.value;
        string identifier = $"{tranformation.GetInstanceID()}_Roll_{value}";

        Tween<float> tween = new Tween<float>(gameObject, identifier, startAngle, endAngle, duration, value =>
        {
            Vector3 rotation = tranformation.rotation.eulerAngles;
            rotation.z = value;
            tranformation.rotation = Quaternion.Euler(rotation);

        }, type, onComplete);

        return tween;


    }

    public static Tween<float> NumTween(Func<float> getFloat, Action<float> setFloat, float end, float duration, Eases type = default(Eases), Action onComplete = default(Action))
    {
        float value = UnityEngine.Random.value;
        string identifier = $"{getFloat.Target.GetHashCode()}_Float_{value}";
        object target = getFloat.Target;

        float start = getFloat();

        Tween<float> tween = new Tween<float>(target, identifier, start, end, duration, value => 
        {
            setFloat(value);
        }, type, onComplete);

        return tween;
    }
}
