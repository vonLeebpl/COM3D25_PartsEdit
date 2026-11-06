namespace CM3D2.PartsEdit.Plugin.Db {
    // Nazwy kosci-zastepnikow (Independent bones). Edytowalny wezel dostaje do nazwy prawdziwej kosci sufiks "_Adj", wezel
    // posredni (podaza za prawdziwa kosc) "_Follow". Wersja 0.2 uzywala "_ShoeAdj" / "_ShoeFollow" - takie nazwy w zapisanych
    // danych (baza, presety xml, sceny) sa rozpoznawane i zamieniane na nowe (Normalize).
    public static class ProxyNames {
        public const string Edit = "_Adj";
        public const string Follow = "_Follow";
        public const string LegacyEdit = "_ShoeAdj";
        public const string LegacyFollow = "_ShoeFollow";

        public static bool IsEdit(string name) {
            return name != null && (name.EndsWith(Edit) || name.EndsWith(LegacyEdit));
        }

        public static bool IsFollow(string name) {
            return name != null && (name.EndsWith(Follow) || name.EndsWith(LegacyFollow));
        }

        // Zamienia stary sufiks na nowy (nazwy bez starego sufiksu zostaja bez zmian). Idempotentne.
        public static string Normalize(string name) {
            if (name == null) return null;
            if (name.EndsWith(LegacyEdit)) return name.Substring(0, name.Length - LegacyEdit.Length) + Edit;
            if (name.EndsWith(LegacyFollow)) return name.Substring(0, name.Length - LegacyFollow.Length) + Follow;
            return name;
        }
    }
}
