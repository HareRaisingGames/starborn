using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.Trojan
{
    public class TurbotVirus : Virus
    {
        public Animator animator;
        public AnimationClip openEye;
        public AnimationClip closeEye;
        public AnimationClip eye;
        public SpriteRenderer body;

        protected override void Awake()
        {
            base.Awake();
            if (body != null) sprite = body;
        }

        public override void Rev(float time)
        {
            animator.speed = Conductor.instance.songBpm / 120;
            animator.Play(openEye.name);
            base.Rev(time);
        }

        public override void Attack(float time)
        {
            animator.speed = Conductor.instance.songBpm / 120;
            animator.Play(closeEye.name);
            base.Attack(time);
        }

        public override void Explode(float duration = 0, System.Action callback = null)
        {
            if (body != null)
                ColorUtils.SetAlpha(body.gameObject, 0);
            base.Explode(duration, callback);
        }
    }
}
