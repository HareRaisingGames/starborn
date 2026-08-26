using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleState : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject title;
    public GameObject mainMenu;
    public GameObject gameSelection;
    public GameObject credits;
    static string curOption = "title";

    public Dictionary<string, GameObject> menus = new Dictionary<string, GameObject>();
    static readonly Dictionary<string, string> previousEntries = new Dictionary<string, string>()
    {
        {"title", "" },
        {"mainMenu", "title" },
        {"credits", "title" },
        {"gameSelection", "mainMenu" }
    };
    //public static TitleState instance;

    private void Awake()
    {
        //instance = this;
        if(LoadingManager.instance != null)
        {
            // Debug.Log("Disable");
            foreach(Button button in FindObjectsOfType<Button>(true))
            {
                button.enabled = false;
            }
            // FindObjectOfType<OptionMenu>().EnableInput(false);
        }

    }

    void Start()
    {
        menus.Add("title", title);
        menus.Add("mainMenu", mainMenu);
        menus.Add("credits", credits);
        menus.Add("gameSelection", gameSelection);
        ChangeState(curOption, false);
        //Display.Active();
        /*if (Display.displays.Length > 1)
            Display.displays[1].Activate();*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        //if (FindObjectOfType<LoadingManager>(true) != null)
        //Destroy(FindObjectOfType<LoadingManager>(true).gameObject);
        LoadingManager.LoadScene("Scenes/Main/DialogueState", null, 0.1f, false);
    }

    public void ChangeMenu(string menu) => ChangeState(menu, true);
    void ChangeState(string state, bool change)
    {
        if(change)
        {
            GameObject sound = new GameObject("Select");
            sound.AddComponent<SoundByte>();
            DontDestroyOnLoad(sound);
            sound.GetComponent<AudioSource>().playOnAwake = false;
            sound.GetComponent<AudioSource>().clip = FindObjectOfType<OptionMenu>().select;
            sound.GetComponent<SoundByte>().timeSamples = sound.GetComponent<AudioSource>().timeSamples;
            sound.GetComponent<AudioSource>().Play();
        }
            
        foreach(KeyValuePair<string, GameObject> menu in menus)
        {
            if(menu.Value != null)
                menu.Value.SetActive(false);
        }

        if (menus.ContainsKey(state) && menus[state] != null)
            menus[state].SetActive(true);

        curOption = state;
    }

    public void GoBack()
    {
        foreach (KeyValuePair<string, GameObject> menu in menus)
        {
            if (menu.Value != null)
                menu.Value.SetActive(false);
        }

        if (menus.ContainsKey(previousEntries[curOption]) && menus[previousEntries[curOption]] != null)
            menus[previousEntries[curOption]].SetActive(true);

        curOption = previousEntries[curOption];
    }

    public void LoadMinigame(string game)
    {
        LoadingManager.LoadScene($"Scenes/Minigames/{game}", delegate() {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Pause Menu");
            if (prefab != null)
            {
                Instantiate(prefab, Vector3.zero, Quaternion.identity).name = "Pause";
            }
            FindObjectOfType<Minigame>().SetUpSong();
            StaticProperties.canPause = true;
            MinigameManager.returnCallback = delegate() {
                LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { Time.timeScale = 1; }, 0.01f);
            };

        });
    }
}
