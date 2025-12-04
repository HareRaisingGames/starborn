using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using Rabbyte;
using Starborn.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingState : MonoBehaviour
{
    public GameObject fade;
    public Image rank;

    [Header("Rankings")]
    public Sprite S;
    public Sprite A;
    public Sprite B;
    public Sprite C;
    public Sprite D;
    public Sprite F;

    private StarbornInputSystem m_inputSystem;
    bool canInteract;

    string finalList;

    private void Awake()
    {
        m_inputSystem = new StarbornInputSystem();
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach(KeyValuePair<string, float> accurate in MinigameManager.minigameAccuracies)
        {
            finalList += $"{accurate.Key}\t\t{Mathf.Round(accurate.Value*Mathf.Pow(10,4))/Mathf.Pow(10,2)}%";
        }
        if(fade != null)
        {
            fade.SetActive(true);
            ColorUtils.SetAlpha(fade, 1);
            TweenManager.AlphaTween(fade, 1, 0, 2f, Eases.Linear, delegate ()
            {

            }).SetStartDelay(0.1f);
        }
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Continue()
    {
        LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { Time.timeScale = 1; }, 0.1f);
    }
}
