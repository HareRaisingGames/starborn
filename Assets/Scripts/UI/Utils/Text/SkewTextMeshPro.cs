using UnityEngine;
using TMPro;

public class SkewTextMeshPro : MonoBehaviour
{
    public TMP_Text tmpText;
    public float skewAmount = 0.2f;

    private void OnValidate()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TMP_Text>();
            if (tmpText == null)
            {
                return;
            }
        }

        ApplySkew();
    }

    void Start()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TMP_Text>();
            if (tmpText == null)
            {
                Debug.LogError("TextMeshPro component not found!");
                return;
            }
        }

        ApplySkew();
    }

    void ApplySkew()
    {
        tmpText.ForceMeshUpdate();
        TMP_MeshInfo[] meshInfo = tmpText.textInfo.meshInfo;

        for (int i = 0; i < tmpText.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = tmpText.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = meshInfo[materialIndex].vertices;

            // Calculate a pivot point (e.g., bottom-left corner of the character)
            Vector3 pivot = new Vector3(vertices[vertexIndex + 0].x, vertices[vertexIndex + 0].y, 0);

            // Apply skew transformation
            for (int j = 0; j < 4; j++) // Each character has 4 vertices
            {
                Vector3 originalPos = vertices[vertexIndex + j];
                Vector3 offset = originalPos - pivot;
                offset.x += offset.y * skewAmount; // Skew based on Y position
                vertices[vertexIndex + j] = pivot + offset;
            }
        }

        // Upload the modified vertices to the mesh
        for (int i = 0; i < meshInfo.Length; i++)
        {
            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}