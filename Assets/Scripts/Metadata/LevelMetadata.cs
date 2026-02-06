using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using Rabbyte;

public static class MetadataManager
{
    public static Dictionary<string, LevelMetadata> levels = new Dictionary<string, LevelMetadata>();

    //This is to load every dialogue that's a part of the game
    public static void Load()
    {
        var folderpath = Path.Combine(Application.streamingAssetsPath, "Dialogue");
        if(Directory.Exists(folderpath))
        {
            string[] files = Directory.GetFiles(folderpath, "*.*", SearchOption.TopDirectoryOnly);
            foreach(string file in files)
            {
                if(file.Contains(".sbd") && !file.Contains(".meta"))
                {
                    string filename = file.Replace($"{folderpath}\\", "").Replace(".sbd", "");
                    StarbornFileHandler.ExtractDialogue(file);

                    SimpleSBDFile dialogue = StarbornFileHandler.ReadSimpleDialogue(filename);
                    levels.Add(dialogue.fileName,
                        new LevelMetadata(dialogue.displayName, dialogue.description, dialogue.chapter, dialogue.volume, dialogue.type));
                    //Debug.Log(filename);
                }
            }
        }

        StarbornFileHandler.ClearCache();
    }
}

public struct LevelMetadata
{
    public string name;
    public string description;
    public int? chapter;
    public int? volume;
    public StoryType storyType;

    public LevelMetadata(string name, string description, int? chapter, int? volume, StoryType storyType)
    {
        this.name = name;
        this.description = description;
        this.chapter = chapter;
        this.volume = volume;
        this.storyType = storyType;
    }
}
