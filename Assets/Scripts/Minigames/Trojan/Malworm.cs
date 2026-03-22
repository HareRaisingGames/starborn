using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.Trojan
{
    public class MalwormVirus : Virus
    {
        protected Vector2 startPos;
        protected Vector2 middlePos;
        public AnimationClip inch;
        public AnimationClip rev;
        public AnimationClip charge;
        public AudioSource shuffle;
        public void SetVirus(Vector2 center, Vector2 spawn, Vector2 start, Vector2 move = default, float angle = 0, bool sound = true)
        {
            base.SetVirus("malworm", center, spawn, start, angle);
            transform.position = move;
            startPos = move;

            middlePos = (spawn + move) / 2;

            if (transform.position.x < 0)
                sprite.flipX = true;
            else if(transform.position.x > 0)
                sprite.flipX = false;

            if (shuffle != null && sound)
                shuffle.Play();
        }

        int i = 0;

        public void Move()
        {
            if (i == 0)
                transform.position = middlePos;
            else
                transform.position = spawnPosition;

            if (shuffle != null)
                shuffle.Play();

            i++;
        }

        public void PlayAnimation(string anim)
        {
            animator.speed = Conductor.instance.songBpm / 120;
            switch (anim.ToLower())
            {
                case "inch":
                    if (inch != null)
                        animator.Play(inch.name, -1, 0f);
                    break;
                case "rev":
                    if (rev != null)
                        animator.Play(rev.name, -1, 0f);
                    break;
                case "attack":
                    if (charge != null)
                        animator.Play(charge.name, -1, 0f);
                    break;
            }
        }
    }
}

