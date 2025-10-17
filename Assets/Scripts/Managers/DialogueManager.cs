using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using Rabbyte;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using TMPro;
using System.Threading.Tasks;

public class DialogueManager : MonoBehaviour
{
    #region Dialogue Properties
    public static string filename;
    SimpleSBDFile dialogueFile;
    int curLine;
    int jumpToLine;

    public delegate void StartCallback();
    public delegate void EndCallback();

    #endregion

    #region Dialogue Assets
    List<BetaDialogueSequence> lines = new List<BetaDialogueSequence>();
    List<BetaDialogueSequence> gameOverLines = new List<BetaDialogueSequence>();
    public Dictionary<string, CharacterSprite> sprites = new Dictionary<string, CharacterSprite>();
    public Dictionary<string, GameObject> backgrounds = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> foregrounds = new Dictionary<string, GameObject>();
    public Dictionary<int, AudioClip> dialogueAudios = new Dictionary<int, AudioClip>();
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
    List<string> minigames = new List<string>() { "Tosstail" };
    public Dictionary<string, Dictionary<GameObject, bool>> sceneVisibilities 
        = new Dictionary<string, Dictionary<GameObject, bool>>();
    int minigameCount = 0;
    bool loadedMinigames = false;
    #endregion

    #region UI Properties
    public GameObject backgroundsObject;
    public GameObject spritesObject;
    public GameObject foregroundsObject;
    public GameObject dialogueBox;
    public AudioSource dialogueSource;
    public AudioSource musicSource;
    public GameObject transition;
    public GameObject fade;
    #endregion

    #region Audio
    Dictionary<int, AudioClip> dialogueClips = new Dictionary<int, AudioClip>();
    #endregion

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        var path = Path.Combine(Application.streamingAssetsPath, $"Dialogue/{filename}.sbd");
        if(File.Exists(path))
        {
            StarbornFileHandler.ExtractDialogue(path);
            dialogueFile = StarbornFileHandler.ReadSimpleDialogue(filename);
            UnpackDialogue(dialogueFile);

            if(backgrounds.Count != 0)
            {
                string curBG = dialogueFile.background;
                if (backgrounds.ContainsKey(curBG))
                    PutObjectInFront(backgrounds[curBG]);

                foreach(Transform bg in backgroundsObject.transform)
                {
                    if(backgrounds.ContainsKey(bg.gameObject.name))
                        bg.gameObject.SetActive(false);
                }

                backgrounds[curBG].SetActive(true);
            }
            //TweenManager.AlphaTween(fade, 1, 1, 0.25f);
            //TweenManager.AlphaTween(fade, 1, 0, 2).SetStartDelay(0.5f);
        }

        /*StartCoroutine(LoadMinigame());
        IEnumerator LoadMinigame()
        {
            yield return new WaitForSeconds(1f);
            TweenManager.XTween(transition, -800, 0, 2, Eases.EaseInOutCubic, () =>
            {
                SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive).completed += TransOut;

            });

        }*/
        //fileText.text = File.Exists(path).ToString();
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
                obj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                obj.GetComponent<RectTransform>().anchorMax = Vector2.one;
                obj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                obj.GetComponent<RectTransform>().offsetMax = Vector2.zero;

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(background.Value);
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                sprite.name = background.Key;
                obj.GetComponent<Image>().sprite = sprite;

                backgrounds.Add(background.Key, obj);
            }

        //Get Characters
        if(dialogue.GetCharacters().Count != 0)
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

        //Get Dialogue
        foreach (BetaDialogueSequence line in lines)
        {
            if (line.audio != null)
            {
                AudioClip clip = await AudioUtils.LoadDialogue(line.audio);
                clip.name = line.audio.name;

                dialogueAudios.Add(lines.IndexOf(line), clip);
            }

            if(line.minigame != null || line.minigame != "")
            {
                minigames.Add(line.minigame);
            }
        }

        minigames = minigames.Distinct().ToList();

        for (int i = minigames.Count - 1; i >= 0; i--)
        {
            string game = minigames[i];
            string scenePath = $"Scenes/Minigames/{game}";

            string overallScenePath = $"Assets/{scenePath}.unity";

            Debug.Log(StaticProperties.DoesSceneExistInBuild(overallScenePath));
            if (StaticProperties.DoesSceneExistInBuild(overallScenePath))
            {
                SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive).completed += delegate (AsyncOperation op) {
                    HideEverythingInScene(game);
                };
            }
            else
            {
                minigames.Remove(game);
            }
        }
    }

    void HideEverythingInScene(string name)
    {
        Scene scene = SceneManager.GetSceneByName($"Scenes/Minigames/{name}");
        if(scene != null && scene.isLoaded && !sceneVisibilities.ContainsKey(name))
        {
            Dictionary<GameObject, bool> rootVisibilities = new Dictionary<GameObject, bool>();
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                rootVisibilities.Add(obj, obj.activeInHierarchy);
                obj.SetActive(false);
            }

            sceneVisibilities.Add(name, rootVisibilities);
        }
        minigameCount++;
        if(minigameCount >= minigames.Count && !loadedMinigames)
        {
            loadedMinigames = true;
            //TransIn();
        }
    }

    void TransIn()
    {
        TweenManager.XTween(transition, -800, 0, 2, Eases.EaseInOutCubic, () =>
        {
            TransOut("Tosstail");
        });
    }

    void TransOut(string name)
    {
        string game = $"Scenes/Minigames/{name}";
        TweenManager.XTween(transition, 0, 800, 2, Eases.EaseInOutCubic);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(game));
        List<GameObject> importantComponents = new List<GameObject>();

        Scene targetScene = SceneManager.GetActiveScene();
        if(targetScene.isLoaded)
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
                }

                if (otherHandler != null)
                    importantComponents.Add(otherHandler.gameObject);
            }
        }

        Minigame minigame = FindObjectOfType<Minigame>();

        Camera.main.orthographicSize = minigame.zoom;
        Camera.main.backgroundColor = minigame.bgColor;
        Camera.main.transform.position = minigame.camPosition;

        foreach (GameObject obj in importantComponents)
            obj.SetActive(false);

        dialogueBox.SetActive(false);
        backgroundsObject.SetActive(false);
        spritesObject.SetActive(false);
        foregroundsObject.SetActive(false);

        //if(Camera.main != null) Camera.main.gameObject.SetActive(false);
        //if(minigame.eventSystem != null) minigame.eventSystem.gameObject.SetActive(false);

        //Debug.Log(StaticProperties.GetAllScenes().Length);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartDialogue()
    {

    }

    void NextLine(int line)
    {

    }

    #region Utils
    public void PutObjectInFront(GameObject obj)
    {
        if(obj.transform.parent != null)
        {
            Transform parent = obj.transform.parent;
            Vector3 position = obj.transform.position;
            obj.transform.parent = null;
            obj.transform.parent = parent;
            obj.transform.position = position;
        }
    }
    #endregion
}
