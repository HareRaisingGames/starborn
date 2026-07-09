using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using UnityEngine.InputSystem;

namespace Starborn.GlassPass
{
    public class GlassPass : Minigame
    {
        public ShotGlass testGlass;
        public BugzGP bugz;

        bool specialHand;

        bool specialBody;
        protected AudioSource _tick;
        protected AudioSource _catchGlass;
        public AudioSource tick => _tick;
        public AudioSource catchGlass => _catchGlass;

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
            
            if (GameObject.Find("Pass") == null)
            {
                GameObject gameObject = new GameObject("Pass");
                _tick = gameObject.AddComponent<AudioSource>();
                _tick.clip = Resources.Load<AudioClip>("Audio/Tosstail/long_toss");
            }
            else
                _tick = GameObject.Find("Pass").GetComponent<AudioSource>();

            if (GameObject.Find("Catch") == null)
            {
                GameObject gameObject = new GameObject("Catch");
                _catchGlass = gameObject.AddComponent<AudioSource>();
                _catchGlass.clip = Resources.Load<AudioClip>("Audio/Tosstail/catch_shaker");
            }
            else
                _catchGlass = GameObject.Find("Catch").GetComponent<AudioSource>();
            OnSongStart = ()=>{ Bounce(0); };
            OnBeatChange = Bounce;
        }

        public override void StartSong()
        {
            base.StartSong();
            //Weird bug
            if (song != null) Conductor.instance.music.clip = song;
            // foreach(RhythmInput input in MinigameManager.instance.inputs)
            //     Debug.Log(input.desHit);
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }

        void Bounce(int i)
        {
            if(!specialBody)
                // if(i % 1 == 0 || i == 0)
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);

            if(!specialHand)
                if(i % 2 == 0 || i == 0)
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);

            if(autoPlay || !_startSong)
                return;

            specialHand = true;
            bugz.PlayHand("catch", Conductor.instance.songBpm/120, () =>
            {
                specialHand = false;
            });
        }

        public void SuccessfulCatch()
        {
            specialBody = true;
            bugz.handAnimator.gameObject.SetActive(false);
            bugz.PlayBody("catch", Conductor.instance.songBpm/120, () =>
            {
                specialBody = false;
                bugz.handAnimator.gameObject.SetActive(true);

                if(curStep % 4 == 0)
                {
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                }

            });
        }

        public void UnsuccessfulCatch(bool boolean)
        {
            specialBody = true;
            bugz.PlayBody("half", Conductor.instance.songBpm/120, () =>
            {
                specialBody = false;

                if(curStep % 4 == 0)
                {
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                }

            });
        }
    }

    public class Slide : RhythmEvent
    {
        public GlassPass game;
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<GlassPass>();

        }

        public Slide()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(() => { 
                    // game.testGlass.ResetPosition();
                    game.tick.Play();
                }, 1),
                new CallForAction(() => { 
                    // tick.Play();
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 2),
                new CallForAction(() => { 
                    // game.testGlass.DefaultSlide(Conductor.instance.crochet);
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 2.5f),
                new CallForAction(()=>{
                    // tick.Play();
                }, 3, RhythmInputs.A, 0.5f, 0.5f, ()=>{
                    // game.testGlass.Stop();
                    game.SuccessfulCatch();
                    game.catchGlass.Play();
                }, game.UnsuccessfulCatch)
            };
        }
    }
}

