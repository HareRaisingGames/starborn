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
        public float secPerBeat => (float)secPerBeatAsDouble;
        public double secPerBeatAsDouble { get; private set; }

        // Current song position, in seconds
        private double songPos; // for Conductor use only
        public float songPosition => (float)songPos;
        public double songPositionAsDouble => songPos;

        public static Conductor instance;

        void Awake()
        {
            instance = this;
        }
    }
}
