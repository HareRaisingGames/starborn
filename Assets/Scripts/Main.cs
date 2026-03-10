using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MetadataManager.Load();
        //Resources.Load<GameObject>("Prefabs/Transition");
        StartCoroutine(LoadSceneMode());
        IEnumerator LoadSceneMode()
        {
            yield return new WaitForSeconds(0.5f);
            LoadingManager.LoadScene("Scenes/Main/TitleScreen", delegate () { Time.timeScale = 1; }, 0.1f);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
