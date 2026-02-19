using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rabbyte.Gyotoku;

namespace Starborn.Trojan
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Virus : MonoBehaviour
    {
        protected SpriteRenderer sprite;
        protected Vector2 center;
        protected Vector2 spawnPosition;
        protected Vector2 startPosition;
        protected float angle;
        public ParticleSystem explosion;
        public AudioSource explosionSFX;
        Tween<Vector3> attack;

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
        }

        // Start is called before the first frame update
        void Start()
        {
            if(explosion != null)
            {
                var main = explosion.main;
                main.stopAction = ParticleSystemStopAction.Callback;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void SetVirus(Vector2 center, Vector2 spawn, Vector2 start, float angle = 0)
        {
            this.center = center;
            this.angle = angle;
            spawnPosition = spawn;
            startPosition = start;
        }

        public void Rev(float time)
        {
            TweenManager.PositionTween(gameObject, spawnPosition, startPosition, time, Eases.EaseInOutSine);
        }

        public void Attack(float time)
        {
            attack = TweenManager.PositionTween(gameObject, startPosition, center, time, Eases.EaseInOutQuart, delegate() {
                Destroy(this.gameObject);
            });
        }

        public void Explode(float duration = 0)
        {
            if (attack != null)
            {
                attack.Pause();
                ColorUtils.SetAlpha(gameObject, 0);
                explosion.Play();
                if (explosionSFX != null)
                    explosionSFX.Play();
                LuaMethods.ShakeCamera(duration, 0.25f);
                StartCoroutine(Destruct());
                IEnumerator Destruct()
                {
                    yield return new WaitForSeconds(0.375f);
                    Destroy(this.gameObject);
                }
            }
                
        }

        void OnParticleSystemStopped()
        {
            Debug.Log("Particle System Stopped!");
            Destroy(this.gameObject);
        }
    }
}
