using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.Trojan
{
    public class MalwormVirus : Virus
    {
        protected Vector2 startPos;
        protected Vector2 middlePos;
        public void SetVirus(Vector2 center, Vector2 spawn, Vector2 start, Vector2 move = default, float angle = 0)
        {
            base.SetVirus("malworm", center, spawn, start, angle);
            startPos = move;

            middlePos = (spawn + move) / 2;

            if (transform.position.x < 0)
                sprite.flipX = true;
        }

        int i = 0;

        public void Move()
        {
            if (i == 0)
                transform.position = middlePos;
            else
                transform.position = spawnPosition;

            i++;
        }
    }
}

