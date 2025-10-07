using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGame : MonoBehaviour
{
    Minigame minigame;
    public static bool reset;

    public HeartGroup lives;

    public float livesRemaining = 3;

    // Start is called before the first frame update
    void Start()
    {
        minigame = FindObjectOfType<Minigame>();
        if(reset)
        {
            StartGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lives != null)
            lives.SetLives(livesRemaining);
    }

    public void StartGame()
    {
        minigame.StartSong();
        gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
