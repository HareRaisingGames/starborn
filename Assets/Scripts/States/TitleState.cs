using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleState : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        //if (FindObjectOfType<LoadingManager>(true) != null)
            //Destroy(FindObjectOfType<LoadingManager>(true).gameObject);
        LoadingManager.LoadScene("Scenes/Main/DialogueState");
    }
}
