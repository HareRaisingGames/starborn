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

    public static Action callback;
    public static bool unloadAllScenes;
    readonly static float defaultTime = 0.25f;
    static float waitTime = 0.25f;
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
            if(unloadAllScenes)
                StartCoroutine(UnloadAllScenesCoroutine());
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive).completed += delegate (AsyncOperation op)
            {
                Camera.main.gameObject.SetActive(false);
                SceneManager.UnloadSceneAsync(currentScene);
                if (callback != null)
                {
                    callback.Invoke();
                    FadeOut(null);
                    callback = null;
                }
            };
        }).SetStartDelay(0.1f).SetIgnoreTimeScale();
        TweenManager.XTween(bugz, -1000, -70, 0.5f, Eases.EaseOutSine, delegate () {

        }).SetStartDelay(0.1f).SetIgnoreTimeScale();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void LoadScene(string scene, Action cback = null, float t = 0.25f, bool unloadScenes = true)
    {
        callback = cback;
        sceneToLoad = scene;
        currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene("Scenes/LoadingScreen", LoadSceneMode.Additive);
        waitTime = t;
        unloadAllScenes = unloadScenes;
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
                yield return new WaitForSeconds(waitTime);
                waitTime = defaultTime;
                //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                Destroy(instance.gameObject);
                Destroy(instance);
                instance = null;
            }
        }).SetStartDelay(0.5f).SetIgnoreTimeScale();
        TweenManager.XTween(instance.bugz, -70, 1000, 1f, Eases.EaseInCubic, delegate () {

        }).SetStartDelay(0.5f).SetIgnoreTimeScale();

    }

    private IEnumerator UnloadAllScenesCoroutine()
    {
        int sceneCount = SceneManager.sceneCount;
        // Iterate from the last scene (excluding the active scene if desired, or all)
        for (int i = sceneCount - 1; i >= 0; i--)
        {
            Scene sceneToUnload = SceneManager.GetSceneAt(i);

            //if (sceneToUnload == SceneManager.GetSceneByName(sceneToLoad))
                //continue;

            // You might want to add logic here to avoid unloading the 'main' or active scene
            // if it's not intended to be unloaded.
            if (sceneToUnload.isLoaded)
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToUnload);
                if (asyncUnload == null)
                    continue;
                while (!asyncUnload.isDone)
                {
                    yield return null;
                }
            }
        }
        // Optional: Call Resources.UnloadUnusedAssets to free up memory from assets no longer referenced.
        yield return Resources.UnloadUnusedAssets();
    }
}
