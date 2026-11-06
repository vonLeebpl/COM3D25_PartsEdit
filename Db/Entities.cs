using System;
using System.Collections.Generic;

namespace CM3D2.PartsEdit.Plugin.Db {
    // Jedna modyfikacja = jeden slot. Wpisy sa wspolna biblioteka: zestawy i pokojowki odwoluja sie do nich po Guid.
    public class ModEntry {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }

        // nazwa slotu (np. "shoes", "glove", "accKami_1_")
        public string Slot { get; set; }
        // nazwa obiektu przedmiotu, dla ktorego wpis zapisano (np. "_SM_hhmod_ch1_1503t_strap_heel")
        public string ObjectName { get; set; }
        public MatchMode Match { get; set; }

        // true, gdy zapisano tez korzen obiektu (Maid Parts)
        public bool MaidParts { get; set; }
        // stan "Shaking Bone" w chwili zapisu
        public bool Yure { get; set; }

        public BoneRecord Root { get; set; }
        public List<BoneRecord> Bones { get; set; }

        // skad pochodzi wpis (np. "xml:kelly_shoe" po migracji), pozwala uniknac duplikatow przy ponownej migracji
        public string Source { get; set; }

        // dla ktorej pokojowki wpis zapisano (maid.status.guid); puste dla wpisow z migracji. Sluzy do filtrowania listy.
        public string OwnerGuid { get; set; }
        public string OwnerName { get; set; }

        // Warunki dzialania wpisu (patrz Conditions.cs). Gdy UseConditions = false, obowiazuje stary tryb dopasowania (Match).
        public bool UseConditions { get; set; }
        // true = wszystkie warunki musza byc spelnione, false = wystarczy ktorykolwiek
        public bool RequireAll { get; set; }
        public List<EntryCondition> Conditions { get; set; }

        public ModEntry() {
            Id = Guid.NewGuid();
            Name = "";
            Created = DateTime.UtcNow;
            Slot = "";
            ObjectName = "";
            Match = MatchMode.AnyInSlot;
            Root = new BoneRecord();
            Bones = new List<BoneRecord>();
            Source = "";
            OwnerGuid = "";
            OwnerName = "";
            UseConditions = false;
            RequireAll = true;
            Conditions = new List<EntryCondition>();
        }

        // Czy wpis pasuje do przedmiotu o podanej nazwie obiektu (slot sprawdza wywolujacy).
        public bool Matches(string objectName) {
            if (Match == MatchMode.AnyInSlot) return true;
            return string.Equals(ObjectName, objectName, StringComparison.Ordinal);
        }
    }

    // Zestaw = uporzadkowana lista wpisow (kolejnosc ma znaczenie: pozniejsze nadpisuja wczesniejsze kosci).
    public class ModSet {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public List<Guid> EntryIds { get; set; }
        public string Source { get; set; }
        public string OwnerGuid { get; set; }
        public string OwnerName { get; set; }

        public ModSet() {
            Id = Guid.NewGuid();
            Name = "";
            Created = DateTime.UtcNow;
            EntryIds = new List<Guid>();
            Source = "";
            OwnerGuid = "";
            OwnerName = "";
        }
    }

    // Powiazanie z konkretna pokojowka (klucz: maid.status.guid).
    public class MaidBinding {
        public string MaidGuid { get; set; }
        // ostatnio widziana nazwa, tylko dla czytelnosci pliku i listy w GUI
        public string LastSeenName { get; set; }
        // wpisy przypisane bezposrednio
        public List<Guid> EntryIds { get; set; }
        // zestawy przypisane do pokojowki
        public List<Guid> SetIds { get; set; }
        // wczytuj zestawy/wpisy automatycznie po zaladowaniu pokojowki
        public bool AutoLoad { get; set; }

        public MaidBinding() {
            MaidGuid = "";
            LastSeenName = "";
            EntryIds = new List<Guid>();
            SetIds = new List<Guid>();
            AutoLoad = false;
        }
    }
}
