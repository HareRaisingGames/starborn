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

        AudioSource _bonk;
        public AudioSource bonk => _bonk;
        public Knife knife;
        bool canClick = false;
        int swings = 0;

        // Start is called before the first frame update
        public override void Start()
        {
            lemon = FindObjectOfType<Lemon>();
            // amountJudger = () => successTotal;

            if (GameObject.Find("Bonk") == null)
            {
                GameObject gameObject = new GameObject("Bonk");
                _bonk = gameObject.AddComponent<AudioSource>();
                _bonk.clip = Resources.Load<AudioClip>("Audio/Tosstail/miss");
            }
            else
                _bonk = GameObject.Find("Bonk").GetComponent<AudioSource>();
                

            base.Start();
            // StartCoroutine(PlayMusic());
            // IEnumerator PlayMusic()
            // {
            //     yield return new WaitForSeconds(1);
            //     //Debug.Log("Go!");
            //     SetUpSong();
            // }
        }
        //int i = 0;
        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);

            if(hasCompleted != null && hasCompleted.Invoke()) return;
            if(autoPlay && Conductor.instance.isPlaying) return;
            if(!canClick) return;

            if(knife != null)
                knife.Slice();

            if(tagName == "slash")
            {
                swings++;
            }
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
                    //Rare occurance
                    if(lemon.lime && cutCount >= 1) MinigameManager.instance.AddALife(1f);
                    else MinigameManager.instance.LoseALife(1f);
                    StartCoroutine(PlayMusic());
                    IEnumerator PlayMusic()
                    {
                        yield return new WaitForSeconds(1);
                        StartSong();
                    }

                }

            };
        }

        public override void AdditionalSongSetup(string tag = "")
        {
            base.AdditionalSongSetup(tag);
            if (tag == "slash")
            {
                tagName = "slash";
                amountJudger = () => swings;
                canClick = true;
            }
            else
            {
                tagName = "";
                amountJudger = () => successTotal;
                afterCut = true;
                canClick = true;
                // canClick = false;
            }
        }
        public override void TutorialAdditionals()
        {
            base.TutorialAdditionals();
            canClick = true;
            if (tagName == "slash")
            {
                if (MinigameManager.instance.remainingText != null && amountJudger != null)
                {
                    MinigameManager.instance.remainingText.text = MinigameManager.instance.requiredText;
                }

                if (hasCompleted != null && hasCompleted.Invoke())
                    OnBeatTutorial(0);
            }
            else
            {
                if(cutCount >= 2 && afterCut)
                {
                    successTotal++;
                    cutCount = 0;
                }
            }

        }

        public override void TutorialOnComplete(int amount, string tag = "")
        {
            hasCompleted = null;
            base.TutorialOnComplete(amount, tag);
            if(tag == "slash")
            {
                hasCompleted = delegate ()
                {
                    return swings >= amount;
                };
            }
            else
            {
                hasCompleted = delegate ()
                {
                    return successTotal >= amount;
                };
            }
        }

        public override void TutorialReset()
        {
            base.TutorialReset();
            hasCompleted = null;
            tagName = "";
            successTotal = 0;
            cutCount = 0;
            afterCut = false;
            canClick = false;
            swings = 0;
        }

        public override void OnTutorialStop()
        {
            base.OnTutorialStop();
            tagName = "";
        }
    }
}
