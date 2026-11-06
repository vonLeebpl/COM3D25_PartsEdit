using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace CM3D2.PartsEdit.Plugin.Db {
    public class MigrationReport {
        public int EntriesAdded;
        public int EntriesSkipped;
        public int SetsAdded;
        public int SetsSkipped;
        public int MaidsBound;
        public List<string> Problems = new List<string>();

        public override string ToString() {
            return "entries +" + EntriesAdded + " (skipped " + EntriesSkipped + "), sets +" + SetsAdded
                + " (skipped " + SetsSkipped + "), maids " + MaidsBound + ", problems " + Problems.Count;
        }
    }

    // Jednorazowy import starych danych: Presets\*.xml (ObjectData), Presets\Sets\*.set.xml i Presets\Sets\autoload.xml.
    // Stare pliki zostaja nietkniete. Import jest idempotentny (wpisy rozpoznajemy po polu Source).
    //
    // Pliki czytamy bezposrednio jako XML, a nie przez XmlSerializer z typami Unity: stary format zapisuje obrot jako
    // x,y,z,w ORAZ eulerAngles, a przy deserializacji Unity przelicza kwaternion z eulerAngles (tracac dokladnosc).
    // Tu bierzemy dokladnie x,y,z,w, a eulerAngles ignorujemy.
    public static class LegacyMigration {
        public static MigrationReport Run(PartsEditDb db, string presetsDir) {
            MigrationReport report = new MigrationReport();
            if (string.IsNullOrEmpty(presetsDir) || !Directory.Exists(presetsDir)) {
                db.LegacyImported = true;
                return report;
            }

            // 1) pojedyncze presety -> wpisy
            Dictionary<string, Guid> entryByFile = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in Directory.GetFiles(presetsDir, "*.xml", SearchOption.TopDirectoryOnly)) {
                string fileName = Path.GetFileNameWithoutExtension(path);
                string source = "xml:" + fileName;
                ModEntry existing = db.FindEntryBySource(source);
                if (existing != null) {
                    entryByFile[fileName] = existing.Id;
                    report.EntriesSkipped++;
                    continue;
                }
                try {
                    ModEntry entry = ReadPreset(path, fileName, source);
                    db.AddEntry(entry);
                    entryByFile[fileName] = entry.Id;
                    report.EntriesAdded++;
                } catch (Exception e) {
                    report.Problems.Add(fileName + ".xml: " + e.Message);
                }
            }

            // 2) zestawy
            string setsDir = Path.Combine(presetsDir, "Sets");
            Dictionary<string, Guid> setByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(setsDir)) {
                foreach (string path in Directory.GetFiles(setsDir, "*.set.xml", SearchOption.TopDirectoryOnly)) {
                    string fileBase = Path.GetFileName(path);
                    fileBase = fileBase.Substring(0, fileBase.Length - ".set.xml".Length);
                    string source = "xml:" + fileBase;
                    ModSet existing = db.FindSetBySource(source);
                    if (existing != null) {
                        setByName[fileBase] = existing.Id;
                        report.SetsSkipped++;
                        continue;
                    }
                    try {
                        XElement root = XDocument.Load(path).Root;
                        if (root == null || root.Name.LocalName != "PartsEditSet") throw new InvalidDataException("not a PartsEditSet file");
                        string setName = Text(root, "name");
                        ModSet set = db.AddSet(string.IsNullOrEmpty(setName) ? fileBase : setName);
                        set.Source = source;
                        set.Created = File.GetLastWriteTimeUtc(path);
                        XElement entries = root.Element("entries");
                        if (entries != null) {
                            foreach (XElement member in entries.Elements("SetEntry")) {
                                string memberFile = Text(member, "fileName");
                                Guid entryId;
                                if (entryByFile.TryGetValue(memberFile, out entryId)) {
                                    if (!set.EntryIds.Contains(entryId)) set.EntryIds.Add(entryId);
                                } else {
                                    report.Problems.Add("set '" + set.Name + "': missing member file '" + memberFile + "' (" + Text(member, "slotName") + ")");
                                }
                            }
                        }
                        setByName[fileBase] = set.Id;
                        report.SetsAdded++;
                    } catch (Exception e) {
                        report.Problems.Add(fileBase + ".set.xml: " + e.Message);
                    }
                }

                // 3) autoload: guid pokojowki -> zestaw
                string autoloadPath = Path.Combine(setsDir, "autoload.xml");
                if (File.Exists(autoloadPath)) {
                    try {
                        XElement root = XDocument.Load(autoloadPath).Root;
                        XElement entries = root == null ? null : root.Element("entries");
                        if (entries != null) {
                            foreach (XElement a in entries.Elements("AutoloadEntry")) {
                                string guid = Text(a, "guid");
                                string setName = Text(a, "setName");
                                if (string.IsNullOrEmpty(guid)) continue;
                                Guid setId;
                                if (!setByName.TryGetValue(setName, out setId)) {
                                    report.Problems.Add("autoload for maid " + guid + ": set '" + setName + "' not found");
                                    continue;
                                }
                                MaidBinding m = db.GetOrCreateMaid(guid, null);
                                if (!m.SetIds.Contains(setId)) m.SetIds.Add(setId);
                                m.AutoLoad = true;
                                report.MaidsBound++;
                            }
                        }
                    } catch (Exception e) {
                        report.Problems.Add("autoload.xml: " + e.Message);
                    }
                }
            }

            db.LegacyImported = true;
            return report;
        }

        // ---------- odczyt starego ObjectData ----------

        static ModEntry ReadPreset(string path, string fileName, string source) {
            XElement root = XDocument.Load(path).Root;
            if (root == null || root.Name.LocalName != "ObjectData") throw new InvalidDataException("not an ObjectData file");
            XElement rootData = root.Element("rootData");
            if (rootData == null) throw new InvalidDataException("missing rootData");

            ModEntry e = new ModEntry();
            e.Name = fileName;
            e.Source = source;
            e.Created = File.GetLastWriteTimeUtc(path);
            e.Slot = Text(root, "slotName");
            e.MaidParts = Bool(root, "bMaidParts");
            e.Yure = Bool(root, "bYure");
            e.Match = MatchMode.AnyInSlot; // jak dotychczasowy Import; mozna zmienic na wpisie
            e.Root = ReadBone(rootData);
            e.ObjectName = e.Root.Name;
            XElement list = root.Element("transformDataList");
            if (list != null) {
                foreach (XElement t in list.Elements("TransformData")) e.Bones.Add(ReadBone(t));
            }
            return e;
        }

        static BoneRecord ReadBone(XElement t) {
            BoneRecord b = new BoneRecord();
            b.Name = ProxyNames.Normalize(Text(t, "name")); // nazwy kosci-zastepnikow z wersji 0.2 -> nowe
            XElement pos = t.Element("position");
            if (pos != null) b.Position = new Vec3(Float(pos, "x", 0f), Float(pos, "y", 0f), Float(pos, "z", 0f));
            XElement rot = t.Element("rotation");
            if (rot != null) b.Rotation = new Quat(Float(rot, "x", 0f), Float(rot, "y", 0f), Float(rot, "z", 0f), Float(rot, "w", 1f));
            XElement scl = t.Element("scale");
            if (scl != null) b.Scale = new Vec3(Float(scl, "x", 1f), Float(scl, "y", 1f), Float(scl, "z", 1f));
            return b;
        }

        static string Text(XElement parent, string name) {
            XElement e = parent.Element(name);
            return e == null ? "" : e.Value;
        }

        static bool Bool(XElement parent, string name) {
            string v = Text(parent, name).Trim();
            return string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) || v == "1";
        }

        static float Float(XElement parent, string name, float fallback) {
            string v = Text(parent, name).Trim();
            float f;
            if (v.Length > 0 && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out f)) return f;
            return fallback;
        }
    }
}
