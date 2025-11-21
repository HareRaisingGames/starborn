using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using Starborn.Tosstail;

namespace Starborn.Tosstail
{
    public class Tosstail : Minigame
    {
        [Header("Tosstail")]
        public Shaker shaker;
        public Niko niko;

        public readonly string rightCatch = $"Press <sprite=\"game_icons_white\" name=\"{InputCheck.controller}_A\"> to do a right catch";
        public readonly string leftCatch = $"Press <sprite=\"game_icons_white\" name=\"{InputCheck.controller}_Pad\"> to do a left catch";
        public override void Start()
        {
            base.Start();
            TweenManager.instance.AddManager();
            TweenManager.XTween(shaker.gameObject, 
                shaker.transform.position.x, 
                shaker.transform.position.x, 0.01f, Eases.Linear);

            amountJudger = () => shaker.doubleCatches;
            OnSongStart = niko.Bounce;
            OnStepChange = CheckForActivity;
            /*ShortToss shortT = new ShortToss();
            shortT.AddToChart(Conductor.instance.crochet * 2, Conductor.instance.crochet);
            LongToss longT = new LongToss();
            longT.AddToChart(Conductor.instance.crochet * 4, Conductor.instance.crochet);
            shortT = new ShortToss();
            shortT.AddToChart(Conductor.instance.crochet * 8, Conductor.instance.crochet);
            shortT = new ShortToss();
            shortT.AddToChart(Conductor.instance.crochet * 10, Conductor.instance.crochet);
            longT = new LongToss();
            longT.AddToChart(Conductor.instance.crochet * 12, Conductor.instance.crochet);*/
            /*StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //new ShortToss().AddToChart(0);
                //Debug.Log("Go!");
                Conductor.instance.music.Play();
            }*/
            niko.leftArm.SetUpDictionary(shaker.direction);
            niko.rightArm.SetUpDictionary(!shaker.direction);

            niko.leftArm.isFree = shaker.direction;
            niko.rightArm.isFree = !shaker.direction;

            //niko.leftArm.isOpen = niko.leftArm.isFree = shaker.direction;
            //niko.rightArm.isOpen = niko.rightArm.isFree = !shaker.direction;

            if (!niko.rightArm.isOpen) niko.curArm = niko.rightArm;
            else if (!niko.leftArm.isOpen) niko.curArm = niko.leftArm;
        }
        [HideInInspector]
        public int r = 0;
        void CheckForActivity(int i)
        {
            if(i % 4 == r)
                niko.Bounce();
        }

        public override void AdditionalSongSetup()
        {
            base.AdditionalSongSetup();
            if(Conductor.instance != null && niko != null)
            {
                niko.leftArm.SetSpeed(Conductor.instance.songBpm);
                niko.rightArm.SetSpeed(Conductor.instance.songBpm);
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

        public override void TutorialAdditionals()
        {
            base.TutorialAdditionals();
            if (shaker.direction)
                MinigameManager.instance.text.text = leftCatch;
            else
                MinigameManager.instance.text.text = rightCatch;

            //Debug.Log(amountJudger.Invoke());
        }

        public override void PostTutorialAdditionals()
        {
            base.PostTutorialAdditionals();
            OnSongStart = null;
            if(shaker.direction)
            {
                shaker.AutoToss(0.5f);
            }
        }

        public override void TutorialOnComplete(int amount)
        {
            hasCompleted = null;
            base.TutorialOnComplete(amount);
            hasCompleted = delegate ()
            {
                return shaker.doubleCatches >= amount;
            };
        }

        public override void TutorialReset()
        {
            base.TutorialReset();
            shaker.doubleCatches = 0;
            niko.SetExpression(niko.defaultExpression.name);
        }

        public void Toss(float time, float beat = 0, float reset = 1, bool tall = false)
        {
            shaker.Toss(time, tall, reset);
            RhythmInput input = new RhythmInput(shaker.direction ? RhythmInputs.Pad : RhythmInputs.A)
                .SetDestination(beat)
                    .SetRange(0.5f, 0.5f)
                        .SetOnHit(shaker.SuccessfulCatch)
                            .SetOnHalfHit(shaker.UnsuccessfulCatch)
                                .SetOnMiss(shaker.MissedCatched);
            input.Enable();
        }
        public override void onA(InputAction.CallbackContext context)
        {
            if (autoPlay || !Conductor.instance.isPlaying)
                return;

            base.onA(context);

            if(niko.rightArm.isOpen && niko.rightArm.isFree)
            {
                niko.rightArm.SetArm("close", true);
            }
        }

        public override void onReleaseA(InputAction.CallbackContext context)
        {
            if (autoPlay)
                return;

            base.onReleaseA(context);

            if (niko.rightArm.isOpen)
            {
                niko.rightArm.SetArm("open", true);
                niko.rightArm.hand.sortingOrder = 3;
            }
        }

        public override void onPad(InputAction.CallbackContext context)
        {
            if (autoPlay || !Conductor.instance.isPlaying)
                return;

            base.onPad(context);
            if (niko.leftArm.isOpen && niko.leftArm.isFree)
            {
                niko.leftArm.SetArm("close", true);
                niko.rightArm.hand.sortingOrder = 10;
            }
        }

        public override void onReleasePad(InputAction.CallbackContext context)
        {
            if (autoPlay)
                return;

            base.onReleasePad(context);
            if (niko.leftArm.isOpen)
            {
                niko.leftArm.SetArm("open", true);
            }
        }

        public override void Update()
        {
            base.Update();
            niko.leftArm.Update();
            niko.rightArm.Update();

        }
    }

    public class ShortToss : Toss
    {
        public ShortToss()
        {
            CallForAction toss = new CallForAction(() => { }, 1);
            toss.AddAction(() => {
                float beat = startPoint + Conductor.instance.crochet * toss.beat;
                game.Toss(Conductor.instance.crochet, beat, Conductor.instance.crochet);
            });
            actions = new List<CallForAction>() {
                toss
            };
        }
    }

    public class LongToss : Toss
    {
        public LongToss()
        {
            CallForAction toss = new CallForAction(() => { }, 1);
            toss.AddAction(() => {
                float beat = startPoint + Conductor.instance.crochet * (toss.beat + 1);
                game.Toss(Conductor.instance.crochet * 2, beat, Conductor.instance.crochet, true);
            });
            actions = new List<CallForAction>() {
                toss
            };
        }
    }

    public class StopBouncing : RhythmEvent
    {
        public Tosstail game;
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<Tosstail>();
        }
        public StopBouncing()
        {
            //CallForAction toss = new CallForAction(() => { }, 1);
            actions = new List<CallForAction>()
            {
                new CallForAction(() => { game.niko.StopBouncing(); }, 1)
            };
        }
    }

    public class StartBouncing : RhythmEvent
    {
        public Tosstail game;
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<Tosstail>();
        }
        public StartBouncing()
        {
            //CallForAction toss = new CallForAction(() => { }, 1);
            actions = new List<CallForAction>()
            {
                new CallForAction(() => { 
                    game.niko.StartBouncing();
                    game.r = Conductor.instance.curStep % 4;


                }, 1)
            };
        }
    }
}

public class Toss : RhythmEvent
{
    public Tosstail game;
    public override void SetUp()
    {
        base.SetUp();
        game = Object.FindObjectOfType<Tosstail>();
    }
}

