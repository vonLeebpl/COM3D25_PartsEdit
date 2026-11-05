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

        // Zapisuje aktualny stan slotu jako nowy wpis biblioteki (jeszcze nie dodaje go do bazy).
        public static ModEntry CaptureSlot(Maid maid, int slotNo, string name) {
            GameObject obj = SetManager.GetSlotObject(maid, slotNo);
            if (!obj) return null;
            ObjectData data = SetManager.Capture(maid, slotNo, obj);
            if (data == null) return null;
            return DbConvert.FromObjectData(data, name, "", DateTime.UtcNow);
        }
    }
}
