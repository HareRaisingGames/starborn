using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Starborn
{
    public class Conductor : MonoBehaviour
    {
        const float fallbackSongBpm = 120f;
        const float songEndToleranceSeconds = 0.05f;

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
        public bool isFinished => _isFinished;

        public Action onSongFinished = null;


        double dspSizeSeconds;

        public float increment = 2;

        public float offset = 0f;

        public float crochet => songBpm > 0 ? 60 / songBpm : 0; //Seconds per beat

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
                songBpm = ResolveSongBpm();

                dspTime = (float)AudioSettings.dspTime;
                startTime = DateTime.Now;
            }
        }

        float ResolveSongBpm()
        {
            if (manualBpm > 0)
            {
                return manualBpm;
            }

            if (music == null || music.clip == null)
            {
                return songBpm > 0 ? songBpm : 0;
            }

            float safeIncrement = increment;
            if (safeIncrement <= 0)
            {
                Debug.LogWarning($"Conductor on {gameObject.name} has increment <= 0. Falling back to 1 to avoid invalid BPM calculation.");
                safeIncrement = 1;
            }

            float analyzedBpm = UniBpmAnalyzer.AnalyzeBpm(music.clip);
            if (analyzedBpm > 0)
            {
                return analyzedBpm / safeIncrement;
            }

            Debug.LogWarning($"Conductor on {gameObject.name} could not resolve BPM from clip {music.clip.name}. Falling back to {fallbackSongBpm} BPM.");
            return fallbackSongBpm;
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
            dspStart = AudioSettings.dspTime;
            startTime = DateTime.Now;
            music.Play();
            timeSamples = music.timeSamples;
            startSong = true;
            if (FindObjectOfType<Minigame>() != null)
                FindObjectOfType<Minigame>().OnSongStart?.Invoke();
        }

        public void PlayMusicWithoutCallback()
        {
            _isFinished = false;
            dspStart = AudioSettings.dspTime;
            startTime = DateTime.Now;
            startSong = true;
            music.Play();
            timeSamples = music.timeSamples;
            if (FindObjectOfType<Minigame>() != null)
                FindObjectOfType<Minigame>().OnSongStart?.Invoke();
        }

        void Done()
        {
            if (_isFinished)
                return;

            _isFinished = true;
            onSongFinished?.Invoke();
            onSongFinished = null;
        }

        int timeSamples;

        private float[] sampleBuffer = new float[64];
        private void Update()
        {
            isPlaying = music != null && music.clip != null && music.isPlaying;
            isPaused = music != null && music.clip != null && !music.isPlaying;

            int currentTimeSamples = 0;
            if (music != null && music.clip != null)
            {
                currentTimeSamples = music.timeSamples;
            }

            double dsp = AudioSettings.dspTime;
            if(isPlaying)
            {
                //songPos = (float)(AudioSettings.dspTime - dspTime);
                //songPosPerBeat = songPos / crochet;

                dspTime = dsp - dspStart;

                music.GetSpectrumData(sampleBuffer, 0, FFTWindow.BlackmanHarris);
            
                float totalVolume = 0f;
                foreach (float sample in sampleBuffer)
                {
                    totalVolume += sample;
                }

                hasAudibleSound = totalVolume > 0.001f;

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
                songPos = currentTimeSamples / (double)music.clip.frequency;

                float adjustedTime = (float)songPos - offset;

                float bps = songBpm / 60f;

                float sps = songBpm / 15f;

                curBeat = Mathf.FloorToInt(adjustedTime * bps);

                curStep = Mathf.FloorToInt(adjustedTime * sps);
            }

            UpdateSongFinishState(currentTimeSamples);
            timeSamples = currentTimeSamples;
        }

        void UpdateSongFinishState(int currentTimeSamples)
        {
            if (_isFinished || !startSong || music == null || music.clip == null || music.loop || Time.timeScale == 0)
            {
                return;
            }

            int clipSamples = music.clip.samples;
            int endToleranceSamples = Mathf.CeilToInt(music.clip.frequency * songEndToleranceSeconds);

            bool reachedSongEnd = clipSamples > 0 && currentTimeSamples >= Mathf.Max(0, clipSamples - endToleranceSamples);
            bool wrappedBackToStart = timeSamples > 0 && currentTimeSamples < timeSamples;
            bool stoppedNearSongEnd = !music.isPlaying && timeSamples >= Mathf.Max(0, clipSamples - endToleranceSamples);

            if (reachedSongEnd || wrappedBackToStart || stoppedNearSongEnd)
            {
                Done();
            }
        }

        protected static bool hasAudibleSound;
        public static bool isPlayingSound => hasAudibleSound;
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
