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
            if(animator != null) animator.Play(reverse ? sliceReverseAnimation : sliceAnimation, 0, 0);
            if(sliceSFX != null) sliceSFX.Play();
            reverse = !reverse;
        }
    }
}
