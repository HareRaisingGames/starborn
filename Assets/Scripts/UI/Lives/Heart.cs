using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Animations")]
    public AnimationClip full;
    public AnimationClip half;
    public AnimationClip empty;

    [Header("Instant Animations")]
    public AnimationClip fullInstant;
    public AnimationClip halfInstant;
    public AnimationClip emptyInstant;

    [Header("Audio")]
    public AudioSource lifeLost;

    [Header("Metadata")]
    public int id;
    public bool instant;
    float _value;
    public float value
    {
        set
        {
            if(animator != null)
            {
                /*AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                string statename = "";
                string expectedName = "";
                if (stateInfo.IsName("Base Layer.Full"))
                    statename = "Base Layer.Full";
                else if (stateInfo.IsName("Base Layer.Heart_Start"))
                    statename = "Base Layer.Heart_Start";
                else if (stateInfo.IsName("Base Layer.Half"))
                    statename = "Base Layer.Half";
                else if (stateInfo.IsName("Base Layer.Heart_Half"))
                    statename = "Base Layer.Heart_Half";
                else if (stateInfo.IsName("Base Layer.Empty"))
                    statename = "Base Layer.Empty";
                else if (stateInfo.IsName("Base Layer.Heart_Empty"))
                    statename = "Base Layer.Heart_Empty";

                int stateHash = Animator.StringToHash(statename);

                if(stateInfo.normalizedTime >= 1.0f)
                {

                }
                else
                {

                }

                if (stateInfo.fullPathHash == stateHash)
                    return;*/

                string curAnim = GetHeartType(_value);
                string expAnim = GetHeartType(value);

                if(curAnim == expAnim)
                {
                    _value = value;
                    return;
                }

                if (instant)
                {
                    if (value > id - 0.5f)
                        animator.Play(fullInstant.name);
                    else if (value <= id - 0.5f && value > id - 1)
                        animator.Play(halfInstant.name);
                    else if (value <= id - 1)
                        animator.Play(emptyInstant.name);
                }
                else
                {
                    if (value > id - 0.5f)
                        animator.Play(full.name);
                    else if (value <= id - 0.5f && value > id - 1)
                        animator.Play(half.name);
                    else if (value <= id - 1)
                        animator.Play(empty.name);
                }


                if (lifeLost != null)
                    lifeLost.Play();
            }
            _value = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _value = id;
        value = id;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string GetHeartType(float value)
    {
        if (value > id - 0.5f)
            return "full";
        else if (value <= id - 0.5f && value > id - 1)
            return "half";
        else if (value <= id - 1)
            return "empty";

        return "";
    }
}
