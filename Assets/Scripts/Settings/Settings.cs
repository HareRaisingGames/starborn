using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class Settings
{
    public static readonly string settingsPath = $"{Application.persistentDataPath}/settings.dat";
    //static bool loaded = false;

#if NET_4_6
    static Settings()
    {
        Initialize();
    }
    static void Initialize()
    {
        if (!File.Exists(settingsPath))
        {
            using (StreamWriter writer = File.CreateText(settingsPath))
            {
                foreach (KeyValuePair<string, Setting> setting in settingsList)
                {
                    writer.WriteLine($"{setting.Key}::{setting.Value}");
                }
            }

        }
        else
        {

        }
    }
    static Dictionary<string, Setting> defaultList = new Dictionary<string, Setting>()
    {
        { "muteVoices", new Setting(false) },
        { "language", new Setting("english") }
    };

    static Dictionary<string, Setting> settingsList = new Dictionary<string, Setting>()
    {
        
    };

    public static dynamic GetSettings(string name)
    {
        if (settingsList.ContainsKey(name))
            return settingsList[name].value;
        return null;
    }

    static void Parse()
    {

    }

    public static bool GetBool(string name, bool boolean = false)
    {
        if (!PlayerPrefs.HasKey(name))
        {
            PlayerPrefs.SetInt(name, boolean ? 1 : 0);
            return boolean;
        }
        else
            return PlayerPrefs.GetInt(name) == 1;
    }

    public static void SetBool(string name, bool boolean = false)
    {
        PlayerPrefs.SetInt(name, boolean ? 1 : 0);
    }
#endif
}

#if NET_4_6
public struct Setting
{
    Type type;
    dynamic _value;
    //Func<dynamic> func;
    public dynamic value
    {
        get
        {
            return _value;
        }
        set
        {
            SetValue(value);
        }
    }

    public Setting(Type type, dynamic value)
    {
        this.type = type;
        _value = value;
    }

    //For string values
    public Setting(string value)
    {
        type = typeof(string);
        _value = value;
    }

    //For int values
    public Setting(int value)
    {
        type = typeof(int);
        _value = value;
    }

    //For float values
    public Setting(float value)
    {
        type = typeof(float);
        _value = value;
    }

    //For double values
    public Setting(double value)
    {
        type = typeof(double);
        _value = value;
    }

    //For bool values
    public Setting(bool value)
    {
        type = typeof(bool);
        _value = value;
    }

    //For char values
    public Setting(char value)
    {
        type = typeof(char);
        _value = value;
    }

    void SetValue(dynamic value)
    {
        if (value.GetType == type)
            _value = value;
    }
}
#endif
