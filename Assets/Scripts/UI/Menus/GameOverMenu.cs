using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { Time.timeScale = 1;});
    }
}
