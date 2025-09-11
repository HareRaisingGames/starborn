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
        public Shaker shaker;
        public override void Start()
        {
            base.Start();
            TweenManager.instance.AddManager();
            TweenManager.XTween(shaker.gameObject, shaker.transform.position.x, shaker.transform.position.x, 0.01f, Eases.Linear);

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
            StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //new ShortToss().AddToChart(0);
                //Debug.Log("Go!");
                Conductor.instance.music.Play();
            }
        }

        public void Toss(float time, float beat = 0, float reset = 1, bool tall = false)
        {
            shaker.Toss(time, tall, reset);
            RhythmInput input = new RhythmInput(shaker.direction ? RhythmInputs.Pad : RhythmInputs.A)
                .SetDestination(beat)
                    .SetRange(tall ? 2f : 1f, tall ? 2f : 1f)
                        .SetOnHit(shaker.SuccessfulCatch)
                            .SetOnHalfHit(shaker.UnsuccessfulCatch);
            input.Enable();
        }
        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);
            //shaker.Toss(0.5f);
        }

        public override void onPad(InputAction.CallbackContext context)
        {
            base.onPad(context);
            //Debug.Log("Pad!");
        }

        public override void Update()
        {
            base.Update();
        }
    }

    public class ShortToss : Toss
    {
        public ShortToss()
        {
            CallForAction toss = new CallForAction(() => { }, 1);
            toss.AddAction(() => {
                float beat = startPoint + Conductor.instance.crochet * (toss.beat - 1);
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
                float beat = startPoint + Conductor.instance.crochet * (toss.beat - 1);
                game.Toss(Conductor.instance.crochet * 2, beat, Conductor.instance.crochet, true);
            });
            actions = new List<CallForAction>() {
                toss
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

