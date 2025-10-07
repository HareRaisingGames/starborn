using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Starborn
{
    public class Conductor : MonoBehaviour
    {
        // Song beats per minute
        // This is determined by the song you're trying to sync up to
        public float songBpm { get; private set; }

        public float manualBpm;

        // The number of seconds for each song beat
        //public float secPerBeat => (float)secPerBeatAsDouble;
        //public double secPerBeatAsDouble { get; private set; }

        // Current song position, in seconds
        private double songPos; // for Conductor use only
        public float songPosition => (float)songPos;
        public double songPositionAsDouble => songPos;

        private double songPosPerBeat;

        private double dspTime;

        private double dspStart;
        private float dspStartTime => (float)dspStart;
        public double dspStartTimeAsDouble => dspStart;
        DateTime startTime;

        double dspSizeSeconds;

        public float increment = 2;

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
            AudioConfiguration config = AudioSettings.GetConfiguration();
            dspSizeSeconds = config.dspBufferSize / (double)config.sampleRate;
        }

        public void SetUpBPM()
        {
            if (music != null)
            {
                if(manualBpm > 0)
                {
                    songBpm = manualBpm;
                }
                else
                {
                    songBpm = UniBpmAnalyzer.AnalyzeBpm(music.clip) / increment;
                }

                dspTime = (float)AudioSettings.dspTime;
            }
        }

        public void ManualSetUpBPM(float bpm)
        {
            songBpm = bpm;
        }

        public void Play()
        {
            if (isPlaying) return;

            double dspTime = AudioSettings.dspTime;
            dspStart = dspTime;
            if(music != null)
            {
                music.PlayScheduled(dspStart);
            }

        }

        private void Update()
        {
            isPlaying = music != null && music.clip != null && music.isPlaying;
            isPaused = music != null && music.clip != null && !music.isPlaying;

            double dsp = AudioSettings.dspTime;
            if(isPlaying)
            {
                //songPos = (float)(AudioSettings.dspTime - dspTime);
                //songPosPerBeat = songPos / crochet;

                dspTime = dsp - dspStart;

                songPos = music.time;

                float adjustedTime = music.time - offset;

                float bps = songBpm / 60f;

                float sps = songBpm / 15f;

                curBeat = Mathf.FloorToInt(adjustedTime * bps);

                curStep = Mathf.FloorToInt(adjustedTime * sps);
            }
        }

        private void LateUpdate()
        {
            
        }

        void SeekMusicToTime(double fStartPos, double offset)
        {
            if (music.clip != null && fStartPos < music.clip.length - offset)
            {
                // https://www.desmos.com/calculator/81ywfok6xk
                double musicStartDelay = -offset - fStartPos;
                if (musicStartDelay > 0)
                {
                    music.timeSamples = 0;
                }
                else
                {
                    int freq = music.clip.frequency;
                    int samples = (int)(freq * (fStartPos + offset));

                    music.timeSamples = samples;
                }
            }
        }
    }
}
