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
        private double startPos;
        private double startBeat;
        double dspMargin = 128 / 44100.0;
        private double time;
        double absTime, absTimeAdjust;

        public float songLength => (music != null && music.clip != null) ? music.clip.length : 0;
        bool _isFinished;
        public bool isFinished => Conductor.instance.music.timeSamples < timeSamples && timeSamples > 0 && startSong;

        public Action onSongFinished = null;


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

        public static bool startSong;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();
            dspSizeSeconds = config.dspBufferSize / (double)config.sampleRate;
            MixerSettings.SetAudioGroup(music, "Song");

            // Debug.Log(dspSizeSeconds);
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
                startTime = DateTime.Now;
            }
        }

        public void ManualSetUpBPM(float bpm)
        {
            songBpm = bpm;
            dspTime = (float)AudioSettings.dspTime;
            startTime = DateTime.Now;
        }

        public void Play()
        {
            if (isPlaying) return;

            double dspTime = AudioSettings.dspTime;
            dspStart = dspTime;
            startTime = DateTime.Now;
            if(music != null)
            {
                music.PlayScheduled(dspStart);
            }

        }

        public void PlayMusic()
        {
            _isFinished = false;
            music.Play();
            //CancelInvoke();
            timeSamples = music.timeSamples;
            startSong = true;
            //Invoke("Done", songLength);
            if (FindObjectOfType<Minigame>() != null)
                FindObjectOfType<Minigame>().OnSongStart?.Invoke();
        }

        public void PlayMusicWithoutCallback()
        {
            _isFinished = false;
            startSong = true;
            music.Play();
            timeSamples = music.timeSamples;
            if (FindObjectOfType<Minigame>() != null)
                FindObjectOfType<Minigame>().OnSongStart?.Invoke();
        }

        void Done()
        {
            _isFinished = true;
            onSongFinished?.Invoke();
            onSongFinished = null;
        }

        int timeSamples;

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

                // absTime = (DateTime.Now - startTime).TotalSeconds;

                // if (Math.Abs(absTime + absTimeAdjust - dspTime) > dspMargin)
                // {
                //     int i = 0;
                //     while (Math.Abs(absTime + absTimeAdjust - dspTime) > dspMargin)
                //     {
                //         i++;
                //         absTimeAdjust = (dspTime - absTime + absTimeAdjust) * 0.5;
                //         if (i > 8) break;
                //     }
                // }

                // time = absTime + absTimeAdjust;

                // songPos = startPos + time;
                songPos = music.time;

                float adjustedTime = music.time - offset;

                float bps = songBpm / 60f;

                float sps = songBpm / 15f;

                curBeat = Mathf.FloorToInt(adjustedTime * bps);

                curStep = Mathf.FloorToInt(adjustedTime * sps);

                timeSamples = music.timeSamples;
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
