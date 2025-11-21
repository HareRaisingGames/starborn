using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : PopupMenu
{
    public void TryAgain()
    {
        FindObjectOfType<DialogueManager>(true).TryAgain(delegate() { Destroy(gameObject); });
    }

    public void ExitToMainMenu()
    {
        if (FindObjectOfType<PauseMenu>(true) != null)
            Destroy(FindObjectOfType<PauseMenu>(true).gameObject);

        LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { Time.timeScale = 1;});
    }
}
