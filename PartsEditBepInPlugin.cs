using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Entry point for BepInEx\plugins. UnityInjector can still load PartsEditPlugin.
    [BepInPlugin("cm3d2.partsedit", "CM3D2 PartsEditPlugin", "0.2")]
    public class PartsEditBepInPlugin : BaseUnityPlugin {
        void Awake() {
            if (PartsEditPlugin.started) return;
            PartsEditPlugin.started = true;

            PluginInfo.SetInfo("CM3D2 PartsEditPlugin", "0.2", "CM3D2.PartsEdit.Plugin");
            string configDir = Path.Combine(Paths.ConfigPath, "PartsEdit");
            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(Path.Combine(configDir, "Presets"));
            Directory.CreateDirectory(Path.Combine(Path.Combine(configDir, "Presets"), "IK"));
            PresetManager.directoryPath = Path.Combine(configDir, "Presets") + Path.DirectorySeparatorChar;
            IniUtil.InitFile(Path.Combine(configDir, "COM3D2.PartsEdit.Plugin.ini"));
            Setting.LoadIni();
            Setting.SaveIni();

            GameObject obj = new GameObject("PartsEdit.Plugin");
            DontDestroyOnLoad(obj);
            obj.AddComponent<PartsEditPlugin>();
            obj.AddComponent<PartsEdit>();
            Logger.LogInfo("PartsEdit loaded from BepInEx\\plugins.");
        }
    }
}
