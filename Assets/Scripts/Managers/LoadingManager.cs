using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;
    public GameObject blackScreen;
    public GameObject bugz;

    protected float progress = 0;

    public static string sceneToLoad;
    public static Scene currentScene;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        blackScreen.GetComponent<Image>().color = new Color(0,0,0,0);
        Vector2 bugzPos = bugz.GetComponent<RectTransform>().anchoredPosition;
        bugz.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1000, bugzPos.y);
        TweenManager.AlphaTween(blackScreen, 0, 1, 0.5f, Eases.EaseInOutCubic, delegate () {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(currentScene);
        }).SetStartDelay(0.1f);
        TweenManager.XTween(bugz, -1000, -70, 0.5f, Eases.EaseOutSine, delegate () {

        }).SetStartDelay(0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void LoadScene(string scene)
    {
        sceneToLoad = scene;
        currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene("Scenes/LoadingScreen", LoadSceneMode.Additive);
    }

    public static void FadeOut(Action callback = null)
    {
        if (instance == null)
            return;

        TweenManager.AlphaTween(instance.blackScreen, 1, 0, 1f, Eases.EaseInOutCubic, delegate () {
            callback?.Invoke();
            FindObjectOfType<MonoBehaviour>().StartCoroutine(RemoveScene());
            IEnumerator RemoveScene()
            {
                yield return new WaitForSeconds(0.25f);
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                Destroy(instance.gameObject);
                Destroy(instance);
            }
        }).SetStartDelay(1f);
        TweenManager.XTween(instance.bugz, -70, 1000, 1f, Eases.EaseInCubic, delegate () {

        }).SetStartDelay(1f);

    }
}
