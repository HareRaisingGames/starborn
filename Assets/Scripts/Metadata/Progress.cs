using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class Progress
{
    public static readonly string progressPath = $"{Application.persistentDataPath}/Progress/save";

    public static readonly List<string> unlockableMinigames = new List<string>() 
    { 
        "Jam Session",
        "Hitchhike",
        "Assembly Line",
        "The Great Escape",
        "Trojan"
    };

    public static readonly List<string> demoMinigames = new List<string>()
    {
        "Tosstail",
        "Trojan",
    };

    public static Dictionary<string, float> minigameAccuracies = new Dictionary<string, float>();
    public static Dictionary<string, float> chapterAccuracies = new Dictionary<string, float>();
}

public class AppStartup
{
    static bool isOpen;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnApplicationOpen()
    {
        if(!isOpen)
        {
            //Debug.Log("Application is opening and ready to run!");
            TweenManager.instance.AddManager();
            if(Application.platform != RuntimePlatform.WebGLPlayer)
                MetadataManager.Load();
            AssetsManager.Open();
            isOpen = true;
        }
    }
}

//Used for saving
public class SaveLevelMetadata
{
    public string name;
    public bool passed;
    public float percent;
    public bool unlocked;

}

[JsonConverter(typeof(ProgressParser))]
public class ProgressFile
{

}

public class ProgressParser : JsonConverter<ProgressFile>
{
    public override ProgressFile ReadJson(JsonReader reader, Type objectType, ProgressFile existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return null;
    }

    public override void WriteJson(JsonWriter writer, ProgressFile value, JsonSerializer serializer)
    {

    }
}