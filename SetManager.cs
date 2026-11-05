using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    static class SetManager {
        public static string setDirectory {
            get { return PresetManager.directoryPath + @"Sets\"; }
        }

        public static List<int> GetEquippedSlotIds(Maid maid) {
            List<int> ids = new List<int>();
            if (!maid || maid.body0 == null || maid.body0.goSlot == null) return ids;
            foreach (TBodySkin slot in maid.body0.goSlot.GetListParents()) {
                if (slot == null || !slot.obj || !slot.boVisible) continue;
#if COM3D2
                ids.Add((int)slot.SlotId);
#else
                ids.Add(slot.CategoryIdx);
#endif
            }
            return ids;
        }

        public static string SlotLabel(Maid maid, int slotNo) {
            if (slotNo == (int)EXSlot.Base) return "base";
            if (slotNo < 0 || !maid || maid.body0 == null) return slotNo.ToString();
            string name = SlotUtil.GetSlotName(slotNo);
            string category = maid.body0.goSlot[slotNo].Category;
            if (!string.IsNullOrEmpty(category) && category != name) return name + " (" + category + ")";
            return name;
        }

        public static GameObject GetSlotObject(Maid maid, int slotNo) {
            if (!maid || maid.body0 == null) return null;
            if (slotNo == (int)EXSlot.Base) return maid.body0.m_Bones.gameObject;
            if (slotNo < 0 || slotNo >= maid.body0.goSlot.Count) return null;
            return maid.body0.goSlot[slotNo].obj;
        }

        public static int FindSlotId(Maid maid, string slotName) {
            if (slotName == "base") return (int)EXSlot.Base;
            int id = SlotUtil.GetSlotID(slotName);
            if (id >= 0) return id;
            if (!maid || maid.body0 == null) return (int)EXSlot.None;
            for (int i = 0; i < maid.body0.goSlot.Count; i++) {
                TBodySkin slot = maid.body0.goSlot[i];
                if (slot != null && slot.Category == slotName) return i;
            }
            return (int)EXSlot.None;
        }

        public static string Sanitize(string name) {
            if (string.IsNullOrEmpty(name)) return "set";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            name = name.Trim();
            return name.Length == 0 ? "set" : name;
        }

        public static string[] GetSetNames() {
            if (!Directory.Exists(setDirectory)) return new string[0];
            return Directory.GetFiles(setDirectory, "*.set.xml", SearchOption.TopDirectoryOnly)
                .Select(path => Path.GetFileName(path).Replace(".set.xml", ""))
                .OrderBy(name => name)
                .ToArray();
        }

        public static string SaveSet(Maid maid, string setName, IList<int> slotIds, bool includeIK) {
            setName = Sanitize(setName);
            if (!maid) return "No maid selected.";
            if (slotIds == null || slotIds.Count == 0) return "No categories checked.";
            Directory.CreateDirectory(setDirectory);
            Directory.CreateDirectory(PresetManager.directoryPath);

            PartsEditSet set = new PartsEditSet();
            set.name = setName;
            int saved = 0;
            foreach (int slotNo in slotIds.Distinct()) {
                GameObject obj = GetSlotObject(maid, slotNo);
                if (!obj) continue;
                ObjectData data = Capture(maid, slotNo, obj);
                if (data == null) continue;
                string fileName = setName + "_" + data.slotName;
                WriteObjectXml(fileName, data);
                PresetManager.PresetFileDataDic[fileName] = new PresetFileData { filename = fileName, objectData = data };
                set.entries.Add(new SetEntry { slotName = data.slotName, fileName = fileName, rootName = data.rootData.name });
                if (includeIK) IKManager.SaveKey(obj.name);
                saved++;
            }
            if (saved == 0) return "Nothing equipped in the checked categories.";
            WriteSet(set);
            return "Saved set '" + setName + "' (" + saved + " categories).";
        }

        public static string LoadSet(Maid maid, string setName, bool skipNameMismatch, bool includeIK) {
            setName = Sanitize(setName);
            string path = setDirectory + setName + ".set.xml";
            if (!File.Exists(path)) return "Set not found.";
            if (!maid) return "No maid selected.";
            PartsEditSet set = ReadSet(path);
            int applied = 0;
            List<string> skipped = new List<string>();
            foreach (SetEntry entry in set.entries) {
                if (!PresetManager.PresetFileDataDic.ContainsKey(entry.fileName)) {
                    skipped.Add(entry.slotName + " (missing xml)");
                    continue;
                }
                int slotNo = FindSlotId(maid, entry.slotName);
                GameObject obj = GetSlotObject(maid, slotNo);
                if (!obj) {
                    skipped.Add(entry.slotName + " (empty)");
                    continue;
                }
                ObjectData data = PresetManager.PresetFileDataDic[entry.fileName].objectData;
                if (skipNameMismatch && data.rootData != null && data.rootData.name != obj.name) {
                    skipped.Add(entry.slotName + " (name)");
                    continue;
                }
                Apply(maid, slotNo, obj, data);
                if (includeIK) {
                    IKManager.BindObject();
                    IKManager.SaveKey(obj.name);
                }
                applied++;
            }
            string msg = "Loaded " + applied + "/" + set.entries.Count + " from '" + setName + "'.";
            if (skipped.Count > 0) msg += " Skipped: " + string.Join(", ", skipped.ToArray());
            return msg;
        }

        public static string DeleteSet(string setName, bool deleteMembers) {
            setName = Sanitize(setName);
            string path = setDirectory + setName + ".set.xml";
            if (!File.Exists(path)) return "Set not found.";
            if (deleteMembers) {
                PartsEditSet set = ReadSet(path);
                foreach (SetEntry entry in set.entries) {
                    string member = PresetManager.directoryPath + entry.fileName + ".xml";
                    if (File.Exists(member)) File.Delete(member);
                    PresetManager.PresetFileDataDic.Remove(entry.fileName);
                }
            }
            File.Delete(path);
            return "Deleted set '" + setName + "'.";
        }

        public static string CopyCategories(Maid source, IList<Maid> targets, IList<int> slotIds, bool copyYure, bool skipNameMismatch) {
            if (!source) return "No source maid.";
            if (targets == null || targets.Count == 0) return "No destination maids.";
            if (slotIds == null || slotIds.Count == 0) return "No categories checked.";
            int applied = 0;
            int skipped = 0;
            foreach (int slotNo in slotIds.Distinct()) {
                GameObject srcObj = GetSlotObject(source, slotNo);
                if (!srcObj) { skipped += targets.Count; continue; }
                ObjectData data = Capture(source, slotNo, srcObj);
                if (data == null) { skipped += targets.Count; continue; }
                foreach (Maid dest in targets) {
                    if (!dest || dest == source) continue;
                    GameObject dstObj = GetSlotObject(dest, slotNo);
                    if (!dstObj) { skipped++; continue; }
                    if (skipNameMismatch && data.rootData != null && data.rootData.name != dstObj.name) { skipped++; continue; }
                    bool savedYure = data.bYure;
                    if (!copyYure) data.bYure = YureUtil.GetYureState(dest, slotNo);
                    Apply(dest, slotNo, dstObj, data);
                    data.bYure = savedYure;
                    applied++;
                }
            }
            return "Copied " + applied + " category slots. Skipped " + skipped + ".";
        }

        static void WriteObjectXml(string fileName, ObjectData data) {
            XmlSerializer serializer = new XmlSerializer(typeof(ObjectData));
            using (StreamWriter sw = new StreamWriter(PresetManager.directoryPath + fileName + ".xml", false, new UTF8Encoding(false))) {
                serializer.Serialize(sw, data);
            }
        }

        static void WriteSet(PartsEditSet set) {
            XmlSerializer serializer = new XmlSerializer(typeof(PartsEditSet));
            using (StreamWriter sw = new StreamWriter(setDirectory + set.name + ".set.xml", false, new UTF8Encoding(false))) {
                serializer.Serialize(sw, set);
            }
        }

        static PartsEditSet ReadSet(string path) {
            XmlSerializer serializer = new XmlSerializer(typeof(PartsEditSet));
            using (StreamReader sr = new StreamReader(path, new UTF8Encoding(false))) {
                return (PartsEditSet)serializer.Deserialize(sr);
            }
        }

        public static ObjectData Capture(Maid maid, int slotNo, GameObject obj) {
            ObjectData objectData = new ObjectData();
            objectData.version = "0.1.6";
            objectData.bMaidParts = true;
            objectData.rootData = new ObjectData.TransformData();
            objectData.rootData.name = obj.name;
            objectData.rootData.scale = obj.transform.localScale;
            if (slotNo == (int)EXSlot.Base) {
                objectData.bYure = false;
                objectData.slotName = "base";
                objectData.rootData.position = obj.transform.localPosition;
                objectData.rootData.rotation = obj.transform.localRotation;
            } else {
                objectData.bYure = YureUtil.GetYureState(maid, slotNo);
                objectData.slotName = SlotUtil.GetSlotName(slotNo);
                objectData.rootData.position = obj.transform.localPosition;
                objectData.rootData.rotation = obj.transform.localRotation;
            }

            HashSet<Transform> seen = new HashSet<Transform>();
            Transform[] bones;
            if (slotNo == (int)EXSlot.Base) {
                SkinnedMeshRenderer smr = maid.body0.goSlot[0].obj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr == null) return objectData;
                bones = smr.bones.Where(bone => bone != null)
                    .Select(bone => CMT.SearchObjName(obj.transform, bone.name, true))
                    .Where(bone => bone != null).ToArray();
            } else {
                SkinnedMeshRenderer smr = obj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr == null) return objectData;
                bones = smr.bones.Where(bone => bone != null).ToArray();
            }
            foreach (Transform bone in bones) {
                Transform trs = bone;
                while (trs != obj.transform) {
                    if (!trs) return objectData;
                    if (!seen.Add(trs)) break;
                    objectData.transformDataList.Add(new ObjectData.TransformData {
                        name = trs.name,
                        scale = trs.localScale,
                        position = trs.localPosition,
                        rotation = trs.localRotation
                    });
                    trs = trs.parent;
                }
            }
            return objectData;
        }

        public static void Apply(Maid maid, int slotNo, GameObject obj, ObjectData objectData) {
            ShoeProxy.EnsureFor(obj, objectData.transformDataList.Select(t => t.name));
            BackUpObjectData backObj = BackUpData.GetOrAddMaidObjectData(maid, slotNo, obj);
            BackUpBoneData backRoot = BackUpData.GetOrAddMaidBoneData(maid, slotNo, obj, obj.transform);
            if (slotNo != (int)EXSlot.Base && slotNo != (int)EXSlot.None) {
                bool current = YureUtil.GetYureState(maid, slotNo);
                if (current != objectData.bYure) {
                    if (!backObj.changedYure) {
                        backObj.changedYure = true;
                        backObj.bYure = current;
                    }
                    YureUtil.SetYureState(maid, slotNo, objectData.bYure);
                }
            }
            if (objectData.bMaidParts && objectData.rootData != null) {
                if (!backRoot.changedPos) { backRoot.position = obj.transform.localPosition; backRoot.changedPos = true; }
                obj.transform.localPosition = objectData.rootData.position;
                if (!backRoot.changedRot) { backRoot.rotation = obj.transform.localRotation; backRoot.changedRot = true; }
                obj.transform.localRotation = objectData.rootData.rotation;
            }
            if (objectData.rootData != null) {
                if (!backRoot.changedScl) { backRoot.scale = obj.transform.localScale; backRoot.changedScl = true; }
                obj.transform.localScale = objectData.rootData.scale;
            }
            bool deep = slotNo == (int)EXSlot.Base;
            foreach (ObjectData.TransformData trsData in objectData.transformDataList) {
                Transform bone = CMT.SearchObjName(obj.transform, trsData.name, deep);
                if (!bone) continue;
                BackUpBoneData boneData = BackUpData.GetOrAddMaidBoneData(maid, slotNo, obj, bone);
                if (!boneData.changedPos) { boneData.position = bone.localPosition; boneData.changedPos = true; }
                bone.localPosition = trsData.position;
                if (!boneData.changedRot) { boneData.rotation = bone.localRotation; boneData.changedRot = true; }
                bone.localRotation = trsData.rotation;
                if (!boneData.changedScl) { boneData.scale = bone.localScale; boneData.changedScl = true; }
                bone.localScale = trsData.scale;
            }
        }

        static string autoloadPath {
            get { return setDirectory + "autoload.xml"; }
        }
        static Dictionary<string, string> autoload = null;

        public static string MaidGuid(Maid maid) {
            if (!maid) return "";
#if COM3D2
            return maid.status.guid;
#else
            return maid.Param.status.guid;
#endif
        }

        public static string GetAutoloadSet(Maid maid) {
            LoadAutoload();
            string guid = MaidGuid(maid);
            if (string.IsNullOrEmpty(guid) || !autoload.ContainsKey(guid)) return "";
            return autoload[guid];
        }

        public static string BindAutoload(Maid maid, string setName) {
            string guid = MaidGuid(maid);
            if (string.IsNullOrEmpty(guid)) return "This maid has no guid.";
            if (string.IsNullOrEmpty(setName)) return "Select a set first.";
            LoadAutoload();
            autoload[guid] = Sanitize(setName);
            SaveAutoload();
            return "Autoload '" + autoload[guid] + "' for this maid.";
        }

        public static string ClearAutoload(Maid maid) {
            string guid = MaidGuid(maid);
            if (string.IsNullOrEmpty(guid)) return "This maid has no guid.";
            LoadAutoload();
            autoload.Remove(guid);
            SaveAutoload();
            return "Autoload cleared for this maid.";
        }

        public static void TickAutoload() {
            if (GameMain.Instance == null || GameMain.Instance.CharacterMgr == null) return;
            float now = Time.realtimeSinceStartup;
            List<Maid> active = new List<Maid>();
            for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++) {
                Maid maid = GameMain.Instance.CharacterMgr.GetMaid(i);
                if (maid && maid.gameObject.activeInHierarchy) active.Add(maid);
            }
            HashSet<string> stillHere = new HashSet<string>();
            foreach (Maid maid in active) {
                string guid = MaidGuid(maid);
                if (string.IsNullOrEmpty(guid)) continue;
                stillHere.Add(guid);
                if (appliedAutoload.Contains(guid)) continue;
                if (!MaidBodyReady(maid)) continue;
                AutoloadWatch watch;
                if (!watches.TryGetValue(guid, out watch)) {
                    watch = new AutoloadWatch();
                    watch.signature = ClothingSignature(maid);
                    watch.nextCheck = now + 0.25f;
                    watches[guid] = watch;
                    continue;
                }
                if (now < watch.nextCheck) continue;
                string signature = ClothingSignature(maid);
                if (signature != watch.signature) {
                    watch.signature = signature;
                    watch.nextCheck = now + 0.25f;
                    continue;
                }
                appliedAutoload.Add(guid);
                watches.Remove(guid);
                if (Db.LibraryService.HasAutoLoad(maid)) {
                    // autoload z biblioteki (PartsEdit.db.json)
                    foreach (Db.ApplyResult r in Db.LibraryService.ApplyMaidProfile(maid)) Log.Info("Autoload " + r);
                } else {
                    // stara sciezka: zestaw z autoload.xml (dla pokojowek, ktorych baza nie obejmuje)
                    string setName = GetAutoloadSet(maid);
                    if (!string.IsNullOrEmpty(setName)) LoadSet(maid, setName, false, true);
                }
            }
            appliedAutoload.RemoveWhere(guid => !stillHere.Contains(guid));
            List<string> gone = new List<string>();
            foreach (string guid in watches.Keys) if (!stillHere.Contains(guid)) gone.Add(guid);
            foreach (string guid in gone) watches.Remove(guid);
        }

        static bool MaidBodyReady(Maid maid) {
            return maid && maid.body0 != null && maid.body0.goSlot != null && !maid.IsBusy;
        }

        static string ClothingSignature(Maid maid) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < maid.body0.goSlot.Count; i++) {
                TBodySkin slot = maid.body0.goSlot[i];
                sb.Append(i).Append(':');
                sb.Append(slot != null && slot.obj ? slot.obj.name : "-");
                sb.Append(';');
            }
            return sb.ToString();
        }

        class AutoloadWatch {
            public string signature = "";
            public float nextCheck;
        }
        static Dictionary<string, AutoloadWatch> watches = new Dictionary<string, AutoloadWatch>();

        static HashSet<string> appliedAutoload = new HashSet<string>();

        static void LoadAutoload() {
            if (autoload != null) return;
            autoload = new Dictionary<string, string>();
            if (!File.Exists(autoloadPath)) return;
            XmlSerializer serializer = new XmlSerializer(typeof(AutoloadFile));
            using (StreamReader sr = new StreamReader(autoloadPath, new UTF8Encoding(false))) {
                AutoloadFile file = (AutoloadFile)serializer.Deserialize(sr);
                if (file == null || file.entries == null) return;
                foreach (AutoloadEntry entry in file.entries) {
                    if (!string.IsNullOrEmpty(entry.guid)) autoload[entry.guid] = entry.setName;
                }
            }
        }

        static void SaveAutoload() {
            Directory.CreateDirectory(setDirectory);
            AutoloadFile file = new AutoloadFile();
            foreach (KeyValuePair<string, string> pair in autoload) {
                file.entries.Add(new AutoloadEntry { guid = pair.Key, setName = pair.Value });
            }
            XmlSerializer serializer = new XmlSerializer(typeof(AutoloadFile));
            using (StreamWriter sw = new StreamWriter(autoloadPath, false, new UTF8Encoding(false))) {
                serializer.Serialize(sw, file);
            }
        }
    }

    [XmlRoot("PartsEditAutoload")]
    public class AutoloadFile {
        public List<AutoloadEntry> entries = new List<AutoloadEntry>();
    }

    public class AutoloadEntry {
        public string guid = "";
        public string setName = "";
    }
}
