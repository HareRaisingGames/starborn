using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PerspectiveText : MonoBehaviour
{
    public TMP_Text textComponent;

    public float start = 1;
    public float end = 1;
    // Start is called before the first frame update
    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();

        if (textComponent != null)
            ApplyPerspective();
    }

    // Update is called once per frame
    void Update()
    {
        if (textComponent != null)
            ApplyPerspective();
    }

    void ApplyPerspective()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        float scale = start;

        float increment = (end - start) / textInfo.characterCount;

        //Debug.Log(textInfo.characterCount);
        for(int i = 0; i < textInfo.characterCount; i++)
        {
            ModifyVertex(textInfo, i,scale);
            scale += increment;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            //if (textInfo.meshInfo[i].mesh == null)
            //continue;

            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    void ModifyVertex(TMP_TextInfo textInfo, int i, float scale = 1)
    {
        var charInfo = textInfo.characterInfo[i];
        if (!charInfo.isVisible)
            return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        var destinationVertices = textInfo.meshInfo[materialIndex].vertices;
        var sourceVertices = textInfo.meshInfo[materialIndex].vertices;

        // Apply the scaling to the top and bottom vertices
        // Top vertices (1 and 2) get squished/stretched
        destinationVertices[vertexIndex + 1].y = sourceVertices[vertexIndex + 1].y * scale;
        destinationVertices[vertexIndex + 2].y = sourceVertices[vertexIndex + 2].y * scale;

        // Bottom vertices (0 and 3) also get squished/stretched
        destinationVertices[vertexIndex + 0].y = sourceVertices[vertexIndex + 0].y * scale;
        destinationVertices[vertexIndex + 3].y = sourceVertices[vertexIndex + 3].y * scale;
    }
}
