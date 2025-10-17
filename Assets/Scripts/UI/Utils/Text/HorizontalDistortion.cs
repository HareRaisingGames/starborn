using TMPro;
using UnityEngine;

public class HorizontalDistortion : MonoBehaviour
{
    public TMP_Text textComponent;

    private void OnValidate()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }
        //ApplyDistortion();
    }

    void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }
        ApplyDistortion();
    }

    void ApplyDistortion()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

            // Apply horizontal offset based on character position or a time-based effect
            float distortionAmount = Mathf.Sin(charInfo.bottomLeft.x * 0.5f + Time.time * 2f) * 5f; // Example distortion

            sourceVertices[vertexIndex + 0].x += distortionAmount; // Bottom-left
            sourceVertices[vertexIndex + 1].x += distortionAmount; // Top-left
            sourceVertices[vertexIndex + 2].x += distortionAmount; // Top-right
            sourceVertices[vertexIndex + 3].x += distortionAmount; // Bottom-right
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            //if (textInfo.meshInfo[i].mesh == null)
                //continue;

            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    private void Update()
    {
        ApplyDistortion();
    }
}