using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;

public abstract class Minigame : MonoBehaviour
{
    StarbornInputSystem m_inputSystem = new StarbornInputSystem();
    public string minigameName;
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
