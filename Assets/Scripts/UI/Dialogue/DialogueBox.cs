using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueBox : MonoBehaviour
{
    public string text
    {
        set
        {
            if(textField != null)
            {
                textField.text = value;
                StopAllCoroutines();
            }
            _interacting = true;
        }
    }
    public string typedText
    {
        set
        {
            if(textField != null)
            {
                StopAllCoroutines();
                StartTyping(value, textField, delay);
            }
        }
    }

    public TMP_Text textField;

    public AudioSource click;
    [HideInInspector]
    public int t = -1;

    public Action<int> onStart;
    public Action<int, int> onChar;
    public Action<int> onFinish;

    bool _interacting = true;
    public bool canInteract => _interacting;
    float _time;
    public float time
    {
        set
        {
            _time = value;
        }
    }

    [HideInInspector]
    public float delay = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //int i = 0;
    void StartTyping(string text, TMP_Text field, float time = 0.05f)
    {
        field.text = "";
        _interacting = false;
        onStart?.Invoke(t);
        onStart = null;
        StartCoroutine(Type(text, field, time));
    }

    IEnumerator Type(string text, TMP_Text field, float time)
    {
        yield return new WaitForSeconds(0.075f);

        char[] letters = text.ToCharArray();
        int i = 0;
        foreach (char letter in letters)
        {
            _time = time;
            field.text += letter;
            //int i = new List<char>(letters).IndexOf(letter);
            onChar?.Invoke(t,i);
            i++;
            yield return new WaitForSeconds(_time);
        }

        this.text = field.text;
        onChar = null;
        onFinish?.Invoke(t);
        onFinish = null;
    }
}
