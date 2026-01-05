using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMarty : MonoBehaviour
{
    public AnimationClip idle;
    public AnimationClip prepare;
    public AnimationClip throwRang;
    Animator animator;

    public Material material;

    Color curColor = Color.white;
    Color selectedColor = Color.white;

    public int totalFramesIdle
    {
        get
        {
            if(animator != null && idle != null)
            {
                return (int)(idle.length * idle.frameRate);
            }
            return 0;
        }
    }

    public int totalFramesPrepare
    {
        get
        {
            if (animator != null && prepare != null)
            {
                return (int)(prepare.length * prepare.frameRate);
            }
            return 0;
        }
    }

    public int totalFramesThrow
    {
        get
        {
            if (animator != null && throwRang != null)
            {
                return (int)(throwRang.length * throwRang.frameRate);
            }
            return 0;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        material = GetComponent<SpriteRenderer>().materials[0];
        selectedColor = curColor;
        if (material != null)
        {
            material.SetColor("_Color", curColor);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(material != null)
        {
            curColor = Color.LerpUnclamped(curColor, selectedColor, Time.deltaTime * 5f);
            material.SetColor("_Color", curColor);
        }
        
    }

    public void Idle()
    {
        if (animator != null && idle != null)
            animator.Play(idle.name);
    }

    public void Prepare(Color color, Color startColor)
    {
        if(material != null)
            material.SetColor("_Color", startColor);
        curColor = startColor;
        selectedColor = color;
        if (animator != null && prepare != null)
            animator.Play(prepare.name, -1, 0f);
    }

    public void Throw()
    {
        if (animator != null && throwRang != null)
            animator.Play(throwRang.name);
    }

    public void SetSpeed(float bpm = 120)
    {
        animator.speed = bpm / 120;
    }
}
