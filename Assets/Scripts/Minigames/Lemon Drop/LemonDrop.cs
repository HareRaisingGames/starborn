using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using System.Runtime;

namespace Starborn.LemonDrop
{
    public class LemonDrop : Minigame
    {
        LemonToss lemonEvent;
        RhythmInput input;
        public GameObject lemonPrefab;
        Lemon lemon;
        // Start is called before the first frame update
        public override void Start()
        {
            lemon = FindObjectOfType<Lemon>();
            //lemonEvent.AddToChart(Conductor.instance.crochet * 13, Conductor.instance.crochet);
            //input = new RhythmInput(RhythmInputs.A).SetDestination(Conductor.instance.crochet * 12).SetRange(Conductor.instance.crochet, Conductor.instance.crochet);
            //input.Enable();
            //Conductor.instance.music.Play();

            //Debug.Log(Object.FindObjectsOfType<RhythmEvent>().Length);
            base.Start();
            StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //Debug.Log("Go!");
                StartSong();
            }
        }
        int i = 0;
        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);
            //i++;
            //lemon.Cut(i);
        }
    }
}
