using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using Starborn.LemonDrop;
using Starborn;

public class LemonToss : RhythmEvent
{
    public Lemon lemon;
    public LemonDrop game;

    public LemonToss(bool callout = false)
    {
        //lemon = Object.FindObjectOfType<Lemon>();

        CallForAction setUp = new CallForAction(() => { }, 1f);

        actions = new List<CallForAction>() {
            new CallForAction(()=>{//sfx.Play(); 

                lemon.Reassemble();
                Debug.Log("Assemble!");
                //lemon.transform.position = new Vector3(lemon.transform.position.x, -10, lemon.transform.position.z);

            }, 0f),
            new CallForAction(()=>{
                //tick.Play();
                sfx.Play();
                // lemon.transform.position = new Vector3(lemon.transform.position.x, -10, lemon.transform.position.z);
                TweenManager.YTween(lemon.gameObject, lemon.startY, -4.5f, Conductor.instance.crochet * 0.5f, Eases.Linear);
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
                if(callout) cut1.Play();
            }, 2f, RhythmInputs.A, 1f, 1f, () => {
                if(game.autoPlay) game.knife.Slice();
                lemon.Cut(1);
            }, (bool halfHit) => {
                lemon.Missed();
                game.bonk.Play();
            }),
            new CallForAction(()=>{
                // sfx.Play();
                //tick.Play();
                // TweenManager.YTween(lemon.gameObject, 1f, 0.5f, Conductor.instance.crochet, Eases.EaseInOutSine);
                if(callout) cut1.Play();
            }, 2.5f, RhythmInputs.A, 1f, 1f, () => {
                if(game.autoPlay) game.knife.Slice();
                lemon.Cut(2);
            }, (bool halfHit) => {
                lemon.Missed();
                game.bonk.Play();
            }),
            new CallForAction(()=>{
                // sfx.Play();
                //tick.Play();
                if(callout) cut2.Play();
                TweenManager.YTween(lemon.gameObject, 0.5f, lemon.startY, Conductor.instance.crochet * 0.5f, Eases.EaseInSine, delegate(){

                });
            }, 3f, RhythmInputs.A, 1f, 1f, () => {
                if(game.autoPlay) game.knife.Slice();
                lemon.Cut(3);
            }, (bool halfHit) => {
                lemon.Missed();
                game.bonk.Play();
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
    protected AudioSource cut1;
    protected AudioSource cut2;
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

        if (GameObject.Find("Cut1") == null)
        {
            GameObject gameObject = new GameObject("Cut1");
            cut1 = gameObject.AddComponent<AudioSource>();
            cut1.clip = Resources.Load<AudioClip>("Audio/LemonDrop/cut_1");
        }
        else
            cut1 = GameObject.Find("Cut1").GetComponent<AudioSource>();

        if (GameObject.Find("Cut2") == null)
        {
            GameObject gameObject = new GameObject("Cut2");
            cut2 = gameObject.AddComponent<AudioSource>();
            cut2.clip = Resources.Load<AudioClip>("Audio/LemonDrop/cut_2");
        }
        else
            cut2 = GameObject.Find("Cut2").GetComponent<AudioSource>();


        // Debug.Log(UnityEngine.Random.Range(0f,1f));
        // if (GameObject.Find("Metronome") == null)
        // {
        //     GameObject gameObject = new GameObject("Metronome");
        //     tick = gameObject.AddComponent<AudioSource>();
        //     tick.clip = Resources.Load<AudioClip>("Audio/Tosstail/catch_shaker");
        // }
        // else
        //     tick = GameObject.Find("Metronome").GetComponent<AudioSource>();
    }
}

namespace Starborn.LemonDrop
{
    public class LemonTossMain : LemonToss
    {
        public override void SetUp()
        {
            base.SetUp();
        }
        public LemonTossMain() : base(false)
        {


        }
    }
    public class LemonTossTutorial : LemonToss
    {
        public override void SetUp()
        {
            base.SetUp();
        }
        public LemonTossTutorial() : base(true)
        {


        }
    }
}
