using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using UnityEngine.InputSystem;

namespace Starborn.LemonDrop
{
    public class LemonDrop : Minigame
    {
        LemonToss lemonEvent;
        RhythmInput input;
        // Start is called before the first frame update
        public override void Start()
        {
            lemonEvent = new LemonToss();
            TweenManager.instance.AddManager();
            Conductor.instance.SetUpBPM();
            lemonEvent.AddToChart(Conductor.instance.crochet * 1);
            lemonEvent.AddToChart(Conductor.instance.crochet * 13, Conductor.instance.crochet);
            //input = new RhythmInput(RhythmInputs.A).SetDestination(Conductor.instance.crochet * 12).SetRange(Conductor.instance.crochet, Conductor.instance.crochet);
            //input.Enable();
            //Conductor.instance.music.Play();
            StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //Debug.Log("Go!");
                Conductor.instance.music.Play();
            }
        }

        // Update is called once per frame
        public override void Update()
        {
            if (Conductor.instance.isPlaying)
            {
                lemonEvent.CheckForInvoke(Conductor.instance.songPosition);
                //input.Update(Conductor.instance.songPosition);
            }
                
        }
    }
}
