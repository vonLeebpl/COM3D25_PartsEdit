using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExIni;
using UnityEngine;
using UnityInjector;

namespace CM3D2.PartsEdit.Plugin {
    static class Setting {
        public static Mode mode = Mode.Edit;
        public static BoneDisplay boneDisplay = BoneDisplay.Visible;
        public static BoneGizmoRenderer.COORDINATE coordinateType = BoneGizmoRenderer.COORDINATE.Local;
        public static GizmoType gizmoType = GizmoType.Rotation;
        public static int targetSelectMode = 0;

        public static KeyCode boneSelectKey = KeyCode.LeftAlt;
        public static KeyCode gizmoSmallKey = KeyCode.LeftShift;
        public static KeyCode gizmoBigKey = KeyCode.None;
        public static KeyCode ikPlaneKey = KeyCode.Space;
        public static KeyCode ikAxisKey = KeyCode.LeftControl;
        public static KeyCode ikAngleKey = KeyCode.LeftShift;

        public static SettingValue<Color> normalBoneColor = new SettingValue<Color>(Color.white);
        public static SettingValue<Color> selectBoneColor = new SettingValue<Color>(Color.red);
        public static SettingValue<Color> highlightColor = new SettingValue<Color>(ColorUtil.FromHex("b80000"));
        public static SettingValue<Color> ikChainColor = new SettingValue<Color>(ColorUtil.FromHex("0000bf"));
        public static bool experimental = false;
        public static float physicsRadius = 0.05f;
        public static float gravityFloor = 0f;
        public static bool gravityFloorOn = false;
        public static bool gravityOn = false;
        public static float gravityStrength = 0.05f;
        public static float gravityX, gravityY = -1f, gravityZ;
        public static float windowWidth = 720f;
        public static float windowHeight = 850f;

        public static SettingValue<BodyBoneDisplay> bodyBoneDisplay = new SettingValue<BodyBoneDisplay>(BodyBoneDisplay.Visible);

        public class SettingValue<T> {
            T value;
            public delegate void Delegate(T ret);
            public event Delegate mDelegate = delegate (T ret) { };

            public SettingValue(T initValue) {
                value = initValue;
            }

            public T GetValue() {
                return value;
            }

            public void SetValue(T fValue) {
                value = fValue;
                mDelegate(value);
            }
        }

        public static void LoadIni() {
            LoadSetting();
            LoadBoneDisplay();
            LoadGizmoDisplay();
        }

        static void LoadSetting() {
            boneDisplay = (BoneDisplay)IniUtil.GetIntValue("Setting", "BoneDisplay", (int)BoneDisplay.Visible);
            coordinateType = (BoneGizmoRenderer.COORDINATE)IniUtil.GetIntValue("Setting", "CoordinateType", (int)BoneGizmoRenderer.COORDINATE.Local);
            gizmoType = (GizmoType)IniUtil.GetIntValue("Setting", "GizmoType", (int)GizmoType.Rotation);
            gravityOn = IniUtil.GetIntValue("Gravity", "On", 0) != 0;
            gravityStrength = IniUtil.GetFloatValue("Gravity", "Strength", 0.05f);
        }

        static void LoadBoneDisplay() {
            boneSelectKey = (KeyCode)IniUtil.GetIntValue("BoneDisplay", "BoneSelectKey", (int)KeyCode.LeftAlt);
            normalBoneColor = new SettingValue<Color>(ColorUtil.GetColorFromName(IniUtil.GetStringValue("BoneDisplay", "NormalBoneColor", "white")));
            selectBoneColor = new SettingValue<Color>(ColorUtil.GetColorFromName(IniUtil.GetStringValue("BoneDisplay", "SelectBoneColor", "red")));
            highlightColor = new SettingValue<Color>(ColorUtil.ParseColor(IniUtil.GetStringValue("BoneDisplay", "HighlightColor", "#b80000")));
            ikChainColor = new SettingValue<Color>(ColorUtil.ParseColor(IniUtil.GetStringValue("BoneDisplay", "IKChainColor", "#0000bf")));
            windowWidth = IniUtil.GetFloatValue("Window", "Width", 720f);
            windowHeight = IniUtil.GetFloatValue("Window", "Height", 850f);
            bodyBoneDisplay = new SettingValue<BodyBoneDisplay>((BodyBoneDisplay)IniUtil.GetIntValue("BoneDisplay", "BodyBoneDisplay", (int)BodyBoneDisplay.Visible));
        }

        static void LoadGizmoDisplay() {
            gizmoSmallKey = (KeyCode)IniUtil.GetIntValue("GizmoDisplay", "GizmoSmallKey", (int)KeyCode.LeftShift);
            gizmoBigKey = (KeyCode)IniUtil.GetIntValue("GizmoDisplay", "GizmoBigKey", (int)KeyCode.None);
            ikPlaneKey = (KeyCode)IniUtil.GetIntValue("Hotkeys", "IKPlane", (int)KeyCode.Space);
            ikAxisKey = (KeyCode)IniUtil.GetIntValue("Hotkeys", "IKAxis", (int)KeyCode.LeftControl);
            ikAngleKey = (KeyCode)IniUtil.GetIntValue("Hotkeys", "IKAngle", (int)KeyCode.LeftShift);
        }

        public static void SaveIni() {
            SaveSetting();
            SaveBoneDisplay();
            SaveGizmoDisplay();
            IniUtil.Save();
        }

        static void SaveSetting() {
            IniUtil.preferences["Setting"]["BoneDisplay"].Value = ((int)boneDisplay).ToString();
            IniUtil.preferences["Setting"]["CoordinateType"].Value = ((int)coordinateType).ToString();
            IniUtil.preferences["Setting"]["GizmoType"].Value = ((int)gizmoType).ToString();
            IniUtil.preferences["Gravity"]["On"].Value = gravityOn.ToString();
            IniUtil.preferences["Gravity"]["Strength"].Value = gravityStrength.ToString();
        }

        static void SaveBoneDisplay() {
            IniUtil.preferences["BoneDisplay"]["BoneSelectKey"].Value = ((int)boneSelectKey).ToString();
            IniUtil.preferences["BoneDisplay"]["NormalBoneColor"].Value = ColorUtil.GetNameFromColor(normalBoneColor.GetValue());
            IniUtil.preferences["BoneDisplay"]["SelectBoneColor"].Value = ColorUtil.GetNameFromColor(selectBoneColor.GetValue());
            IniUtil.preferences["BoneDisplay"]["HighlightColor"].Value = ColorUtil.ToHex(highlightColor.GetValue());
            IniUtil.preferences["BoneDisplay"]["IKChainColor"].Value = ColorUtil.ToHex(ikChainColor.GetValue());
            IniUtil.preferences["Window"]["Width"].Value = windowWidth.ToString();
            IniUtil.preferences["Window"]["Height"].Value = windowHeight.ToString();
            IniUtil.preferences["BoneDisplay"]["BodyBoneDisplay"].Value = ((int)bodyBoneDisplay.GetValue()).ToString();
        }

        static void SaveGizmoDisplay() {
            IniUtil.preferences["GizmoDisplay"]["GizmoSmallKey"].Value = ((int)gizmoSmallKey).ToString();
            IniUtil.preferences["GizmoDisplay"]["GizmoBigKey"].Value = ((int)gizmoBigKey).ToString();
            IniUtil.preferences["Hotkeys"]["IKPlane"].Value = ((int)ikPlaneKey).ToString();
            IniUtil.preferences["Hotkeys"]["IKAxis"].Value = ((int)ikAxisKey).ToString();
            IniUtil.preferences["Hotkeys"]["IKAngle"].Value = ((int)ikAngleKey).ToString();
        }
    }

    enum Mode {
        Edit,
        Import,
        Export,
        BoneDisplaySetting,
        GizmoSetting,
        Set,
        Copy,
        IK,
        IKLimit,
        IKGravity,
        Experimental,
    }

    enum BoneDisplay {
        None,
        Visible,
        Choisable
    }

    enum GizmoType {
        None,
        Position,
        Rotation,
        Scale,
        IK
    }

    enum TargetType {
        MaidParts,
        MultipleMaids
    }

    enum BodyBoneDisplay {
        Invisible,
        Visible
    }
}
