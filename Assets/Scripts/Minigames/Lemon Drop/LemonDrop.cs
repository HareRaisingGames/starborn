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
        public GameObject lemonPrefab;
        Lemon lemon;
        [HideInInspector]
        public int cutCount;
        [HideInInspector]
        public bool afterCut;
        [HideInInspector]
        public int successTotal;
        [HideInInspector]
        public bool canCheck;

        public Knife knife;
        bool canClick = false;

        // Start is called before the first frame update
        public override void Start()
        {
            lemon = FindObjectOfType<Lemon>();
            amountJudger = () => successTotal;
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
                SetUpSong();
            }
        }
        //int i = 0;
        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);

            if(!canClick) return;

            if(knife != null)
                knife.Slice();
        }

        public override void StartSong()
        {
            canClick = true;
            hasCompleted = delegate ()
            {
                return cutCount >= 2 && afterCut && Conductor.instance.isFinished;
            };
            base.StartSong();
            Conductor.instance.onSongFinished += delegate ()
            {
                if (!hasCompleted.Invoke())
                {
                    Debug.Log("Failed");
                    MinigameManager.instance.LoseALife(1f);
                    StartCoroutine(PlayMusic());
                    IEnumerator PlayMusic()
                    {
                        yield return new WaitForSeconds(1);
                        StartSong();
                    }

                }

            };
        }

        public override void TutorialAdditionals()
        {
            base.TutorialAdditionals();
            if(!canCheck)
            {
                if(cutCount >= 2 && afterCut)
                {
                    successTotal++;
                    canCheck = true;
                }
            }
        }

        public override void TutorialOnComplete(int amount, string tag = "")
        {
            hasCompleted = null;
            base.TutorialOnComplete(amount, tag);
            hasCompleted = delegate ()
            {
                return successTotal >= amount;
            };
        }

        public override void TutorialReset()
        {
            base.TutorialReset();
            successTotal = 0;
            cutCount = 0;
            afterCut = false;
        }
    }
}
