using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Tlumaczy napisy ze zrodla (japonski) na podstawie tego samego pliku, ktorego uzywa IMGUITranslationLoader:
    //   <gra>\IMGUITranslationLoader\IMGUIStrings\<nazwa assembly>.txt   (format: japonski<TAB>angielski)
    // IMGUITranslationLoader nie tlumaczy elementow rysowanych przez GUILayout.Toolbar / SelectionGrid,
    // wiec napisy trafiajace do GUIContent[] tlumaczymy sami.
    static class Tr {
        static Dictionary<string, string> dic;

        public static string T(string s) {
            if (string.IsNullOrEmpty(s)) return s;
            if (dic == null) Load();
            string v;
            return dic.TryGetValue(s, out v) ? v : s;
        }

        static void Load() {
            dic = new Dictionary<string, string>();
            try {
                string gameRoot = Directory.GetParent(Application.dataPath).FullName;
                string dir = Path.Combine(Path.Combine(gameRoot, "IMGUITranslationLoader"), "IMGUIStrings");
                string name = Assembly.GetExecutingAssembly().GetName().Name + ".txt";
                string path = Path.Combine(dir, name);
                if (!File.Exists(path)) {
                    Log.Warn("translation file not found: " + path);
                    return;
                }
                foreach (string raw in File.ReadAllLines(path, System.Text.Encoding.UTF8)) {
                    string line = raw.TrimEnd('\r', '\n');
                    int tab = line.IndexOf('\t');
                    if (tab <= 0) continue;
                    string key = line.Substring(0, tab);
                    if (key.Length > 0 && key[0] == '﻿') key = key.Substring(1);
                    string val = line.Substring(tab).TrimStart('\t');
                    if (key.Length == 0 || val.Length == 0) continue;
                    dic[key] = val;
                }
            } catch (Exception e) {
                Log.Warn("failed to load translation: " + e.Message);
            }
        }
    }
}
