using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rabbyte.Gyotoku;
using System.IO;
using System.Linq;

namespace Starborn.Trojan
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Virus : MonoBehaviour
    {
        protected string being;
        protected SpriteRenderer sprite;
        protected Vector2 center;
        protected Vector2 _spawnPosition;
        protected Vector2 _startPosition;
        protected float angle;
        public float radius;
        public ParticleSystem explosion;
        public ParticleSystem burned;
        public AudioSource explosionSFX;
        protected Tween<Vector3> attack;

        public System.Action onHitAddtional;
        public Animator animator;
        public Sprite burnedSprite;

        bool _isAttacking;
        public bool isAttacking => _isAttacking;
        public Vector2 startPosition => _startPosition;
        public string virusName => being;

        [HideInInspector]
        bool isFried;

        protected virtual void Awake()
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

        public Vector2 spawnPosition => _spawnPosition;

        public virtual void SetVirus(string name, Vector2 center, Vector2 spawn, Vector2 start, float angle = 0)
        {
            being = name;
            this.center = center;
            this.angle = angle;
            _spawnPosition = spawn;
            _startPosition = start;
            transform.position = _spawnPosition;
        }

        public virtual void Rev(float time)
        {
            _isAttacking = true;
            TweenManager.PositionTween(gameObject, _spawnPosition, _startPosition, time, Eases.EaseInOutSine);
        }

        public virtual void Attack(float time)
        {
            _isAttacking = true;
            attack = TweenManager.PositionTween(gameObject, _startPosition, center, time, Eases.EaseInOutQuart, delegate() {
                if(isFried)
                    MinigameManager.instance.LoseALife(0.5f);
                else
                    MinigameManager.instance.LoseALife(1f);

                onHitAddtional?.Invoke();
                Destroy(this.gameObject);
            });
        }

        public virtual void Explode(float duration = 0, System.Action callback = null, bool healthBack = false)
        {
            if (attack != null)
            {
                attack.FullKill();

                AudioClip dieSFX = MusicUtils.GetRandomClip("Audio/Trojan/death", being);

                if(dieSFX != null)
                {
                    GameObject sound = new GameObject("RIP");
                    SoundByte sfx = sound.AddComponent<SoundByte>();
                    sfx.type = "SongSFX";
                    sound.GetComponent<AudioSource>().playOnAwake = false;
                    sound.GetComponent<AudioSource>().clip = dieSFX;
                    sfx.timeSamples = sound.GetComponent<AudioSource>().timeSamples;
                    sound.GetComponent<AudioSource>().Play();
                }

                LuaMethods.ShakeCamera(duration, 0.25f);
            }

            if(healthBack) MinigameManager.instance.AddALife(1f);

            ColorUtils.SetAlpha(gameObject, 0);
            explosion.Play();
            explosion.gameObject.layer = gameObject.layer;
            if (explosionSFX != null)
                explosionSFX.Play();
            callback?.Invoke();
            StartCoroutine(Destruct());
            IEnumerator Destruct()
            {
                yield return new WaitForSeconds(0.375f);
                Destroy(this.gameObject);
            }
        }

        public void Charred()
        {
            isFried = true;
            if(burned != null)
            {
                burned.Play();
                if(animator != null) animator.enabled = false;
                if (burnedSprite != null)
                    sprite.sprite = burnedSprite;
                else
                    sprite.color = Color.black;
                explosionSFX.clip = Resources.Load<AudioClip>($"Audio/Trojan/burst");
                explosionSFX.Play();
            }
        }

        void OnParticleSystemStopped()
        {
            Debug.Log("Particle System Stopped!");
            Destroy(this.gameObject);
        }
    }
}
