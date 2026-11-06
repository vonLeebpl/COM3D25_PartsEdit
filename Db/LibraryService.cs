using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin.Db {
    internal enum ApplyStatus {
        Applied,
        UnknownSlot,        // pokojowka nie ma takiego slotu
        SlotEmpty,          // w slocie nic nie jest zalozone
        ObjectNameMismatch  // wpis wymaga tej samej nazwy obiektu (MatchMode.SameObjectName), a zalozono inny przedmiot
    }

    internal class ApplyResult {
        public ModEntry Entry;
        public ApplyStatus Status;

        public override string ToString() {
            return "'" + Entry.Name + "' [" + Entry.Slot + "]: " + Status;
        }
    }

    // Laczy baze (Db) z logika gry: nakladanie wpisow na pokojowke, cofanie, zapis stanu slotu jako nowego wpisu.
    // Korzysta z istniejacej logiki (SetManager.Apply / Capture), wiec zachowuje sie tak samo jak dotychczasowy Import/Set
    // (m.in. automatycznie wlacza Independent bones dla danych z kosciami *_ShoeAdj).
    internal static class LibraryService {
        static PartsEditDb Db { get { return PartsEditStore.Db; } }

        public static string MaidName(Maid maid) {
            if (!maid) return "";
#if COM3D2
            return maid.status.lastName + " " + maid.status.firstName;
#else
            return maid.Param.status.last_name + " " + maid.Param.status.first_name;
#endif
        }

        // Naklada jeden wpis na slot pokojowki, w ktorym jest zalozony pasujacy przedmiot.
        public static ApplyResult Apply(Maid maid, ModEntry entry) {
            ApplyResult result = new ApplyResult();
            result.Entry = entry;

            int slotNo = SetManager.FindSlotId(maid, entry.Slot);
            if (slotNo == (int)EXSlot.None) { result.Status = ApplyStatus.UnknownSlot; return result; }
            GameObject obj = SetManager.GetSlotObject(maid, slotNo);
            if (!obj) { result.Status = ApplyStatus.SlotEmpty; return result; }
            if (!entry.Matches(obj.name)) { result.Status = ApplyStatus.ObjectNameMismatch; return result; }

            SetManager.Apply(maid, slotNo, obj, DbConvert.ToObjectData(entry));
            result.Status = ApplyStatus.Applied;
            return result;
        }

        // Naklada wpisy po kolei (przy tej samej kosci pozniejszy nadpisuje wczesniejszy).
        public static List<ApplyResult> ApplyAll(Maid maid, IEnumerable<ModEntry> entries) {
            List<ApplyResult> results = new List<ApplyResult>();
            foreach (ModEntry entry in entries) results.Add(Apply(maid, entry));
            return results;
        }

        // Pokojowka ma w bazie wlaczony autoload i cos do nalozenia.
        public static bool HasAutoLoad(Maid maid) {
            if (Db == null || !maid) return false;
            string guid = SetManager.MaidGuid(maid);
            MaidBinding binding;
            if (string.IsNullOrEmpty(guid) || !Db.Maids.TryGetValue(guid, out binding)) return false;
            return binding.AutoLoad && (binding.SetIds.Count > 0 || binding.EntryIds.Count > 0);
        }

        // Naklada wszystko, co baza przypisuje tej pokojowce (zestawy, potem wpisy bezposrednie).
        public static List<ApplyResult> ApplyMaidProfile(Maid maid) {
            if (Db == null || !maid) return new List<ApplyResult>();
            string guid = SetManager.MaidGuid(maid);
            Db.GetOrCreateMaid(guid, MaidName(maid)); // odswiez ostatnio widziana nazwe
            return ApplyAll(maid, Db.EffectiveEntries(guid));
        }

        // Cofa wszystkie zmiany w danym slocie (do stanu sprzed pierwszej edycji).
        public static void RevertSlot(Maid maid, string slotName) {
            int slotNo = SetManager.FindSlotId(maid, slotName);
            if (slotNo == (int)EXSlot.None) return;
            BackUpData.RestoreSlot(maid, slotNo);
        }

        // Cofa wplyw podanych wpisow na pokojowke (tylko kosci, ktorych dotycza, do stanu sprzed pierwszej zmiany),
        // a potem naklada ponownie pozostale wpisy tej pokojowki z tych samych slotow. Przypisania w bazie zostaja bez zmian.
        public static void Revert(Maid maid, IList<ModEntry> entries) {
            if (!maid || entries == null || entries.Count == 0) return;
            HashSet<string> slots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<Guid> excluded = new HashSet<Guid>();
            foreach (ModEntry e in entries) {
                excluded.Add(e.Id);
                int slotNo = SetManager.FindSlotId(maid, e.Slot);
                if (slotNo == (int)EXSlot.None) continue;
                GameObject obj = SetManager.GetSlotObject(maid, slotNo);
                if (!obj) continue;
                slots.Add(e.Slot);
                bool deep = slotNo == (int)EXSlot.Base;
                bool hasProxy = e.Bones.Any(b => b.Name != null && b.Name.EndsWith(ShoeProxy.Suffix));
                foreach (BoneRecord b in e.Bones) {
                    if (ShoeProxy.SkipOnApply(b.Name, hasProxy)) continue;
                    Transform t = CMT.SearchObjName(obj.transform, b.Name, deep);
                    if (t) BackUpData.RestoreBone(maid, slotNo, obj, t);
                }
                BackUpData.RestoreBone(maid, slotNo, obj, obj.transform); // korzen obiektu (dziala tylko jesli byl zmieniony)
            }
            BackUpData.Refresh();

            if (Db == null) return;
            foreach (ModEntry rest in Db.EffectiveEntries(SetManager.MaidGuid(maid))) {
                if (excluded.Contains(rest.Id) || !slots.Contains(rest.Slot)) continue;
                Apply(maid, rest);
            }
        }

        // Zamienia poze korzenia obiektu (Maid Parts) na rownowazne korekty kosci-zastepnikow (Independent bones).
        // Korzen obraca caly szkielet slotu jak sztywna bryle wokol jednego punktu, wiec taki obrot pasuje tylko do jednej pozy.
        // Po zamianie wyglad w biezacej pozie jest taki sam, ale korekta jest wzgledem kosci i podaza za nimi w kazdej pozie.
        public static string BakeRootIntoBones(Maid maid, int slotNo) {
            GameObject obj = SetManager.GetSlotObject(maid, slotNo);
            if (!obj) return "Nothing is equipped in this slot.";
            ShoeProxyMarker marker = obj.GetComponent<ShoeProxyMarker>();
            if (marker == null || marker.suspended) return "Enable 'Independent bones' for this slot first.";

            Transform root = obj.transform;
            BackUpBoneData rb = BackUpData.GetOrNullMaidBoneData(maid, slotNo, obj, root);
            if (rb == null || !(rb.changedPos || rb.changedRot || rb.changedScl)) return "The object root pose of this slot is not changed - nothing to convert.";

            // wplyw zmiany korzenia jako przeksztalcenie w przestrzeni swiata
            Matrix4x4 parent = root.parent ? root.parent.localToWorldMatrix : Matrix4x4.identity;
            Matrix4x4 current = Matrix4x4.TRS(root.localPosition, root.localRotation, root.localScale);
            Matrix4x4 original = Matrix4x4.TRS(
                rb.changedPos ? rb.position : root.localPosition,
                rb.changedRot ? rb.rotation : root.localRotation,
                rb.changedScl ? rb.scale : root.localScale);
            Matrix4x4 delta = parent * current * original.inverse * parent.inverse;

            // cofnij korzen, a to samo przeksztalcenie zapisz w kosciach (kosci wezlow "follow" maja stale pozy wzgledne, wiec
            // ich swiat od razu odpowiada nowemu polozeniu korzenia)
            BackUpData.RestoreBone(maid, slotNo, obj, root);
            BackUpData.Refresh();

            // Zastepniki tworza hierarchie (przesuniecie gornego przenosi tez nizsze), wiec korekte zapisujemy tylko w wezlach
            // BEZ zastepnikow nizej (stopy itp.), kazdy wzgledem wlasnej kosci - inaczej korekta zostalaby podwojona albo zalezalaby
            // od bioder zamiast od stop.
            List<Transform> edits = new List<Transform>();
            foreach (KeyValuePair<Transform, Transform> pair in marker.nodes) if (pair.Key) edits.Add(pair.Key);

            int count = 0;
            foreach (Transform edit in edits) {
                if (!edit.parent) continue;
                Transform self = edit;
                if (edits.Any(other => other != self && other.IsChildOf(self))) continue;
                BackUpBoneData backup = BackUpData.GetOrAddMaidBoneData(maid, slotNo, obj, edit);
                if (!backup.changedPos) { backup.position = edit.localPosition; backup.changedPos = true; }
                if (!backup.changedRot) { backup.rotation = edit.localRotation; backup.changedRot = true; }
                if (!backup.changedScl) { backup.scale = edit.localScale; backup.changedScl = true; }

                Matrix4x4 parentWorld = edit.parent.localToWorldMatrix;
                Matrix4x4 local = Matrix4x4.TRS(edit.localPosition, edit.localRotation, edit.localScale);
                ShoeProxy.SetLocalFromMatrix(edit, parentWorld.inverse * delta * parentWorld * local);
                count++;
            }
            BackUpData.Refresh();
            BoneEdit.requestRebuild = true;
            Log.Info("root pose of '" + obj.name + "' baked into " + count + " bone adjustments");
            return "Object root pose converted into " + count + " bone adjustments (relative to the bones). Save the slot as a modification to keep it.";
        }

        // Wolane przed zapisem slotu do biblioteki: jesli slot ma wlaczone Independent bones, a jego korzen zostal zmieniony
        // (np. wpis z poza korzenia), zamienia te zmiane na korekty kosci (patrz BakeRootIntoBones). Dzieki temu zapisany wpis
        // jest wzgledny wobec kosci i podaza za nimi w kazdej pozie. Zwraca komunikat albo pusty napis, gdy nic nie zrobiono.
        public static string BakeRootIfIndependent(Maid maid, int slotNo) {
            GameObject obj = SetManager.GetSlotObject(maid, slotNo);
            if (!obj) return "";
            ShoeProxyMarker marker = obj.GetComponent<ShoeProxyMarker>();
            if (marker == null || marker.suspended) return "";
            BackUpBoneData rb = BackUpData.GetOrNullMaidBoneData(maid, slotNo, obj, obj.transform);
            if (rb == null || !(rb.changedPos || rb.changedRot || rb.changedScl)) return "";
            return BakeRootIntoBones(maid, slotNo);
        }

        // Otwiera wpis do edycji: naklada go na pokojowke, wybiera jej slot na glownym ekranie (Maid Parts) i wraca do ekranu edycji.
        // Zwraca komunikat (pusty = wszystko w porzadku).
        public static string Edit(Maid maid, ModEntry entry) {
            if (!maid) return "Select a maid first.";
            ApplyResult result = Apply(maid, entry);
            if (result.Status != ApplyStatus.Applied) return result.ToString();

            int slotNo = SetManager.FindSlotId(maid, entry.Slot);
            Setting.targetSelectMode = 0;                // Maid Parts
            CommonUIData.SetMaid(maid);
            MaidSlotSelectUI.requestedSlot = slotNo;
            BoneEdit.requestRebuild = true;
            Setting.mode = Mode.Edit;
            return "";
        }

        // Cofa tylko poze korzenia obiektu (np. po wylaczeniu "Object root pose" we wpisie).
        public static void RevertRoot(Maid maid, ModEntry entry) {
            if (!maid) return;
            int slotNo = SetManager.FindSlotId(maid, entry.Slot);
            if (slotNo == (int)EXSlot.None) return;
            GameObject obj = SetManager.GetSlotObject(maid, slotNo);
            if (!obj) return;
            BackUpData.RestoreBone(maid, slotNo, obj, obj.transform);
            BackUpData.Refresh();
        }

        // Zapisuje aktualny stan slotu jako nowy wpis biblioteki (jeszcze nie dodaje go do bazy).
        // Zapisujemy TYLKO kosci, ktore uzytkownik zmienil (wg kopii zapasowych PartsEdit), a nie caly lancuch kosci z ich
        // chwilowymi pozami: pozy kosci sterowanych przez gre sa przestarzale po zmianie pozy postaci.
        // hasChanges = false, gdy w slocie nic nie zmieniono (wtedy wpis byl by pusty).
        public static ModEntry CaptureSlot(Maid maid, int slotNo, string name, out bool hasChanges) {
            hasChanges = false;
            GameObject obj = SetManager.GetSlotObject(maid, slotNo);
            if (!obj) return null;
            ObjectData data = SetManager.Capture(maid, slotNo, obj);
            if (data == null) return null;

            HashSet<string> changed = new HashSet<string>();
            bool rootChanged = false;
            BackUpObjectData backup = BackUpData.GetOrNullMaidObjectData(maid, slotNo, obj);
            if (backup != null) {
                foreach (KeyValuePair<Transform, BackUpBoneData> pair in backup.boneDataDic) {
                    if (!pair.Key || !pair.Value.HasChange()) continue;
                    if (pair.Key == obj.transform) rootChanged = true; else changed.Add(pair.Key.name);
                }
                hasChanges = changed.Count > 0 || rootChanged || backup.changedYure;
            }
            data.transformDataList = data.transformDataList.Where(t => changed.Contains(t.name)).ToList();
            if (!rootChanged) data.bMaidParts = false;
            return DbConvert.FromObjectData(data, name, "", DateTime.UtcNow);
        }
    }
}
