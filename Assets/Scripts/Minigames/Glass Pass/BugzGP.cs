using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Starborn.GlassPass
{
    public class BugzGP : MonoBehaviour
    {
        public Animator bodyAnimator;
        public Animator handAnimator;
        
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void PlayBody(string code = "", float speed = 1, Action onFinish = null)
        {
            bodyAnimator.speed = speed;
            switch(code)
            {
                case "idle":
                    bodyAnimator.Play("Idle_Animation", -1, 0f);
                    break;
                case "catch":
                    StartCoroutine(AnimationUtils.OnAnimationFinish(bodyAnimator, "Success_Body", onFinish));
                    break;
                case "half":
                    StartCoroutine(AnimationUtils.OnAnimationFinish(bodyAnimator, "HalfMiss", onFinish));
                    break;
                default:
                    break;
            }
        }

        public void PlayHand(string code = "", float speed = 1, Action onFinish = null)
        {
            handAnimator.speed = speed;
            switch(code)
            {
                case "idle":
                    if(handAnimator.gameObject.activeInHierarchy)
                    handAnimator.Play("Hand_Idle_Animation", -1, 0f);
                    break;
                case "catch":
                    StartCoroutine(AnimationUtils.OnAnimationFinish(handAnimator, "Grab", onFinish));
                    break;
                default:
                    break;
            }
        }

    }
}

