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
        [HideInInspector]
        public bool successful = false;
        public struct GlassBounds
        {
            Vector2 offset;
            Vector2 size;
            public GlassBounds(Vector2 offset, Vector2 size)
            {
                this.offset = offset;
                this.size = size;
            }
            public Vector2 GetOffset() => offset;
            public Vector2 GetSize() => size;
        }
        public GlassBounds glassScale => new GlassBounds(Vector2.zero, new Vector2(0.85f, 1.2f));
        public GlassBounds cupScale => new GlassBounds(new Vector2(0.076f, -0.1375f), new Vector2(1.525f, 0.8f));
        protected BoxCollider2D _collider;
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
            _collider = GetComponent<BoxCollider2D>();
        }
        // Start is called before the first frame update
        void Start()
        {
            
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
            // Adjust collider based on the type of drink
            if(_collider == null) return;
            if(type == DrinkType.Coffee)
            {
                _collider.offset = cupScale.GetOffset();
                _collider.size = cupScale.GetSize();
            }
            else if(type != DrinkType.None)
            {
                _collider.offset = glassScale.GetOffset();
                _collider.size = glassScale.GetSize();
            }
        }

        public void SetSpeed(float speed)
        {
            animator.speed = speed / 120f;
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

        bool hasTossed = false;
        public bool tossed => hasTossed;
        public void Toss(float time = 1, Vector2 startPos = default(Vector2), Vector2 endPos = default(Vector2))
        {
            animator.Play("Empty", layer);
            ColorUtils.SetAlpha(transform.Find("Shadow").gameObject, 0);
            if(!hasTossed)
            {
                transform.localPosition = startPos;
                TweenManager.LocalXTween(gameObject, startPos.x, endPos.x, 3f * time,Eases.EaseOutQuad);
                TweenManager.LocalYTween(gameObject, startPos.y, endPos.y, 2f * time,Eases.EaseOutBounce);
                TweenManager.RollTween(gameObject, 0, -360, 2.25f * time);
                hasTossed = true;
            }
            SetGlassAlpha(1);
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
