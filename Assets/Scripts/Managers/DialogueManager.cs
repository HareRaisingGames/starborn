using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using Rabbyte;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using System;
using System.Linq;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using Starborn;
using Rabbyte.Gyotoku;

public class DialogueManager : MonoBehaviour
{
    #region Dialogue Properties
    public static string filename;
    public static string sceneName;
    static Scene scene;
    SimpleSBDFile dialogueFile;
    int curLine;
    int dialogueLine;
    int jumpToLine;

    bool firstLineHasName;
    string startWithName;

    bool paused = false;

    bool interact = false;
    bool isGameOver;
    bool minigameNext;
    bool minigameIsFinal;

    public delegate void StartCallback();
    public delegate void EndCallback();

    public static string curMinigame;


    #endregion

    #region Dialogue Assets
    List<BetaDialogueSequence> curLines = new List<BetaDialogueSequence>();
    List<BetaDialogueSequence> lines = new List<BetaDialogueSequence>();
    List<BetaDialogueSequence> gameOverLines = new List<BetaDialogueSequence>();
    BetaDialogueSequence previousLine;
    public Dictionary<string, CharacterSprite> sprites = new Dictionary<string, CharacterSprite>();
    public Dictionary<string, GameObject> backgrounds = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> foregrounds = new Dictionary<string, GameObject>();
    public Dictionary<int, AudioClip> dialogueAudios = new Dictionary<int, AudioClip>();

    protected List<CharacterSprite> curSprites = new List<CharacterSprite>();
    protected List<CharacterSprite> prevSprites = new List<CharacterSprite>();
    #endregion

    #region Minigame Assets
    /// <summary>
    /// Steps for loading minigames
    /// 1. Using the dialogue line, find every minigame name in dialogue and filter them so that they're all unique
    /// 2. Preload all minigames avaiable into scene
    /// 3. Grab all of their root game object's active in hiearchy and store them
    /// 4. Set all game objects in other scenes to false
    /// </summary>
    /// 
    List<string> minigames = new List<string>();
    public Dictionary<string, Dictionary<GameObject, bool>> sceneVisibilities
        = new Dictionary<string, Dictionary<GameObject, bool>>();
    int minigameCount = 0;
    bool loadedMinigames = false;

    static DialogueManager instance;
    #endregion

    #region UI Properties
    public GameObject objectHolder;
    public GameObject backgroundsObject;
    public GameObject spritesObject;
    public GameObject foregroundsObject;
    public DialogueBox dialogueBox;
    private RectTransform dialogueBoxTransform;
    public GameObject nameObject; //Up = 115, Down = 65
    private RectTransform nameTransform;
    public TMP_Text nameTxt;
    public GameObject transitionCanvas;
    public GameObject transition;
    public GameObject fade;
    public GameObject flash;
    public GameObject loadingIcon;
    public GameObject nextButton;

    static TranscriptGroup transcript = new TranscriptGroup();

    UniversalAdditionalCameraData baseCameraData;
    UniversalAdditionalCameraData defaultBaseCameraData;
    LayerMask defaultMask;

    #endregion

    #region Audio
    public AudioSource musicSource;
    public AudioSource dialogueSource;
    #endregion

    private StarbornInputSystem m_inputSystem;

    private void Awake()
    {
        instance = this;
        m_inputSystem = new StarbornInputSystem();
        m_inputSystem.Dialogue.A.performed += onA;
        m_inputSystem.Dialogue.Pause.performed += OnPause;

        LuaMethods.SetInstance(this);
    }

    private void OnEnable()
    {
        m_inputSystem.Dialogue.Enable();
    }

    private void OnDisable()
    {
        m_inputSystem.Dialogue.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        Resources.Load<GameObject>("Prefabs/GameOver");
        Resources.Load<GameObject>("Prefabs/Pause Menu");

        scene = SceneManager.GetSceneByName("DialogueState");
        sceneName = scene.name;
        if (flash != null)
            flash.SetActive(false);

        filename = "dialogue_test";

        MixerSettings.SetAudioGroup(musicSource, "Music");
        MixerSettings.SetAudioGroup(dialogueSource, "Dialogue");

        baseCameraData = Camera.main.GetUniversalAdditionalCameraData();
        defaultBaseCameraData = baseCameraData;
        defaultMask = Camera.main.cullingMask;

        StartCoroutine(LoadStreamingAsset());
        IEnumerator LoadStreamingAsset()
        {
            var path = Path.Combine(Application.streamingAssetsPath, $"Dialogue/{filename}.sbd");

            GameObject prefab = Resources.Load<GameObject>("Prefabs/Pause Menu");
            if (prefab != null)
            {
                Instantiate(prefab, Vector3.zero, Quaternion.identity).name = "Pause";
            }
            StaticProperties.canPause = false;
            //fade.SetActive(true);
            dialogueBoxTransform = dialogueBox.GetComponent<RectTransform>();
            dialogueBox.gameObject.SetActive(false);

            nameTransform = nameObject.GetComponent<RectTransform>();
            UnityWebRequest www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Loaded streaming asset: " + www.downloadHandler.text);
                    //StarbornFileHandler.ExtractDialogue(www.downloadHandler.);
                }
                else
                {
                    Debug.LogError("Failed to load streaming asset: " + www.error);
                    yield break;
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    StarbornFileHandler.ExtractDialogue(path);
                }
                else
                {
                    yield break;
                }
            }
            dialogueFile = StarbornFileHandler.ReadSimpleDialogue(filename);
            UnpackDialogue(dialogueFile);

            LuaMethods.AddGlobal(musicGlobals);
            //backgroundGlobals.Add("curBG", GetImageFromBGName(dialogueFile.GetLines()[curLine].background));
            LuaMethods.AddGlobal("curBG", GetImageFromBGName(dialogueFile.GetLines()[curLine].background));
            LuaMethods.AddGlobal(backgroundGlobals);
            LuaMethods.AddGlobal("lastChar", dialogueFile.GetLines()[curLine].text[dialogueFile.GetLines()[curLine].text.Length - 1]);
            LuaMethods.AddGlobal("lastCharPos", dialogueFile.GetLines()[curLine].text.Length - 1);
            LuaMethods.AddGlobal(dialogueGlobals);
            LuaMethods.AddGlobal(tweenGlobals);

            LuaFunctions.dialogueFile = dialogueFile;

            if (backgrounds.Count != 0)
            {
                string curBG = dialogueFile.background;
                if (backgrounds.ContainsKey(curBG))
                    PutObjectInFront(backgrounds[curBG]);

                foreach (Transform bg in backgroundsObject.transform)
                {
                    if (backgrounds.ContainsKey(bg.gameObject.name))
                        bg.gameObject.SetActive(false);
                }

                backgrounds[curBG].SetActive(true);
            }

            if (sprites.Count != 0)
            {
                foreach (KeyValuePair<string, CharacterSprite> character in sprites)
                {
                    character.Value.gameObject.SetActive(false);
                }
            }

            if (nextButton != null) nextButton.SetActive(false);

            //TweenManager.AlphaTween(fade, 1, 1, 0.25f);
            //TweenManager.AlphaTween(fade, 1, 0, 2).SetStartDelay(0.5f);

            /*StartCoroutine(LoadMinigame());
            IEnumerator LoadMinigame()
            {
                yield return new WaitForSeconds(1f);
                TweenManager.XTween(transition, -800, 0, 2, Eases.EaseInOutCubic, () =>
                {
                    SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive).completed += GameOut;

                });

            }*/
            //fileText.text = File.Exists(path).ToString();
        }
    }

    async void UnpackDialogue(SimpleSBDFile dialogue)
    {
        //Get Backgrounds
        if (dialogue.GetBackgrounds().Count != 0)
            foreach (KeyValuePair<string, byte[]> background in dialogue.GetBackgrounds())
            {
                GameObject obj = new GameObject(background.Key);
                obj.AddComponent<Image>();
                obj.transform.parent = backgroundsObject.transform;
                obj.transform.localScale = Vector3.one;
                /*obj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                obj.GetComponent<RectTransform>().anchorMax = Vector2.one;
                obj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                obj.GetComponent<RectTransform>().offsetMax = Vector2.zero;*/

                obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                //obj.GetComponent<RectTransform>().offsetMin = new Vector2(140.00f, -192.50f);
                //obj.GetComponent<RectTransform>().offsetMax = new Vector2(380.00f, 152.50f);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(background.Value);
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                sprite.name = background.Key;
                obj.GetComponent<Image>().sprite = sprite;
                obj.tag = "Background";
                obj.layer = LayerMask.NameToLayer("UI");

                DialogueUtils.SetImageFixedPosition(obj.GetComponent<Image>());

                backgrounds.Add(background.Key, obj);
            }

        //Get Characters
        if (dialogue.GetCharacters().Count != 0)
            foreach (KeyValuePair<string, List<Emotion>> character in dialogue.GetCharacters())
            {
                SBCFile characterFile = new SBCFile(character.Key, true);
                foreach (Emotion emotion in character.Value)
                {
                    characterFile.addExpression(emotion.expression, emotion.sprite, emotion.scale, emotion.offset[0], emotion.offset[1]);
                }
                CharacterSprite characterSprite = new GameObject(character.Key).AddComponent<CharacterSprite>();
                characterSprite.transform.parent = spritesObject.transform;
                characterSprite.character = characterFile;
                characterSprite.rectTransform.anchoredPosition = Vector2.zero;
                characterSprite.rectTransform.localScale = Vector3.one;
                foreach(Emotion emotion in characterFile.expressions)
                {
                    characterSprite.expression = emotion.expression;
                }
                //characterSprite.gameObject.SetActive(false);

                sprites.Add(character.Key, characterSprite);
            }

        //Get Foregrounds
        if (dialogue.GetForegrounds().Count != 0)
            foreach (KeyValuePair<string, byte[]> foreground in dialogue.GetForegrounds())
            {
                GameObject obj = new GameObject(foreground.Key);
                obj.AddComponent<Image>();
                obj.transform.parent = foregroundsObject.transform;
                obj.transform.localScale = Vector3.one;
                obj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                obj.GetComponent<RectTransform>().anchorMax = Vector2.one;
                obj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                obj.GetComponent<RectTransform>().offsetMax = Vector2.zero;

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(foreground.Value);
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                sprite.name = foreground.Key;
                obj.GetComponent<Image>().sprite = sprite;

                foregrounds.Add(foreground.Key, obj);
            }

        //Get Music
        if (dialogue.music != null)
        {
            AudioClip clip = await AudioUtils.LoadMusic(dialogue.music);
            clip.name = dialogue.music.name;
            musicSource.clip = clip;
        }

        //Get Lines
        lines = dialogue.GetLines();

        if (lines.Count > 0)
        {
            firstLineHasName = lines[0].name != null && lines[0].name != "";
            startWithName = lines[0].name;
        }


        //Get Dialogue
        foreach (BetaDialogueSequence line in lines)
        {
            if (line.audio != null)
            {
                AudioClip clip = await AudioUtils.LoadDialogue(line.audio);
                clip.name = line.audio.name;

                dialogueAudios.Add(lines.IndexOf(line), clip);
            }

            if (line.minigame != null || line.minigame != "")
            {
                minigames.Add(line.minigame);
            }
        }

        LuaFunctions.OnLoad();

        minigames = minigames.Distinct().ToList();

        for (int i = minigames.Count - 1; i >= 0; i--)
        {
            string game = minigames[i];
            string scenePath = $"Scenes/Minigames/{game}";

            string overallScenePath = $"Assets/{scenePath}.unity";

            if (StaticProperties.DoesSceneExistInBuild(overallScenePath))
            {
                LoadScene(scenePath, game);
            }
            else
            {
                minigames.Remove(game);
            }
        }

        if (minigames.Count == 0)
            SceneFadeOut();

    }

    async void LoadScene(string path, string name)
    {
        var scene = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
        scene.completed += delegate (AsyncOperation op)
        {
            HideEverythingInScene(name);
        };
        scene.allowSceneActivation = false;
        //int i = 0;
        do
        {
            await Task.Delay(100);
            //Debug.Log(scene.progress);
        }
        while (scene.progress < 0.9f);

        //await Task.Delay(1000);
        scene.allowSceneActivation = true;
    }

    void HideEverythingInScene(string name)
    {
        Scene scene = SceneManager.GetSceneByName($"Scenes/Minigames/{name}");
        if (scene != null && scene.isLoaded && !sceneVisibilities.ContainsKey(name))
        {
            Dictionary<GameObject, bool> rootVisibilities = new Dictionary<GameObject, bool>();
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                rootVisibilities.Add(obj, obj.activeInHierarchy);
                //obj.SetActive(false);
            }

            sceneVisibilities.Add(name, rootVisibilities);
        }
        List<GameObject> keys = sceneVisibilities[name].Keys.ToList();

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (keys[i] == null)
            {
                sceneVisibilities[name].Remove(keys[i]);
                continue;
            }
            keys[i].SetActive(false);

        }

        minigameCount++;
        if (minigameCount >= minigames.Count && !loadedMinigames)
        {
            loadedMinigames = true;
            Debug.Log("Loaded!");
            SceneFadeOut();
        }
    }

    void SceneFadeOut()
    {
        if (loadingIcon != null) TweenManager.AlphaTween(loadingIcon, 1, 0, 0.5f, Eases.EaseInOutQuad);
        Time.timeScale = 1;
        if (LoadingManager.instance != null)
            LoadingManager.FadeOut(delegate ()
            {
                if (!isGameOver)
                    StaticProperties.canPause = true;
                dialogueBox.gameObject.SetActive(true);
                BoxTransition(true, StartDialogue);
            });
        else
            TweenManager.AlphaTween(fade, 1, 0, 2, Eases.Linear, delegate ()
            {

                if (!isGameOver)
                    StaticProperties.canPause = true;
                dialogueBox.gameObject.SetActive(true);
                BoxTransition(true, StartDialogue);
            }).SetStartDelay(1f);
    }

    public void GameOver()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        gameObject.SetActive(true);
        backgroundsObject.SetActive(true);
        GameObject baseBG = null;
        foreach (Transform child in backgroundsObject.transform)
        {
            if (child.tag == "Base") baseBG = child.gameObject;
            else child.gameObject.SetActive(false);
        }

        dialogueBox.text = "";
        dialogueBox.gameObject.SetActive(true);
        if (baseBG != null)
        {
            ColorUtils.SetAlpha(baseBG, 0);
            TweenManager.AlphaTween(baseBG, 0, 0.5f, 0.25f, Eases.EaseInOutCubic);
        }

        BoxTransition(true, GameOverDialogue);

    }

    public void FromGame()
    {
        if (transitionCanvas != null)
            transitionCanvas.SetActive(true);
        transition.SetActive(true);
        if (minigameNext)
        {
            curLine = jumpToLine;
            string nextMinigame = lines[jumpToLine].minigame;
            GameIn(nextMinigame);
        }
        else if (minigameIsFinal)
        {
            ExitDialogue();
        }
        else
        {
            TweenManager.XTween(transition, -800, 0, 2, Eases.EaseInOutCubic, () =>
            {
                baseCameraData.cameraStack.Clear();
                SetMainCameraRenderer(defaultBaseCameraData);
                Camera.main.cullingMask = defaultMask;
                HideEverythingInScene(SceneManager.GetActiveScene().name);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                gameObject.SetActive(true);
                backgroundsObject.SetActive(true);
                if (lines.Count > 0)
                {
                    firstLineHasName = lines[jumpToLine].name != null && lines[jumpToLine].name != "";
                    startWithName = lines[jumpToLine].name;
                }
                PutObjectInFront(backgrounds[lines[jumpToLine].background]);
                TweenManager.XTween(transition, 0, 800, 2, Eases.EaseInOutCubic, delegate ()
                {
                    BoxTransition(true, BackToDialogue);
                }).SetStartDelay(0.1f);
                //GameOut(minigame);
            });
        }
    }

    void GameIn(string minigame)
    {
        gameOverLines.Clear();
        StaticProperties.canPause = false;
        int line = curLine;
        interact = false;
        while (line < lines.Count && lines[line].minigame == minigame)
        {
            gameOverLines.Add(lines[line]);
            line++;
        }

        jumpToLine = line;
        minigameNext = line < lines.Count && lines[jumpToLine].minigame != null && lines[jumpToLine].minigame != "";
        minigameIsFinal = line >= lines.Count;
        curLine = 0;

        Countdown.folder = "base";
        Countdown.mode = CountdownMode.Default;
        Countdown.cam = null;

        GameOverMenu.tryAgainCallback = delegate () { TryAgain(delegate () { Destroy(gameObject); }); };

        TweenManager.XTween(transition, -800, 0, 2, Eases.EaseInOutCubic, () =>
        {
            GameOut(minigame);
        });
    }

    void GameOut(string name, bool trans = true)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            HideEverythingInScene(SceneManager.GetActiveScene().name);
        }

        if (gameOverLines.Count > 0)
        {
            firstLineHasName = gameOverLines[0].name != null && gameOverLines[0].name != "";
        }


        string game = $"Scenes/Minigames/{name}";

        if (trans)
            TweenManager.XTween(transition, 0, 800, 2, Eases.EaseInOutCubic, delegate ()
            {
                if (transitionCanvas != null)
                    transitionCanvas.SetActive(false);
                StaticProperties.canPause = true;
                Minigame.instance.SetUpSong();
                Minigame.instance.setUpSong = true;

            }).SetStartDelay(0.1f);
        else
        {
            StaticProperties.canPause = true;
        }

        if (SceneManager.GetSceneByName(game) == null)
            return;

        curMinigame = game;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(game));
        List<GameObject> importantComponents = new List<GameObject>();

        UniversalAdditionalCameraData cameraData = null;

        Scene targetScene = SceneManager.GetActiveScene();
        if (targetScene.isLoaded)
        {
            GameObject[] rootObjects = targetScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                bool on = sceneVisibilities[name][obj];
                obj.SetActive(on);

                Camera otherSceneCamera = obj.GetComponentInChildren<Camera>();
                EventSystem otherHandler = obj.GetComponentInChildren<EventSystem>();

                if (otherSceneCamera != null && otherSceneCamera.tag == "MainCamera")
                {
                    importantComponents.Add(otherSceneCamera.gameObject);
                    cameraData = otherSceneCamera.GetUniversalAdditionalCameraData();
                    Camera.main.cullingMask = otherSceneCamera.cullingMask;
                }

                if (otherHandler != null)
                    importantComponents.Add(otherHandler.gameObject);
            }
        }

        Minigame minigame = FindObjectOfType<Minigame>();

        Camera.main.orthographicSize = minigame.zoom;
        Camera.main.backgroundColor = minigame.bgColor;
        Camera.main.transform.position = minigame.camPosition;

        if (cameraData != null)
        {
            baseCameraData.cameraStack.AddRange(cameraData.cameraStack);
            SetMainCameraRenderer(cameraData);
        }

        //Debug.Log(baseCameraData.cameraStack.Count);

        foreach (GameObject obj in importantComponents)
            obj.SetActive(false);

        dialogueBox.gameObject.SetActive(false);
        backgroundsObject.SetActive(false);
        spritesObject.SetActive(false);
        foregroundsObject.SetActive(false);
        gameObject.SetActive(false);

        //if(Camera.main != null) Camera.main.gameObject.SetActive(false);
        //if(minigame.eventSystem != null) minigame.eventSystem.gameObject.SetActive(false);

        //Debug.Log(StaticProperties.GetAllScenes().Length);
    }
    // Update is called once per frame
    void Update()
    {

    }

    void BoxTransition(bool on = false, Action callback = null)
    {
        dialogueBox.text = "";
        dialogueBox.gameObject.SetActive(true);
        Vector2 bugzPos = dialogueBoxTransform.anchoredPosition;
        dialogueBoxTransform.anchoredPosition = new Vector2(bugzPos.x, on ? 25 : 75);
        TweenManager.YTween(dialogueBox.gameObject, on ? 25 : 75, on ? 75 : 25, 0.25f, Eases.EaseInOutCubic);
        foreach (Transform child in dialogueBox.transform)
        {
            if (child.gameObject.GetComponent<Image>() || child.gameObject.GetComponent<SpriteRenderer>())
            {
                ColorUtils.SetAlpha(child.gameObject, on ? 0 : 1);
                TweenManager.AlphaTween(child.gameObject, on ? 0 : 1, on ? 1 : 0, 0.25f, Eases.EaseInOutCubic, delegate ()
                {
                    callback?.Invoke();
                });
            }
        }

        Vector2 namePos = nameTransform.anchoredPosition;
        foreach (Transform child in nameObject.transform)
        {
            ColorUtils.SetAlpha(child.gameObject, 0);
        }

        if (firstLineHasName)
        {
            //nameTransform.anchoredPosition = new Vector2(namePos.x, 115);
            nameTxt.text = startWithName;
            foreach (Transform child in nameObject.transform)
            {
                if (child.gameObject.GetComponent<Image>()
                    || child.gameObject.GetComponent<SpriteRenderer>()
                    || child.gameObject.GetComponent<TMP_Text>())
                {
                    //Color color = child.GetComponent<Image>().color;
                    //child.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1);
                    TweenManager.AlphaTween(child.gameObject, on ? 0 : 1, on ? 1 : 0, 0.25f, Eases.EaseInOutCubic, delegate ()
                    {
                    });
                }
            }
        }
        else
        {
            //nameTransform.anchoredPosition = new Vector2(namePos.x, 65);
        }
    }
    void PlayDialogue()
    {
        interact = true;
        StartCoroutine(PlaySong());
        IEnumerator PlaySong()
        {
            yield return new WaitForSeconds(0.1f);
            MusicUtils.MusicFade(musicSource, 0.5f, 2f);
        }
        NextLine(curLine);
    }
    public void StartDialogue()
    {
        curLines = lines;
        curMinigame = "";
        returnFromMinigame = false;
        musicSource.volume = 0;
        musicSource.Play();
        PlayDialogue();
    }

    public void GameOverDialogue()
    {
        curLines = gameOverLines;
        isGameOver = true;
        returnFromMinigame = true;
        PlayDialogue();
    }

    bool returnFromMinigame;
    public void BackToDialogue()
    {
        curLines = lines;
        curLine = jumpToLine;
        dialogueLine = jumpToLine;
        curMinigame = "";
        returnFromMinigame = true;
        musicSource.volume = 0;
        musicSource.Play();
        PlayDialogue();
    }

    public void onA(InputAction.CallbackContext context)
    {
        if (paused)
        {

        }
        else
        if (interact)
        {
            if (dialogueBox != null)
            {
                if (dialogueBox.canInteract)
                {
                    NextLine(curLine);
                    if (dialogueBox.click != null) dialogueBox.click.Play();
                }
                else
                {
                    TweenManager.instance.RemoveAllActiveTweens();
                    dialogueBox.text = lines[curLine].text;
                    dialogueBox.onFinish?.Invoke(curLine);
                    dialogueBox.onFinish = null;
                }

            }
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (StaticProperties.canPause && !PauseMenu.pauseTrans)
        {
            if (paused)
            {
                if (PauseMenu.inMainMenu)
                    PauseMenu.Close(CloseCallback, true);
            }
            else
            {
                PauseMenu.Open(OpenCallback, CloseCallback, delegate ()
                {
                    StaticProperties.canPause = true;
                }, delegate ()
                {
                    paused = false;
                    StaticProperties.canPause = false;
                },
                delegate ()
                {
                    LoadingManager.LoadScene(SceneManager.GetActiveScene().name);
                    Destroy(PauseMenu.instance.gameObject);
                    Conductor.startSong = false;
                    //PauseMenu.instance.hasSelected = false;
                });
                PauseMenu.restartMessage = " this level";
            }
            paused = !paused;
            StaticProperties.canPause = false;
        }
    }

    void OpenCallback()
    {
        Time.timeScale = 0;
        foreach (KeyValuePair<string, ITween> tween in TweenManager.instance.activeTweens)
        {
            tween.Value.Pause();
        }

        if (dialogueSource.clip != null)
            dialogueSource.Pause();

        musicSource.Pause();
    }

    void CloseCallback()
    {
        StaticProperties.canPause = true;
        Time.timeScale = 1;
        foreach (KeyValuePair<string, ITween> tween in TweenManager.instance.activeTweens)
        {
            tween.Value.Resume();
        }
        if (dialogueSource.clip != null)
            dialogueSource.UnPause();

        musicSource.UnPause();
    }

    void NextLine(int line)
    {
        //Stop everything before moving onto the next line
        //LuaFunctions.dialogueFile = dialogueFile;
        if (nextButton != null) nextButton.SetActive(false);
        TweenManager.instance.RemoveTween("ping-pong");
        if (dialogueSource != null && dialogueSource.isPlaying)
            dialogueSource.Stop();
        dialogueSource.clip = null;

        minigameNext = false;

        if (line >= curLines.Count)
        {
            interact = false;
            //close the box or call a game over if it's under a game over
            if (!isGameOver)
            {
                foreach (Transform child in dialogueBox.transform)
                {
                    if (child.gameObject.GetComponent<Image>()
                        || child.gameObject.GetComponent<SpriteRenderer>()
                        || child.gameObject.GetComponent<TMP_Text>())
                    {
                        //Color color = child.GetComponent<Image>().color;
                        //child.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1);
                        TweenManager.AlphaTween(child.gameObject, 1, 0, 0.25f, Eases.EaseInOutCubic, delegate ()
                        {
                            ExitDialogue();
                        }).SetStartDelay(0.125f);
                    }

                }
            }
            else
            {
                Minigame.gotGameOver = true;
                RestartGame();
            }
            return;
        }

        spritesObject.SetActive(true);
        if (curLines[line].minigame != null && curLines[line].minigame != "" && !isGameOver)
        {
            MusicUtils.MusicFadeOut(musicSource, 0.5f, delegate () { musicSource.Pause(); });
            GameIn(lines[line].minigame);
            MinigameManager.returnCallback = FromGame;
            previousLine = null;
            return;
        }

        dialogueBox.t = line;
        dialogueBox.onFinish = delegate (int t)
        {
            if (!isGameOver)
                if (curLines[line].name != null)
                    transcript.Add(curLines[line].name, curLines[line].text);
                else
                    transcript.Add(curLines[line].text);

            curLine++;
            dialogueLine++;
            LuaFunctions.OnLineEnd(line);
            if (nextButton != null)
            {
                nextButton.SetActive(true);
                ColorUtils.SetAlpha(nextButton, 0);
                TweenManager.AlphaTween(nextButton, 0, 1, 0.5f, Eases.EaseInOutCubic, delegate ()
                {
                    TweenManager.AlphaTween(nextButton, 1, 0.75f, 0.5f, Eases.Linear, null, "ping-pong").SetPingPong(0);
                });
            }
        };
        dialogueBox.onStart = LuaFunctions.OnLineStart;
        dialogueBox.onChar = LuaFunctions.OnLineInterval;

        dialogueBox.typedText = curLines[line].text;
        //sprites.Add(character.Key, characterSprite);
        LoadCharacters(curLines[line].characters, previousLine != null ? previousLine.characters : null);

        if (previousLine != null)
        {
            if (previousLine.name == null || previousLine.name == "")
            {
                if (curLines[line].name != null && curLines[line].name != "")
                {
                    //TweenManager.YTween(nameObject, -115, 0, 0.25f, Eases.EaseInOutCubic);
                    foreach (Transform child in nameObject.transform)
                    {
                        TweenManager.AlphaTween(child.gameObject, 0, 1, 0.25f, Eases.EaseInOutCubic);
                    }
                }
            }
            else
            {
                if (curLines[line].name == null || curLines[line].name == "")
                {
                    //TweenManager.YTween(nameObject, 0, -115, 0.25f, Eases.EaseInOutCubic);
                    foreach (Transform child in nameObject.transform)
                    {
                        TweenManager.AlphaTween(child.gameObject, 1, 0, 0.25f, Eases.EaseInOutCubic);
                    }
                }
            }
        }
        //Debug.Log(previousLine);

        if (curLines[line].name != null && curLines[line].name != "")
            nameTxt.text = curLines[line].name;

        if (!isGameOver)
        {
            if (previousLine != null)
            {
                if (previousLine.background != curLines[line].background)
                {
                    GameObject bg = backgrounds[curLines[line].background];
                    PutObjectInFront(bg);
                    bg.SetActive(true);
                    TweenManager.AlphaTween(bg, 0, 1, 0.5f, Eases.Linear, delegate ()
                    {
                        foreach (KeyValuePair<string, GameObject> background in backgrounds)
                        {
                            if (background.Value != bg)
                                background.Value.SetActive(false);
                        }
                    });
                }
            }
        }

        //curLines[line].characters;

        if (dialogueAudios.ContainsKey(dialogueLine))
            dialogueSource.clip = dialogueAudios[dialogueLine];
        dialogueSource.Play();

        minigameNext = line + 1 < lines.Count && lines[line + 1].minigame != null && lines[line + 1].minigame != "";

        previousLine = lines[line];

        returnFromMinigame = false;
    }

    public void LoadCharacters(List<CharacterPack> curPack, List<CharacterPack> prevPack = null, int index = 0)
    {
        //Grab characters from list
        curSprites.Clear();
        foreach (CharacterPack pack in curPack)
        {
            if (sprites.ContainsKey(pack.character))
                curSprites.Add(sprites[pack.character]);
        }

        List<CharacterSprite> charactersInOnlyCurScene = new List<CharacterSprite>();
        List<CharacterSprite> charactersInOnlyPrevScene = new List<CharacterSprite>();
        List<CharacterSprite> charactersInBoth = new List<CharacterSprite>();

        if (prevPack != null && prevPack.Count != 0)
        {
            //For characters transitioning out
            foreach (CharacterSprite charact in curSprites)
            {
                if (!prevSprites.Contains(charact))
                    charactersInOnlyCurScene.Add(charact);
                else
                    charactersInBoth.Add(charact);
            }
            //For characters transitioning out
            foreach (CharacterSprite charact in prevSprites)
            {
                if (!curSprites.Contains(charact))
                    charactersInOnlyPrevScene.Add(charact);
            }
        }
        else
        {
            //If there isn't a previous pack, then all characters in that line get added on
            charactersInOnlyCurScene.AddRange(curSprites);
        }

        //Check if character is contained in both curPack and prevPack
        //Check if character is contained in curPack but not prevPack or is the start of the dialogue, TransIn
        //Check if character is contained in prevPack but not curPack or is end of dialogue, TransOut

        foreach (KeyValuePair<string, ITween> tween in TweenManager.instance.activeTweens)
        {
            tween.Value.FullKill();
        }

        if (!returnFromMinigame)
        {
            //Characters fading out
            foreach (CharacterSprite character in charactersInOnlyPrevScene)
            {
                TransitionSprite(character, prevPack[charactersInOnlyPrevScene.IndexOf(character)]);
            }

            //Characters fading in
            foreach (CharacterSprite character in charactersInOnlyCurScene)
            {
                TransitionSprite(character, curPack[charactersInOnlyCurScene.IndexOf(character)], true);
            }

            foreach (CharacterSprite character in charactersInBoth)
            {
                TweenManager.instance.RemoveTween(character.charName + "alpha");
                TweenManager.instance.RemoveTween(character.charName + "x");
                TweenManager.instance.RemoveTween(character.charName + "y");
                character.isMoving = true;
                MoveCharacter(character, curPack[charactersInBoth.IndexOf(character)]);
            }
        }
        else
        {
            foreach (CharacterSprite character in curSprites)
            {
                foreach (CharacterPack pack in curPack)
                {
                    if (character.charName == pack.character)
                    {
                        character.offsetX = pack.offset;
                        Alignment align = pack.alignment;
                        float xPos = 0;
                        switch (align)
                        {
                            case Alignment.left:
                                xPos = -325;
                                break;
                            case Alignment.right:
                                xPos = 325;
                                break;
                            default:
                                xPos = 0;
                                break;
                        }
                        character.position = new Vector2(xPos, -50);
                        TweenManager.NumTween(() => character.position.y, (value) => { character.position = new Vector2(xPos, value); }, 0, 0.25f, Eases.EaseInOutCubic);
                        ColorUtils.SetAlpha(character.gameObject, 0);
                        TweenManager.AlphaTween(character.gameObject, 0, 1, 0.25f, Eases.EaseInOutCubic);
                        break;
                    }
                }
            }
        }


        //Set up sprites for each character
        foreach (CharacterSprite character in curSprites)
        {
            foreach (CharacterPack pack in curPack)
            {
                if (character.charName == pack.character)
                {
                    character.gameObject.SetActive(true);
                    if (!character.isMoving) character.flipX = pack.flipX;
                    character.expression = pack.emotion;
                    //Alignment align = pack.alignment;
                    break;
                }
            }



        }

        prevSprites.Clear();
        prevSprites.AddRange(curSprites);
    }

    void TransitionSprite(CharacterSprite character, CharacterPack pack, bool transIn = false)
    {
        List<SpriteTransition> fade = new List<SpriteTransition>() { SpriteTransition.Fade, SpriteTransition.FadeLeft, SpriteTransition.FadeRight, SpriteTransition.FadeVertical };
        List<SpriteTransition> left = new List<SpriteTransition>() { SpriteTransition.Left, SpriteTransition.FadeLeft };
        List<SpriteTransition> right = new List<SpriteTransition>() { SpriteTransition.Right, SpriteTransition.FadeRight };
        List<SpriteTransition> vertical = new List<SpriteTransition>() { SpriteTransition.Vertical, SpriteTransition.FadeVertical };

        Alignment align = pack.alignment;
        float xPos = 0;
        switch (align)
        {
            case Alignment.left:
                xPos = -325;
                break;
            case Alignment.right:
                xPos = 325;
                break;
            default:
                xPos = 0;
                break;
        }

        if (transIn) //Transition in
        {
            if (fade.Contains(pack.transition))
            {
                ColorUtils.SetAlpha(character.gameObject, 0);
                TweenManager.AlphaTween(character.gameObject, 0, 1, transTime, Eases.EaseInOutCubic, null, pack.character + "alpha").SetIgnoreTimeScale();
            }

            character.offsetX = pack.offset;

            if (left.Contains(pack.transition))
            {
                float x = -400 - character.rectTransform.sizeDelta.x / 2;
                character.position = new Vector2(x, 0);
                TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(value, 0); }, xPos, transTime, Eases.EaseInOutCubic, null, pack.character + "x");
            }
            else if (right.Contains(pack.transition))
            {
                float x = 400 + character.rectTransform.sizeDelta.x / 2;
                character.position = new Vector2(x, 0);
                TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(value, 0); }, xPos, transTime, Eases.EaseInOutCubic, null, pack.character + "x");
            }
            else if (vertical.Contains(pack.transition))
            {
                float y = -450 - character.rectTransform.sizeDelta.y / 2;
                character.position = new Vector2(xPos, y);
                TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(xPos, value); }, 0, transTime, Eases.EaseInOutCubic, null, pack.character + "y");
            }
            else
            {
                character.position = new Vector2(xPos, 0);
            }
        }
        else
        {
            if (fade.Contains(pack.transition))
            {
                ColorUtils.SetAlpha(character.gameObject, 1);
                TweenManager.AlphaTween(character.gameObject, 1, 0, transTime, Eases.EaseInOutCubic, null, pack.character + "alpha");
            }

            if (left.Contains(pack.transition))
            {
                float x = -400 - character.rectTransform.sizeDelta.x / 2;
                character.position = new Vector2(xPos, 0);

                TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(value, 0); }, x, transTime, Eases.EaseInOutCubic, delegate () { character.gameObject.SetActive(false); }, pack.character + "x");
            }
            else if (right.Contains(pack.transition))
            {
                float x = 400 + character.rectTransform.sizeDelta.x / 2;
                character.position = new Vector2(xPos, 0);

                TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(value, 0); }, x, transTime, Eases.EaseInOutCubic, delegate () { character.gameObject.SetActive(false); }, pack.character + "x");
            }
            else if (vertical.Contains(pack.transition))
            {
                float y = -450 - character.rectTransform.sizeDelta.y / 2;
                character.position = new Vector2(xPos, 0);
                TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(xPos, value); }, y, transTime, Eases.EaseInOutCubic, delegate () { character.gameObject.SetActive(false); }, pack.character + "y");
            }
            else
            {
                character.position = new Vector2(xPos, 0);
                character.gameObject.SetActive(false);
            }
        }
    }

    float transTime = 1f;
    void MoveCharacter(CharacterSprite character, CharacterPack pack)
    {
        //Vector2
        float xPos = 0;
        Alignment align = pack.alignment;
        switch (align)
        {
            case Alignment.left:
                xPos = -325;
                break;
            case Alignment.right:
                xPos = 325;
                break;
            default:
                xPos = 0;
                break;
        }
        ColorUtils.SetAlpha(character.gameObject, 1);
        character.offsetX = pack.offset;
        TweenManager.NumTween(() => character.position.x, (value) => { character.position = new Vector2(value, 0); }, xPos, transTime * 1.5f, Eases.EaseOutQuart, delegate () { character.flipX = pack.flipX; character.isMoving = false; }, pack.character + "x")
            .SetOnPercentCompleted(0.25f, delegate ()
            {
                character.flipX = pack.flipX;
                character.isMoving = false;
            });
    }
    void RestartGame()
    {
        //curMinigame
        MinigameManager.Clear();
        if (transitionCanvas != null) transitionCanvas.SetActive(true);
        if (fade != null)
        {
            fade.SetActive(true);
            ColorUtils.SetAlpha(fade, 0);
        }
        Instantiate(Resources.Load<GameObject>("Prefabs/GameOver"), Vector3.zero, Quaternion.identity);


    }

    public void TryAgain(Action between = null)
    {
        TweenManager.AlphaTween(fade, 0, 1, 0.5f, Eases.Linear, delegate ()
        {
            Scene latestMinigame = SceneManager.GetSceneByName(curMinigame);

            //Perhaps in the near future, I'll figure out how to reset the minigames from their respected scene instead of just reloading the scenes entirely
            SceneManager.UnloadSceneAsync(latestMinigame).completed += delegate (AsyncOperation async)
            {
                //Reset dialogue to beginning of game over
                //Reload level scene
                //Remove the previous scene from all the scene visibility dictionary and add it back in
                string minigame = curMinigame.Replace("Scenes/Minigames/", "");
                curLine = 0;
                dialogueLine -= curLines.Count;
                SceneManager.LoadSceneAsync(curMinigame, LoadSceneMode.Additive).completed += delegate (AsyncOperation async)
                {
                    between?.Invoke();
                    TweenManager.AlphaTween(fade, 1, 0, 1f, Eases.Linear, delegate ()
                    {
                        if (transitionCanvas != null) transitionCanvas.SetActive(false);
                    }).SetStartDelay(0.25f).SetIgnoreTimeScale();
                    Debug.Log(sceneVisibilities.ContainsKey(minigame));
                    sceneVisibilities.Remove(minigame);
                    HideEverythingInScene(minigame);
                    GameOut(minigame, false);
                };
            };
        }).SetIgnoreTimeScale();
    }

    bool isExiting;
    public void ExitDialogue()
    {
        if (!isExiting)
        {
            if (fade != null)
            {
                fade.SetActive(true);
                ColorUtils.SetAlpha(fade, 0);
            }
            transcript.Clear();
            MusicUtils.MusicFadeOut(musicSource, 0.25f, delegate () { musicSource.Stop(); });
            if (FindObjectOfType<PauseMenu>(true) != null)
                Destroy(FindObjectOfType<PauseMenu>(true).gameObject);
            Countdown.folder = "base";
            Countdown.mode = CountdownMode.Default;
            Countdown.cam = null;
            LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { Time.timeScale = 1; TitleState.canInteract = true; }, 0.1f);
            isExiting = true;

            /*TweenManager.AlphaTween(fade, 0, 1, 1, Eases.EaseInOutCubic, delegate() {
                if (FindObjectOfType<PauseMenu>(true) != null)
                    Destroy(FindObjectOfType<PauseMenu>(true).gameObject);
                
                //SceneManager.LoadScene("TitleScreen");
            }).SetStartDelay(0.25f);*/
        }

    }

    public void FinishDialogue()
    {
        if (minigames.Count == 0)
            ExitDialogue();
        else
        {
            if (fade != null)
            {
                fade.SetActive(true);
                ColorUtils.SetAlpha(fade, 0);
                TweenManager.AlphaTween(fade, 0, 1, 2f, Eases.Linear, delegate ()
                {

                });
            }
        }

    }

    #region Utils
    public void PutObjectInFront(GameObject obj)
    {
        if (obj.transform.parent != null)
        {
            Transform parent = obj.transform.parent;
            Vector3 position = obj.transform.position;
            obj.transform.parent = null;
            obj.transform.parent = parent;
            obj.transform.position = position;
        }
    }

    public void SetMainCameraRenderer(UniversalAdditionalCameraData data)
    {
        baseCameraData.renderPostProcessing = data.renderPostProcessing;
        baseCameraData.renderShadows = data.renderShadows;
        baseCameraData.antialiasing = data.antialiasing;
        baseCameraData.dithering = data.dithering;
    }
    #endregion

    #region Dialogue Lua
    public Dictionary<string, dynamic> dialogueGlobals = new Dictionary<string, dynamic>()
    {
        { "SetDelayTime", (Action<float>)SetPauseTime },
        { "Flash", (Action<float, string>)Flash },
        { "Shake", (Action<float, float>)Shake }
    };

    public static void SetPauseTime(float time)
    {
        if (instance.dialogueBox != null)
            instance.dialogueBox.time = time;
    }

    static void Flash(float duration = 1f, string color = "white")
    {
        if (duration <= 0) return;

        if (instance.flash != null)
        {
            instance.flash.SetActive(true);
            ColorUtils.SetAlpha(instance.flash, 1);
            ColorUtils.SetColorByString(instance.flash, color);
            TweenManager.AlphaTween(instance.flash, 1, 0, duration, Eases.Linear, delegate ()
            {
                instance.flash.SetActive(false);
            });
        }
    }

    static void Shake(float duration, float magnitude)
    {
        if (instance.objectHolder != null)
            LuaMethods.ShakeScreen(duration, magnitude, instance.objectHolder);
    }
    #endregion

    #region Music Lua
    public Dictionary<string, dynamic> musicGlobals = new Dictionary<string, dynamic>()
    {
        { "StopMusic", (Action)StopMusic },
        { "PauseMusic", (Action)PauseMusic },
        { "FadeMusic", (Action<float, float>)FadeMusic }
    };

    static void PauseMusic()
    {
        if (instance.musicSource.isPlaying)
            instance.musicSource.Pause();
        else
            instance.musicSource.UnPause();
    }

    static void StopMusic()
    {
        instance.musicSource.Stop();
    }

    static void FadeMusic(float finish, float duration)
    {
        MusicUtils.MusicFade(instance.musicSource, finish, duration);
    }

    static void PlayMusic(string track)
    {
        FindObjectOfType<MonoBehaviour>().StartCoroutine(PlaySong());
        IEnumerator PlaySong()
        {
            yield return new WaitForSeconds(0.1f);
            MusicUtils.MusicFadeIn(instance.musicSource, 0.5f);
        }
    }

    //Will add a function for multiple songs
    #endregion

    #region Background Lua
    public Dictionary<string, dynamic> backgroundGlobals = new Dictionary<string, dynamic>()
    {
        { "GetBackgroundByName", (Func<string, Image>)GetImageFromBGName},
        { "SetBackgroundAlpha", (Action<string, float>)SetBGAlpha }
    };

    static Image GetImageFromBGName(string name)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Background"))
        {
            if (obj.GetComponent<Image>() != null && obj.name == name)
                return obj.GetComponent<Image>();
        }
        return null;
    }

    static void SetBGAlpha(string name, float value)
    {
        value = Mathf.Clamp01(value);
        ColorUtils.SetAlpha(GetImageFromBGName(name).gameObject, value);
    }
    #endregion

    #region Tweens Lua
    public Dictionary<string, dynamic> tweenGlobals = new Dictionary<string, dynamic>()
    {
        { "DoXTween", (Action<string, string, float, float, string>)DoXTween },
        { "DoYTween", (Action<string, string, float, float, string>)DoYTween },
        { "DoAngleTween", (Action<string, string, float, float, string>)DoAngleTween },
        { "DoScaleTween", (Action<string, string, float, float, string>)DoScaleTween },
        { "DoScale2Tween", (Action<string, string, float, float, float, string>)DoScaleTween },
        { "DoScale3Tween", (Action<string, string, float, float, float, float, string>)DoScaleTween },
        { "DoAlphaTween", (Action<string, string, float, float, string>)DoAlphaTween },
        { "DoColorTween", (Action<string, string, string, float, string>)DoColorTween },

        //Utils
        { "SetTweenLoop", (Action<string, int>)SetLoop },
        { "SetTweenPingPong", (Action<string, int>)SetPingPong },
        { "StopTween", (Action<string>)StopTween},
        { "PauseTween", (Action<string>)PauseTween}
    };

    private static GameObject GetObject(string name)
    {
        Image image = GetImageFromBGName(name);
        if (image != null)
            return image.gameObject;

        if (LuaMethods.GetProperty(name) != null)
            return LuaMethods.GetProperty(name).gameObject;


        return GameObject.Find(name);
    }

    static void DoXTween(string obj, string id, float value, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        if (gameObj == null)
            return;

        dynamic tranformation;
        bool is2D = false;
        if (gameObj.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObj.GetComponent<RectTransform>();
            is2D = true;
        }
        else
            tranformation = gameObj.GetComponent<Transform>();

        TweenManager.XTween(gameObj,
        is2D ? tranformation.anchoredPosition.x : tranformation.position.x,
        value, duration, Tween<float>.GetEaseFromString(ease), null, id, true);


    }
    static void DoYTween(string obj, string id, float value, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        if (gameObj == null)
            return;

        dynamic tranformation;
        bool is2D = false;
        if (gameObj.GetComponent<RectTransform>() != null)
        {
            tranformation = gameObj.GetComponent<RectTransform>();
            is2D = true;
        }
        else
            tranformation = gameObj.GetComponent<Transform>();

        TweenManager.YTween(gameObj,
        is2D ? tranformation.anchoredPosition.y : tranformation.position.y,
        value, duration, Tween<float>.GetEaseFromString(ease), null, id, true);


    }

    static void DoAngleTween(string obj, string id, float value, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        if (gameObj == null)
            return;

        TweenManager.AngleTween(gameObj, gameObj.transform.eulerAngles.z, value, duration, Tween<float>.GetEaseFromString(ease), null, id, true);
    }
    static void DoAlphaTween(string obj, string id, float value, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        dynamic spriteRenderer = null;
        if (gameObj.GetComponent<Image>() != null)
            spriteRenderer = gameObj.GetComponent<Image>();
        else if (gameObj.GetComponent<TMP_Text>() != null)
            spriteRenderer = gameObj.GetComponent<TMP_Text>();
        else if (gameObj.GetComponent<SpriteRenderer>() != null)
            spriteRenderer = gameObj.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) return;

        TweenManager.AlphaTween(gameObj, spriteRenderer.color.a, value, duration, Tween<float>.GetEaseFromString(ease), null, id, true);
    }

    static void DoColorTween(string obj, string id, string value, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        dynamic spriteRenderer = null;
        if (gameObj.GetComponent<Image>() != null)
            spriteRenderer = gameObj.GetComponent<Image>();
        else if (gameObj.GetComponent<TMP_Text>() != null)
            spriteRenderer = gameObj.GetComponent<TMP_Text>();
        else if (gameObj.GetComponent<SpriteRenderer>() != null)
            spriteRenderer = gameObj.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) return;

        TweenManager.ColorTween(gameObj, spriteRenderer.color, LuaMethods.GetColorByString(value), duration, Tween<Color>.GetEaseFromString(ease), null, id, true);
    }

    static void DoScaleTween(string obj, string id, float value, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        if (gameObj == null)
            return;

        Vector3 scale = Vector3.one * value;
        TweenManager.ScaleTween(gameObj, gameObj.transform.localScale, scale, duration, Tween<float>.GetEaseFromString(ease), null, id, true);
    }

    static void DoScaleTween(string obj, string id, float x, float y, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        if (gameObj == null)
            return;

        Vector3 scale = new Vector3(x, y, 1);
        TweenManager.ScaleTween(gameObj, gameObj.transform.localScale, scale, duration, Tween<float>.GetEaseFromString(ease), null, id, true);
    }

    static void DoScaleTween(string obj, string id, float x, float y, float z, float duration, string ease = "linear")
    {
        GameObject gameObj = GetObject(obj);

        if (gameObj == null)
            return;

        Vector3 scale = new Vector3(x, y, z);
        TweenManager.ScaleTween(gameObj, gameObj.transform.localScale, scale, duration, Tween<float>.GetEaseFromString(ease), null, id, true);
    }

    #region Tween Utils
    private static void SetLoop(string id, int loop = 1)
    {
        if (TweenManager.instance.luaTweens.ContainsKey(id))
            TweenManager.instance.luaTweens[id].Loop(loop);
    }

    private static void SetPingPong(string id, int loop = 1)
    {
        if (TweenManager.instance.luaTweens.ContainsKey(id))
            TweenManager.instance.luaTweens[id].PingPong(loop);
    }

    private static void StopTween(string id)
    {
        TweenManager.instance.RemoveLuaTween(id);
    }

    private static void PauseTween(string id)
    {
        TweenManager.instance.PauseLuaTween(id);
    }
    #endregion

    #endregion

    #region Structs
    public struct Transcript
    {
        private string name;
        private string dialogue;

        public Transcript(string name, string dialogue)
        {
            this.name = name;
            this.dialogue = dialogue;
        }

        public Transcript(string dialogue)
        {
            name = "";
            this.dialogue = dialogue;
        }

        public override string ToString()
        {
            if (name != null && name != "")
                return $"{name}:\n  {dialogue}";

            return $"  {dialogue}";
        }
    }

    public class TranscriptGroup
    {
        protected List<Transcript> transcript = new List<Transcript>();

        public void Add(string name, string description) => transcript.Add(new Transcript(name, description));
        public void Add(string description) => transcript.Add(new Transcript(description));

        public void Clear() => transcript.Clear();

        public override string ToString()
        {
            string fullTranscript = "";
            foreach(Transcript line in transcript)
            {
                fullTranscript += line;
                if (transcript.IndexOf(line) < transcript.Count - 1)
                    fullTranscript += "\n";
            }
            return fullTranscript;
        }
    }
    #endregion
}
