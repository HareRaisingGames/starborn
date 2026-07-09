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
                    StartCoroutine(OnAnimationFinish(bodyAnimator, "Success_Body", onFinish));
                    break;
                case "half":
                    StartCoroutine(OnAnimationFinish(bodyAnimator, "HalfMiss", onFinish));
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
                    handAnimator.Play("Hand_Idle_Animation", -1, 0f);
                    break;
                case "catch":
                    StartCoroutine(OnAnimationFinish(handAnimator, "Grab", onFinish));
                    break;
                default:
                    break;
            }
        }

        private IEnumerator OnAnimationFinish(Animator animator, string stateName, Action onFinish = null)
        {
            animator.Play(stateName, -1, 0f);
            if(onFinish == null)
                yield break;
            
            yield return null;

            // 3. Keep looping as long as we are in the state and it hasn't reached 1.0 (100% completion)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1.0f)
            {
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }

            // 4. Animation is finished! Run your code here
            onFinish?.Invoke();
        }

    }
}

