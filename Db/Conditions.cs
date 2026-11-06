using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CM3D2.PartsEdit.Plugin.Db {
    // Warunki, kiedy wpis ma dzialac (w stylu ShapeKeyMaster: wzorce nazw plikow menu z *, wszystkie / ktorykolwiek).
    // Ta warstwa nie zalezy od gry: gra podaje liste zalozonych przedmiotow (WornItem), a logika decyduje, czy wpis pasuje.

    public enum ConditionKind {
        // w slocie cos jest zalozone
        SlotEquipped = 0,
        // nazwa pliku menu zalozonego przedmiotu pasuje do wzorca (np. *heel.menu)
        MenuFile = 1,
        // nazwa obiektu przedmiotu (np. _SM_hhmod*heel)
        ObjectName = 2,
        // nazwa pliku modelu (.model)
        ModelFile = 3
    }

    public class EntryCondition {
        public ConditionKind Kind { get; set; }
        // pusty = slot wpisu, "*" = dowolny slot, inaczej nazwa slotu
        public string Slot { get; set; }
        public string Pattern { get; set; }
        // odwrocenie wyniku ("NIE pasuje")
        public bool Negate { get; set; }

        public EntryCondition() {
            Kind = ConditionKind.MenuFile;
            Slot = "";
            Pattern = "";
            Negate = false;
        }
    }

    // Zalozony przedmiot pokojowki (to, co gra wie o slocie).
    public class WornItem {
        public string Slot { get; set; }
        public string Category { get; set; }
        public string ObjectName { get; set; }
        public string MenuFile { get; set; }
        public string ModelFile { get; set; }

        public WornItem() {
            Slot = "";
            Category = "";
            ObjectName = "";
            MenuFile = "";
            ModelFile = "";
        }
    }

    // Wynik jednego warunku (do podgladu w oknie).
    public class ConditionResult {
        public EntryCondition Condition;
        // wynik po uwzglednieniu negacji
        public bool Result;
        // wartosc, ktora pasowala (np. nazwa menu); puste, gdy nic nie pasuje
        public string Found = "";
    }

    // Dopasowanie z gwiazdka (*) i znakiem zapytania (?), bez rozrozniania wielkosci liter. Wzorzec bez znakow
    // specjalnych musi pasowac w calosci. Pusty wzorzec nie pasuje do niczego.
    public static class Wildcard {
        public static bool IsMatch(string pattern, string text) {
            if (string.IsNullOrEmpty(pattern) || text == null) return false;
            if (pattern.IndexOf('*') < 0 && pattern.IndexOf('?') < 0) {
                return string.Equals(pattern, text, StringComparison.OrdinalIgnoreCase);
            }
            string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(text, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }

    public static class ConditionLogic {
        static IEnumerable<WornItem> ItemsFor(ModEntry entry, EntryCondition c, IList<WornItem> worn) {
            string slot = string.IsNullOrEmpty(c.Slot) ? entry.Slot : c.Slot;
            if (slot == "*") return worn;
            return worn.Where(w => string.Equals(w.Slot, slot, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(w.Category, slot, StringComparison.OrdinalIgnoreCase));
        }

        // Wynik jednego warunku przed negacja; found = pasujaca wartosc.
        static bool Raw(ModEntry entry, EntryCondition c, IList<WornItem> worn, out string found) {
            found = "";
            foreach (WornItem item in ItemsFor(entry, c, worn)) {
                switch (c.Kind) {
                    case ConditionKind.SlotEquipped:
                        found = item.ObjectName;
                        return true;
                    case ConditionKind.MenuFile:
                        if (Wildcard.IsMatch(c.Pattern, item.MenuFile)) { found = item.MenuFile; return true; }
                        break;
                    case ConditionKind.ObjectName:
                        if (Wildcard.IsMatch(c.Pattern, item.ObjectName)) { found = item.ObjectName; return true; }
                        break;
                    case ConditionKind.ModelFile:
                        if (Wildcard.IsMatch(c.Pattern, item.ModelFile)) { found = item.ModelFile; return true; }
                        break;
                }
            }
            return false;
        }

        // Czy wpis pasuje do tego, co pokojowka ma teraz na sobie: w jego slocie musi cos byc zalozone, a potem
        //  - bez warunkow: stary tryb dopasowania (Match: dowolny przedmiot / ta sama nazwa obiektu),
        //  - z warunkami: wszystkie (RequireAll) albo ktorykolwiek z nich musi byc spelniony.
        public static bool Matches(ModEntry entry, IList<WornItem> worn) {
            WornItem own = worn.FirstOrDefault(w => string.Equals(w.Slot, entry.Slot, StringComparison.OrdinalIgnoreCase)
                                                 || string.Equals(w.Category, entry.Slot, StringComparison.OrdinalIgnoreCase));
            if (own == null) return false;

            if (!entry.UseConditions) return entry.Matches(own.ObjectName);
            if (entry.Conditions.Count == 0) return true;

            foreach (EntryCondition c in entry.Conditions) {
                string found;
                bool result = Raw(entry, c, worn, out found);
                if (c.Negate) result = !result;
                if (entry.RequireAll && !result) return false;
                if (!entry.RequireAll && result) return true;
            }
            return entry.RequireAll;
        }

        // Wynik kazdego warunku z osobna (podglad w oknie: "czy pasuje do tego, co ma teraz na sobie").
        public static List<ConditionResult> Explain(ModEntry entry, IList<WornItem> worn) {
            List<ConditionResult> list = new List<ConditionResult>();
            foreach (EntryCondition c in entry.Conditions) {
                ConditionResult r = new ConditionResult();
                r.Condition = c;
                bool raw = Raw(entry, c, worn, out r.Found);
                r.Result = c.Negate ? !raw : raw;
                list.Add(r);
            }
            return list;
        }
    }
}
