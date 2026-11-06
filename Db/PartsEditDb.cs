using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CM3D2.PartsEdit.Plugin.Db {
    // Jedna baza (jeden plik JSON): biblioteka wpisow, zestawy i powiazania z pokojowkami.
    public class PartsEditDb {
        // Wersja formatu pliku. Zwiekszac przy zmianach niezgodnych wstecz i dopisac uaktualnienie w Upgrade().
        public const int CurrentVersion = 1;

        public int Version { get; set; }
        // true po jednorazowym imporcie starych plikow XML (patrz LegacyMigration)
        public bool LegacyImported { get; set; }

        public Dictionary<Guid, ModEntry> Entries { get; set; }
        public Dictionary<Guid, ModSet> Sets { get; set; }
        public Dictionary<string, MaidBinding> Maids { get; set; }

        // true, gdy plik zapisala nowsza wersja pluginu. Nieznane pola ignorujemy przy odczycie, wiec zapis by je usunal -
        // dlatego takiej bazy nie nadpisujemy (Save zglasza wyjatek).
        [JsonIgnore]
        public bool NewerThanSupported { get; private set; }

        public PartsEditDb() {
            Version = CurrentVersion;
            Entries = new Dictionary<Guid, ModEntry>();
            Sets = new Dictionary<Guid, ModSet>();
            Maids = new Dictionary<string, MaidBinding>();
        }

        // ---------- zapis / odczyt ----------

        static JsonSerializerSettings Settings() {
            JsonSerializerSettings s = new JsonSerializerSettings();
            s.Formatting = Formatting.Indented;
            // nieznane pola (nowszy plik) ignorujemy, brakujace dostaja wartosci domyslne z konstruktorow
            s.MissingMemberHandling = MissingMemberHandling.Ignore;
            s.NullValueHandling = NullValueHandling.Ignore;
            s.ObjectCreationHandling = ObjectCreationHandling.Replace;
            return s;
        }

        // Wczytuje baze. Brak pliku -> pusta baza. Uszkodzony plik jest odkladany obok (.corrupt-...), a zwracana jest
        // pusta baza, zeby nigdy nie stracic danych przez nadpisanie.
        public static PartsEditDb Load(string path, out string problem) {
            problem = null;
            if (!File.Exists(path)) return new PartsEditDb();
            try {
                string json = File.ReadAllText(path);
                PartsEditDb db = JsonConvert.DeserializeObject<PartsEditDb>(json, Settings());
                if (db == null) throw new InvalidDataException("empty file");
                db.Normalize();
                if (db.Version > CurrentVersion) {
                    db.NewerThanSupported = true;
                    problem = "Database version " + db.Version + " is newer than supported (" + CurrentVersion + "); it is used read-only and will not be overwritten.";
                }
                db.Upgrade();
                return db;
            } catch (Exception e) {
                string aside = path + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                try { File.Move(path, aside); } catch (Exception) { aside = "(could not move)"; }
                problem = "Could not read " + Path.GetFileName(path) + " (" + e.Message + "), moved to " + aside + ", starting with an empty database.";
                return new PartsEditDb();
            }
        }

        // Zapis przez plik tymczasowy, poprzednia wersja zostaje jako .bak.
        public void Save(string path) {
            if (NewerThanSupported) throw new InvalidOperationException("database was written by a newer version, not overwriting it");
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonConvert.SerializeObject(this, Settings()));
            if (File.Exists(path)) {
                File.Replace(tmp, path, path + ".bak");
            } else {
                File.Move(tmp, path);
            }
        }

        // Uzupelnia null-e po wczytaniu (np. gdy ktos recznie edytowal plik).
        void Normalize() {
            if (Entries == null) Entries = new Dictionary<Guid, ModEntry>();
            if (Sets == null) Sets = new Dictionary<Guid, ModSet>();
            if (Maids == null) Maids = new Dictionary<string, MaidBinding>();
            foreach (ModEntry e in Entries.Values) {
                if (e.Name == null) e.Name = "";
                if (e.Slot == null) e.Slot = "";
                if (e.ObjectName == null) e.ObjectName = "";
                if (e.Source == null) e.Source = "";
                if (e.OwnerGuid == null) e.OwnerGuid = "";
                if (e.OwnerName == null) e.OwnerName = "";
                if (e.Conditions == null) e.Conditions = new List<EntryCondition>();
                foreach (EntryCondition c in e.Conditions) {
                    if (c.Slot == null) c.Slot = "";
                    if (c.Pattern == null) c.Pattern = "";
                }
                // Stary tryb "Same item only" (Match) to teraz zwykly warunek na nazwe obiektu. Gdy warunki byly juz wlaczone,
                // Match byl ignorowany, wiec tylko go zerujemy. Operacja jest idempotentna.
                if (e.Match == MatchMode.SameObjectName) {
                    if (!e.UseConditions && !string.IsNullOrEmpty(e.ObjectName)) {
                        e.UseConditions = true;
                        e.RequireAll = true;
                        e.Conditions.Add(new EntryCondition { Kind = ConditionKind.ObjectName, Pattern = e.ObjectName });
                    }
                    e.Match = MatchMode.AnyInSlot;
                }
                if (e.Root == null) e.Root = new BoneRecord();
                if (e.Bones == null) e.Bones = new List<BoneRecord>();
            }
            foreach (ModSet s in Sets.Values) {
                if (s.Name == null) s.Name = "";
                if (s.EntryIds == null) s.EntryIds = new List<Guid>();
                if (s.Source == null) s.Source = "";
                if (s.OwnerGuid == null) s.OwnerGuid = "";
                if (s.OwnerName == null) s.OwnerName = "";
            }
            foreach (MaidBinding m in Maids.Values) {
                if (m.EntryIds == null) m.EntryIds = new List<Guid>();
                if (m.SetIds == null) m.SetIds = new List<Guid>();
                if (m.LastSeenName == null) m.LastSeenName = "";
            }
        }

        // Miejsce na uaktualnianie starszych wersji formatu (na razie jest tylko wersja 1).
        void Upgrade() {
            if (Version < CurrentVersion) Version = CurrentVersion;
        }

        // ---------- wpisy ----------

        public ModEntry AddEntry(ModEntry entry) {
            if (entry.Id == Guid.Empty) entry.Id = Guid.NewGuid();
            Entries[entry.Id] = entry;
            return entry;
        }

        public bool RemoveEntry(Guid id) {
            if (!Entries.Remove(id)) return false;
            foreach (ModSet s in Sets.Values) s.EntryIds.RemoveAll(e => e == id);
            foreach (MaidBinding m in Maids.Values) m.EntryIds.RemoveAll(e => e == id);
            return true;
        }

        public ModEntry FindEntryBySource(string source) {
            if (string.IsNullOrEmpty(source)) return null;
            return Entries.Values.FirstOrDefault(e => e.Source == source);
        }

        public IEnumerable<ModEntry> EntriesForSlot(string slot) {
            return Entries.Values.Where(e => string.Equals(e.Slot, slot, StringComparison.OrdinalIgnoreCase));
        }

        // ---------- zestawy ----------

        public ModSet AddSet(string name) {
            ModSet set = new ModSet();
            set.Name = name ?? "";
            Sets[set.Id] = set;
            return set;
        }

        public ModSet FindSetBySource(string source) {
            if (string.IsNullOrEmpty(source)) return null;
            return Sets.Values.FirstOrDefault(s => s.Source == source);
        }

        public ModSet FindSetByName(string name) {
            return Sets.Values.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveSet(Guid id) {
            if (!Sets.Remove(id)) return false;
            foreach (MaidBinding m in Maids.Values) m.SetIds.RemoveAll(s => s == id);
            return true;
        }

        // Dopisuje wpis do zestawu (bez duplikatow). Zwraca false, gdy zestaw/wpis nie istnieje lub juz jest w zestawie.
        public bool AddToSet(Guid setId, Guid entryId) {
            ModSet set;
            if (!Sets.TryGetValue(setId, out set) || !Entries.ContainsKey(entryId)) return false;
            if (set.EntryIds.Contains(entryId)) return false;
            set.EntryIds.Add(entryId);
            return true;
        }

        public bool RemoveFromSet(Guid setId, Guid entryId) {
            ModSet set;
            if (!Sets.TryGetValue(setId, out set)) return false;
            return set.EntryIds.RemoveAll(e => e == entryId) > 0;
        }

        // ---------- "tej pokojowki" (filtr listy) ----------

        // Wpis nalezy do pokojowki, gdy zapisano go przy niej albo jest do niej przypisany (bezposrednio lub przez zestaw).
        // Pusty guid (brak wybranej pokojowki) = nie filtrujemy.
        public bool IsEntryOfMaid(string maidGuid, ModEntry entry) {
            if (string.IsNullOrEmpty(maidGuid)) return true;
            if (entry.OwnerGuid == maidGuid) return true;
            MaidBinding b;
            if (!Maids.TryGetValue(maidGuid, out b)) return false;
            if (b.EntryIds.Contains(entry.Id)) return true;
            foreach (Guid setId in b.SetIds) {
                ModSet set;
                if (Sets.TryGetValue(setId, out set) && set.EntryIds.Contains(entry.Id)) return true;
            }
            return false;
        }

        // Zestaw nalezy do pokojowki, gdy utworzono go przy niej albo jest do niej przypisany.
        public bool IsSetOfMaid(string maidGuid, ModSet set) {
            if (string.IsNullOrEmpty(maidGuid)) return true;
            if (set.OwnerGuid == maidGuid) return true;
            MaidBinding b;
            return Maids.TryGetValue(maidGuid, out b) && b.SetIds.Contains(set.Id);
        }

        // ---------- pokojowki ----------

        public MaidBinding GetOrCreateMaid(string maidGuid, string lastSeenName) {
            MaidBinding m;
            if (!Maids.TryGetValue(maidGuid, out m)) {
                m = new MaidBinding();
                m.MaidGuid = maidGuid;
                Maids[maidGuid] = m;
            }
            if (!string.IsNullOrEmpty(lastSeenName)) m.LastSeenName = lastSeenName;
            return m;
        }

        // Wpisy do nalozenia na pokojowke, w kolejnosci: najpierw zestawy (w kolejnosci przypisania, a w nich wpisy po kolei),
        // potem wpisy przypisane bezposrednio. Powtorzenia pomijane. Przy nakladaniu pozniejsze nadpisuja wczesniejsze kosci.
        public List<ModEntry> EffectiveEntries(string maidGuid) {
            List<ModEntry> result = new List<ModEntry>();
            MaidBinding m;
            if (string.IsNullOrEmpty(maidGuid) || !Maids.TryGetValue(maidGuid, out m)) return result;
            HashSet<Guid> seen = new HashSet<Guid>();
            foreach (Guid setId in m.SetIds) {
                ModSet set;
                if (!Sets.TryGetValue(setId, out set)) continue;
                foreach (Guid entryId in set.EntryIds) AddEffective(entryId, seen, result);
            }
            foreach (Guid entryId in m.EntryIds) AddEffective(entryId, seen, result);
            return result;
        }

        void AddEffective(Guid entryId, HashSet<Guid> seen, List<ModEntry> result) {
            ModEntry e;
            if (Entries.TryGetValue(entryId, out e) && seen.Add(entryId)) result.Add(e);
        }
    }
}
