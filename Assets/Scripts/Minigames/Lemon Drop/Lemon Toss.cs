using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.LemonDrop
{
    public class LemonToss : RhythmEvent
    {
        public Lemon lemon;
        public LemonDrop game;

        public LemonToss()
        {
            //lemon = Object.FindObjectOfType<Lemon>();

            CallForAction setUp = new CallForAction(() => { }, 1f);

            actions = new List<CallForAction>() {
            new CallForAction(()=>{//sfx.Play(); 

                lemon.Reassemble();
                //lemon.transform.position = new Vector3(lemon.transform.position.x, -10, lemon.transform.position.z);

            }, 0f),
            new CallForAction(()=>{
                //tick.Play();
                sfx.Play();
                // lemon.transform.position = new Vector3(lemon.transform.position.x, -10, lemon.transform.position.z);
                TweenManager.YTween(lemon.gameObject, -10f, -4.5f, Conductor.instance.crochet * 0.5f, Eases.Linear);
                }, 
                1f),
            new CallForAction(()=>{
                //sfx.Play();
                TweenManager.YTween(lemon.gameObject, -4.5f, 1f, Conductor.instance.crochet * 0.5f, Eases.EaseOutSine);
                }, 
                1.5f),
            new CallForAction(()=>{
                // sfx.Play();
                //tick.Play();
                TweenManager.YTween(lemon.gameObject, 1f, 0.5f, Conductor.instance.crochet, Eases.EaseInOutSine);
            }, 2f, RhythmInputs.A, 1f, 1f, () => { 
                lemon.Cut(1); 
            }),
            new CallForAction(()=>{
                // sfx.Play();
                //tick.Play();
                TweenManager.YTween(lemon.gameObject, 0.5f, -10f, Conductor.instance.crochet * 0.5f, Eases.EaseInSine, delegate(){
                    
                }); 
            }, 3f, RhythmInputs.A, 1f, 1f, () => { 
                lemon.Cut(3); 
            }),
            // setUp,
            new CallForAction(() =>
            {
                game.afterCut = true;

            }, 4f),
            };
        }

        protected AudioSource sfx;
        protected AudioSource tick;
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<LemonDrop>();
            if (Object.FindObjectOfType<Lemon>() == null)
            {
                lemon = Object.Instantiate(game.lemonPrefab, new Vector3(0, -20, 0), Quaternion.Euler(0, 90, 0)).GetComponent<Lemon>();
            }
            else
                lemon = Object.FindObjectOfType<Lemon>();
            
            //To start off the tween so it doesn't glitch when starting
            TweenManager.XTween(lemon.gameObject, lemon.transform.position.x, lemon.transform.position.x, 0.01f, Eases.Linear);
            if (GameObject.Find("Hai") == null)
            {
                GameObject gameObject = new GameObject("Hai");
                sfx = gameObject.AddComponent<AudioSource>();
                sfx.clip = Resources.Load<AudioClip>("Audio/Tosstail/long_toss");
            }
            else
                sfx = GameObject.Find("Hai").GetComponent<AudioSource>();

            if (GameObject.Find("Metronome") == null)
            {
                GameObject gameObject = new GameObject("Metronome");
                tick = gameObject.AddComponent<AudioSource>();
                tick.clip = Resources.Load<AudioClip>("Audio/Tosstail/catch_shaker");
            }
            else
                tick = GameObject.Find("Metronome").GetComponent<AudioSource>();
        }
    }

    public class LemonTossTutorial : LemonToss
    {
        public override void SetUp()
        {
            base.SetUp();
        }
        public LemonTossTutorial()
        {
        //     actions = new List<CallForAction>()
        //     {
        //         new CallForAction(()=>{
        //             // tick.Play();
        //             game.afterCut = false;
        //             game.cutCount = 0;
        //             game.canCheck = false;
        //         },
        //         1f),
        //         new CallForAction(()=>{
        //             // tick.Play();
        //         },
        //         2f),
        //         new CallForAction(()=>{
        //             // tick.Play();
        //         },
        //         3f),
        //         new CallForAction(()=>{
        //             // tick.Play();
        //         },
        //         4f),
        //         new CallForAction(()=>{
        //             // tick.Play();
        //             lemon.Reassemble();
        //         },
        //         5f),
        //     new CallForAction(()=>{
        //             // tick.Play();
        //         sfx.Play();
        //         // TweenManager.YTween(lemon.gameObject, -10f, -4.5f, Conductor.instance.crochet * 0.5f, Eases.Linear);
        //         },
        //         6f),
        //     new CallForAction(()=>{
        //         //sfx.Play();
        //         // TweenManager.YTween(lemon.gameObject, -4.5f, 1f, Conductor.instance.crochet * 0.5f, Eases.EaseOutSine);
        //         },
        //         6.5f),
        //     new CallForAction(()=>{//sfx.Play();
        //             tick.Play();
        //         // TweenManager.YTween(lemon.gameObject, 1f, 0.5f, Conductor.instance.crochet, Eases.EaseInOutSine);
        //     }, 7f, RhythmInputs.A, 0.5f, 0.5f, () => {
        //         lemon.Cut(1);
        //     }),
        //     new CallForAction(()=>{
        //             tick.Play();
        //     }, 7.5f/*, RhythmInputs.A, 0.5f, 0.5f, () => {
        //         lemon.Cut(2);
        //     }*/),
        //     new CallForAction(()=>{
        //             tick.Play();
        //         // TweenManager.YTween(lemon.gameObject, 0.5f, -10f, Conductor.instance.crochet * 0.5f, Eases.EaseInSine, delegate(){
                    
        //         // });
        //     }, 8f, RhythmInputs.A, 0.5f, 0.5f, () => {
        //         // lemon.Cut(3);
        //     }),
        //     new CallForAction(() =>
        //     {
        //         game.afterCut = true;
        //     }, 8.5f),
        //     };
        CallForAction remon = new CallForAction(()=>{}, 2f);

            remon.AddAction(() => {
                sfx.Play();
                float beat1 = startPoint + Conductor.instance.crochet * remon.beat;
                float beat3 = startPoint + Conductor.instance.crochet * (remon.beat + 1);
                Debug.Log(beat1);
                Debug.Log(beat3);
                lemon.AddThrow(beat1, 0.5f, 0.7f);
                lemon.AddThrow(beat3, 0.5f, 0.7f);
            });

        actions = new List<CallForAction>()
        {
            new CallForAction(() =>
            {
                tick.Play();
            }, 1f),
            remon,
            new CallForAction(() =>
            {
                // sfx.Play();
            }, 2.5f)
        };

        }
    }
}
