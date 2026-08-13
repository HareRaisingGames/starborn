using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameOverMenu : PopupMenu
{
    public static Action tryAgainCallback;
    public void TryAgain()
    {
        tryAgainCallback?.Invoke();
        tryAgainCallback = null;
        /*if (FindObjectOfType<DialogueManager>(true))
            FindObjectOfType<DialogueManager>(true).TryAgain(delegate () { Destroy(gameObject); });*/
    }

    public void ExitToMainMenu()
    {
        if (FindObjectOfType<PauseMenu>(true) != null)
            Destroy(FindObjectOfType<PauseMenu>(true).gameObject);
        MinigameManager.EndChapter();
        LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () {
            Time.timeScale = 1; 
            foreach(Button button in FindObjectsOfType<Button>(true))
                button.enabled = true;
        });
    }
}
