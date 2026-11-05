using System;
using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin.Db {
    // Konwersja miedzy starym ObjectData (typy Unity) a rekordami bazy.
    public static class DbConvert {
        public static Vec3 ToVec3(Vector3 v) { return new Vec3(v.x, v.y, v.z); }
        public static Quat ToQuat(Quaternion q) { return new Quat(q.x, q.y, q.z, q.w); }
        public static Vector3 ToVector3(Vec3 v) { return new Vector3(v.x, v.y, v.z); }
        public static Quaternion ToQuaternion(Quat q) { return new Quaternion(q.x, q.y, q.z, q.w); }

        public static BoneRecord ToRecord(ObjectData.TransformData t) {
            BoneRecord r = new BoneRecord();
            if (t == null) return r;
            r.Name = t.name ?? "";
            r.Position = ToVec3(t.position);
            r.Rotation = ToQuat(t.rotation);
            r.Scale = ToVec3(t.scale);
            return r;
        }

        public static ObjectData.TransformData ToTransformData(BoneRecord r) {
            ObjectData.TransformData t = new ObjectData.TransformData();
            t.name = r.Name;
            t.position = ToVector3(r.Position);
            t.rotation = ToQuaternion(r.Rotation);
            t.scale = ToVector3(r.Scale);
            return t;
        }

        // Stary preset/czlonek zestawu -> wpis biblioteki. Domyslnie pasuje do dowolnego przedmiotu w slocie
        // (tak jak dotychczasowy Import), uzytkownik moze to potem zmienic na wpisie.
        public static ModEntry FromObjectData(ObjectData od, string name, string source, DateTime created) {
            ModEntry e = new ModEntry();
            e.Name = name ?? "";
            e.Source = source ?? "";
            e.Created = created;
            e.Slot = od.slotName ?? "";
            e.ObjectName = od.rootData != null && od.rootData.name != null ? od.rootData.name : "";
            e.MaidParts = od.bMaidParts;
            e.Yure = od.bYure;
            e.Match = MatchMode.AnyInSlot;
            e.Root = ToRecord(od.rootData);
            if (od.transformDataList != null) {
                foreach (ObjectData.TransformData t in od.transformDataList) e.Bones.Add(ToRecord(t));
            }
            return e;
        }

        // Wpis -> ObjectData, zeby w nastepnych etapach uzyc istniejacej logiki nakladania (PresetManager / SetManager).
        public static ObjectData ToObjectData(ModEntry e) {
            ObjectData od = new ObjectData();
            od.version = "0.1.6";
            od.slotName = e.Slot;
            od.bMaidParts = e.MaidParts;
            od.bYure = e.Yure;
            od.rootData = ToTransformData(e.Root);
            od.transformDataList = new List<ObjectData.TransformData>();
            foreach (BoneRecord b in e.Bones) od.transformDataList.Add(ToTransformData(b));
            return od;
        }
    }
}
