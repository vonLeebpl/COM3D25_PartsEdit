using System;
using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Ustawienia w pliku konfiguracyjnym BepInEx (BepInEx\config\com3d25.partsedit.cfg), widoczne w oknie F1 (Configuration Manager).
    // Zastepuje dawny plik ini: przy pierwszym uruchomieniu (brak pliku cfg) wartosci wczytane z ini trafiaja do cfg, potem
    // zrodlem prawdy jest cfg. Pola Setting.* nadal sa uzywane przez reszte kodu: Init kopiuje wartosci cfg -> Setting,
    // Push (wywolywany przez Setting.SaveIni) kopiuje Setting -> cfg, a zmiany z F1 aktualizuja Setting na zywo.
    static class ConfigBridge {
        public static bool Ready;
        public static event Action WindowSizeChanged = delegate { };
        public static event Action FontScaleChanged = delegate { };

        static ConfigFile cfg;
        static bool busy;

        static ConfigEntry<float> eWidth, eHeight, eLabelScale, eButtonScale, eGravityStrength;
        static ConfigEntry<bool> eGravityOn;
        static ConfigEntry<string> eNormalColor, eSelectColor, eHighlightColor, eIkChainColor;
        static ConfigEntry<KeyCode> eBoneSelectKey, eGizmoSmallKey, eGizmoBigKey, eIkPlaneKey, eIkAxisKey, eIkAngleKey;
        static ConfigEntry<BoneDisplay> eBoneDisplay;
        static ConfigEntry<BoneGizmoRenderer.COORDINATE> eCoordinate;
        static ConfigEntry<GizmoType> eGizmoType;
        static ConfigEntry<BodyBoneDisplay> eBodyBone;

        static AcceptableValueRange<float> Scale() { return new AcceptableValueRange<float>(0.5f, 1.5f); }

        public static void Init(ConfigFile config, string legacyIniPath) {
            cfg = config;
            // Brak pliku cfg = pierwsze uruchomienie po zmianie: wartosci z ini (juz wczytane do Setting.*) stana sie startowymi.
            bool fresh = !File.Exists(cfg.ConfigFilePath);
            cfg.SaveOnConfigSet = false;

            eWidth = cfg.Bind("Window", "Width", 720f, new ConfigDescription("Width of the PartsEdit window in pixels.", new AcceptableValueRange<float>(320f, 4000f)));
            eHeight = cfg.Bind("Window", "Height", 850f, new ConfigDescription("Height of the PartsEdit window in pixels.", new AcceptableValueRange<float>(320f, 4000f)));
            eLabelScale = cfg.Bind("Window", "LabelFontScale", 0.85f, new ConfigDescription("Font size of the static texts (labels) relative to the interface font.", Scale()));
            eButtonScale = cfg.Bind("Window", "ButtonFontScale", 0.80f, new ConfigDescription("Font size of the buttons relative to the interface font (also changed with A- / A+ in the Library window).", Scale()));

            eBoneDisplay = cfg.Bind("Setting", "BoneDisplay", BoneDisplay.Visible, "Bone display mode.");
            eCoordinate = cfg.Bind("Setting", "CoordinateType", BoneGizmoRenderer.COORDINATE.Local, "Coordinate type of the gizmo.");
            eGizmoType = cfg.Bind("Setting", "GizmoType", GizmoType.Rotation, "Gizmo type.");
            eGravityOn = cfg.Bind("Gravity", "On", false, "Gravity for IK handles.");
            eGravityStrength = cfg.Bind("Gravity", "Strength", 0.05f, "Gravity strength.");

            eBoneSelectKey = cfg.Bind("BoneDisplay", "BoneSelectKey", KeyCode.LeftAlt, "Hold this key and click to select a bone.");
            eNormalColor = cfg.Bind("BoneDisplay", "NormalBoneColor", "#FFFFFF", "Colour of the bone lines (hex #RRGGBB).");
            eSelectColor = cfg.Bind("BoneDisplay", "SelectBoneColor", "#FF0000", "Colour of the selected bone (hex #RRGGBB).");
            eHighlightColor = cfg.Bind("BoneDisplay", "HighlightColor", Setting.DefaultHighlightHex, "Highlight colour of selected buttons and list items (hex #RRGGBB).");
            eIkChainColor = cfg.Bind("BoneDisplay", "IKChainColor", "#0000BF", "Colour of the IK chain (hex #RRGGBB).");
            eBodyBone = cfg.Bind("BoneDisplay", "BodyBoneDisplay", BodyBoneDisplay.Visible, "Display of the body bones.");

            eGizmoSmallKey = cfg.Bind("Hotkeys", "GizmoSmallKey", KeyCode.LeftShift, "Gizmo: hold for small steps.");
            eGizmoBigKey = cfg.Bind("Hotkeys", "GizmoBigKey", KeyCode.None, "Gizmo: hold for big steps.");
            eIkPlaneKey = cfg.Bind("Hotkeys", "IKPlane", KeyCode.Space, "IK: plane key.");
            eIkAxisKey = cfg.Bind("Hotkeys", "IKAxis", KeyCode.LeftControl, "IK: axis key.");
            eIkAngleKey = cfg.Bind("Hotkeys", "IKAngle", KeyCode.LeftShift, "IK: angle key.");

            if (fresh) {
                busy = true;
                CopySettingToEntries(); // wartosci z dawnego ini
                busy = false;
                if (!string.IsNullOrEmpty(legacyIniPath) && File.Exists(legacyIniPath))
                    Log.Info("Settings migrated from the old ini (" + legacyIniPath + ") to the BepInEx config " + cfg.ConfigFilePath);
            }
            CopyEntriesToSetting();
            cfg.SettingChanged += OnSettingChanged;
            Ready = true;
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        // Setting.* -> cfg (wywolywane przez Setting.SaveIni).
        public static void Push() {
            if (!Ready) return;
            busy = true;
            try {
                cfg.SaveOnConfigSet = false;
                CopySettingToEntries();
                cfg.Save();
            } finally {
                cfg.SaveOnConfigSet = true;
                busy = false;
            }
        }

        static void CopySettingToEntries() {
            eWidth.Value = Setting.windowWidth;
            eHeight.Value = Setting.windowHeight;
            eLabelScale.Value = Setting.labelFontScale;
            eButtonScale.Value = Setting.buttonFontScale;
            eBoneDisplay.Value = Setting.boneDisplay;
            eCoordinate.Value = Setting.coordinateType;
            eGizmoType.Value = Setting.gizmoType;
            eGravityOn.Value = Setting.gravityOn;
            eGravityStrength.Value = Setting.gravityStrength;
            eBoneSelectKey.Value = Setting.boneSelectKey;
            eNormalColor.Value = ColorUtil.ToHex(Setting.normalBoneColor.GetValue());
            eSelectColor.Value = ColorUtil.ToHex(Setting.selectBoneColor.GetValue());
            eHighlightColor.Value = ColorUtil.ToHex(Setting.highlightColor.GetValue());
            eIkChainColor.Value = ColorUtil.ToHex(Setting.ikChainColor.GetValue());
            eBodyBone.Value = Setting.bodyBoneDisplay.GetValue();
            eGizmoSmallKey.Value = Setting.gizmoSmallKey;
            eGizmoBigKey.Value = Setting.gizmoBigKey;
            eIkPlaneKey.Value = Setting.ikPlaneKey;
            eIkAxisKey.Value = Setting.ikAxisKey;
            eIkAngleKey.Value = Setting.ikAngleKey;
        }

        // cfg -> Setting.* (przy starcie i przy zmianach z okna F1).
        static void CopyEntriesToSetting() {
            Setting.windowWidth = eWidth.Value;
            Setting.windowHeight = eHeight.Value;
            Setting.labelFontScale = eLabelScale.Value;
            Setting.buttonFontScale = eButtonScale.Value;
            Setting.boneDisplay = eBoneDisplay.Value;
            Setting.coordinateType = eCoordinate.Value;
            Setting.gizmoType = eGizmoType.Value;
            Setting.gravityOn = eGravityOn.Value;
            Setting.gravityStrength = eGravityStrength.Value;
            Setting.boneSelectKey = eBoneSelectKey.Value;
            Setting.gizmoSmallKey = eGizmoSmallKey.Value;
            Setting.gizmoBigKey = eGizmoBigKey.Value;
            Setting.ikPlaneKey = eIkPlaneKey.Value;
            Setting.ikAxisKey = eIkAxisKey.Value;
            Setting.ikAngleKey = eIkAngleKey.Value;
            SetColor(Setting.normalBoneColor, eNormalColor.Value);
            SetColor(Setting.selectBoneColor, eSelectColor.Value);
            SetColor(Setting.highlightColor, eHighlightColor.Value);
            SetColor(Setting.ikChainColor, eIkChainColor.Value);
            if (Setting.bodyBoneDisplay.GetValue() != eBodyBone.Value) Setting.bodyBoneDisplay.SetValue(eBodyBone.Value);
        }

        static void SetColor(Setting.SettingValue<Color> target, string hex) {
            Color c = ColorUtil.ParseColor(hex);
            if (ColorUtil.ToHex(target.GetValue()) != ColorUtil.ToHex(c)) target.SetValue(c);
        }

        static void OnSettingChanged(object sender, SettingChangedEventArgs e) {
            if (busy) return;
            ConfigDefinition def = e.ChangedSetting.Definition;
            CopyEntriesToSetting();
            if (def.Section == "Window") {
                if (def.Key == "Width" || def.Key == "Height") WindowSizeChanged();
                else FontScaleChanged();
            }
        }
    }
}
