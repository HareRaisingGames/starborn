using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using Starborn.Boomerang;
using UnityEngine.InputSystem;

namespace Starborn.Boomerang
{
    public class Boomerang : Minigame
    {
        [HideInInspector]
        public bool turnOffDefenses;
        AudioSource ducking;

        bool attacking;
        [Header("Boomerang")]
        public Vector3 attackCamPos = new Vector3(0,0,-10);
        public float attackZoom = 2.5f;

        // Start is called before the first frame update
        public override void Start()
        {
            if(Resources.Load<AudioClip>("Audio/Boomerang/bugz_duck"))
            {
                GameObject gameObject = new GameObject("Duck");
                ducking = gameObject.AddComponent<AudioSource>();
                ducking.playOnAwake = false;
                ducking.clip = Resources.Load<AudioClip>("Audio/Boomerang/bugz_duck");
            }
            base.Start();

            
            StartCoroutine(SongSetUp());
            IEnumerator SongSetUp()
            {
                yield return new WaitForSeconds(2.1f);
                SetUpSong();
            }
        }

        public override void StartSong()
        {
            hasCompleted = delegate ()
            {
                return Conductor.instance.isFinished;
            };
            base.StartSong();
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();

            if(attacking)
            {
                foreach(Camera camera in FindObjectsOfType<Camera>())
                {
                    camera.transform.position = Vector3.Lerp(camera.transform.position, attackCamPos, Time.deltaTime * 4f);
                    camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, attackZoom - 0.1f, Time.deltaTime * 5f), attackZoom, Mathf.Infinity);
                }
            }
            else
            {
                foreach (Camera camera in FindObjectsOfType<Camera>())
                {
                    camera.transform.position = Vector3.Lerp(camera.transform.position, camPosition, Time.deltaTime * 4f);
                    camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, zoom + 0.1f, Time.deltaTime * 5f), 0, zoom);
                }
            }
        }

        public override void onDown(InputAction.CallbackContext context)
        {
            base.onDown(context);
            Duck();
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);
            attacking = true;
            StopCoroutine("ResetAttack");
            StartCoroutine("ResetAttack");
        }

        void Duck()
        {
            if (ducking != null)
                ducking.Play();
        }

        public IEnumerator ResetAttack()
        {
            yield return new WaitForSeconds(1f);
            attacking = false;
        }

        public void AutoDuck()
        {
            if (autoPlay) Duck();
        }
    }
    public class GreenBoomerang : Rang
    {
        public GreenBoomerang()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(null, 1f),
                new CallForAction(()=>{ boomerangWhoosh.Play(); boomerangWhoosh.volume = 1f; game.AutoDuck(); }, 2f, RhythmInputs.Down),
                new CallForAction(()=>{ boomerangWhoosh.Play(); boomerangWhoosh.volume = 0.5f; }, 3f),
                new CallForAction(()=>{ boomerangWhoosh.Play(); boomerangWhoosh.volume = 1f; game.AutoDuck();}, 4f, RhythmInputs.Down)
            };
        }
    }

    public class RedBoomerang : Rang
    {
        public RedBoomerang()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(null, 1f),
                new CallForAction(null, 2f),
                new CallForAction(()=>{ boomerangWhoosh.Play(); boomerangWhoosh.volume = 1f; game.AutoDuck(); }, 3f, RhythmInputs.Down),
                new CallForAction(()=>{ boomerangWhoosh.Play(); boomerangWhoosh.volume = 1f; game.AutoDuck(); }, 4f, RhythmInputs.Down)
            };
        }
    }

    public class RandomBoomerang : Rang
    {
        public RandomBoomerang()
        {
            //System.Random will return different values always instead of UnityEngine.Random
            System.Random random = new System.Random();
            float randomFloat = (float)random.NextDouble();
            Rang boomerang = randomFloat <= 0.5f ? new RedBoomerang() : new GreenBoomerang();
            actions = boomerang.actions;
        }
    }
}

public class Rang : RhythmEvent
{
    public Boomerang game;
    public AudioSource boomerangWhoosh;
    public override void SetUp()
    {
        base.SetUp();
        game = Object.FindObjectOfType<Boomerang>();

        if (GameObject.Find("Whoosh") == null)
        {
            GameObject gameObject = new GameObject("Whoosh");
            boomerangWhoosh = gameObject.AddComponent<AudioSource>();
            boomerangWhoosh.clip = Resources.Load<AudioClip>("Audio/Boomerang/whoosh_fast");
        }
        else
            boomerangWhoosh = GameObject.Find("Whoosh").GetComponent<AudioSource>();
    }
}

