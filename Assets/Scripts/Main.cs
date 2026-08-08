using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Preload dialogue state
        //Resources.Load<GameObject>("Prefabs/Transition");
        StartCoroutine(LoadSceneMode());
        IEnumerator LoadSceneMode()
        {
            yield return new WaitForSeconds(0.5f);
            LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { 
                Time.timeScale = 1; 
                foreach(Button button in FindObjectsOfType<Button>(true))
                    button.enabled = true;
                }, 0.1f);
        }
        
    }

    public IEnumerator SwitchSceneRoutine(string newSceneName, string oldSceneName, Action callback = null)
    {
        // Load the new scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Set the newly loaded scene as the active scene
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        SceneManager.SetActiveScene(newScene);

        // Unload the old scene
        yield return SceneManager.UnloadSceneAsync(oldSceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
