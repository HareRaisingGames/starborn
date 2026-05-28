using System;
using System.Collections;
using System.Collections.Generic;
using Starborn.GreatEscape.Templates;

//using System.Numerics;
using Starborn.InputSystem;
using UnityEngine;

namespace Starborn.GreatEscape
{
    /// <summary>
    /// Types of Obstacles
    /// Inside
    /// - Copy Machine (1 jump)
    /// - File Cabinet (1 slide)
    /// - Window (1 jump)
    /// Outside 
    /// - Air Duct (1 slide)
    /// - Ventilator (1 jump)
    /// - Billboard (1 slide)
    /// - Clothesline (2 jump)
    /// - Building (1 jump)
    /// </summary>
    public class TheGreatEscape : Minigame
    {
        protected bool isOutside;
        public bool isBouncing = true;

        [Header("Audio")]
        public AudioSource jumpCall;
        public AudioSource downCall;
        [Header("Camera Stuff")]
        public Camera bounceCamera;
        protected float bounceStart;
        public float bounceOffset;
        protected float runningBPM;
        protected int counter;
        protected int prevCounter = -1;
        protected float bps;
        [Header("Looping")]
        public LoopingBackground background;
        public LoopingBackground foreground;
        public GameObject bugzTemplate;
        public Transform layout;

        [HideInInspector]
        public AudioSource blip;
        #region Object Motion
        public float worldHeight => Camera.main.orthographicSize * 2.0f;
        public float worldWidth => worldHeight * Camera.main.aspect;
        public Vector3 cameraCenter => Camera.main.transform.position;
        [Header("Speed")]
        [Range(1,5)]
        public int interval = 2;
        public float spaceDistance => worldWidth/interval;
        public float duration => interval + 1;
        public float minCamX => cameraCenter.x - (worldWidth/2);
        public float maxCamX => cameraCenter.x + (worldWidth/2);
        public float centerPerSpace => spaceDistance/2;
        public float bugzXPosition => maxCamX - centerPerSpace;
        public float objectStartPosition => minCamX + centerPerSpace - spaceDistance;
        public float objectEndPosition => maxCamX - centerPerSpace + spaceDistance;

        protected float totalDistance => objectEndPosition - objectStartPosition;
        protected float standardTimeCrochet => 60/runningBPM;

        public float standardSpeed => totalDistance/(standardTimeCrochet*duration);
        #endregion

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            
            if(bounceCamera != null)
                bounceStart = bounceCamera.transform.position.y;
            
            if(selectedCharting != null)
            {
                if (selectedCharting.setBPM && selectedCharting.bpm > 0)
                {
                    runningBPM = selectedCharting.bpm;
                }
            }
            else
                runningBPM = 120;

            // Debug.Log(totalDistance);
            // Debug.Log(standardSpeed);
            if(background != null)
                background.speed = standardSpeed * 2f/3f;

            if(foreground != null)
                foreground.speed = standardSpeed;
            
            if(bugzTemplate != null)
                bugzTemplate.transform.position = 
                    new Vector3(bugzXPosition, bugzTemplate.transform.position.y, bugzTemplate.transform.position.z);

            GameObject blipObj = new GameObject("Blip");
            blip = blipObj.AddComponent<AudioSource>();
            blip.playOnAwake = false;
            blip.clip = Resources.Load<AudioClip>($"Audio/blip");

        }

        protected float yBounce;
        // Update is called once per frame
        public override void Update()
        {
            base.Update();

            bps = runningBPM / 15f;
            counter = Mathf.FloorToInt(Time.time * bps);

            if(isBouncing)
            {
                //Debug.Log(counter);
                if(prevCounter != counter)
                {
                    // if(counter % 2 == 0 || counter == 0)
                    // {
                    //     yBounce = bounceStart + bounceOffset;
                    // }
                    // else if(counter % 2 == 1)
                    // {
                    //     yBounce = bounceStart;
                    // }
                    if(counter % 4 == 0 || counter % 4 == 1 || counter == 0)
                    {
                        yBounce = bounceStart + bounceOffset;
                    }
                    else if(counter % 4 == 2 || counter % 4 == 3)
                    {
                        yBounce = bounceStart;
                    }
                }
                if(bounceCamera != null)
                {
                    Vector3 newPos = new Vector3(bounceCamera.transform.position.x, yBounce, bounceCamera.transform.position.z);
                    bounceCamera.transform.position = Vector3.Slerp(bounceCamera.transform.position, newPos, standardSpeed * Time.deltaTime);
                }

            }
            else
            {
                if(bounceCamera != null)
                {
                    Vector3 newPos = new Vector3(bounceCamera.transform.position.x, bounceStart, bounceCamera.transform.position.z);
                    bounceCamera.transform.position = Vector3.Slerp(bounceCamera.transform.position, newPos, standardSpeed * Time.deltaTime);
                }
            }

            prevCounter = counter;
        }

        public override void StartSong()
        {
            base.StartSong();
            //Weird bug
            if (song != null) Conductor.instance.music.clip = song;
            foreach(RhythmInput input in MinigameManager.instance.inputs)
                Debug.Log(input.desHit);
        }

        public void ObjectInMotion(GameObject obj, float duration)
        {
            TweenManager.XTween(obj, objectStartPosition, objectEndPosition, duration);
        }
    }

    public class Copier : Jump
    {
        
    }
}

namespace Starborn.GreatEscape.Templates
{
    public class EscAction : RhythmEvent
    {
        protected TheGreatEscape game;
        protected AudioSource audio;
        protected AudioClip miss;
    
        protected float duration;
        public Action signalAction;
        public Action signalActionB;
        public override void SetUp()
        {
            base.SetUp();
            game = UnityEngine.Object.FindObjectOfType<TheGreatEscape>();
            duration = 2 - (game.interval-1);
            //Debug.Log(duration);
        }
    }

    public class Jump : EscAction
    {
        GameObject test;

        public override void SetUp()
        {
            base.SetUp();

            //Will modify once more has gotten done

            test = new GameObject("Test");
            Texture2D tex = Texture2D.whiteTexture;
            test.AddComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            test.transform.localScale = Vector3.one * 25;
            test.GetComponent<SpriteRenderer>().sortingOrder = 1;
            test.transform.parent = game.layout;
            test.layer = game.layout.gameObject.layer;
            float y = test.transform.position.y;
            if(game.bugzTemplate != null) y = game.bugzTemplate.transform.position.y;
            test.transform.position = new Vector3(game.objectStartPosition, y, test.transform.position.z);
            
        }

        //For some reason, the timing on some of these is not that good
        public Jump()
        {
            actions = new List<CallForAction>() {
                new CallForAction(() =>{
                    game.ObjectInMotion(test, Conductor.instance.crochet * game.duration);
                    // game.blip.Play();
                }, duration),
                new CallForAction(()=>{
                    signalAction?.Invoke();
                    // game.blip.Play();
                    if(game.jumpCall != null) game.jumpCall.Play();
                }, 1f),
                new CallForAction(()=>{
                    game.blip.Play();
                }, 2f, RhythmInputs.Up, 1.5f, 1.5f, ()=>{

                }, (value) => { 
                }),
            };
        }
    }

    public class DoubleJump : EscAction
    {
        public DoubleJump()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    signalAction?.Invoke();
                    if(game.jumpCall != null) game.jumpCall.Play();
                }, 1f),
                new CallForAction(()=>{
                    signalActionB?.Invoke();
                    if(game.jumpCall != null) game.jumpCall.Play();
                }, 2f),
                new CallForAction(()=>{

                }, 3f, RhythmInputs.Up, 1f, 1f, ()=>{

                }, (value) => { 
                }),
                new CallForAction(()=>{

                }, 4f, RhythmInputs.Up, 1f, 1f, ()=>{

                }, (value) => { 
                }),
            };
        }
    }

    public class Slide : EscAction
    {
        public override void SetUp()
        {
            base.SetUp();
            CallForAction start = new CallForAction(() =>
            {
                game.ObjectInMotion(null, Conductor.instance.crochet * game.duration);
            }, 2 - game.interval);
            actions.Insert(0, start);
        }

        public Slide()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    signalAction?.Invoke();
                    if(game.downCall != null) game.downCall.Play();
                }, 1f),
                new CallForAction(()=>{

                }, 2f, RhythmInputs.Down, 1f, 1f, ()=>{

                }, (value) => { 
                }),
            };
        }
    }
}

