using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            if(onTextFinish != null)
            {
                onTextFinish();
                onTextFinish = null;
            }
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
    public delegate void OnTextFinish();
    public OnTextFinish onTextFinish;

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
        //i = 0;
        StartCoroutine(Type(text, field, time));
    }

    IEnumerator Type(string text, TMP_Text field, float time)
    {
        yield return new WaitForSeconds(0.075f);

        char[] letters = text.ToCharArray();
        foreach (char letter in letters)
        {
            field.text += letter;
            //i++;
            yield return new WaitForSeconds(time);
        }

        this.text = field.text;
    }
}
