using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class CopyUI {
        MaidObserver observer = new MaidObserver();
        List<Maid> maids = new List<Maid>();
        // zrodla = aktywne pokojowki poza wybrana; miejscem docelowym jest pokojowka wybrana na glownym ekranie
        List<Maid> sources = new List<Maid>();
        Maid lastTarget;
        int sourceIndex = 0;
        GUIContent[] maidContents = new GUIContent[] { new GUIContent(Tr.T(Text.NoneSelected)) };
        ComboBoxLO sourceCombo;
        Dictionary<int, bool> slotChecks = new Dictionary<int, bool>();
        bool copyYure = true;
        bool skipNameMismatch = false;
        string status = "";
        Vector2 slotScroll;

        public void Draw() {
            Maid target = CommonUIData.maid;
            if (sourceCombo == null || observer.CheckActiveChange() || target != lastTarget) RefreshMaids(target);
            GUILayout.Label(Text.Copy, UIParams.Instance.lStyle);
            GUILayout.Label(Tr.T("別のメイドから、選択中のメイドへカテゴリ編集をコピーします。"), UIParams.Instance.lStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Text.Source, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            int next = sourceCombo.ShowScroll(GUILayout.ExpandHeight(false));
            if (next != sourceIndex) {
                sourceIndex = next;
                slotChecks.Clear();
            }
            GUILayout.EndHorizontal();

            Maid source = sourceIndex > 0 && sourceIndex <= sources.Count ? sources[sourceIndex - 1] : null;
            DrawSlots(source);
            DrawDestination(target);

            copyYure = GUILayout.Toggle(copyYure, Tr.T("揺れ状態もコピー"), UIParams.Instance.tStyle);
            skipNameMismatch = GUILayout.Toggle(skipNameMismatch, Tr.T("オブジェクト名が違う場合はスキップ"), UIParams.Instance.tStyle);
            GUI.enabled = source && target;
            if (GUILayout.Button(Text.Copy, UIParams.Instance.bStyle)) {
                status = SetManager.CopyCategories(
                    source,
                    new List<Maid> { target },
                    slotChecks.Where(pair => pair.Value).Select(pair => pair.Key).ToList(),
                    copyYure,
                    skipNameMismatch);
            }
            GUI.enabled = true;
            if (!string.IsNullOrEmpty(status)) GUILayout.Label(status, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            GUILayout.EndHorizontal();
        }

        void DrawSlots(Maid source) {
            GUILayout.Label(Text.Categories, UIParams.Instance.lStyle);
            if (!source) {
                GUILayout.Label(Tr.T("コピー元メイドを選択してください。"), UIParams.Instance.lStyle);
                return;
            }
            List<int> slots = new List<int> { (int)EXSlot.Base };
            slots.AddRange(SetManager.GetEquippedSlotIds(source));
            slotScroll = GUILayout.BeginScrollView(slotScroll, GUILayout.Height(120));
            foreach (int slotNo in slots) {
                if (!slotChecks.ContainsKey(slotNo)) slotChecks[slotNo] = false;
                slotChecks[slotNo] = GUILayout.Toggle(slotChecks[slotNo], SetManager.SlotLabel(source, slotNo), UIParams.Instance.tStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Text.All, UIParams.Instance.bStyle)) foreach (int slotNo in slots) slotChecks[slotNo] = true;
            if (GUILayout.Button(Text.NoneWord, UIParams.Instance.bStyle)) foreach (int slotNo in slots) slotChecks[slotNo] = false;
            GUILayout.EndHorizontal();
        }

        // Miejsce docelowe: pokojowka wybrana na glownym ekranie (jedna).
        void DrawDestination(Maid target) {
            GUILayout.Label(Text.Destinations, UIParams.Instance.lStyle);
            if (!target) {
                GUILayout.Label(Tr.T("コピー先のメイドが選択されていません。"), UIParams.Instance.lStyle);
                return;
            }
            GUILayout.Label(MaidName(target), UIParams.Instance.lStyleB);
        }

        void RefreshMaids(Maid target) {
            observer.UpdateMaidActiveState();
            maids = observer.GetActiveMaidList();
            sources = maids.Where(m => m != target).ToList();
            lastTarget = target;
            maidContents = new GUIContent[sources.Count + 1];
            maidContents[0] = new GUIContent(Tr.T(Text.NoneSelected));
            for (int i = 0; i < sources.Count; i++) maidContents[i + 1] = new GUIContent(MaidName(sources[i]));
            sourceIndex = 0;
            sourceCombo = new ComboBoxLO(maidContents[0], maidContents, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
            slotChecks.Clear();
        }

        string MaidName(Maid maid) {
#if COM3D2
            return maid.status.lastName + " " + maid.status.firstName;
#else
            return maid.Param.status.last_name + " " + maid.Param.status.first_name;
#endif
        }
    }
}
