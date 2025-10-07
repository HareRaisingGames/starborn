using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartGroup : MonoBehaviour
{
    //public Heart life1;
    //public Heart life2;
    //public Heart life3;

    public List<Heart> lives = new List<Heart>();

    MinigameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLives(float health)
    {
        foreach (Heart heart in lives)
            heart.value = health;
    }
}
