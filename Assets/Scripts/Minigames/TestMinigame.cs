using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using System.Reflection;
using UnityEngine.InputSystem;

namespace Starborn.Test
{
    public class TestMinigame : Minigame
    {
        TestEvent newEvent;
        RhythmInput input;
        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override void Start()
        {
            base.Start();
            TweenManager.instance.AddManager();
            Conductor.instance.SetUpBPM();
            newEvent = new TestEvent();
            input = new RhythmInput(RhythmInputs.A).SetDestination(Conductor.instance.crochet * 4).SetRange(0.5f,0.5f);
            input.Enable();
            newEvent.AddToChart(0);
            newEvent.AddToChart(Conductor.instance.crochet * 4);
            StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //Debug.Log("Go!");
                Conductor.instance.music.Play();
            }
            
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);
            //Debug.Log("Hit!");

            //TweenManager.YTween(lemon, -20, 0f, 0.5f, Eases.EaseInOutQuad).SetPingPong(2);
            //TweenManager.YTween(lemon, -10, 0.05f, 1f, Eases.EaseInOutCubic, () => {
            //TweenManager.YTween(lemon, 0.05f, -10f, 1f, Eases.EaseInOutCubic);
            //});

        }


        public override void Update()
        {
            base.Update();
            if(Conductor.instance.isPlaying)
            {
                newEvent.CheckForInvoke(Conductor.instance.songPosition);
                input.Update(Conductor.instance.songPosition);
            }
                

            //lemon.transform.Rotate(Vector3.one * 50 * Time.deltaTime);
        }
    }

    public class TestEvent : RhythmEvent
    {
        public TestEvent()
        {
            //lemon = Object.FindObjectOfType<Lemon>();
            actions = actions = new List<CallForAction>() {
            new CallForAction(()=>{Debug.Log("1"); sfx.Play(); }, 1),
            new CallForAction(()=>{Debug.Log("2"); sfx.Play();}, 2),
            new CallForAction(()=>{Debug.Log("3"); sfx.Play();}, 3),
            new CallForAction(()=>{Debug.Log("and"); sfx.Play();}, 3.5f),
            new CallForAction(()=>{Debug.Log("4"); sfx.Play();}, 4)
            };
        }

        AudioSource sfx;
        public override void SetUp()
        {
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
