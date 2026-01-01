using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureManager : MonoBehaviour
{
    private void Awake()
    {
        var tex = new RenderTexture(Screen.width, Screen.height, 8);
        Debug.Assert(tex.Create(), "Failed to create camera blend");

        Camera cam = GetComponent<Camera>();
        cam.targetTexture = tex;
    }
}
