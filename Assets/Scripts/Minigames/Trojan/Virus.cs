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
        protected Vector2 startPosition;
        protected float angle;
        public float radius;
        public ParticleSystem explosion;
        public ParticleSystem burned;
        public AudioSource explosionSFX;
        Tween<Vector3> attack;

        [HideInInspector]
        bool isFried;

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

        public Vector2 spawnPosition => _spawnPosition;

        public virtual void SetVirus(string name, Vector2 center, Vector2 spawn, Vector2 start, float angle = 0)
        {
            being = name;
            this.center = center;
            this.angle = angle;
            _spawnPosition = spawn;
            startPosition = start;
        }

        public void Rev(float time)
        {
            TweenManager.PositionTween(gameObject, _spawnPosition, startPosition, time, Eases.EaseInOutSine);
        }

        public void Attack(float time)
        {
            attack = TweenManager.PositionTween(gameObject, startPosition, center, time, Eases.EaseInOutQuart, delegate() {
                if(isFried)
                    MinigameManager.instance.LoseALife(0.5f);
                else
                    MinigameManager.instance.LoseALife(1f);

                MinigameManager.instance.accuracies.Add(-0.1f);
                MinigameManager.instance.displayAccuracy = 0;
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
                explosion.gameObject.layer = gameObject.layer;
                if (explosionSFX != null)
                    explosionSFX.Play();

                AudioClip[] allDeathSounds = Resources.LoadAll<AudioClip>("Audio/Trojan/death");
                IEnumerable<AudioClip> filteredSFX = allDeathSounds.Where(sound => sound.name.Contains(being));
                AudioClip[] virusDeathSfx = filteredSFX.ToArray();

                if(virusDeathSfx.Length != 0)
                {
                    System.Random random = new System.Random();
                    int s = random.Next(0, virusDeathSfx.Length);
                    AudioClip randomClip = virusDeathSfx[s];

                    GameObject sound = new GameObject("RIP");
                    SoundByte sfx = sound.AddComponent<SoundByte>();
                    sfx.type = "SongSFX";
                    sound.GetComponent<AudioSource>().playOnAwake = false;
                    sound.GetComponent<AudioSource>().clip = randomClip;
                    sfx.timeSamples = sound.GetComponent<AudioSource>().timeSamples;
                    sound.GetComponent<AudioSource>().Play();
                }

                LuaMethods.ShakeCamera(duration, 0.25f);
                StartCoroutine(Destruct());
                IEnumerator Destruct()
                {
                    yield return new WaitForSeconds(0.375f);
                    Destroy(this.gameObject);
                }
            }
                
        }

        public void Charred()
        {
            isFried = true;
            if(burned != null)
            {
                burned.Play();
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
