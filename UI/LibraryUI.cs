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

        // filtr wlasciciela: false = tylko ta pokojowka (domyslnie), true = wszystkie
        bool showAll = false;
        bool addShowOthers = false;
        bool addSetShowOthers = false;
        bool setAddShowOthers = false;

        // wpis, ktorego warunki sa rozwiniete w zakladce Library
        Guid conditionsOpenId = Guid.Empty;
        bool conditionsFromWornOpen = false;

        // wpis otwarty przyciskiem Edit - mozna go potem zaktualizowac zamiast zapisywac nowy
        Guid editingId = Guid.Empty;

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
            GUILayout.Label(Text.Library, UIParams.Instance.lStyle);
            if (Db == null) {
                GUILayout.Label(TL.LibraryUnavailable, UIParams.Instance.lStyle);
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
            // liczniki na zakladkach uwzgledniaja filtr wlasciciela (tylko ta pokojowka / wszystkie), tak jak listy pod nimi
            Maid tabMaid = CommonUIData.maid;
            int entryCount = Db.Entries.Values.Count(en => showAll || Mine(tabMaid, en));
            int setCount = Db.Sets.Values.Count(st => showAll || MineSet(tabMaid, st));
            string[] names = new string[] { TL.TabMaid, string.Format(TL.TabLibraryFmt, entryCount), string.Format(TL.TabSetsFmt, setCount) };
            int selected = GUILayout.Toolbar((int)tab, names, UIParams.Instance.tbStyle);
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
            sb.Append(string.Format(TL.AppliedFmt, ok, results.Count));
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

        // Wpisy / zestawy "tej pokojowki": zapisane przy niej albo do niej przypisane (bezposrednio lub przez zestaw).
        bool Mine(Maid maid, ModEntry e) { return !maid || Db.IsEntryOfMaid(SetManager.MaidGuid(maid), e); }
        bool MineSet(Maid maid, ModSet s) { return !maid || Db.IsSetOfMaid(SetManager.MaidGuid(maid), s); }

        // Przelacznik widoku (zakladki Library i Sets): tylko ta pokojowka (domyslnie) / wszystkie.
        void DrawOwnerFilter(Maid maid) {
            if (!maid) return;
            int sel = GUILayout.Toolbar(showAll ? 1 : 0, new string[] { TL.FilterThisMaid, TL.FilterAll }, UIParams.Instance.tbStyle);
            if ((sel == 1) != showAll) { showAll = sel == 1; page = 0; setsPage = 0; }
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
                GUILayout.Label(TL.SelectMaidFirst, UIParams.Instance.lStyle);
                return;
            }
            MaidBinding b = Binding(maid, false);

            bool auto = b != null && b.AutoLoad;
            bool autoNew = GUILayout.Toggle(auto, TL.AutoLoad, UIParams.Instance.tStyle);
            if (autoNew != auto) {
                Binding(maid, true).AutoLoad = autoNew;
                Save();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(TL.ApplyAll, UIParams.Instance.bStyle)) {
                status = ResultText(LibraryService.ApplyMaidProfile(maid));
            }
            if (GUILayout.Button(TL.ResetMaid, UIParams.Instance.bStyle)) {
                BackUpData.RestoreMaid(maid);
                status = TL.ResetDone;
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
            GUILayout.Label(TL.SetsOfMaid, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            if (b == null || b.SetIds.Count == 0) GUILayout.Label(TL.None, UIParams.Instance.lStyle);
            if (b != null) {
                Guid remove = Guid.Empty;
                foreach (Guid setId in b.SetIds) {
                    ModSet set;
                    if (!Db.Sets.TryGetValue(setId, out set)) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Short(set.Name, 28) + " (" + set.EntryIds.Count + ")", UIParams.Instance.lStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(TL.Apply, UIParams.Instance.bStyle, GUILayout.Width(70))) status = ResultText(LibraryService.ApplyAll(maid, EntriesOf(set)));
                    if (GUILayout.Button(TL.Revert, UIParams.Instance.bStyle, GUILayout.Width(70))) { LibraryService.Revert(maid, EntriesOf(set)); status = string.Format(TL.RevertedSetFmt, set.Name); }
                    if (GUILayout.Button("x", UIParams.Instance.bStyle, GUILayout.Width(32))) remove = setId;
                    GUILayout.EndHorizontal();
                }
                if (remove != Guid.Empty) {
                    ModSet removed;
                    b.SetIds.Remove(remove);
                    Save();
                    if (Db.Sets.TryGetValue(remove, out removed)) { LibraryService.Revert(maid, EntriesOf(removed)); status = string.Format(TL.SetRemovedFmt, removed.Name); }
                }
            }
            addSetOpen = GUILayout.Toggle(addSetOpen, TL.AddSet, UIParams.Instance.tStyle);
            if (addSetOpen) {
                addSetShowOthers = GUILayout.Toggle(addSetShowOthers, TL.ShowOtherSets, UIParams.Instance.tStyle);
                List<ModSet> choices = Db.Sets.Values.Where(s => (b == null || !b.SetIds.Contains(s.Id)) && (addSetShowOthers || MineSet(maid, s))).OrderBy(s => s.Name).ToList();
                if (choices.Count == 0) GUILayout.Label(TL.NoOtherSets, UIParams.Instance.lStyle);
                foreach (ModSet s in choices.Take(PageSize)) {
                    if (GUILayout.Button(Short(s.Name, 40) + " (" + s.EntryIds.Count + ")", UIParams.Instance.bStyleL)) {
                        Binding(maid, true).SetIds.Add(s.Id);
                        addSetOpen = false;
                        Save();
                        status = string.Format(TL.SetAddedFmt, s.Name);
                    }
                }
            }
            UIUtil.EndoIndentArea();
        }

        void DrawMaidEntries(Maid maid, MaidBinding b) {
            GUILayout.Label(TL.ModsOfMaid, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            if (b == null || b.EntryIds.Count == 0) GUILayout.Label(TL.None, UIParams.Instance.lStyle);
            if (b != null) {
                Guid remove = Guid.Empty;
                foreach (Guid entryId in b.EntryIds) {
                    ModEntry e;
                    if (!Db.Entries.TryGetValue(entryId, out e)) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Short(e.Name, 24) + "  [" + e.Slot + "]", UIParams.Instance.lStyle);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(TL.Apply, UIParams.Instance.bStyle, GUILayout.Width(70))) status = ResultText(new List<ApplyResult> { LibraryService.Apply(maid, e) });
                    if (GUILayout.Button(TL.Revert, UIParams.Instance.bStyle, GUILayout.Width(70))) { LibraryService.Revert(maid, new List<ModEntry> { e }); status = string.Format(TL.RevertedEntryFmt, e.Name); }
                    if (GUILayout.Button(TL.Edit, UIParams.Instance.bStyle, GUILayout.Width(60))) StartEdit(maid, e);
                    if (GUILayout.Button("x", UIParams.Instance.bStyle, GUILayout.Width(32))) remove = entryId;
                    GUILayout.EndHorizontal();
                }
                if (remove != Guid.Empty) {
                    ModEntry removed;
                    b.EntryIds.Remove(remove);
                    Save();
                    if (Db.Entries.TryGetValue(remove, out removed)) { LibraryService.Revert(maid, new List<ModEntry> { removed }); status = string.Format(TL.EntryRemovedFmt, removed.Name); }
                }
            }

            addEntryOpen = GUILayout.Toggle(addEntryOpen, TL.AddMod, UIParams.Instance.tStyle);
            if (addEntryOpen) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(TL.Search, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
                string s2 = GUILayout.TextField(addEntrySearch, UIParams.Instance.textStyle);
                if (s2 != addEntrySearch) { addEntrySearch = s2; addPage = 0; }
                GUILayout.EndHorizontal();

                addShowOthers = GUILayout.Toggle(addShowOthers, TL.ShowOthers, UIParams.Instance.tStyle);

                // domyslnie tylko wpisy tej pokojowki, pasujace do zalozonych na niej przedmiotow
                HashSet<string> equipped = new HashSet<string>(
                    SetManager.GetEquippedSlotIds(maid).Select(id => SlotUtil.GetSlotName(id)), StringComparer.OrdinalIgnoreCase);
                List<ModEntry> choices = FilterEntries(addEntrySearch)
                    .Where(e => equipped.Contains(e.Slot) && (b == null || !b.EntryIds.Contains(e.Id)) && (addShowOthers || Mine(maid, e)))
                    .OrderBy(e => e.Name).ToList();
                if (choices.Count == 0) GUILayout.Label(TL.NoMatchingMods, UIParams.Instance.lStyle);
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

        void Resync(Maid maid) {
            if (maid) LibraryService.RequestSync(maid);
        }

        static string KindLabel(ConditionKind kind) {
            switch (kind) {
                case ConditionKind.SlotEquipped: return TL.CondKindSlot;
                case ConditionKind.MenuFile: return TL.CondKindMenu;
                case ConditionKind.ObjectName: return TL.CondKindObject;
                default: return TL.CondKindModel;
            }
        }

        // Edytor warunkow wpisu (jak w SKM): wlacznik, wszystkie/ktorykolwiek, lista warunkow z wzorcami (* ?), negacja,
        // dodawanie z aktualnie zalozonego przedmiotu oraz podglad, czy to pasuje do tego, co pokojowka ma teraz na sobie.
        void DrawConditionsEditor(Maid maid, ModEntry e) {
            UIUtil.BeginIndentArea();

            bool use = GUILayout.Toggle(e.UseConditions, TL.UseConditions, UIParams.Instance.tStyle);
            if (use != e.UseConditions) { e.UseConditions = use; Save(); Resync(maid); }

            if (e.UseConditions) {
                int mode = GUILayout.Toolbar(e.RequireAll ? 0 : 1, new string[] { TL.CondAll, TL.CondAny }, UIParams.Instance.tbStyle);
                if ((mode == 0) != e.RequireAll) { e.RequireAll = mode == 0; Save(); Resync(maid); }

                List<WornItem> worn = maid ? LibraryService.WornItems(maid) : new List<WornItem>();
                List<ConditionResult> results = ConditionLogic.Explain(e, worn);

                EntryCondition remove = null;
                for (int i = 0; i < e.Conditions.Count; i++) {
                    EntryCondition c = e.Conditions[i];
                    GUILayout.BeginVertical(UIParams.Instance.boxStyle);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(KindLabel(c.Kind), UIParams.Instance.bStyle)) {
                        c.Kind = (ConditionKind)(((int)c.Kind + 1) % 4);
                        Save(); Resync(maid);
                    }
                    bool neg = GUILayout.Toggle(c.Negate, "NOT", UIParams.Instance.tStyle, GUILayout.Width(70));
                    if (neg != c.Negate) { c.Negate = neg; Save(); Resync(maid); }
                    if (GUILayout.Button("x", UIParams.Instance.bStyle, GUILayout.Width(32))) remove = c;
                    GUILayout.EndHorizontal();

                    if (c.Kind != ConditionKind.SlotEquipped) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(TL.CondPattern, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
                        string pattern = GUILayout.TextField(c.Pattern, UIParams.Instance.textStyle);
                        if (pattern != c.Pattern) { c.Pattern = pattern; Save(); Resync(maid); }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(TL.CondSlotHint, UIParams.Instance.lStyleS, GUILayout.ExpandWidth(false));
                    string slotText = GUILayout.TextField(c.Slot, UIParams.Instance.textStyle, GUILayout.Width(110));
                    if (slotText != c.Slot) { c.Slot = slotText; Save(); Resync(maid); }
                    GUILayout.EndHorizontal();

                    if (maid && i < results.Count) {
                        GUILayout.Label((results[i].Result ? "[OK] " : "[--] ") + (results[i].Found.Length > 0 ? results[i].Found : ""), UIParams.Instance.lStyleS);
                    }
                    GUILayout.EndVertical();
                }
                if (remove != null) { e.Conditions.Remove(remove); Save(); Resync(maid); }

                GUILayout.Label(TL.CondWildcardHint, UIParams.Instance.lStyleS);

                if (GUILayout.Button(TL.CondAdd, UIParams.Instance.bStyle)) {
                    e.Conditions.Add(new EntryCondition());
                    Save(); Resync(maid);
                }

                conditionsFromWornOpen = GUILayout.Toggle(conditionsFromWornOpen, TL.CondFromWorn, UIParams.Instance.tStyle);
                if (conditionsFromWornOpen) {
                    if (!maid) {
                        GUILayout.Label(TL.CondNeedMaid, UIParams.Instance.lStyleS);
                    } else {
                        WornItem item = worn.FirstOrDefault(w => string.Equals(w.Slot, e.Slot, StringComparison.OrdinalIgnoreCase));
                        if (item == null) {
                            GUILayout.Label(TL.CondNoWorn, UIParams.Instance.lStyleS);
                        } else {
                            if (item.MenuFile.Length > 0 && GUILayout.Button(KindLabel(ConditionKind.MenuFile) + ": " + Short(item.MenuFile, 40), UIParams.Instance.bStyleL)) {
                                e.Conditions.Add(new EntryCondition { Kind = ConditionKind.MenuFile, Pattern = item.MenuFile });
                                Save(); Resync(maid);
                            }
                            if (item.ObjectName.Length > 0 && GUILayout.Button(KindLabel(ConditionKind.ObjectName) + ": " + Short(item.ObjectName, 40), UIParams.Instance.bStyleL)) {
                                e.Conditions.Add(new EntryCondition { Kind = ConditionKind.ObjectName, Pattern = item.ObjectName });
                                Save(); Resync(maid);
                            }
                            if (item.ModelFile.Length > 0 && GUILayout.Button(KindLabel(ConditionKind.ModelFile) + ": " + Short(item.ModelFile, 40), UIParams.Instance.bStyleL)) {
                                e.Conditions.Add(new EntryCondition { Kind = ConditionKind.ModelFile, Pattern = item.ModelFile });
                                Save(); Resync(maid);
                            }
                        }
                    }
                }

                if (maid) {
                    GUILayout.Label(ConditionLogic.Matches(e, worn) ? TL.CondMatchesNow : TL.CondNoMatchNow, UIParams.Instance.lStyle);
                } else {
                    GUILayout.Label(TL.CondNeedMaid, UIParams.Instance.lStyleS);
                }
            }
            UIUtil.EndoIndentArea();
        }

        // Przycisk Edit: naklada wpis, wybiera jego slot na glownym ekranie i wraca do edycji kosci.
        void StartEdit(Maid maid, ModEntry e) {
            string message = LibraryService.Edit(maid, e);
            if (string.IsNullOrEmpty(message)) {
                editingId = e.Id;
                status = string.Format(TL.EditingFmt, e.Name);
            } else {
                status = message;
            }
        }

        // Zapis biezacego stanu slotu jako nowej modyfikacji (odpowiednik starego Export).
        void DrawCapture(Maid maid) {
            GUILayout.Label(TL.SaveSlotHeader, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();

            List<int> slotIds = SetManager.GetEquippedSlotIds(maid);
            slotIds.Insert(0, (int)EXSlot.Base);
            if (captureSlotIndex >= slotIds.Count) captureSlotIndex = -1;

            string current = captureSlotIndex >= 0 ? SetManager.SlotLabel(maid, slotIds[captureSlotIndex]) : TL.ChooseSlot;
            slotPickerOpen = GUILayout.Toggle(slotPickerOpen, string.Format(TL.SlotFmt, current), UIParams.Instance.tStyle);
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
            GUILayout.Label(Text.Name, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            captureName = GUILayout.TextField(captureName, UIParams.Instance.textStyle);
            GUILayout.EndHorizontal();

            GUI.enabled = captureSlotIndex >= 0 && !string.IsNullOrEmpty(captureName.Trim());
            if (GUILayout.Button(TL.SaveAsMod, UIParams.Instance.bStyle)) {
                string baked = LibraryService.BakeRootIfIndependent(maid, slotIds[captureSlotIndex]);
                bool hasChanges;
                ModEntry entry = LibraryService.CaptureSlot(maid, slotIds[captureSlotIndex], captureName.Trim(), out hasChanges);
                if (entry == null) {
                    status = TL.NothingEquipped;
                } else if (!hasChanges) {
                    status = TL.NothingToSave;
                } else {
                    Db.AddEntry(entry);
                    Binding(maid, true).EntryIds.Add(entry.Id);
                    Save();
                    status = (baked.Length > 0 ? TL.RootConvertedPrefix : "") + string.Format(TL.SavedFmt, entry.Name, entry.Slot, entry.Bones.Count);
                }
            }
            GUI.enabled = true;

            ModEntry editing;
            if (editingId != Guid.Empty && Db.Entries.TryGetValue(editingId, out editing)) {
                if (GUILayout.Button(string.Format(TL.UpdateBtnFmt, Short(editing.Name, 24)), UIParams.Instance.bStyle)) {
                    int editSlot = SetManager.FindSlotId(maid, editing.Slot);
                    if (editSlot == (int)EXSlot.None) {
                        status = string.Format(TL.SlotUnavailableFmt, editing.Name);
                    } else {
                        string bakedNote = LibraryService.BakeRootIfIndependent(maid, editSlot);
                        bool editChanged;
                        ModEntry fresh = LibraryService.CaptureSlot(maid, editSlot, editing.Name, out editChanged);
                        if (fresh == null || !editChanged) {
                            status = TL.NothingToUpdate;
                        } else {
                            editing.Bones = fresh.Bones;
                            editing.Root = fresh.Root;
                            editing.MaidParts = fresh.MaidParts;
                            editing.Yure = fresh.Yure;
                            Save();
                            status = (bakedNote.Length > 0 ? TL.RootConvertedPrefix : "") + string.Format(TL.UpdatedFmt, editing.Name, editing.Bones.Count);
                        }
                    }
                }
            }

            GUI.enabled = captureSlotIndex >= 0;
            if (GUILayout.Button(TL.ConvertBtn, UIParams.Instance.bStyle)) {
                status = LibraryService.BakeRootIntoBones(maid, slotIds[captureSlotIndex]);
            }
            GUI.enabled = true;
            UIUtil.EndoIndentArea();
        }

        // ---------- zakladka Library ----------

        void DrawLibraryTab(Maid maid) {
            DrawOwnerFilter(maid);
            GUILayout.BeginHorizontal();
            GUILayout.Label(TL.Search, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            string s2 = GUILayout.TextField(search, UIParams.Instance.textStyle);
            if (s2 != search) { search = s2; page = 0; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(TL.Sort, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            string[] modes = new string[] { Text.Name, TL.SortDate, TL.SortSlot };
            if (GUILayout.Button(modes[sortMode], UIParams.Instance.bStyle)) sortMode = (sortMode + 1) % modes.Length;
            if (GUILayout.Button(ascending ? TL.Asc : TL.Desc, UIParams.Instance.bStyle, GUILayout.Width(60))) ascending = !ascending;
            GUILayout.EndHorizontal();

            List<ModEntry> entries = Sorted(FilterEntries(search).Where(en => showAll || Mine(maid, en))).ToList();
            if (entries.Count == 0) GUILayout.Label(maid && !showAll ? TL.NoMineHint : TL.LibraryEmpty, UIParams.Instance.lStyle);

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
                    if (GUILayout.Button(TL.Rename, UIParams.Instance.bStyle, GUILayout.Width(80))) {
                        renameId = e.Id;
                        renameText = e.Name;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (e.UseConditions) {
                    GUILayout.Label(e.RequireAll ? TL.CondActiveAll : TL.CondActiveAny, UIParams.Instance.lStyle);
                } else {
                    string matchLabel = e.Match == MatchMode.AnyInSlot ? TL.MatchAny : TL.MatchSame;
                    if (GUILayout.Button(matchLabel, UIParams.Instance.bStyle)) {
                        e.Match = e.Match == MatchMode.AnyInSlot ? MatchMode.SameObjectName : MatchMode.AnyInSlot;
                        Save();
                        Resync(maid);
                    }
                }
                if (GUILayout.Button(TL.CondButton, UIParams.Instance.bStyle, GUILayout.Width(90))) {
                    conditionsOpenId = conditionsOpenId == e.Id ? Guid.Empty : e.Id;
                    conditionsFromWornOpen = false;
                }
                GUI.enabled = maid;
                if (GUILayout.Button(TL.Apply, UIParams.Instance.bStyle, GUILayout.Width(70))) status = ResultText(new List<ApplyResult> { LibraryService.Apply(maid, e) });
                if (GUILayout.Button(TL.Revert, UIParams.Instance.bStyle, GUILayout.Width(70))) { LibraryService.Revert(maid, new List<ModEntry> { e }); status = string.Format(TL.RevertedEntryFmt, e.Name); }
                if (GUILayout.Button(TL.Edit, UIParams.Instance.bStyle, GUILayout.Width(60))) StartEdit(maid, e);
                GUI.enabled = true;
                string delLabel = confirmDelete == e.Id ? TL.ConfirmQ : TL.Delete;
                if (GUILayout.Button(delLabel, UIParams.Instance.bStyle, GUILayout.Width(80))) {
                    if (confirmDelete == e.Id) delete = e.Id; else confirmDelete = e.Id;
                }
                GUILayout.EndHorizontal();
                if (conditionsOpenId == e.Id) DrawConditionsEditor(maid, e);
                if (!e.UseConditions && e.Match == MatchMode.SameObjectName) GUILayout.Label(string.Format(TL.ItemFmt, Short(e.ObjectName, 40)), UIParams.Instance.lStyle);
                if (showAll || !maid) GUILayout.Label(string.IsNullOrEmpty(e.OwnerName) ? TL.NoOwner : string.Format(TL.OwnerFmt, e.OwnerName), UIParams.Instance.lStyleS);

                // Poza korzenia obiektu (Maid Parts). Korzen obraca/przesuwa caly szkielet slotu jak sztywna bryle, wiec zapisany
                // niezerowy obrot potrafi "rozjechac" przedmiot po zmianie pozy postaci - dlatego mozna to wylaczyc dla wpisu.
                float rootRot = Quaternion.Angle(Quaternion.identity, DbConvert.ToQuaternion(e.Root.Rotation));
                float rootMove = DbConvert.ToVector3(e.Root.Position).magnitude;
                if (rootRot > 0.5f || rootMove > 0.002f) {
                    string rootLabel = string.Format(TL.RootPoseLabelFmt, rootRot.ToString("F1"), (rootMove * 100f).ToString("F1"));
                    bool rootOn = GUILayout.Toggle(e.MaidParts, rootLabel, UIParams.Instance.tStyle);
                    if (rootOn != e.MaidParts) {
                        e.MaidParts = rootOn;
                        Save();
                        if (!rootOn && maid) {
                            LibraryService.RevertRoot(maid, e);
                            status = string.Format(TL.RootOffRevertedFmt, e.Name);
                        } else {
                            status = string.Format(rootOn ? TL.RootOnFmt : TL.RootOffFmt, e.Name);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            if (delete != Guid.Empty) {
                ModEntry gone = Db.Entries[delete];
                Db.RemoveEntry(delete);
                confirmDelete = Guid.Empty;
                Save();
                status = string.Format(TL.DeletedFmt, gone.Name);
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
            GUILayout.Label(TL.NewSet, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
            newSetName = GUILayout.TextField(newSetName, UIParams.Instance.textStyle);
            GUI.enabled = !string.IsNullOrEmpty(newSetName.Trim());
            if (GUILayout.Button(TL.Create, UIParams.Instance.bStyle, GUILayout.Width(80))) {
                ModSet created = Db.AddSet(newSetName.Trim());
                if (maid) { created.OwnerGuid = SetManager.MaidGuid(maid) ?? ""; created.OwnerName = LibraryService.MaidName(maid); }
                openSetId = created.Id;
                newSetName = "";
                Save();
                status = string.Format(TL.CreatedSetFmt, created.Name);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            DrawOwnerFilter(maid);
            List<ModSet> sets = Db.Sets.Values.Where(s => showAll || MineSet(maid, s)).OrderBy(s => s.Name).ToList();
            if (sets.Count == 0) GUILayout.Label(TL.NoSetsYet, UIParams.Instance.lStyle);

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
                status = string.Format(TL.DeletedSetFmt, name);
            }
            setsPage = DrawPager(setsPage, sets.Count);
        }

        void DrawSetDetails(ModSet set, Maid maid, ref Guid deleteSet) {
            UIUtil.BeginIndentArea();
            List<ModEntry> members = EntriesOf(set);
            if (members.Count == 0) GUILayout.Label(TL.Empty, UIParams.Instance.lStyle);

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

            setAddOpen = GUILayout.Toggle(setAddOpen, TL.AddModToSet, UIParams.Instance.tStyle);
            if (setAddOpen) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(TL.Search, UIParams.Instance.lStyle, GUILayout.ExpandWidth(false));
                string s2 = GUILayout.TextField(setAddSearch, UIParams.Instance.textStyle);
                if (s2 != setAddSearch) { setAddSearch = s2; setAddPage = 0; }
                GUILayout.EndHorizontal();
                setAddShowOthers = GUILayout.Toggle(setAddShowOthers, TL.ShowOthers, UIParams.Instance.tStyle);
                List<ModEntry> choices = FilterEntries(setAddSearch).Where(e => !set.EntryIds.Contains(e.Id) && (setAddShowOthers || Mine(maid, e))).OrderBy(e => e.Name).ToList();
                if (choices.Count == 0) GUILayout.Label(TL.NothingToAdd, UIParams.Instance.lStyle);
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
            if (GUILayout.Button(TL.ApplyToMaid, UIParams.Instance.bStyle)) status = ResultText(LibraryService.ApplyAll(maid, members));
            if (GUILayout.Button(TL.Revert, UIParams.Instance.bStyle)) { LibraryService.Revert(maid, members); status = string.Format(TL.RevertedSetFmt, set.Name); }
            GUI.enabled = maid;
            MaidBinding b = maid ? Binding(maid, false) : null;
            bool assigned = b != null && b.SetIds.Contains(set.Id);
            if (GUILayout.Button(assigned ? TL.UnassignFromMaid : TL.AssignToMaid, UIParams.Instance.bStyle)) {
                MaidBinding mb = Binding(maid, true);
                if (assigned) mb.SetIds.Remove(set.Id); else mb.SetIds.Add(set.Id);
                Save();
                if (assigned) LibraryService.Revert(maid, members);
                status = assigned ? TL.SetUnassigned : TL.SetAssigned;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(TL.Rename, UIParams.Instance.bStyle)) {
                setRenameId = set.Id;
                setRenameText = set.Name;
            }
            string delLabel = confirmDeleteSet == set.Id ? TL.ConfirmDeleteQ : TL.DeleteSet;
            if (GUILayout.Button(delLabel, UIParams.Instance.bStyle)) {
                if (confirmDeleteSet == set.Id) deleteSet = set.Id; else confirmDeleteSet = set.Id;
            }
            GUILayout.EndHorizontal();
            UIUtil.EndoIndentArea();
        }
    }
}
