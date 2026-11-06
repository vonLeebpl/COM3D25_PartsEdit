using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    static class PresetManager {
        public static string directoryPath = "";

        public static SortedDictionary<string, PresetFileData> presetFileDataDic = null;
        public static SortedDictionary<string, PresetFileData> PresetFileDataDic {
            get {
                if (presetFileDataDic == null) {
                    LoadPresetFileList();
                }
                return presetFileDataDic;
            }
            set {
                presetFileDataDic = value;
            }
        }

        public static void SaveObjectData(string fileName) {
            DirectoryCheckAndCreate();
            ObjectData objectData = GetObjectDataFromObject();
            XmlSerializer serializer = new XmlSerializer(typeof(ObjectData));
            StreamWriter sw = new StreamWriter(directoryPath + fileName + ".xml", false, new UTF8Encoding(false));
            serializer.Serialize(sw, objectData);
            sw.Close();

            PresetFileData pData = new PresetFileData();
            pData.filename = fileName;
            pData.objectData = objectData;

            PresetFileDataDic[fileName] = pData;
        }

        public static void LoadObjectData(string fileName) {
            ObjectData objectData = PresetFileDataDic[fileName].objectData;
            ApplyObjectDataToObject(objectData);
        }

        public static string[] GetFileList() {
            if (!Directory.Exists(directoryPath)) {
                return new string[0];
            }
            return PresetFileDataDic.Keys.ToArray();
        }

        public static string[] GetFileList(string category, string name) {
            PresetFileData[] list = PresetFileDataDic.Values.ToArray();
            if(category != null) {
                list = list.Where(data => data.objectData.slotName == category).ToArray();
            }
            if(name != null) {
                list = list.Where(data => data.objectData.rootData.name == name).ToArray();
            }
            return list.Select(data => data.filename).ToArray();
        }

        static void LoadPresetFileList() {
            presetFileDataDic = new SortedDictionary<string, PresetFileData>();

            if (!Directory.Exists(directoryPath)) {
                return;
            }

            string[] filePathList = Directory.GetFiles(directoryPath, "*.xml", SearchOption.TopDirectoryOnly);
            foreach(string filePath in filePathList) {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                PresetFileData pData = new PresetFileData();
                pData.filename = fileName;

                XmlSerializer serializer = new XmlSerializer(typeof(ObjectData));
                StreamReader sr = new StreamReader(filePath, new UTF8Encoding(false));
                pData.objectData = (ObjectData)serializer.Deserialize(sr);
                sr.Close();

                presetFileDataDic[fileName] = pData;
            }
        }

        static void DirectoryCheckAndCreate() {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
        }

        static FieldInfo fi_m_bEnable = Helper.GetFieldInfo(typeof(TBoneHair_), "m_bEnable");
        static HashSet<Transform> trsHash = null;
        public static ObjectData GetObjectDataFromObject() {
            trsHash = new HashSet<Transform>();
            ObjectData objectData = new ObjectData();
            objectData.version = "0.1.6";

            objectData.bMaidParts = Setting.targetSelectMode == 0;

            ObjectData.TransformData rootData = new ObjectData.TransformData();
            rootData.name = CommonUIData.obj.name;
            if (Setting.targetSelectMode == 0) {
                rootData.position = CommonUIData.obj.transform.localPosition;
                rootData.rotation = CommonUIData.obj.transform.localRotation;
            }
            rootData.scale = CommonUIData.obj.transform.localScale;
            objectData.rootData = rootData;

            if (Setting.targetSelectMode == 0) {
                if (CommonUIData.slotNo == (int)EXSlot.Base) {
                    objectData.bYure = false;
                    objectData.slotName = "base";
                } else {
                    objectData.bYure = YureUtil.GetYureState(CommonUIData.maid, CommonUIData.slotNo);
                    objectData.slotName = SlotUtil.GetSlotName(CommonUIData.slotNo);
                }
            } else {
                objectData.bYure = false;
            }

            Transform[] bones;
            if (Setting.targetSelectMode == 0 && CommonUIData.slotNo == -1) {
                SkinnedMeshRenderer smr = CommonUIData.maid.body0.goSlot[0].obj.GetComponentInChildren<SkinnedMeshRenderer>();
                bones = 
                    smr.bones
                    .Where(bone => bone != null)
                    .Select(bone => CMT.SearchObjName(CommonUIData.obj.transform, bone.name, true))
                    .Where(bone => bone != null)
                    .ToArray();
            } else {
                SkinnedMeshRenderer smr = CommonUIData.obj.GetComponentInChildren<SkinnedMeshRenderer>();
                bones = smr.bones.Where(bone => bone != null).ToArray();
            }
            foreach (Transform bone in bones) {
                Transform trs = bone;
                while (trs != CommonUIData.obj.transform) {
                    if (!trs) {
                        Log.Info("bone is not under the root object: " + bone.name); // ja: ルートオブジェクト配下にありません
                        return null;
                    }
                    if (trsHash.Contains(trs)) break;
                    ObjectData.TransformData trsData = new ObjectData.TransformData();

                    trsData.name = trs.name;
                    trsData.scale = trs.localScale;
                    trsData.position = trs.localPosition;
                    trsData.rotation = trs.localRotation;
                    objectData.transformDataList.Add(trsData);
                    trsHash.Add(trs);
                    trs = trs.parent;
                }
            }

            return objectData;
        }

        static void ApplyObjectDataToObject(ObjectData objectData) {
            ShoeProxy.EnsureFor(CommonUIData.obj, objectData.transformDataList.Select(t => t.name));
            BackUpObjectData backObjData = null;
            BackUpBoneData backRootData = null;
            if(Setting.targetSelectMode == 0) {
                backObjData = BackUpData.GetOrAddMaidObjectData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj);
                backRootData = BackUpData.GetOrAddMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.obj.transform);
            } else {
                backObjData = BackUpData.GetOrAddObjectData(CommonUIData.obj);
                backRootData = BackUpData.GetOrAddBoneData(CommonUIData.obj, CommonUIData.obj.transform);
            }
            if (Setting.targetSelectMode == 0) {
                if (CommonUIData.slotNo != -1) {
                    if (backObjData.changedYure) {
                        if (YureUtil.GetYureState(CommonUIData.maid, CommonUIData.slotNo) != objectData.bYure) {
                            backObjData.changedYure = false;
                            backObjData.bYure = true;
                            YureUtil.SetYureState(CommonUIData.maid, CommonUIData.slotNo, objectData.bYure);
                        }
                    } else {
                        if (YureUtil.GetYureState(CommonUIData.maid, CommonUIData.slotNo) != objectData.bYure) {
                            backObjData.changedYure = true;
                            backObjData.bYure = YureUtil.GetYureState(CommonUIData.maid, CommonUIData.slotNo);
                            YureUtil.SetYureState(CommonUIData.maid, CommonUIData.slotNo, objectData.bYure);
                        }
                    }
                }

                if (objectData.bMaidParts) {
                    if (!backRootData.changedPos) {
                        backRootData.position = CommonUIData.obj.transform.localPosition;
                        backRootData.changedPos = true;
                    }
                    CommonUIData.obj.transform.localPosition = objectData.rootData.position;
                    if (!backRootData.changedRot) {
                        backRootData.rotation = CommonUIData.obj.transform.localRotation;
                        backRootData.changedRot = true;
                    }
                    CommonUIData.obj.transform.localRotation = objectData.rootData.rotation;
                }
            }
            if (!backRootData.changedScl) {
                backRootData.scale = CommonUIData.obj.transform.localScale;
                backRootData.changedScl = true;
            }
            CommonUIData.obj.transform.localScale = objectData.rootData.scale;

            bool hasProxyData = objectData.transformDataList.Any(t => t.name != null && t.name.EndsWith(ShoeProxy.Suffix));
            foreach (ObjectData.TransformData trsData in objectData.transformDataList) {
                if (ShoeProxy.SkipOnApply(trsData.name, hasProxyData)) continue;
                Transform bone;
                if (Setting.targetSelectMode == 0 && CommonUIData.slotNo == -1) {
                    bone = CMT.SearchObjName(CommonUIData.obj.transform, trsData.name, true);
                }else {
                    bone = CMT.SearchObjName(CommonUIData.obj.transform, trsData.name, false);
                }
                if (!bone) continue;
                BackUpBoneData boneData = null;
                if (Setting.targetSelectMode == 0) {
                    boneData = BackUpData.GetOrAddMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, bone);
                } else {
                    boneData = BackUpData.GetOrAddBoneData(CommonUIData.obj, bone);
                }
                if (!boneData.changedPos) {
                    boneData.position = bone.localPosition;
                    boneData.changedPos = true;
                }
                bone.localPosition = trsData.position;
                if (!boneData.changedRot) {
                    boneData.rotation = bone.localRotation;
                    boneData.changedRot = true;
                }
                bone.localRotation = trsData.rotation;
                if (!boneData.changedScl) {
                    boneData.scale = bone.localScale;
                    boneData.changedScl = true;
                }
                bone.localScale = trsData.scale;
            }
        }
    }
}
