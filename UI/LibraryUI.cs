using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.PartsEdit.Plugin.Db;

namespace CM3D2.PartsEdit.Plugin {
    // Ekran biblioteki modyfikacji (styl SKM): zakladki Maid / Library / Sets, wiersze z przyciskami, stronicowanie.
    // Teksty tego ekranu sa po angielsku (nie ida przez plik tlumaczen).
    class LibraryUI {
        enum Tab { Maid = 0, Library = 1, Sets = 2 }

        const int PageSize = 8;

        Tab tab = Tab.Maid;
        MaidSelectUI maidSelectUI = new MaidSelectUI();
        string status = "";

        // zakladka Library
        string search = "";
        int sortMode = 0;          // 0 nazwa, 1 data, 2 slot
        bool ascending = true;
        int page = 0;
        Guid renameId = Guid.Empty;
        string renameText = "";
        Guid confirmDelete = Guid.Empty;

        // zakladka Maid
        bool addEntryOpen = false;
        string addEntrySearch = "";
        bool addSetOpen = false;
        bool slotPickerOpen = false;
        int captureSlotIndex = -1;
        string captureName = "";
        int addPage = 0;

        // zakladka Sets
        string newSetName = "";
        Guid openSetId = Guid.Empty;
        bool setAddOpen = false;
        string setAddSearch = "";
        Guid confirmDeleteSet = Guid.Empty;
        string setRenameText = "";
        Guid setRenameId = Guid.Empty;
        int setsPage = 0;
        int setAddPage = 0;

        static PartsEditDb Db { get { return PartsEditStore.Db; } }

        // wolane przy wejsciu na ekran: odswieza liste pokojowek (po zmianie wyboru na glownym ekranie)
        public void OnEnter() {
            maidSelectUI = new MaidSelectUI();
            status = "";
        }

        public void Draw() {
            GUILayout.Label("Library", UIParams.Instance.lStyle);
            if (Db == null) {
                GUILayout.Label("The library is not available (see the log).", UIParams.Instance.lStyle);
                DrawBack();
                return;
            }

            maidSelectUI.Draw();
            DrawTabs();
            GUILayout.Space(6);

            Maid maid = CommonUIData.maid;
            switch (tab) {
                case Tab.Maid: DrawMaidTab(maid); break;
                case Tab.Library: DrawLibraryTab(maid); break;
                case Tab.Sets: DrawSetsTab(maid); break;
            }

            GUILayout.FlexibleSpace();
            if (!string.IsNullOrEmpty(status)) GUILayout.Label(status, UIParams.Instance.lStyle);
            DrawBack();
        }

        // ---------- wspolne ----------

        void DrawBack() {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            GUILayout.EndHorizontal();
        }

        void DrawTabs() {
            string[] names = new string[] { "Maid", "Library (" + Db.Entries.Count + ")", "Sets (" + Db.Sets.Count + ")" };
            int selected = GUILayout.Toolbar((int)tab, names, UIParams.Instance.bStyle);
            if (selected != (int)tab) {
                tab = (Tab)selected;
                page = 0; addPage = 0; setsPage = 0; setAddPage = 0;
                confirmDelete = Guid.Empty; confirmDeleteSet = Guid.Empty;
                renameId = Guid.Empty; setRenameId = Guid.Empty;
                status = "";
            }
        }

        static void Save() { PartsEditStore.Save(); }

        // Strony: zwraca nowy numer strony (przyciski < >), rysuje tylko gdy jest wiecej niz jedna strona.
        int DrawPager(int current, int total) {
            int pages = Math.Max(1, (total + PageSize - 1) / PageSize);
            if (current >= pages) current = pages - 1;
            if (current < 0) current = 0;
            if (pages > 1) {
                GUILayout.BeginHorizontal();
                GUI.enabled = current > 0;
                if (GUILayout.Button("<", UIParams.Instance.bStyle, GUILayout.Width(40))) current--;
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                GUILayout.Label((current + 1) + " / " + pages, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                GUI.enabled = current < pages - 1;
                if (GUILayout.Button(">", UIParams.Instance.bStyle, GUILayout.Width(40))) current++;
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            return current;
        }

        static IEnumerable<T> Slice<T>(List<T> list, int pageNo) {
            return list.Skip(pageNo * PageSize).Take(PageSize);
        }

        static string Short(string s, int max) {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
            return s.Substring(0, max - 1) + "…";
        }

        static string ResultText(List<ApplyResult> results) {
            int ok = results.Count(r => r.Status == ApplyStatus.Applied);
            StringBuilder sb = new StringBuilder();
            sb.Append("Applied " + ok + "/" + results.Count + ".");
            foreach (ApplyResult r in results.Where(r => r.Status != ApplyStatus.Applied)) sb.Append(" ").Append(r.ToString()).Append(";");
            return sb.ToString();
        }

        MaidBinding Binding(Maid maid, bool create) {
            string guid = SetManager.MaidGuid(maid);
            if (string.IsNullOrEmpty(guid)) return null;
            MaidBinding b;
            if (Db.Maids.TryGetValue(guid, out b)) return b;
            return create ? Db.GetOrCreateMaid(guid, LibraryService.MaidName(maid)) : null;
        }

        List<ModEntry> EntriesOf(ModSet set) {
            List<ModEntry> list = new List<ModEntry>();
            foreach (Guid id in set.EntryIds) {
                ModEntry e;
                if (Db.Entries.TryGetValue(id, out e)) list.Add(e);
            }
            return list;
        }

        IEnumerable<ModEntry> FilterEntries(string text) {
            IEnumerable<ModEntry> q = Db.Entries.Values;
            if (!string.IsNullOrEmpty(text)) {
                string t = text.ToLowerInvariant();
                q = q.Where(e => e.Name.ToLowerInvariant().Contains(t) || e.Slot.ToLowerInvariant().Contains(t) || e.ObjectName.ToLowerInvariant().Contains(t));
            }
            return q;
        }

        // ---------- zakladka Maid ----------

        void DrawMaidTab(Maid maid) {
            if (!maid) {
                GUILayout.Label("Select a maid first.", UIParams.Instance.lStyle);
                return;
            }
            MaidBinding b = Binding(maid, false);

            bool auto = b != null && b.AutoLoad;
            bool autoNew = GUILayout.Toggle(auto, "Auto-load when this maid loads", UIParams.Instance.tStyle);
            if (autoNew != auto) {
                Binding(maid, true).AutoLoad = autoNew;
                Save();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply all", UIParams.Instance.bStyle)) {
                status = ResultText(LibraryService.ApplyMaidProfile(maid));
            }
            if (GUILayout.Button("Revert maid", UIParams.Instance.bStyle)) {
                BackUpData.RestoreMaid(maid);
                status = "Reverted all changes of this maid.";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            DrawMaidSets(maid, b);
            GUILayout.Space(6);
            DrawMaidEntries(maid, b);
            GUILayout.Space(6);
            DrawCapture(maid);
        }

        void DrawMaidSets(Maid maid, MaidBinding b) {
            GUILayout.Label("Sets of this maid", UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            if (b == null || b.SetIds.Count == 0) GUILayout.Label("(none)", UIParams.Instance.lStyle);
            if (b != null) {
                Guid remove = Guid.Empty;
                foreach (Guid setId in b.SetIds) {
                    ModSet set;
                    if (!Db.Sets.TryGetValue(setId, out set)) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Short(set.Name, 28) + " (" + set.EntryIds.Count + ")", UIParams.Instance.lStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Apply", UIParams.Instance.bStyle, GUILayout.Width(70))) status = ResultText(LibraryService.ApplyAll(maid, EntriesOf(set)));
                    if (GUILayout.Button("x", UIParams.Instance.bStyle, GUILayout.Width(32))) remove = setId;
                    GUILayout.EndHorizontal();
                }
                if (remove != Guid.Empty) {
                    b.SetIds.Remove(remove);
                    Save();
                }
            }
            addSetOpen = GUILayout.Toggle(addSetOpen, "+ Add set", UIParams.Instance.tStyle);
            if (addSetOpen) {
                List<ModSet> choices = Db.Sets.Values.Where(s => b == null || !b.SetIds.Contains(s.Id)).OrderBy(s => s.Name).ToList();
                if (choices.Count == 0) GUILayout.Label("(no other sets, create one in the Sets tab)", UIParams.Instance.lStyle);
                foreach (ModSet s in choices.Take(PageSize)) {
                    if (GUILayout.Button(Short(s.Name, 40) + " (" + s.EntryIds.Count + ")", UIParams.Instance.bStyleL)) {
                        Binding(maid, true).SetIds.Add(s.Id);
                        addSetOpen = false;
                        Save();
                        status = "Set '" + s.Name + "' added to this maid.";
                    }
                }
            }
            UIUtil.EndoIndentArea();
        }

        void DrawMaidEntries(Maid maid, MaidBinding b) {
            GUILayout.Label("Modifications of this maid", UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            if (b == null || b.EntryIds.Count == 0) GUILayout.Label("(none)", UIParams.Instance.lStyle);
            if (b != null) {
                Guid remove = Guid.Empty;
                foreach (Guid entryId in b.EntryIds) {
                    ModEntry e;
                    if (!Db.Entries.TryGetValue(entryId, out e)) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Short(e.Name, 24) + "  [" + e.Slot + "]", UIParams.Instance.lStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Apply", UIParams.Instance.bStyle, GUILayout.Width(70))) status = ResultText(new List<ApplyResult> { LibraryService.Apply(maid, e) });
                    if (GUILayout.Button("x", UIParams.Instance.bStyle, GUILayout.Width(32))) remove = entryId;
                    GUILayout.EndHorizontal();
                }
                if (remove != Guid.Empty) {
                    b.EntryIds.Remove(remove);
                    Save();
                }
            }

            addEntryOpen = GUILayout.Toggle(addEntryOpen, "+ Add modification", UIParams.Instance.tStyle);
            if (addEntryOpen) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Search", UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
                string s2 = GUILayout.TextField(addEntrySearch, UIParams.Instance.textStyle);
                if (s2 != addEntrySearch) { addEntrySearch = s2; addPage = 0; }
                GUILayout.EndHorizontal();

                // domyslnie tylko wpisy pasujace do slotow zalozonych na pokojowce
                HashSet<string> equipped = new HashSet<string>(
                    SetManager.GetEquippedSlotIds(maid).Select(id => SlotUtil.GetSlotName(id)), StringComparer.OrdinalIgnoreCase);
                List<ModEntry> choices = FilterEntries(addEntrySearch)
                    .Where(e => equipped.Contains(e.Slot) && (b == null || !b.EntryIds.Contains(e.Id)))
                    .OrderBy(e => e.Name).ToList();
                if (choices.Count == 0) GUILayout.Label("(no matching modifications for the worn items)", UIParams.Instance.lStyle);
                foreach (ModEntry e in Slice(choices, addPage)) {
                    if (GUILayout.Button(Short(e.Name, 34) + "  [" + e.Slot + "]", UIParams.Instance.bStyleL)) {
                        Binding(maid, true).EntryIds.Add(e.Id);
                        Save();
                        status = ResultText(new List<ApplyResult> { LibraryService.Apply(maid, e) });
                    }
                }
                addPage = DrawPager(addPage, choices.Count);
            }
            UIUtil.EndoIndentArea();
        }

        // Zapis biezacego stanu slotu jako nowej modyfikacji (odpowiednik starego Export).
        void DrawCapture(Maid maid) {
            GUILayout.Label("Save current slot as a modification", UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();

            List<int> slotIds = SetManager.GetEquippedSlotIds(maid);
            slotIds.Insert(0, (int)EXSlot.Base);
            if (captureSlotIndex >= slotIds.Count) captureSlotIndex = -1;

            string current = captureSlotIndex >= 0 ? SetManager.SlotLabel(maid, slotIds[captureSlotIndex]) : "(choose slot)";
            slotPickerOpen = GUILayout.Toggle(slotPickerOpen, "Slot: " + current, UIParams.Instance.tStyle);
            if (slotPickerOpen) {
                for (int i = 0; i < slotIds.Count; i++) {
                    if (GUILayout.Button(SetManager.SlotLabel(maid, slotIds[i]), UIParams.Instance.bStyleL)) {
                        captureSlotIndex = i;
                        slotPickerOpen = false;
                        if (string.IsNullOrEmpty(captureName)) captureName = SlotUtil.GetSlotName(slotIds[i]);
                    }
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            captureName = GUILayout.TextField(captureName, UIParams.Instance.textStyle);
            GUILayout.EndHorizontal();

            GUI.enabled = captureSlotIndex >= 0 && !string.IsNullOrEmpty(captureName.Trim());
            if (GUILayout.Button("Save as modification (also assign to this maid)", UIParams.Instance.bStyle)) {
                ModEntry entry = LibraryService.CaptureSlot(maid, slotIds[captureSlotIndex], captureName.Trim());
                if (entry == null) {
                    status = "Nothing is equipped in this slot.";
                } else {
                    Db.AddEntry(entry);
                    Binding(maid, true).EntryIds.Add(entry.Id);
                    Save();
                    status = "Saved '" + entry.Name + "' [" + entry.Slot + "] (" + entry.Bones.Count + " bones).";
                }
            }
            GUI.enabled = true;
            UIUtil.EndoIndentArea();
        }

        // ---------- zakladka Library ----------

        void DrawLibraryTab(Maid maid) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            string s2 = GUILayout.TextField(search, UIParams.Instance.textStyle);
            if (s2 != search) { search = s2; page = 0; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Sort", UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            string[] modes = new string[] { "Name", "Date", "Slot" };
            if (GUILayout.Button(modes[sortMode], UIParams.Instance.bStyle)) sortMode = (sortMode + 1) % modes.Length;
            if (GUILayout.Button(ascending ? "Asc" : "Desc", UIParams.Instance.bStyle, GUILayout.Width(60))) ascending = !ascending;
            GUILayout.EndHorizontal();

            List<ModEntry> entries = Sorted(FilterEntries(search)).ToList();
            if (entries.Count == 0) GUILayout.Label("(library is empty)", UIParams.Instance.lStyle);

            Guid delete = Guid.Empty;
            foreach (ModEntry e in Slice(entries, page)) {
                GUILayout.BeginVertical(UIParams.Instance.boxStyle);
                GUILayout.BeginHorizontal();
                if (renameId == e.Id) {
                    renameText = GUILayout.TextField(renameText, UIParams.Instance.textStyle);
                    if (GUILayout.Button("OK", UIParams.Instance.bStyle, GUILayout.Width(48)) && !string.IsNullOrEmpty(renameText.Trim())) {
                        e.Name = renameText.Trim();
                        renameId = Guid.Empty;
                        Save();
                    }
                } else {
                    GUILayout.Label(Short(e.Name, 26) + "  [" + e.Slot + "]", UIParams.Instance.lStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Rename", UIParams.Instance.bStyle, GUILayout.Width(80))) {
                        renameId = e.Id;
                        renameText = e.Name;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                string matchLabel = e.Match == MatchMode.AnyInSlot ? "Any item in slot" : "Same item only";
                if (GUILayout.Button(matchLabel, UIParams.Instance.bStyle)) {
                    e.Match = e.Match == MatchMode.AnyInSlot ? MatchMode.SameObjectName : MatchMode.AnyInSlot;
                    Save();
                }
                GUI.enabled = maid;
                if (GUILayout.Button("Apply", UIParams.Instance.bStyle, GUILayout.Width(70))) status = ResultText(new List<ApplyResult> { LibraryService.Apply(maid, e) });
                GUI.enabled = true;
                string delLabel = confirmDelete == e.Id ? "Confirm?" : "Delete";
                if (GUILayout.Button(delLabel, UIParams.Instance.bStyle, GUILayout.Width(80))) {
                    if (confirmDelete == e.Id) delete = e.Id; else confirmDelete = e.Id;
                }
                GUILayout.EndHorizontal();
                if (e.Match == MatchMode.SameObjectName) GUILayout.Label("item: " + Short(e.ObjectName, 40), UIParams.Instance.lStyle);
                GUILayout.EndVertical();
            }
            if (delete != Guid.Empty) {
                ModEntry gone = Db.Entries[delete];
                Db.RemoveEntry(delete);
                confirmDelete = Guid.Empty;
                Save();
                status = "Deleted '" + gone.Name + "'.";
            }
            page = DrawPager(page, entries.Count);
        }

        IEnumerable<ModEntry> Sorted(IEnumerable<ModEntry> q) {
            switch (sortMode) {
                case 1: return ascending ? q.OrderBy(e => e.Created) : q.OrderByDescending(e => e.Created);
                case 2: return ascending ? q.OrderBy(e => e.Slot).ThenBy(e => e.Name) : q.OrderByDescending(e => e.Slot).ThenBy(e => e.Name);
                default: return ascending ? q.OrderBy(e => e.Name) : q.OrderByDescending(e => e.Name);
            }
        }

        // ---------- zakladka Sets ----------

        void DrawSetsTab(Maid maid) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("New set", UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            newSetName = GUILayout.TextField(newSetName, UIParams.Instance.textStyle);
            GUI.enabled = !string.IsNullOrEmpty(newSetName.Trim());
            if (GUILayout.Button("Create", UIParams.Instance.bStyle, GUILayout.Width(80))) {
                ModSet created = Db.AddSet(newSetName.Trim());
                openSetId = created.Id;
                newSetName = "";
                Save();
                status = "Created set '" + created.Name + "'.";
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            List<ModSet> sets = Db.Sets.Values.OrderBy(s => s.Name).ToList();
            if (sets.Count == 0) GUILayout.Label("(no sets yet)", UIParams.Instance.lStyle);

            Guid deleteSet = Guid.Empty;
            foreach (ModSet set in Slice(sets, setsPage)) {
                bool open = openSetId == set.Id;
                GUILayout.BeginVertical(UIParams.Instance.boxStyle);
                GUILayout.BeginHorizontal();
                if (setRenameId == set.Id) {
                    setRenameText = GUILayout.TextField(setRenameText, UIParams.Instance.textStyle);
                    if (GUILayout.Button("OK", UIParams.Instance.bStyle, GUILayout.Width(48)) && !string.IsNullOrEmpty(setRenameText.Trim())) {
                        set.Name = setRenameText.Trim();
                        setRenameId = Guid.Empty;
                        Save();
                    }
                } else {
                    if (GUILayout.Button((open ? "v " : "> ") + Short(set.Name, 26) + " (" + set.EntryIds.Count + ")", UIParams.Instance.bStyleL)) {
                        openSetId = open ? Guid.Empty : set.Id;
                        setAddOpen = false;
                        setAddPage = 0;
                    }
                }
                GUILayout.EndHorizontal();

                if (open) DrawSetDetails(set, maid, ref deleteSet);
                GUILayout.EndVertical();
            }
            if (deleteSet != Guid.Empty) {
                string name = Db.Sets[deleteSet].Name;
                Db.RemoveSet(deleteSet);
                if (openSetId == deleteSet) openSetId = Guid.Empty;
                confirmDeleteSet = Guid.Empty;
                Save();
                status = "Deleted set '" + name + "' (its modifications stay in the library).";
            }
            setsPage = DrawPager(setsPage, sets.Count);
        }

        void DrawSetDetails(ModSet set, Maid maid, ref Guid deleteSet) {
            UIUtil.BeginIndentArea();
            List<ModEntry> members = EntriesOf(set);
            if (members.Count == 0) GUILayout.Label("(empty)", UIParams.Instance.lStyle);

            Guid remove = Guid.Empty;
            int move = 0; Guid moveId = Guid.Empty;
            for (int i = 0; i < members.Count; i++) {
                ModEntry e = members[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label((i + 1) + ". " + Short(e.Name, 22) + "  [" + e.Slot + "]", UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                GUI.enabled = i > 0;
                if (GUILayout.Button("^", UIParams.Instance.bStyle, GUILayout.Width(32))) { move = -1; moveId = e.Id; }
                GUI.enabled = i < members.Count - 1;
                if (GUILayout.Button("v", UIParams.Instance.bStyle, GUILayout.Width(32))) { move = 1; moveId = e.Id; }
                GUI.enabled = true;
                if (GUILayout.Button("x", UIParams.Instance.bStyle, GUILayout.Width(32))) remove = e.Id;
                GUILayout.EndHorizontal();
            }
            if (remove != Guid.Empty) { Db.RemoveFromSet(set.Id, remove); Save(); }
            if (move != 0) {
                int idx = set.EntryIds.IndexOf(moveId);
                int to = idx + move;
                if (idx >= 0 && to >= 0 && to < set.EntryIds.Count) {
                    set.EntryIds.RemoveAt(idx);
                    set.EntryIds.Insert(to, moveId);
                    Save();
                }
            }

            setAddOpen = GUILayout.Toggle(setAddOpen, "+ Add modification to set", UIParams.Instance.tStyle);
            if (setAddOpen) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Search", UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
                string s2 = GUILayout.TextField(setAddSearch, UIParams.Instance.textStyle);
                if (s2 != setAddSearch) { setAddSearch = s2; setAddPage = 0; }
                GUILayout.EndHorizontal();
                List<ModEntry> choices = FilterEntries(setAddSearch).Where(e => !set.EntryIds.Contains(e.Id)).OrderBy(e => e.Name).ToList();
                if (choices.Count == 0) GUILayout.Label("(nothing to add)", UIParams.Instance.lStyle);
                foreach (ModEntry e in Slice(choices, setAddPage)) {
                    if (GUILayout.Button(Short(e.Name, 34) + "  [" + e.Slot + "]", UIParams.Instance.bStyleL)) {
                        Db.AddToSet(set.Id, e.Id);
                        Save();
                    }
                }
                setAddPage = DrawPager(setAddPage, choices.Count);
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = maid && members.Count > 0;
            if (GUILayout.Button("Apply to maid", UIParams.Instance.bStyle)) status = ResultText(LibraryService.ApplyAll(maid, members));
            GUI.enabled = maid;
            MaidBinding b = maid ? Binding(maid, false) : null;
            bool assigned = b != null && b.SetIds.Contains(set.Id);
            if (GUILayout.Button(assigned ? "Unassign from maid" : "Assign to maid", UIParams.Instance.bStyle)) {
                MaidBinding mb = Binding(maid, true);
                if (assigned) mb.SetIds.Remove(set.Id); else mb.SetIds.Add(set.Id);
                Save();
                status = assigned ? "Set unassigned." : "Set assigned to this maid.";
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rename", UIParams.Instance.bStyle)) {
                setRenameId = set.Id;
                setRenameText = set.Name;
            }
            string delLabel = confirmDeleteSet == set.Id ? "Confirm delete?" : "Delete set";
            if (GUILayout.Button(delLabel, UIParams.Instance.bStyle)) {
                if (confirmDeleteSet == set.Id) deleteSet = set.Id; else confirmDeleteSet = set.Id;
            }
            GUILayout.EndHorizontal();
            UIUtil.EndoIndentArea();
        }
    }
}
