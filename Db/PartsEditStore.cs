using System;
using System.IO;

namespace CM3D2.PartsEdit.Plugin.Db {
    // Trzyma jedna wspolna baze dla pluginu i dba o jej wczytanie, migracje starych plikow i zapis.
    // Jedyne miejsce w Db\ zalezne od BepInEx (logowanie), reszta klas bazy jest niezalezna od gry.
    internal static class PartsEditStore {
        public const string FileName = "PartsEdit.db.json";

        static string path = "";
        public static PartsEditDb Db { get; private set; }
        public static string FilePath { get { return path; } }

        // Wolane raz przy starcie pluginu. Blad bazy nigdy nie moze przeszkodzic w zaladowaniu pluginu.
        public static void Init(string configDir, string presetsDir) {
            path = Path.Combine(configDir, FileName);
            try {
                string problem;
                Db = PartsEditDb.Load(path, out problem);
                if (problem != null) Log.Warn("Library: " + problem);

                bool changed = false;
                if (!Db.LegacyImported) {
                    MigrationReport report = LegacyMigration.Run(Db, presetsDir);
                    Log.Info("Library: imported legacy presets - " + report);
                    foreach (string p in report.Problems) Log.Warn("Library migration: " + p);
                    changed = true;
                }
                if (changed || !File.Exists(path)) Save();

                Log.Info("Library: " + Db.Entries.Count + " entries, " + Db.Sets.Count + " sets, " + Db.Maids.Count + " maids (" + path + ")");
            } catch (Exception e) {
                Log.Error("Library init failed: " + e);
                Db = new PartsEditDb();
            }
        }

        public static void Save() {
            if (Db == null || string.IsNullOrEmpty(path)) return;
            try {
                Db.Save(path);
            } catch (Exception e) {
                Log.Error("Library save failed: " + e.Message);
            }
        }
    }
}
