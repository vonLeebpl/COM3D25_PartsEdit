using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class SetUI {
        string setName = "";
        string status = "";
        bool skipNameMismatch = false;
        bool deleteMembers = false;
        bool includeIK = true;
        Vector2 scroll;
        Dictionary<int, bool> checks = new Dictionary<int, bool>();
        int setIndex = 0;
        string[] setNames = new string[0];
        GUIContent[] setContents = new GUIContent[] { new GUIContent(Tr.T(Text.NoneSelected)) };
        ComboBoxLO setCombo;

        public void Draw() {
            GUILayout.Label(Text.Set, UIParams.Instance.lStyle);
            GUILayout.Label(Tr.T("チェックしたカテゴリをまとめて保存します。各カテゴリは通常のxmlのままです。"), UIParams.Instance.lStyle);
            DrawSave();
            GUILayout.Space(8);
            DrawLoad();
            GUILayout.FlexibleSpace();
            if (!string.IsNullOrEmpty(status)) GUILayout.Label(status, UIParams.Instance.lStyle);
            DrawBack();
        }

        void DrawSave() {
            if (!CommonUIData.maid) {
                GUILayout.Label(Tr.T("先にメイドを選択してください。"), UIParams.Instance.lStyle);
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label(Text.Name, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            setName = GUILayout.TextField(setName, UIParams.Instance.textStyle);
            GUILayout.EndHorizontal();

            List<int> slots = new List<int> { (int)EXSlot.Base };
            slots.AddRange(SetManager.GetEquippedSlotIds(CommonUIData.maid));
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(180));
            foreach (int slotNo in slots) {
                if (!checks.ContainsKey(slotNo)) checks[slotNo] = false;
                checks[slotNo] = GUILayout.Toggle(checks[slotNo], SetManager.SlotLabel(CommonUIData.maid, slotNo), UIParams.Instance.tStyle);
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Text.All, UIParams.Instance.bStyle)) foreach (int slotNo in slots) checks[slotNo] = true;
            if (GUILayout.Button(Text.NoneWord, UIParams.Instance.bStyle)) foreach (int slotNo in slots) checks[slotNo] = false;
            GUI.enabled = setName != "";
            if (GUILayout.Button(Text.SaveSet, UIParams.Instance.bStyle)) {
                status = SetManager.SaveSet(CommonUIData.maid, setName, checks.Where(pair => pair.Value).Select(pair => pair.Key).ToList(), includeIK);
                RefreshSets();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        void DrawLoad() {
            if (setCombo == null) RefreshSets();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Tr.T("読込"), UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            setIndex = setCombo.ShowScroll(GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();
            skipNameMismatch = GUILayout.Toggle(skipNameMismatch, Tr.T("オブジェクト名が違う場合はスキップ"), UIParams.Instance.tStyle);
            includeIK = GUILayout.Toggle(includeIK, Text.IKInclude, UIParams.Instance.tStyle);
            deleteMembers = GUILayout.Toggle(deleteMembers, Tr.T("削除時に構成xmlも消す"), UIParams.Instance.tStyle);
            GUILayout.BeginHorizontal();
            GUI.enabled = setIndex > 0 && CommonUIData.maid;
            if (GUILayout.Button(Text.LoadSet, UIParams.Instance.bStyle)) {
                status = SetManager.LoadSet(CommonUIData.maid, setNames[setIndex - 1], skipNameMismatch, includeIK);
            }
            GUI.enabled = setIndex > 0;
            if (GUILayout.Button(Text.DeleteSet, UIParams.Instance.bStyle)) {
                status = SetManager.DeleteSet(setNames[setIndex - 1], deleteMembers);
                RefreshSets();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            DrawAutoload();
        }

        void DrawAutoload() {
            GUILayout.Space(6);
            GUILayout.Label(Text.Autoload, UIParams.Instance.lStyle);
            string bound = CommonUIData.maid ? SetManager.GetAutoloadSet(CommonUIData.maid) : "";
            GUILayout.Label(string.IsNullOrEmpty(bound) ? Tr.T("このメイドにセットは未設定です。") : Tr.T("このメイドの自動読込: ") + bound, UIParams.Instance.lStyle);
            GUILayout.BeginHorizontal();
            GUI.enabled = setIndex > 0 && CommonUIData.maid;
            if (GUILayout.Button(Tr.T("このメイドにこのセットを自動読込"), UIParams.Instance.bStyle)) {
                status = SetManager.BindAutoload(CommonUIData.maid, setNames[setIndex - 1]);
            }
            GUI.enabled = CommonUIData.maid && !string.IsNullOrEmpty(bound);
            if (GUILayout.Button(Tr.T("自動読込を解除"), UIParams.Instance.bStyle)) {
                status = SetManager.ClearAutoload(CommonUIData.maid);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        void RefreshSets() {
            setNames = SetManager.GetSetNames();
            setContents = new GUIContent[setNames.Length + 1];
            setContents[0] = new GUIContent(Tr.T(Text.NoneSelected));
            for (int i = 0; i < setNames.Length; i++) setContents[i + 1] = new GUIContent(setNames[i]);
            setIndex = 0;
            setCombo = new ComboBoxLO(setContents[0], setContents, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
        }

        void DrawBack() {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            GUILayout.EndHorizontal();
        }
    }
}
