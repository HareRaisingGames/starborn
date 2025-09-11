using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MinigameManager : MonoBehaviour
{
    Minigame minigame;
    Conductor conductor;

    [HideInInspector]
    public List<RhythmEvent> events = new List<RhythmEvent>();
    [HideInInspector]
    public List<RhythmInput> inputs = new List<RhythmInput>();

    [HideInInspector]
    public List<float> accuracies = new List<float>();

    public int maxLives = 3;
    int _lives;
    bool _gameOver;
    bool _consequences;
    public bool gameOver
    {
        get
        {
            return _gameOver;
        }
        set
        {
            _gameOver = value;
        }
    }
    public int lives
    {
        get
        {
            return _lives;
        }
        set
        {
            _lives = value;
        }
    }

    public bool consequences
    {
        set
        {
            _consequences = value;
        }
    }

    private static MinigameManager _instance;

    public static MinigameManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject manager = new GameObject("Minigame");
                _instance = manager.AddComponent<MinigameManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        Conductor.instance.SetUpBPM();
    }

    // Start is called before the first frame update
    void Start()
    {
        _lives = maxLives;
    }

    public void LoseALife(int amount = 1)
    {
        if(!_consequences)
            _lives -= amount;
    }

    // Update is called once per frame
    void Update()
    {
        if (Conductor.instance.isPlaying)
        {
            foreach(RhythmEvent e in events)
            {
                e.CheckForInvoke(Conductor.instance.songPosition);
            }
            foreach (RhythmInput input in inputs)
            {
                input.Update(Conductor.instance.songPosition);
            }
        }

        if(_lives <= 0 && !_gameOver)
        {
            _gameOver = true;
            MusicUtils.SlowDownMusic(Conductor.instance.music, 2.5f);
        }

        /*if (Gamepad.current != null)
        {
            // Iterate through all buttons on the current gamepad
            Debug.Log(Gamepad.current.name);
            foreach (InputControl control in Gamepad.current.allControls)
            {
                // Check if the control is a button and if it's currently pressed
                if (control is ButtonControl button && button.isPressed)
                {
                    Debug.Log($"Any gamepad button pressed: {button.name}");
                    // You can add your desired action here, e.g., trigger an event
                    return; // Exit after finding the first pressed button
                }
            }
        }*/

        //inputs = Object.FindObjectsOfType<RhythmInput>();
        //inputs = new List<RhythmInput>(FindObjectsOfType<RhythmInput>());
    }
}
