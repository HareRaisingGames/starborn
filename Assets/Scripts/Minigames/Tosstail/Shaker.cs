using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.Tosstail
{
    public class Shaker : MonoBehaviour
    {
        private bool _right;
        public bool direction => _right;
        public bool startOnRight = true;
        private bool isTossed = false;

        public float startPosition;
        public float endPosition;

        float startY;

        public float shortHeight = 1;
        public float longHeight = 2;

        [HideInInspector]
        public float estimatedTime;

        //[HideInInspector]
        bool _canPlay = true;
        public bool canPlay => _canPlay;

        public AudioSource sfx;
        // Start is called before the first frame update
        void Start()
        {
            _right = startOnRight;
            startY = transform.position.y;

            if (GameObject.Find("Metronome") == null)
            {
                GameObject gameObject = new GameObject("Metronome");
                sfx = gameObject.AddComponent<AudioSource>();
                sfx.clip = Resources.Load<AudioClip>("Audio/blip");
            }
            else
                sfx = GameObject.Find("Metronome").GetComponent<AudioSource>();
        }

        Tween<float> xTween;
        Tween<float> yTween;
        Tween<float> angleTween;

        float reset;
        float yDistance;

        // Update is called once per frame
        void Update()
        {

        }

        public void Toss(float duration, bool longToss = false, float reset = 1)
        {
            if(!isTossed && _canPlay)
            {
                this.reset = reset;
                isTossed = true;
                _canPlay = false;
                float start = _right ? startPosition : endPosition;
                float end = _right ? endPosition : startPosition;

                float xDistance = end - start;

                float xVelocity = xDistance / duration;

                float newDistance = xVelocity * duration * 1.5f;

                float newEnd = start + newDistance;

                /*xTween = TweenManager.XTween(gameObject, start, end, duration, Eases.Linear, () =>
                {
                        _right = !_right;
                    isTossed = false;
                });*/

                sfx.clip = longToss ? Resources.Load<AudioClip>("Audio/Tosstail/cake") : Resources.Load<AudioClip>("Audio/Tosstail/donut");
                sfx.Play();

                float sHeight = longToss ? longHeight : shortHeight;
                float yVelocity = sHeight / (duration * 0.5f);

                yDistance = yVelocity * duration;

                xTween = TweenManager.XTween(gameObject, start, newEnd, duration * 1.5f, Eases.Linear, () =>
                {
                    if (isTossed)
                    {
                        _right = !_right;
                        TossBack(newEnd, startY - yDistance / 2, reset);
                    }
                        
                    //isTossed = false;
                });



                /*
                 yTween = TweenManager.YTween(gameObject, startY, startY + sHeight, duration / 2, Eases.EaseOutSine, () => {
                    yTween = TweenManager.YTween(gameObject, startY + sHeight, startY, duration / 2, Eases.EaseInSine);
                });
                 */

                yTween = TweenManager.YTween(gameObject, startY, startY + sHeight, duration / 2, Eases.EaseOutSine, () => {
                    yTween = TweenManager.YTween(gameObject, startY + sHeight, startY, duration / 2, Eases.EaseInSine, () =>
                    {
                        if(isTossed)
                            yTween = TweenManager.YTween(gameObject, startY, startY - yDistance/2, duration / 2, Eases.Linear, () =>
                            {
                                
                            });
                    });
                });

                /*
                angleTween = TweenManager.RollTween(gameObject, 0, _right ? 360 : -360, duration, Eases.Linear, () =>
                {
                    gameObject.transform.rotation = Quaternion.identity;
                });
                */

                angleTween = TweenManager.RollTween(gameObject, 0, _right ? 360 : -360, duration, Eases.Linear, () =>
                {
                    gameObject.transform.rotation = Quaternion.identity;
                }).SetLoop(2);

            }

        }

        public void SuccessfulCatch()
        {
            Debug.Log("Go!");
            xTween.OnCompleteKill();
            yTween.OnCompleteKill();
            angleTween.OnCompleteKill();
            isTossed = false;
            _canPlay = true;
            transform.position = new Vector3(_right ? endPosition : startPosition, startY, transform.position.z);
            transform.rotation = Quaternion.identity;
            _right = !_right;
        }

        public void UnsuccessfulCatch(bool early)
        {
            xTween.OnCompleteKill();
            yTween.OnCompleteKill();
            angleTween.OnCompleteKill();

            isTossed = false;
            float x = _right ? endPosition : startPosition;

            transform.position = new Vector3(_right ? endPosition : startPosition, startY, transform.position.z);
            transform.rotation = Quaternion.identity;
            _right = !_right;

            TweenManager.XTween(gameObject, x, x + 3 * (x > 0 ? -1 : 1), reset * 0.75f, Eases.EaseInSine, () =>
            {
                TossBack(x + 3 * (x > 0 ? -1 : 1), startY - yDistance, reset, 0.25f);
            });
            TweenManager.YTween(gameObject, startY, startY + 0.5f, reset * 0.25f, Eases.EaseOutSine, () =>
            {
                TweenManager.YTween(gameObject, startY + 1, startY - yDistance, reset * 0.5f, Eases.EaseInSine);
            });
            TweenManager.RollTween(gameObject, 0, _right ? -270 : 270, reset * 0.75f, Eases.Linear);

            MinigameManager.instance.LoseALife();
        }

        void TossBack(float startX, float startY, float duration, float delay = 0)
        {
            xTween.OnCompleteKill();
            yTween.OnCompleteKill();
            angleTween.OnCompleteKill();
            transform.rotation = Quaternion.identity;
            TweenManager.XTween(gameObject, startX, _right ? startPosition : endPosition, duration * 0.75f, Eases.Linear, () =>
            {
                isTossed = false;
                _canPlay = true;
                transform.position = new Vector3(_right ? startPosition : endPosition, this.startY);

            }).SetStartDelay(delay);

            TweenManager.YTween(gameObject, startY, this.startY + 1, duration * 0.375f, Eases.EaseOutSine, () =>
            {
                TweenManager.YTween(gameObject, this.startY + 1, this.startY, duration * 0.375f, Eases.EaseInSine);
                transform.position = new Vector3(_right ? startPosition : endPosition, this.startY);
            }).SetStartDelay(delay);

            TweenManager.RollTween(gameObject, _right ? 270 : -270, _right ? 360 : -360, duration * 0.75f, Eases.Linear, () =>
            {
                gameObject.transform.rotation = Quaternion.identity;
            }).SetStartDelay(delay);
        }
    }
}


