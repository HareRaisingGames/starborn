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
            actions = new List<CallForAction>() {
            new CallForAction(()=>{sfx.Play(); 
                TweenManager.YTween(lemon.gameObject, -10f, -4.75f, Conductor.instance.crochet * 0.5f, Eases.Linear); }, 
                1),
            new CallForAction(()=>{/*sfx.Play();*/ 
                TweenManager.YTween(lemon.gameObject, -4.75f, 0.5f, Conductor.instance.crochet * 1.5f, Eases.EaseOutQuad); }, 
                1.5f),
            new CallForAction(()=>{/*sfx.Play();*/ }, 2, RhythmInputs.A, 0.5f, 0.25f, () => { lemon.Cut(1); }),
            new CallForAction(()=>{/*sfx.Play();*/ }, 2.5f, RhythmInputs.A, 0.25f, 0.25f, () => { lemon.Cut(2); }),
            new CallForAction(()=>{/* sfx.Play();*/ TweenManager.YTween(lemon.gameObject, 0f, -5f, Conductor.instance.crochet * 0.5f, Eases.EaseInQuad); }, 3, RhythmInputs.A, 0.25f, 0.5f, () => { lemon.Cut(3); }),
            new CallForAction(()=>{TweenManager.YTween(lemon.gameObject, -5f, -20f, Conductor.instance.crochet * 0.5f, Eases.EaseInQuad); }, 3.5f)
            };
        }

        AudioSource sfx;
        public override void SetUp()
        {
            base.SetUp();
            if (Object.FindObjectOfType<Lemon>() == null)
            {
                game = Object.FindObjectOfType<LemonDrop>();
                lemon = Object.Instantiate(game.lemonPrefab, new Vector3(0, -20, 0), Quaternion.Euler(0, 90, 0)).GetComponent<Lemon>();
            }
            else
                lemon = Object.FindObjectOfType<Lemon>();
            
            //To start off the tween so it doesn't glitch when starting
            TweenManager.XTween(lemon.gameObject, lemon.transform.position.x, lemon.transform.position.x, 0.01f, Eases.Linear);
            if (GameObject.Find("Metronome") == null)
            {
                GameObject gameObject = new GameObject("Metronome");
                sfx = gameObject.AddComponent<AudioSource>();
                sfx.clip = Resources.Load<AudioClip>("Audio/blip");
            }
            else
                sfx = GameObject.Find("Metronome").GetComponent<AudioSource>();
        }
    }
}
