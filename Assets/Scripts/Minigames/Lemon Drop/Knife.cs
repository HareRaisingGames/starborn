using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.LemonDrop
{
    public class Knife : MonoBehaviour
    {
        bool reverse = false;
        public Animator animator;
        protected string sliceAnimation = "Slice";
        protected string sliceReverseAnimation = "Reverse";
        public AudioSource sliceSFX;

        [Header("Layer")]
        public string backLayer;
        public string frontLayer;
        
        // Start is called before the first frame update
        void Start()
        {
            if(animator == null)
                animator = GetComponentInChildren<Animator>();
            
            animator.speed = 2f;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void Slice()
        {
            if(animator != null) {
                animator.Play(reverse ? sliceReverseAnimation : sliceAnimation, 0, 0);
                StartCoroutine(WaitForIndicator());
                IEnumerator WaitForIndicator()
                {
                    yield return null;
                    AnimatorStateInfo indicatorInfo = animator.GetCurrentAnimatorStateInfo(0);
                    while(indicatorInfo.normalizedTime < 0.5f)
                    {
                        yield return null;
                        indicatorInfo = animator.GetCurrentAnimatorStateInfo(0);
                    }
                    int layer = 0;
                    if(indicatorInfo.IsName(sliceReverseAnimation))
                        layer = LayerMask.NameToLayer(backLayer);
                    else if(indicatorInfo.IsName(sliceAnimation))
                        layer = LayerMask.NameToLayer(frontLayer);
                    foreach(Transform child in GetComponentsInChildren<Transform>())
                        child.gameObject.layer = layer;
                }

            }
            if(sliceSFX != null) sliceSFX.Play();
            reverse = !reverse;
        }
    }
}
