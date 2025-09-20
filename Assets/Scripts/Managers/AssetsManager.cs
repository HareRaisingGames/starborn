using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rabbyte;
using System.IO;

public static class AssetsManager
{
    static bool _hasLoaded;
    public static bool hasLoaded => _hasLoaded;
    //static readonly int saveFiles = 4;
    static int curFolder = 0;
    public static string curDirectoryPath => Path.Combine(Application.persistentDataPath, curFolder.ToString());

    //Load all character file names into list. They will be pulled into the specific
    public static List<string> characterFileNames = new List<string>();

    //All the dialogue files categorized by volume and chapter
    public static List<string> mainDialogueNames {
        get
        {
            List<string> template = new List<string>(mainDialogueFiles.Keys);
            template.Sort((a, b) => {
                if (mainDialogueFiles[a].volume.CompareTo(mainDialogueFiles[b].volume) == 0)
                {
                    return mainDialogueFiles[a].chapter.CompareTo(mainDialogueFiles[b].chapter);
                }

                return mainDialogueFiles[a].volume.CompareTo(mainDialogueFiles[b].volume);
            });
            return template;
        }
    }
    public static List<string> sideDialogueNames
    {
        get
        {
            List<string> template = new List<string>(sideDialogueFiles.Keys);
            template.Sort((a, b) => {
                if (sideDialogueFiles[a].volume.CompareTo(sideDialogueFiles[b].volume) == 0)
                {
                    return sideDialogueFiles[a].chapter.CompareTo(sideDialogueFiles[b].chapter);
                }

                return sideDialogueFiles[a].volume.CompareTo(sideDialogueFiles[b].volume);
            });
            return template;
        }
    }
    static Dictionary<string, DialogueMetadata> mainDialogueFiles = new Dictionary<string, DialogueMetadata>();
    static Dictionary<string, DialogueMetadata> sideDialogueFiles = new Dictionary<string, DialogueMetadata>();


    static Dictionary<string, bool> levels = new Dictionary<string, bool>();


    static AssetsManager()
    {
        if (!_hasLoaded)
            LoadDialogueFiles();

        /*
         * for(int i = 1; i < 5; i++)
         * {
         *      if(!Directory.Exists(Path.Combine(Application.persistentDataPath, i.ToString()))
         *          Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, i.ToString()));
         * }
         */
    }

    public static void LoadDialogueFiles()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "Dialogue");
        if(Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);
            foreach(string file in files)
            {
                if(Path.GetFileName(file).Contains(".sbd"))
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    StarbornFileHandler.ExtractDialogue(path);
                    SimpleSBDFile dialogueFile = StarbornFileHandler.ReadSimpleDialogue(filename);

                    DialogueMetadata metadata = new DialogueMetadata(dialogueFile.volume.HasValue ? dialogueFile.volume.Value : 0, 
                        dialogueFile.chapter.HasValue ? dialogueFile.chapter.Value : 0, dialogueFile.displayName, dialogueFile.description);

                    if (dialogueFile.type == StoryType.Main)
                        mainDialogueFiles.Add(filename, metadata);
                    else if (dialogueFile.type == StoryType.Side)
                        sideDialogueFiles.Add(filename, metadata);
                }
            }
        }

        StarbornFileHandler.ClearCache();
        _hasLoaded = true;
    }

    public static void SelectFolder(int index)
    {
        levels.Clear();
        curFolder = index;
    }
}

[System.Serializable]
public struct DialogueMetadata
{
    public int volume;
    public int chapter;
    public string title;
    public string description;

    public DialogueMetadata(int volume, int chapter, string title = "", string description = "")
    {
        this.volume = volume;
        this.chapter = chapter;
        this.title = title;
        this.description = description;
    }
}