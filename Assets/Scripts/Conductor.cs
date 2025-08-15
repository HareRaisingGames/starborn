using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn
{
    public class Conductor : MonoBehaviour
    {
        // Song beats per minute
        // This is determined by the song you're trying to sync up to
        public float songBpm { get; private set; }

        // The number of seconds for each song beat
        //public float secPerBeat => (float)secPerBeatAsDouble;
        //public double secPerBeatAsDouble { get; private set; }

        // Current song position, in seconds
        private double songPos; // for Conductor use only
        public float songPosition => (float)songPos;
        public double songPositionAsDouble => songPos;

        private double songPosPerBeat;

        private float dspTime;

        public float offset = 0f;

        public float crochet => 60 / songBpm; //Seconds per beat

        public float stepCrochet => crochet / 4;

        public AudioSource music;

        // Conductor instance
        public static Conductor instance;

        // Conductor is currently playing song
        public bool isPlaying;

        // Conductor is currently paused, but not fully stopped
        public bool isPaused;

        [HideInInspector] public int curBeat;

        [HideInInspector] public int curStep;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {

        }

        public void SetUpBPM()
        {
            if (music != null)
            {
                songBpm = UniBpmAnalyzer.AnalyzeBpm(music.clip) / 2;
                dspTime = (float)AudioSettings.dspTime;
            }
        }

        private void Update()
        {
            isPlaying = music != null && music.clip != null && music.isPlaying;
            isPaused = music != null && music.clip != null && !music.isPlaying;

            if(isPlaying)
            {
                //songPos = (float)(AudioSettings.dspTime - dspTime);
                //songPosPerBeat = songPos / crochet;

                songPos = music.time;

                float adjustedTime = music.time - offset;

                float bps = songBpm / 60f;

                float sps = songBpm / 15f;

                curBeat = Mathf.FloorToInt(adjustedTime * bps);

                curStep = Mathf.FloorToInt(adjustedTime * sps);


            }
        }
    }
}
