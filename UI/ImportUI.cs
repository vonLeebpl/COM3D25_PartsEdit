using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class ImportUI {
        int index = 0;
        string[] fileNameList = null;
        GUIContent[] contentList = null;
        ComboBoxLO combo = null;
        string selectedCategory = null;
        string objectName = null;

        // カテゴリー絞り込み
        int cateSelectNum = 0;
        GUIContent[] cateSelectList = new GUIContent[] {
            new GUIContent(Tr.T(Text.None)),
            new GUIContent(Tr.T(Text.SameCategory)),
            new GUIContent(Tr.T(Text.Selected))
        };
        int cateComboNum = 0;
        string[] cateNameList = null;
        GUIContent[] cateComboList = null;
        ComboBoxLO cateCombo = null;

        // オブジェクト名絞り込み
        int objectNameSelectNum = 0;
        GUIContent[] objectNameSelectList = new GUIContent[] {
            new GUIContent(Tr.T(Text.None)),
            new GUIContent(Tr.T(Text.SameName)),
            //new GUIContent(Tr.T(Text.Selected))
        };
        //int objectNameComboNum = 0;
        //string[] objectNameList = null;
        //GUIContent[] objectNameComboList = null;
        //ComboBoxLO objectNameCombo = null;

        public ImportUI() {
            InitCategorySelect();
        }

        void InitCategorySelect() {
            int cateNum = (TBody.m_strDefSlotName.Length - 1) / 3;
            cateNameList = new string[cateNum];
            cateComboList = new GUIContent[cateNum];
            for (int i = 0; i < cateNum; i++) {
                cateNameList[i] = TBody.m_strDefSlotName[i * 3];
                cateComboList[i] = new GUIContent(TBody.m_strDefSlotName[i * 3]);
            }
            cateCombo = new ComboBoxLO(cateComboList[0], cateComboList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
            ResetCategorySelect();
        }

        void ResetCategorySelect() {
            switch (cateSelectNum) {
                case 0:
                    selectedCategory = null;
                    break;
                case 1:
                    if (Setting.targetSelectMode != 0) {
                        selectedCategory = null;
                    } else {
                        if (CommonUIData.slotNo == (int)EXSlot.Base) {
                            selectedCategory = "base";
                        } else {
                            selectedCategory = cateNameList[CommonUIData.slotNo];
                        }
                    }
                    break;
                case 2:
                    selectedCategory = cateNameList[cateComboNum];
                    break;
            }
            ResetFileList();
        }

        void ResetObjectNameSelect() {
            switch (objectNameSelectNum) {
                case 0:
                    objectName = null;
                    break;
                case 1:
                    objectName = CommonUIData.obj.name;
                    break;
            }
            ResetFileList();
        }

        void ResetObjectName() {
            if (objectNameSelectNum == 0) {
                objectName = null;
            } else {
                objectName = CommonUIData.obj.name;
            }
        }

        public void Draw() {
            DrawPresetFile();
            GUILayout.FlexibleSpace();
            DrawBackButton();
        }

        void DrawPresetFile() {
            GUILayout.Label(Text.PresetFile, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            {
                DrawNarrowing();
                DrawFileList();
                DrawImportButton();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawNarrowing() {
            GUILayout.Label(Text.Filter, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            {
                DrawCategoryNarrow();
                DrawObjectNameNarrow();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawCategoryNarrow() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.Category, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                int tempNum = GUILayout.Toolbar(cateSelectNum, cateSelectList, UIParams.Instance.tStyle);
                if (cateSelectNum != tempNum) {
                    cateSelectNum = tempNum;
                    ResetCategorySelect();
                }

                if (cateSelectNum != 2) {
                    GUI.enabled = false;
                }
                int tempComboNum = cateCombo.ShowScroll(GUILayout.ExpandHeight(false));
                if (cateComboNum != tempComboNum) {
                    ResetCategorySelect();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }

        void DrawObjectNameNarrow() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.ObjectName, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                int tempNum = GUILayout.Toolbar(objectNameSelectNum, objectNameSelectList, UIParams.Instance.tStyle);
                if (objectNameSelectNum != tempNum) {
                    objectNameSelectNum = tempNum;
                    ResetObjectNameSelect();
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawFileList() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.SelectFile, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                index = combo.ShowScroll(GUILayout.ExpandHeight(false));
            }
            GUILayout.EndHorizontal();
        }

        void DrawImportButton() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (index <= 0) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Import", UIParams.Instance.bStyle)) {
                    PresetManager.LoadObjectData(fileNameList[index - 1]);
                    Setting.mode = Mode.Edit;
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }

        void DrawBackButton() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) {
                    Setting.mode = Mode.Edit;
                }
            }
            GUILayout.EndHorizontal();
        }

        public void ResetFileList() {
            index = 0;
            fileNameList = PresetManager.GetFileList(selectedCategory, objectName);
            contentList = new GUIContent[fileNameList.Length + 1];
            contentList[0] = new GUIContent(Tr.T(Text.NoneSelected));
            for(int i = 0; i < fileNameList.Length; i++) {
                contentList[i + 1] = new GUIContent(fileNameList[i]);
            }
            combo = new ComboBoxLO(contentList[0], contentList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
        }

        public void Reset() {
            cateSelectNum = 0;
            objectNameSelectNum = 0;
            ResetCategorySelect();
            ResetObjectName();
            ResetObjectNameSelect();
        }
    }
}
