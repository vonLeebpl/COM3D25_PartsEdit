using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Stand-in for the missing original undo store. First edit is remembered, R restores it.
    static class BackUpData {
        public static Dictionary<Maid, BackUpMaidData> maidDataDic = new Dictionary<Maid, BackUpMaidData>();
        public static Dictionary<GameObject, BackUpObjectData> objectDataDic = new Dictionary<GameObject, BackUpObjectData>();

        public static bool GetMaidDataExist(Maid maid) {
            return maid && maidDataDic.ContainsKey(maid);
        }

        public static bool GetMaidSlotDataExist(Maid maid, int slotNo) {
            BackUpSlotData slot = GetOrNullMaidSlotData(maid, slotNo);
            return slot != null && slot.objectDataDic.Count > 0;
        }

        public static bool GetMaidBoneDataExist(Maid maid, int slotNo, GameObject obj, Transform bone) {
            return GetOrNullMaidBoneData(maid, slotNo, obj, bone) != null;
        }

        public static bool GetObjectDataExist(GameObject obj) {
            return obj && objectDataDic.ContainsKey(obj);
        }

        public static bool GetBoneDataExist(GameObject obj, Transform bone) {
            return GetOrNullBoneData(obj, bone) != null;
        }

        public static BackUpMaidData GetOrNullMaidData(Maid maid) {
            if (!maid || !maidDataDic.ContainsKey(maid)) return null;
            return maidDataDic[maid];
        }

        public static BackUpSlotData GetOrNullMaidSlotData(Maid maid, int slotNo) {
            BackUpMaidData maidData = GetOrNullMaidData(maid);
            if (maidData == null || !maidData.slotDataDic.ContainsKey(slotNo)) return null;
            return maidData.slotDataDic[slotNo];
        }

        public static BackUpObjectData GetOrNullMaidObjectData(Maid maid, int slotNo, GameObject obj) {
            BackUpSlotData slot = GetOrNullMaidSlotData(maid, slotNo);
            if (slot == null || !obj || !slot.objectDataDic.ContainsKey(obj)) return null;
            return slot.objectDataDic[obj];
        }

        public static BackUpBoneData GetOrNullMaidBoneData(Maid maid, int slotNo, GameObject obj, Transform bone) {
            BackUpObjectData data = GetOrNullMaidObjectData(maid, slotNo, obj);
            if (data == null || !bone || !data.boneDataDic.ContainsKey(bone)) return null;
            return data.boneDataDic[bone];
        }

        public static BackUpBoneData GetOrNullBoneData(GameObject obj, Transform bone) {
            if (!obj || !bone || !objectDataDic.ContainsKey(obj)) return null;
            BackUpObjectData data = objectDataDic[obj];
            if (!data.boneDataDic.ContainsKey(bone)) return null;
            return data.boneDataDic[bone];
        }

        public static BackUpObjectData GetOrAddMaidObjectData(Maid maid, int slotNo, GameObject obj) {
            if (!maidDataDic.ContainsKey(maid)) maidDataDic[maid] = new BackUpMaidData();
            BackUpMaidData maidData = maidDataDic[maid];
            if (!maidData.slotDataDic.ContainsKey(slotNo)) maidData.slotDataDic[slotNo] = new BackUpSlotData();
            BackUpSlotData slot = maidData.slotDataDic[slotNo];
            if (!obj) obj = new GameObject("PartsEditMissingObject");
            if (!slot.objectDataDic.ContainsKey(obj)) slot.objectDataDic[obj] = new BackUpObjectData();
            return slot.objectDataDic[obj];
        }

        public static BackUpBoneData GetOrAddMaidBoneData(Maid maid, int slotNo, GameObject obj, Transform bone) {
            BackUpObjectData data = GetOrAddMaidObjectData(maid, slotNo, obj);
            if (!bone) return new BackUpBoneData();
            if (!data.boneDataDic.ContainsKey(bone)) data.boneDataDic[bone] = new BackUpBoneData();
            return data.boneDataDic[bone];
        }

        public static BackUpObjectData GetOrAddObjectData(GameObject obj) {
            if (!objectDataDic.ContainsKey(obj)) objectDataDic[obj] = new BackUpObjectData();
            return objectDataDic[obj];
        }

        public static BackUpBoneData GetOrAddBoneData(GameObject obj, Transform bone) {
            BackUpObjectData data = GetOrAddObjectData(obj);
            if (!bone) return new BackUpBoneData();
            if (!data.boneDataDic.ContainsKey(bone)) data.boneDataDic[bone] = new BackUpBoneData();
            return data.boneDataDic[bone];
        }

        public static void Restore() {
            foreach (KeyValuePair<Maid, BackUpMaidData> pair in maidDataDic) pair.Value.Restore(pair.Key);
            foreach (KeyValuePair<GameObject, BackUpObjectData> pair in objectDataDic) pair.Value.Restore(null, -2, pair.Key);
            Refresh();
        }

        public static void RestoreMaid(Maid maid) {
            BackUpMaidData data = GetOrNullMaidData(maid);
            if (data != null) data.Restore(maid);
            Refresh();
        }

        public static void RestoreSlot(Maid maid, int slotNo) {
            BackUpSlotData data = GetOrNullMaidSlotData(maid, slotNo);
            if (data != null) data.Restore(maid, slotNo);
            Refresh();
        }

        public static void RestoreObject(GameObject obj) {
            if (obj && objectDataDic.ContainsKey(obj)) objectDataDic[obj].Restore(null, -2, obj);
            Refresh();
        }

        public static void RestoreBone(Maid maid, int slotNo, GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullMaidBoneData(maid, slotNo, obj, bone);
            if (data != null) data.Restore(bone);
            Refresh();
        }

        public static void RestoreBone(GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullBoneData(obj, bone);
            if (data != null) data.Restore(bone);
            Refresh();
        }

        public static void RestorePosition(Maid maid, int slotNo, GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullMaidBoneData(maid, slotNo, obj, bone);
            if (data != null && data.changedPos && bone) {
                bone.localPosition = data.position;
                data.changedPos = false;
            }
            Refresh();
        }

        public static void RestorePosition(GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullBoneData(obj, bone);
            if (data != null && data.changedPos && bone) {
                bone.localPosition = data.position;
                data.changedPos = false;
            }
            Refresh();
        }

        public static void RestoreRotation(Maid maid, int slotNo, GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullMaidBoneData(maid, slotNo, obj, bone);
            if (data != null && data.changedRot && bone) {
                bone.localRotation = data.rotation;
                data.changedRot = false;
            }
            Refresh();
        }

        public static void RestoreRotation(GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullBoneData(obj, bone);
            if (data != null && data.changedRot && bone) {
                bone.localRotation = data.rotation;
                data.changedRot = false;
            }
            Refresh();
        }

        public static void RestoreScale(Maid maid, int slotNo, GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullMaidBoneData(maid, slotNo, obj, bone);
            if (data != null && data.changedScl && bone) {
                bone.localScale = data.scale;
                data.changedScl = false;
            }
            Refresh();
        }

        public static void RestoreScale(GameObject obj, Transform bone) {
            BackUpBoneData data = GetOrNullBoneData(obj, bone);
            if (data != null && data.changedScl && bone) {
                bone.localScale = data.scale;
                data.changedScl = false;
            }
            Refresh();
        }

        public static void Refresh() {
            List<Maid> emptyMaids = new List<Maid>();
            foreach (KeyValuePair<Maid, BackUpMaidData> maidPair in maidDataDic) {
                List<int> emptySlots = new List<int>();
                foreach (KeyValuePair<int, BackUpSlotData> slotPair in maidPair.Value.slotDataDic) {
                    List<GameObject> emptyObjects = new List<GameObject>();
                    foreach (KeyValuePair<GameObject, BackUpObjectData> objPair in slotPair.Value.objectDataDic) {
                        List<Transform> emptyBones = new List<Transform>();
                        foreach (KeyValuePair<Transform, BackUpBoneData> bonePair in objPair.Value.boneDataDic) {
                            if (!bonePair.Value.HasChange()) emptyBones.Add(bonePair.Key);
                        }
                        foreach (Transform bone in emptyBones) objPair.Value.boneDataDic.Remove(bone);
                        if (objPair.Value.boneDataDic.Count == 0 && !objPair.Value.changedYure) emptyObjects.Add(objPair.Key);
                    }
                    foreach (GameObject obj in emptyObjects) slotPair.Value.objectDataDic.Remove(obj);
                    if (slotPair.Value.objectDataDic.Count == 0) emptySlots.Add(slotPair.Key);
                }
                foreach (int slotNo in emptySlots) maidPair.Value.slotDataDic.Remove(slotNo);
                if (maidPair.Value.slotDataDic.Count == 0) emptyMaids.Add(maidPair.Key);
            }
            foreach (Maid maid in emptyMaids) maidDataDic.Remove(maid);

            List<GameObject> emptyFree = new List<GameObject>();
            foreach (KeyValuePair<GameObject, BackUpObjectData> pair in objectDataDic) {
                if (pair.Value.boneDataDic.Count == 0 && !pair.Value.changedYure) emptyFree.Add(pair.Key);
            }
            foreach (GameObject obj in emptyFree) objectDataDic.Remove(obj);
        }
    }
}
