using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BoneDisplaySettingUI {
        int keyIndex = 0;
        KeyCode[] keyCodeList = null;
        GUIContent[] keyContentList = null;
        ComboBoxLO keyCombo = null;
        string normalHex = "#FFFFFF";
        string selectHex = "#FF0000";
        string highlightHex = "#B80000";
        string ikHex = "#0000BF";

        int bodyBoneDisplayIndex = 0;
        GUIContent[] bodyBoneDsiplayContents = new GUIContent[] {
            new GUIContent(Tr.T(Text.Hidden)),
            new GUIContent(Tr.T(Text.Visible))
        };

        public BoneDisplaySettingUI() {
            InitColorList();
            InitKeyList();
        }

        public void Draw() {
            GUILayout.Label(Text.BoneDisplaySettings, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            {
                DrawNormalBoneColorSetting();
                DrawSelectBoneColorSetting();
                DrawHighlightColorSetting();
                DrawIKColorSetting();
                DrawSelectKeySetting();
                DrawBodyBoneDisplaySetting();
                GUILayout.FlexibleSpace();
                DrawReturnButton();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawNormalBoneColorSetting() {
            DrawHex(Text.NormalBoneColor, ref normalHex, Setting.normalBoneColor, false);
        }

        void DrawSelectBoneColorSetting() {
            DrawHex(Text.SelectedBoneColor, ref selectHex, Setting.selectBoneColor, false);
        }

        void DrawHighlightColorSetting() {
            DrawHex(Text.HighlightColor, ref highlightHex, Setting.highlightColor, true);
        }

        void DrawIKColorSetting() {
            DrawHex(Text.IKChainColor, ref ikHex, Setting.ikChainColor, false);
        }

        void DrawHex(string label, ref string hex, Setting.SettingValue<Color> target, bool highlight) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            string next = GUILayout.TextField(hex, UIParams.Instance.textStyle, GUILayout.Width(90));
            if (next != hex) {
                hex = next;
                Color color;
                if (ColorUtil.TryParseHex(hex, out color)) {
                    target.SetValue(color);
                    if (highlight) UIParams.Instance.ApplyHighlight(color);
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawSelectKeySetting() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.SelectKey, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                int tempIndex = keyCombo.ShowScroll(GUILayout.ExpandHeight(false));
                if (keyIndex != tempIndex) {
                    keyIndex = tempIndex;
                    Setting.boneSelectKey = keyCodeList[keyIndex];
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawBodyBoneDisplaySetting() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.BodyBoneDisplay, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                int tempIndex = GUILayout.Toolbar((int)(Setting.bodyBoneDisplay.GetValue()), bodyBoneDsiplayContents, UIParams.Instance.tStyle);
                if(bodyBoneDisplayIndex != tempIndex) {
                    bodyBoneDisplayIndex = tempIndex;
                    Setting.bodyBoneDisplay.SetValue((BodyBoneDisplay)tempIndex);
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawReturnButton() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) {
                    Setting.mode = Mode.Edit;
                    Setting.SaveIni();
                }
            }
            GUILayout.EndHorizontal();
        }

        void InitKeyList() {
            keyCodeList = (KeyCode[])Enum.GetValues(typeof(KeyCode));
            keyIndex = Array.IndexOf(keyCodeList, Setting.boneSelectKey);
            keyContentList = keyCodeList.Select(code => new GUIContent(code.ToString())).ToArray();
            keyCombo = new ComboBoxLO(keyContentList[keyIndex], keyContentList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
        }

        void InitColorList() {
            normalHex = ColorUtil.ToHex(Setting.normalBoneColor.GetValue());
            selectHex = ColorUtil.ToHex(Setting.selectBoneColor.GetValue());
            highlightHex = ColorUtil.ToHex(Setting.highlightColor.GetValue());
            ikHex = ColorUtil.ToHex(Setting.ikChainColor.GetValue());
            UIParams.Instance.ApplyHighlight(Setting.highlightColor.GetValue());
        }
    }
}
