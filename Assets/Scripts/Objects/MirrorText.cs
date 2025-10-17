using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class MirrorText : MonoBehaviour
{
    public TMP_Text original;
    public Vector3 offset;
    private TMP_Text baseText;

    public bool lerp = false;
    public float lerpAmount = 25;
    // Start is called before the first frame update
    void Start()
    {
        baseText = GetComponent<TMP_Text>();
        if(original != null)
        {
            transform.position = original.transform.position + offset;
            transform.rotation = original.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (original != null)
        {
            if (lerp)
            {
                transform.position = Vector3.Lerp(transform.position, original.transform.position + offset, Time.deltaTime * lerpAmount);
                transform.rotation = Quaternion.Lerp(transform.rotation, original.transform.rotation, Time.deltaTime * lerpAmount);
                if (baseText != null)
                    baseText.text = original.text;
            }
            else
            {
                transform.position = original.transform.position  + offset;
                transform.rotation = original.transform.rotation;
                if (baseText != null)
                    baseText.text = original.text;
            }

        }
    }
}
