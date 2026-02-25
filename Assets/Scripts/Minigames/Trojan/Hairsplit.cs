using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.Trojan
{
    public class HairsplitVirus : Virus
    {
        public GameObject miniPrefab;
        public void Split(float time, out Virus one, out Virus two)
        {
            Vector2 split = new Vector2(angle - 5, angle + 5);

            Vector2 spawnPos1 = Trojan.PositionFromRadius(center, Trojan.game.spawnRadius, split.x);
            Vector2 revPos1 = Trojan.PositionFromRadius(center, Trojan.game.revRadius, split.x);

            Vector2 spawnPos2 = Trojan.PositionFromRadius(center, Trojan.game.spawnRadius, split.y);
            Vector2 revPos2 = Trojan.PositionFromRadius(center, Trojan.game.revRadius, split.y);

            one = Instantiate(miniPrefab, transform.position, Quaternion.identity).GetComponent<Virus>();
            two = Instantiate(miniPrefab, transform.position, Quaternion.identity).GetComponent<Virus>();

            one.SetVirus("hairsplit", center, spawnPos1, revPos1);
            two.SetVirus("hairsplit", center, spawnPos2, revPos2);
            TweenManager.PositionTween(one.gameObject, spawnPosition, spawnPos1, time, Eases.EaseInOutQuart, delegate() {
                
            });
            TweenManager.PositionTween(two.gameObject, spawnPosition, spawnPos2, time, Eases.EaseInOutQuart, delegate () {
                
            });

            one.transform.parent = transform.parent;
            two.transform.parent = transform.parent;

            one.gameObject.layer = gameObject.layer;
            two.gameObject.layer = gameObject.layer;

            if (explosion != null)
            {
                explosion.transform.parent = null;
                var main = explosion.main;
                main.stopAction = ParticleSystemStopAction.Destroy;
                explosion.Play();
            }

            Destroy(this.gameObject);
        }
    }
}

