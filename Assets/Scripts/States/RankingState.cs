using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using Rabbyte;
using Starborn.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RankingState : MonoBehaviour
{
    // public GameObject fade;
    [Header("Assets")]
    public GameObject disc;
    private float rotationSpeed = -100f;
    public GameObject results;
    private float resultsXPos;
    public Image albumCover; //This will be applied once we've got a bunch of different chapters down
    private float albumCoverXPos;
    public TMP_Text scoringText;
    public TMP_Text finalScore;

    [Header("Rankings")]
    public Ranking rankingSprite;
    [Header("Renderer")]
    public Image backgroundRender;
    private Material instMaterial;

    [Header("Audio")]
    public AudioSource music;
    public AudioSource listSFX;
    public AudioSource drumRoll;

    // private StarbornInputSystem m_inputSystem;
    bool interact;
    public bool canInteract => interact;
    string finalList;
    
    [Header("Debugging Tools")]
    public bool debug;
    public static RankingState instance;

    private Dictionary<string, float> accuracies = new Dictionary<string, float>(MinigameManager.minigameAccuracies);
    private float average
    {
        get
        {
            List<float> recordedAccuracies = new List<float>();
            foreach(float accuracy in accuracies.Values)
            {
                recordedAccuracies.Add(accuracy);
            }
            if(recordedAccuracies.Count == 0) return 0;
            return MathUtils.ListAverage(recordedAccuracies);
        }
    }

    // public int indentLevel = 4;
    // string indent1 => new string(' ', indentLevel);

    private void Awake()
    {
        instance = this;
        instance.GetComponent<Canvas>().worldCamera = Camera.main;
        // m_inputSystem = new StarbornInputSystem();
        // backgroundRender.gameObject.SetActive(false);
        if(backgroundRender.material != null)
        {
            instMaterial = new Material(backgroundRender.material);
            backgroundRender.material = instMaterial;
        }
            
    }
    // Start is called before the first frame update
    void Start()
    {
        if(debug)
        {
            MinigameManager.totalAccuracies.Add(0.958f);
            MinigameManager.totalAccuracies.Add(0.958f);
            MinigameManager.minigameAccuracies.Add("Tosstail",0.958f);
            MinigameManager.minigameAccuracies.Add("LemonDrop", 0.958f);
        }

        if(instMaterial != null)
        {
            instMaterial.SetFloat("_Alpha", 0);
        }

        if(disc != null) ColorUtils.SetAlpha(disc, 0);
        if(results != null) 
        {
            ColorUtils.SetAlpha(results, 0);
            resultsXPos = results.GetComponent<RectTransform>().anchoredPosition.x;
        }
        if(albumCover != null)
        {
            ColorUtils.SetAlpha(albumCover, 0);
            albumCoverXPos = albumCover.rectTransform.anchoredPosition.x;
        }
        
        StartCoroutine(TransitionIn(0.25f));
    }

    public static RankingState OpenRanking()
    {
        // RankingState rank
        GameObject obj = Resources.Load<GameObject>("Prefabs/Ranking Screen");
        Instantiate(obj);
        return instance;
    }

    public IEnumerator TransitionIn(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        float discStartX = disc.GetComponent<RectTransform>().sizeDelta.x / 2f;
        TweenManager.AlphaTween(disc, 0, 1, 1.5f, Eases.EaseInOutQuad);
        TweenManager.XTween(disc, -discStartX, 0, 1.5f, Eases.EaseInOutQuad, delegate ()
        {
            StartCoroutine(StartJudging());
        });

        float resultX = results.GetComponent<RectTransform>().sizeDelta.x / 2f;
        TweenManager.AlphaTween(results, 0, 1, 1f, Eases.EaseInOutQuad);
        TweenManager.XTween(results, -resultX * 2, resultsXPos, 1f, Eases.EaseInOutQuad);

        float albumStartX = albumCover.rectTransform.sizeDelta.x / 2f;
        TweenManager.AlphaTween(albumCover.gameObject, 0, 1, 1.5f, Eases.EaseInOutQuad);
        TweenManager.XTween(albumCover.gameObject, albumStartX * 2, albumCoverXPos, 1.5f, Eases.EaseInOutQuad);
        TweenManager.AngleTween(albumCover.gameObject, -95,-5, 1.5f, Eases.EaseInOutQuad);

        if(instMaterial != null)
        TweenManager.NumTween
        (()=> instMaterial.GetFloat("_Alpha"), 
            (value) => instMaterial.SetFloat("_Alpha", value), 0.5f, 1f, Eases.EaseInOutQuad);
    }

    // Update is called once per frame
    void Update()
    {
        disc.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
    private string indent = new string(' ', 8);
    public IEnumerator StartJudging()
    {
        yield return new WaitForSeconds(1f);
        if(scoringText != null)
        foreach(KeyValuePair<string, float> accurate in debug ? MinigameManager.minigameAccuracies : accuracies)
        {
            if(listSFX != null) listSFX.Play();
            scoringText.text += $"{StringUtils.SplitToUpper(accurate.Key)}{indent}{accurate.Value:P2}\n";
            yield return new WaitForSeconds(1f);
        }
        if(drumRoll != null) drumRoll.Play();
        yield return new WaitForSeconds(2f);
        if(drumRoll != null) drumRoll.Stop();
        float total = debug ? MinigameManager.FinalAccuracy() : average;
        if(finalScore != null) finalScore.text = $"{total:P2}";
        if(listSFX != null) listSFX.Play();
        yield return new WaitForSeconds(1.5f);
        rankingSprite.DisplayRanking(total);

        yield return new WaitForSeconds(1f);
        interact = true;
    }

    public void Continue()
    {
        if(!canInteract) return;
        if(music != null) MusicUtils.MusicFadeOut(music, 0.5f, delegate(){ music.Stop(); });
        MinigameManager.EndChapter();
        LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { 
            Time.timeScale = 1; 
            foreach(Button button in FindObjectsOfType<Button>(true))
                button.enabled = true;
        }, 0.1f);
    }
}
