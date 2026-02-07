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
        if(settingsList.Count == 0)
        {
            foreach (KeyValuePair<string, Setting> setting in defaultList)
            {
                settingsList.Add(setting.Key, setting.Value);
            }
        }

        if (!File.Exists(settingsPath))
        {
            using (StreamWriter writer = File.CreateText(settingsPath))
            {
                foreach (KeyValuePair<string, Setting> setting in defaultList)
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
        { "language", new Setting("english") },
        { "fullScreen", new Setting(true, Screen.fullScreen) }
    };

    static Dictionary<string, Setting> settingsList = new Dictionary<string, Setting>();

    public static void Load()
    {

    }

    public static dynamic GetSettings(string name)
    {
        if (settingsList.ContainsKey(name))
            return settingsList[name].value;
        return null;
    }

    public static void SetSettings(string name, dynamic value)
    {
        if (settingsList.ContainsKey(name))
        {
            Setting setting = settingsList[name];
            setting.value = value;
            settingsList[name] = setting;
        }
            
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
    Action<dynamic> func;
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
        func = null;
    }

    //For string values
    public Setting(string value)
    {
        type = typeof(string);
        _value = value;
        func = null;
    }

    //For int values
    public Setting(int value)
    {
        type = typeof(int);
        _value = value;
        func = null;
    }

    //For float values
    public Setting(float value)
    {
        type = typeof(float);
        _value = value;
        func = null;
    }

    //For double values
    public Setting(double value)
    {
        type = typeof(double);
        _value = value;
        func = null;
    }

    //For bool values
    public Setting(bool value)
    {
        type = typeof(bool);
        _value = value;
        func = null;
    }

    //For char values
    public Setting(char value)
    {
        type = typeof(char);
        _value = value;
        func = null;
    }

    public Setting(Type type, dynamic value, dynamic outer)
    {
        this.type = type;
        _value = value;
        func = delegate(dynamic v) {
            outer = v;
        };
    }

    public Setting(string value, string affecter)
    {
        type = typeof(string);
        _value = value;
        func = delegate (dynamic v) {
            affecter = v;
        };
    }

    //For int values
    public Setting(int value, int affecter)
    {
        type = typeof(int);
        _value = value;
        func = delegate (dynamic v) {
            affecter = v;
        };
    }

    //For float values
    public Setting(float value, float affecter)
    {
        type = typeof(float);
        _value = value;
        func = delegate (dynamic v) {
            affecter = v;
        };
    }

    //For double values
    public Setting(double value, double affecter)
    {
        type = typeof(double);
        _value = value;
        func = delegate (dynamic v) {
            affecter = v;
        };
    }

    //For bool values
    public Setting(bool value, bool affecter)
    {
        type = typeof(bool);
        _value = value;
        func = delegate (dynamic v) {
            affecter = v;
        };
    }

    //For char values
    public Setting(char value, char affecter)
    {
        type = typeof(char);
        _value = value;
        func = delegate (dynamic v) {
            affecter = v;
        };
    }

    void SetValue(dynamic value)
    {
        if (value.GetType == type)
        {
            _value = value;
            func?.Invoke(_value);
        }
            
    }
}
#endif
