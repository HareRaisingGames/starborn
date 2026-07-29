using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public static class MiscUtils
{
#if NET_4_6
    public static dynamic Random(params dynamic[] items)
    {
        System.Random random = new System.Random();
        float r = (float)random.NextDouble();

        if (items.Length <= 0) return null;
        float prob = 1f / items.Length;
        int i = 0;
        foreach(dynamic item in items)
        {
            float min = i * prob;
            i++;
            float max = i * prob;

            if (r > min && r <= max)
                return item;

        }
        return null;
    }
#endif

    public static void CameraFunction(Action<Camera> camFunction)
    {
        List<Camera> cameras = FindObjectsOfTypeInAllScenes<Camera>(true);
        foreach (Camera camera in cameras)
            camFunction?.Invoke(camera);

    }

    public static List<T> FindObjectsOfTypeInAllScenes<T>(bool includeInactive = true) where T : Component
    {
        List<T> results = new List<T>();
        // Iterate through all loaded scenes in the SceneManager
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            // Get all root game objects for the current scene
            GameObject[] rootObjects = scene.GetRootGameObjects();

            // Iterate through root objects and get all components of type T in children
            foreach (GameObject rootObject in rootObjects)
            {
                // Use GetComponentsInChildren to find components including inactive ones
                results.AddRange(rootObject.GetComponentsInChildren<T>(includeInactive));
            }
        }
        return results;
    }
}
