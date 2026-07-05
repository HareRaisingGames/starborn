using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.GlassPass
{
    public class GlassPass : Minigame
    {
        public ShotGlass testGlass;
        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            // if(FindObjectOfType<PauseMenu>(true) == null)
            // {
            //     StartCoroutine(PlayMusic());
            //     IEnumerator PlayMusic()
            //     {
            //         yield return new WaitForSeconds(1);
            //         SetUpSong();
            //     }
            // }
        }

        public override void StartSong()
        {
            base.StartSong();
            //Weird bug
            if (song != null) Conductor.instance.music.clip = song;
            foreach(RhythmInput input in MinigameManager.instance.inputs)
                Debug.Log(input.desHit);
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }
    }

    public class Slide : RhythmEvent
    {
        public GlassPass game;
        public AudioSource tick;
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<GlassPass>();

            if (GameObject.Find("Metronome") == null)
            {
                GameObject gameObject = new GameObject("Metronome");
                tick = gameObject.AddComponent<AudioSource>();
                tick.clip = Resources.Load<AudioClip>("Audio/blip");
            }
            else
                tick = GameObject.Find("Metronome").GetComponent<AudioSource>();
        }

        public Slide()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(() => { 
                    game.testGlass.ResetPosition();
                    tick.Play();
                }, 1),
                new CallForAction(() => { 
                    tick.Play();
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 2),
                new CallForAction(() => { 
                    // game.testGlass.DefaultSlide(Conductor.instance.crochet);
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 2.5f),
                new CallForAction(()=>{
                    tick.Play();
                }, 3, RhythmInputs.A, 0.5f, 0.5f, game.testGlass.Stop)
            };
        }
    }
}

