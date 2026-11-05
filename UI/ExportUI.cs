using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class ExportUI {
        string fileName = "";
        ObjectData objectData = null;
        public void Draw() {
            if (objectData == null) {
                objectData = PresetManager.GetObjectDataFromObject();
            }

            DrawFileNameInput();

            GUILayout.FlexibleSpace();
            DrawBackButton();
        }

        void DrawFileNameInput() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.FileName, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
                fileName = GUILayout.TextField(fileName, UIParams.Instance.textStyle, GUILayout.ExpandWidth(true));
                if (fileName == "") {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Export", UIParams.Instance.bStyle, GUILayout.ExpandWidth(false))) {
                    PresetManager.SaveObjectData(fileName);
                    if (IKManager.includeInPreset) IKManager.Save();
                    Setting.mode = Mode.Edit;
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            IKManager.includeInPreset = GUILayout.Toggle(IKManager.includeInPreset, Text.IKInclude, UIParams.Instance.tStyle);
        }

        void DrawObjectData() {
            if (objectData.rootData != null) {
                GUILayout.Label("RootObject");
            }
        }

        void DrawBackButton() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) {
                    Setting.mode = Mode.Edit;
                    objectData = null;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
