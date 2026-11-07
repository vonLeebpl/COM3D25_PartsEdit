using System;
using System.Collections.Generic;
using System.Linq;
using CM3D2.PartsEdit.Plugin.Db;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Panel Copy: kopiowanie NA pokojowke wybrana na glownym ekranie Z innej postaci.
    //  - zrodlem moze byc postac z biblioteki (ma sety / modyfikacje w bazie) i/lub postac zaladowana w scenie;
    //  - z biblioteki kopiujemy przypisania: wybrane sety i modyfikacje dochodza do wybranej pokojowki i sa od razu nakladane;
    //  - ze sceny kopiujemy biezace edycje kategorii (jak dawniej, SetManager.CopyCategories).
    // Filtr "tylko w scenie" zaweza liste zrodel do postaci obecnych w scenie.
    class CopyUI {
        class Source {
            public string Guid = "";
            public string Name = "";
            public Maid SceneMaid; // null, gdy postaci nie ma w scenie
        }

        MaidObserver observer = new MaidObserver();
        List<Source> allSources = new List<Source>();
        List<Source> shown = new List<Source>();
        GUIContent[] contents = new GUIContent[] { new GUIContent(Tr.T(Text.NoneSelected)) };
        ComboBoxLO sourceCombo;
        int sourceIndex = 0;
        string sourceGuid = "";
        Maid lastTarget;
        bool onlyScene = false;

        Dictionary<int, bool> slotChecks = new Dictionary<int, bool>();
        Dictionary<Guid, bool> entryChecks = new Dictionary<Guid, bool>();
        Dictionary<Guid, bool> setChecks = new Dictionary<Guid, bool>();
        bool copyYure = true;
        bool skipNameMismatch = false;
        string status = "";
        Vector2 slotScroll;
        Vector2 libScroll;

        static PartsEditDb Db { get { return PartsEditStore.Db; } }

        public void Draw() {
            Maid target = CommonUIData.maid;
            if (sourceCombo == null || observer.CheckActiveChange() || target != lastTarget) Refresh(target);

            GUILayout.Label(Text.Copy, UIParams.Instance.lStyle);
            GUILayout.Label(Tr.T("別のメイドから、選択中のメイドへコピーします。"), UIParams.Instance.lStyle);

            bool newOnlyScene = GUILayout.Toggle(onlyScene, Tr.T("シーン内のメイドのみ"), UIParams.Instance.tStyle);
            if (newOnlyScene != onlyScene) {
                onlyScene = newOnlyScene;
                Refresh(target);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(Text.Source, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            int next = sourceCombo.ShowScroll(GUILayout.ExpandHeight(false));
            if (next != sourceIndex) {
                sourceIndex = next;
                sourceGuid = sourceIndex > 0 && sourceIndex <= shown.Count ? shown[sourceIndex - 1].Guid : "";
                slotChecks.Clear();
                entryChecks.Clear();
                setChecks.Clear();
            }
            GUILayout.EndHorizontal();
            if (shown.Count == 0) GUILayout.Label(Tr.T("コピーできる他のメイドがいません。"), UIParams.Instance.lStyle);

            Source source = sourceIndex > 0 && sourceIndex <= shown.Count ? shown[sourceIndex - 1] : null;
            DrawDestination(target);

            if (source == null) {
                GUILayout.Label(Tr.T("コピー元メイドを選択してください。"), UIParams.Instance.lStyle);
            } else {
                DrawLibrary(source, target);
                DrawScene(source, target);
            }

            if (!string.IsNullOrEmpty(status)) GUILayout.Label(status, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            GUILayout.EndHorizontal();
        }

        // ---------- biblioteka ----------

        List<ModEntry> EntriesOf(string guid) {
            if (Db == null || string.IsNullOrEmpty(guid)) return new List<ModEntry>();
            return Db.Entries.Values.Where(e => Db.IsEntryOfMaid(guid, e)).OrderBy(e => e.Slot).ThenBy(e => e.Name).ToList();
        }

        List<ModSet> SetsOf(string guid) {
            if (Db == null || string.IsNullOrEmpty(guid)) return new List<ModSet>();
            return Db.Sets.Values.Where(s => Db.IsSetOfMaid(guid, s)).OrderBy(s => s.Name).ToList();
        }

        void DrawLibrary(Source source, Maid target) {
            List<ModSet> sets = SetsOf(source.Guid);
            List<ModEntry> entries = EntriesOf(source.Guid);
            GUILayout.Label(Tr.T("ライブラリ"), UIParams.Instance.lStyleB);
            if (sets.Count == 0 && entries.Count == 0) {
                GUILayout.Label(Tr.T("このメイドにはライブラリの項目がありません。"), UIParams.Instance.lStyle);
                return;
            }

            libScroll = GUILayout.BeginScrollView(libScroll, GUILayout.Height(150));
            foreach (ModSet set in sets) {
                bool on;
                if (!setChecks.TryGetValue(set.Id, out on)) setChecks[set.Id] = false;
                setChecks[set.Id] = GUILayout.Toggle(setChecks[set.Id], string.Format(Tr.T("{0}（セット）"), set.Name), UIParams.Instance.tStyle);
            }
            foreach (ModEntry entry in entries) {
                bool on;
                if (!entryChecks.TryGetValue(entry.Id, out on)) entryChecks[entry.Id] = false;
                entryChecks[entry.Id] = GUILayout.Toggle(entryChecks[entry.Id], entry.Name + " [" + entry.Slot + "]", UIParams.Instance.tStyle);
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Text.All, UIParams.Instance.bStyle)) {
                foreach (ModSet s in sets) setChecks[s.Id] = true;
                foreach (ModEntry e in entries) entryChecks[e.Id] = true;
            }
            if (GUILayout.Button(Text.NoneWord, UIParams.Instance.bStyle)) {
                foreach (ModSet s in sets) setChecks[s.Id] = false;
                foreach (ModEntry e in entries) entryChecks[e.Id] = false;
            }
            GUILayout.EndHorizontal();

            bool any = setChecks.Values.Any(v => v) || entryChecks.Values.Any(v => v);
            GUI.enabled = target && any;
            if (GUILayout.Button(Tr.T("ライブラリの項目をコピー"), UIParams.Instance.bStyle)) CopyLibrary(target, sets, entries);
            GUI.enabled = true;
        }

        void CopyLibrary(Maid target, List<ModSet> sets, List<ModEntry> entries) {
            string targetGuid = SetManager.MaidGuid(target);
            if (Db == null || string.IsNullOrEmpty(targetGuid)) return;
            MaidBinding binding = Db.GetOrCreateMaid(targetGuid, LibraryService.MaidName(target));
            int setCount = 0, entryCount = 0;
            List<ModEntry> toApply = new List<ModEntry>();
            foreach (ModSet set in sets) {
                bool on;
                if (!setChecks.TryGetValue(set.Id, out on) || !on) continue;
                if (!binding.SetIds.Contains(set.Id)) binding.SetIds.Add(set.Id);
                setCount++;
                foreach (Guid id in set.EntryIds) {
                    ModEntry e;
                    if (Db.Entries.TryGetValue(id, out e)) toApply.Add(e);
                }
            }
            foreach (ModEntry entry in entries) {
                bool on;
                if (!entryChecks.TryGetValue(entry.Id, out on) || !on) continue;
                if (!binding.EntryIds.Contains(entry.Id)) binding.EntryIds.Add(entry.Id);
                entryCount++;
                toApply.Add(entry);
            }
            PartsEditStore.Save();
            int applied = LibraryService.ApplyAll(target, toApply).Count(r => r.Status == ApplyStatus.Applied);
            status = string.Format(Tr.T("コピーしました: セット {0}、修正 {1}（適用 {2}）"), setCount, entryCount, applied);
        }

        // ---------- scena ----------

        void DrawScene(Source source, Maid target) {
            if (!source.SceneMaid) return;
            GUILayout.Label(Tr.T("シーン内のカテゴリ編集"), UIParams.Instance.lStyleB);
            List<int> slots = new List<int> { (int)EXSlot.Base };
            slots.AddRange(SetManager.GetEquippedSlotIds(source.SceneMaid));
            slotScroll = GUILayout.BeginScrollView(slotScroll, GUILayout.Height(120));
            foreach (int slotNo in slots) {
                if (!slotChecks.ContainsKey(slotNo)) slotChecks[slotNo] = false;
                slotChecks[slotNo] = GUILayout.Toggle(slotChecks[slotNo], SetManager.SlotLabel(source.SceneMaid, slotNo), UIParams.Instance.tStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Text.All, UIParams.Instance.bStyle)) foreach (int slotNo in slots) slotChecks[slotNo] = true;
            if (GUILayout.Button(Text.NoneWord, UIParams.Instance.bStyle)) foreach (int slotNo in slots) slotChecks[slotNo] = false;
            GUILayout.EndHorizontal();

            copyYure = GUILayout.Toggle(copyYure, Tr.T("揺れ状態もコピー"), UIParams.Instance.tStyle);
            skipNameMismatch = GUILayout.Toggle(skipNameMismatch, Tr.T("オブジェクト名が違う場合はスキップ"), UIParams.Instance.tStyle);
            GUI.enabled = target && slotChecks.Values.Any(v => v);
            if (GUILayout.Button(Text.Copy, UIParams.Instance.bStyle)) {
                status = SetManager.CopyCategories(
                    source.SceneMaid,
                    new List<Maid> { target },
                    slotChecks.Where(pair => pair.Value).Select(pair => pair.Key).ToList(),
                    copyYure,
                    skipNameMismatch);
            }
            GUI.enabled = true;
        }

        // Miejsce docelowe: pokojowka wybrana na glownym ekranie (jedna).
        void DrawDestination(Maid target) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Text.Destinations, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            if (!target) GUILayout.Label(Tr.T("コピー先のメイドが選択されていません。"), UIParams.Instance.lStyle);
            else GUILayout.Label(MaidName(target), UIParams.Instance.lStyleB);
            GUILayout.EndHorizontal();
        }

        // ---------- lista zrodel ----------

        void Refresh(Maid target) {
            observer.UpdateMaidActiveState();
            List<Maid> sceneMaids = observer.GetActiveMaidList();
            string targetGuid = target ? SetManager.MaidGuid(target) : "";
            lastTarget = target;

            Dictionary<string, Source> byGuid = new Dictionary<string, Source>();
            // postacie z biblioteki: te, ktore maja zapisane wpisy / sety albo przypisania
            if (Db != null) {
                foreach (MaidBinding b in Db.Maids.Values) {
                    if (string.IsNullOrEmpty(b.MaidGuid) || b.MaidGuid == targetGuid) continue;
                    if (b.SetIds.Count == 0 && b.EntryIds.Count == 0) continue;
                    AddKnown(byGuid, b.MaidGuid, b.LastSeenName);
                }
                foreach (ModEntry e in Db.Entries.Values) {
                    if (!string.IsNullOrEmpty(e.OwnerGuid) && e.OwnerGuid != targetGuid) AddKnown(byGuid, e.OwnerGuid, e.OwnerName);
                }
                foreach (ModSet s in Db.Sets.Values) {
                    if (!string.IsNullOrEmpty(s.OwnerGuid) && s.OwnerGuid != targetGuid) AddKnown(byGuid, s.OwnerGuid, s.OwnerName);
                }
            }
            // postacie ze sceny (takze te, ktorych nie ma w bazie)
            foreach (Maid m in sceneMaids) {
                string guid = SetManager.MaidGuid(m);
                if (m == target || string.IsNullOrEmpty(guid) || guid == targetGuid) continue;
                Source s;
                if (!byGuid.TryGetValue(guid, out s)) { s = new Source(); s.Guid = guid; byGuid[guid] = s; }
                s.SceneMaid = m;
                s.Name = MaidName(m);
            }

            allSources = byGuid.Values.OrderBy(s => s.Name).ToList();
            shown = allSources.Where(s => !onlyScene || s.SceneMaid).ToList();

            contents = new GUIContent[shown.Count + 1];
            contents[0] = new GUIContent(Tr.T(Text.NoneSelected));
            for (int i = 0; i < shown.Count; i++) {
                string label = string.IsNullOrEmpty(shown[i].Name) ? shown[i].Guid : shown[i].Name;
                if (shown[i].SceneMaid) label += " (" + Tr.T("シーン内") + ")";
                contents[i + 1] = new GUIContent(label);
            }
            sourceIndex = 0;
            sourceGuid = "";
            sourceCombo = new ComboBoxLO(contents[0], contents, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
            slotChecks.Clear();
            entryChecks.Clear();
            setChecks.Clear();
        }

        static void AddKnown(Dictionary<string, Source> byGuid, string guid, string name) {
            Source s;
            if (!byGuid.TryGetValue(guid, out s)) { s = new Source(); s.Guid = guid; byGuid[guid] = s; }
            if (string.IsNullOrEmpty(s.Name) && !string.IsNullOrEmpty(name)) s.Name = name;
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
