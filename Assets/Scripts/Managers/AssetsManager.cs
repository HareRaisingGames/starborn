using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class AssetsManager
{
    //Load all character file names into list. They will be pulled into the specific
    public static List<string> characterFileNames = new List<string>();


    static AssetsManager()
    {
#if UNITY_EDITOR
        string[] sbcs = AssetDatabase.FindAssets("t:defaultAsset", new[] {"Assets/Resources/Characters"});

        foreach (string sbc in sbcs)
            if(!characterFileNames.Contains(sbc))
            {
                string filename = AssetDatabase.GUIDToAssetPath(sbc);
                filename = filename.Split("/")[filename.Split("/").Length - 1];
                filename = filename.Remove(filename.Length - 4);
                characterFileNames.Add(filename);
            }
#endif
    }

    public static void nun()
    {

    }
}
