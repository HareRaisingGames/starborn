using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rabbyte;

public static class StaticProperties
{
    public static string controlType;

    public static Scene[] GetAllScenes()
    {
        List<Scene> scenes = new List<Scene>();
        for(int i = 0; i < SceneManager.sceneCount; i++)
        {
            scenes.Add(SceneManager.GetSceneAt(i));
        }

        return scenes.ToArray();
    }

    public static bool DoesSceneExistInBuild(string scenePath)
    {
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
        return sceneIndex >= 0;
    }
}
