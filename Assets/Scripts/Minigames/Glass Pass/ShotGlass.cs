using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.GlassPass
{
    public class ShotGlass : MonoBehaviour
    {
        public Vector2 startPoint = new Vector2(-9.5f,-1.5f);
        public Vector2 endPoint = new Vector2(2.5f,-1.5f);

        Animator animator;
        int layer = 0;

        public Tween<float> xTween;
        protected DrinkType _type;
        public DrinkType type => _type;
        protected GameObject _base;
        protected GameObject _shadow;
        protected float shadowAlpha
        {
            get
            {
                if(_shadow != null)
                    return _shadow.GetComponent<SpriteRenderer>().color.a;
                else
                    return 0;
            }
        }
        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            _base = transform.Find("Base").gameObject;
            _shadow = transform.Find("Shadow").gameObject;
        }
        // Start is called before the first frame update
        void Start()
        {
            // SetShotGlass(DrinkType.Coffee);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetShotGlass(DrinkType type = DrinkType.Beer)
        {
            // Implementation for setting the shot glass type
            int state = (int)type;
            _type = type;
            layer = state;
            animator.SetLayerWeight(state, 1.0f);
        }

        public void SetGlassAlpha(float alpha)
        {
            ColorUtils.SetAlpha(_base, alpha);
            ColorUtils.SetAlpha(_shadow, alpha * shadowAlpha);
        }

        public void Slide(float duration)
        {
            float xDistance = endPoint.x - startPoint.x;

            float xVelocity = xDistance / duration;

            float newDistance = xVelocity * duration * 2;

            animator.Play("Idle", layer);
            
            xTween = TweenManager.LocalXTween(gameObject, startPoint.x, newDistance, duration * 2, Eases.Linear, () =>
            {
                
            });

        }

        public void DefaultSlide(float duration)
        {
            animator.Play("Idle", layer);
            xTween = TweenManager.LocalXTween(gameObject, startPoint.x, endPoint.x, duration * 0.5f, Eases.Linear, () =>
            {
                
            });

        }

        public void Stop()
        {
            if(xTween != null) xTween.FullKill();
                transform.localPosition = new Vector3(endPoint.x, endPoint.y, transform.localPosition.z);
        }

        public void Spill()
        {
            if(xTween != null) xTween.FullKill();
            animator.Play("Spilt", layer);
        }

        public void ResetPosition()
        {
            transform.localPosition = new Vector3(startPoint.x, startPoint.y, transform.localPosition.z);
            SetGlassAlpha(1);
            animator.Play("Static", layer);
        }
            
    }

    public enum DrinkType
    {
        Beer,
        Water,
        Coffee,
        None
    }
}
