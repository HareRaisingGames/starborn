using TMPro;
using UnityEngine;

public class TextDistortion : MonoBehaviour
{
    public TextMeshProUGUI textComponent;

    private void OnValidate()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>(); // Or GetComponent<TextMeshProUGUI>()
        }
        ApplyDistortion();
    }

    void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>(); // Or GetComponent<TextMeshProUGUI>()
        }
        ApplyDistortion();
    }

    void ApplyDistortion()
    {
        // ... distortion logic ...
        textComponent.ForceMeshUpdate();
        TMP_MeshInfo[] meshInfo = textComponent.textInfo.meshInfo;
        // Example: Simple wobble effect
        for (int i = 0; i < textComponent.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = meshInfo[materialIndex].vertices;

            // Get the four vertices of the current character
            int vertexIndex = charInfo.vertexIndex;

            // Apply a simple sine wave distortion
            float offset = Mathf.Sin(Time.time * 5f + i * 0.5f) * 0.1f;

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j].y += offset;
            }
        }

        for (int i = 0; i < meshInfo.Length; i++)
        {
            textComponent.UpdateGeometry(meshInfo[i].mesh, i);
        }
    }

    void Update()
    {
        ApplyDistortion(); // Call the distortion method in Update for continuous effects
    }
}