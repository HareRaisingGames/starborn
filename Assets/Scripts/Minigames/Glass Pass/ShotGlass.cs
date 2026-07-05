using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.GlassPass
{
    public class ShotGlass : MonoBehaviour
    {
        public Vector2 startPoint = new Vector2(-9.5f,-1.5f);
        public Vector2 endPoint = new Vector2(2.5f,-1.5f);

        public Tween<float> xTween;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }


        public void Slide(float duration)
        {
            float xDistance = endPoint.x - startPoint.x;

            float xVelocity = xDistance / duration;

            float newDistance = xVelocity * duration * 2;
            
            xTween = TweenManager.XTween(gameObject, startPoint.x, newDistance, duration * 2, Eases.Linear, () =>
            {
                
            });

        }

        public void DefaultSlide(float duration)
        {
            xTween = TweenManager.XTween(gameObject, startPoint.x, endPoint.x, duration * 0.5f, Eases.Linear, () =>
            {
                
            });

        }

        public void Stop()
        {
            if(xTween != null) xTween.FullKill();
                transform.position = new Vector3(endPoint.x, endPoint.y, transform.position.z);
        }

        public void ResetPosition() =>
            transform.position = new Vector3(startPoint.x, startPoint.y, transform.position.z);
    }
}
