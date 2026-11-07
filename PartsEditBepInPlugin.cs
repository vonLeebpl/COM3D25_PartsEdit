using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Entry point for BepInEx\plugins. UnityInjector can still load PartsEditPlugin.
    [BepInPlugin("com3d25.partsedit", "COM3D25_PartsEdit", "0.3.1")]
    public class PartsEditBepInPlugin : BaseUnityPlugin {
        void Awake() {
            Log.Source = Logger;
            if (PartsEditPlugin.started) return;
            PartsEditPlugin.started = true;

            PluginInfo.SetInfo("COM3D25_PartsEdit", "0.3.1", "CM3D2.PartsEdit.Plugin");
            string configDir = Path.Combine(Paths.ConfigPath, "PartsEdit");
            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(Path.Combine(configDir, "Presets"));
            Directory.CreateDirectory(Path.Combine(Path.Combine(configDir, "Presets"), "IK"));
            PresetManager.directoryPath = Path.Combine(configDir, "Presets") + Path.DirectorySeparatorChar;
            // Ustawienia: plik cfg BepInEx (okno F1). Dawne ini jest tylko czytane, zeby przy pierwszym uruchomieniu przeniesc wartosci.
            string legacyIni = Path.Combine(configDir, "COM3D2.PartsEdit.Plugin.ini");
            IniUtil.InitFileReadOnly(legacyIni);
            Setting.LoadIni();
            ConfigBridge.Init(Config, legacyIni);
            Setting.SaveIni();
            Db.PartsEditStore.Init(configDir, PresetManager.directoryPath);

            GameObject obj = new GameObject("PartsEdit.Plugin");
            DontDestroyOnLoad(obj);
            obj.AddComponent<PartsEditPlugin>();
            obj.AddComponent<PartsEdit>();
            Logger.LogInfo("PartsEdit loaded from BepInEx\\plugins.");
        }
    }
}
