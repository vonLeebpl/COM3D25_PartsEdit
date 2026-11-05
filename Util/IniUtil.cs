using System;
using System.IO;
using ExIni;
using UnityInjector;

internal static class IniUtil {
    public static IniFile preferences;
    public delegate void Delegate();
    public static event Delegate saveMethod = delegate () { };
    static string filePath;

    public static void Init(PluginBase pBase) {
        preferences = pBase.Preferences;
    }

    // Used when the plugin is loaded by BepInEx instead of UnityInjector.
    public static void InitFile(string path) {
        filePath = path;
        preferences = File.Exists(path) ? IniFile.FromFile(path) : new IniFile();
        saveMethod += SaveFile;
    }

    static void SaveFile() {
        if (preferences == null || string.IsNullOrEmpty(filePath)) return;
        string dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        preferences.Save(filePath);
    }

    public static int GetIntValue(string section, string key, int defaultValue) {
        IniKey iniKey = preferences[section][key];
        int outValue;
        if (iniKey == null || string.IsNullOrEmpty(iniKey.Value) || !int.TryParse(iniKey.Value, out outValue)) {
            return defaultValue;
        }
        return outValue;
    }

    public static float GetFloatValue(string section, string key, float defaultValue) {
        IniKey iniKey = preferences[section][key];
        float outValue;
        if (iniKey == null || string.IsNullOrEmpty(iniKey.Value) || !float.TryParse(iniKey.Value, out outValue)) {
            return defaultValue;
        }
        return outValue;
    }

    public static string GetStringValue(string section, string key, string defaultValue) {
        IniKey iniKey = preferences[section][key];
        if (iniKey == null || iniKey.Value == null) {
            return defaultValue;
        }
        return iniKey.Value;
    }

    public static void Save() {
        saveMethod.Invoke();
    }
}
