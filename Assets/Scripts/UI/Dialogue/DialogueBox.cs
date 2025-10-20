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

    public Action onStart;
    public Action<int> onChar;
    public Action onFinish;

    bool _interacting = true;
    public bool canInteract => _interacting;

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
        onStart?.Invoke();
        onStart = null;
        StartCoroutine(Type(text, field, time));
    }

    IEnumerator Type(string text, TMP_Text field, float time)
    {
        yield return new WaitForSeconds(0.075f);

        char[] letters = text.ToCharArray();
        foreach (char letter in letters)
        {
            field.text += letter;
            onChar?.Invoke(new List<char>(letters).IndexOf(letter));
            //i++;
            yield return new WaitForSeconds(time);
        }

        this.text = field.text;
        onChar = null;
        onFinish?.Invoke();
        onFinish = null;
    }
}
