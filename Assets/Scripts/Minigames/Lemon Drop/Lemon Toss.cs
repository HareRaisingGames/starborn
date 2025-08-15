using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.LemonDrop
{
    public class LemonToss : RhythmEvent
    {
        public Lemon lemon;

        public LemonToss()
        {
            //lemon = Object.FindObjectOfType<Lemon>();
            actions = new List<CallForAction>() {
            new CallForAction(()=>{TweenManager.YTween(lemon.gameObject, -20f, -5f, Conductor.instance.crochet, Eases.Linear); }, 1),
            new CallForAction(()=>{sfx.Play(); TweenManager.YTween(lemon.gameObject, -5f, 0.5f, Conductor.instance.crochet, Eases.EaseOutQuad); }, 2, RhythmInputs.A, 1, 0.5f),
            new CallForAction(()=>{sfx.Play(); }, 2.5f, RhythmInputs.A, 0.5f, 0.5f),
            new CallForAction(()=>{sfx.Play(); TweenManager.YTween(lemon.gameObject, 0.5f, -5f, Conductor.instance.crochet * 0.5f, Eases.EaseInQuad); }, 3, RhythmInputs.A, 0.5f),
            new CallForAction(()=>{TweenManager.YTween(lemon.gameObject, -5f, -20f, Conductor.instance.crochet * 0.5f, Eases.EaseInQuad); }, 3.5f)
            };
        }

        AudioSource sfx;
        public override void SetUp()
        {
            base.SetUp();
            lemon = Object.FindObjectOfType<Lemon>();

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
