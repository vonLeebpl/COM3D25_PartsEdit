using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

    internal static class YureUtil {
        static FieldInfo fi_hair1list = Helper.GetFieldInfo(typeof(TBoneHair_), "hair1list");
        // 揺れボーンがあるかを取得
        public static bool GetYureAble(Maid maid, int slotNo) {
            if (!maid) return false;
            if (slotNo == -2 || slotNo == -1) return false;
            if (maid.body0.goSlot[slotNo].obj == null) return false;

#if COM3D2
            DynamicBone db = maid.body0.goSlot[slotNo].obj.GetComponent<DynamicBone>();
            DynamicSkirtBone dsb = maid.body0.goSlot[slotNo].obj.GetComponent<DynamicSkirtBone>();
            if (db || dsb) return true;
#endif

            TBoneHair_ tbh = maid.body0.goSlot[slotNo].bonehair;
            if (tbh == null) return false;
            List<THair1> hairList = (List<THair1>)fi_hair1list.GetValue(tbh);
            if (hairList == null || (hairList.Count == 0 && !tbh.boSkirt)) return false;
            return true;
        }

        static FieldInfo fi_m_bEnable = Helper.GetFieldInfo(typeof(TBoneHair_), "m_bEnable");
#if COM3D2
        static FieldInfo fi_m_SkirtBone = Helper.GetFieldInfo(typeof(BoneHair3), "m_SkirtBone");
#endif
        // 揺れ状態取得
        public static bool GetYureState(Maid maid, int slotNo) {
            if (!maid) return false;
            if (slotNo == -2 || slotNo == -1) return false;
            if (maid.body0.goSlot[slotNo].obj == null) return false;

            TBoneHair_ tbh = maid.body0.goSlot[slotNo].bonehair;
            bool yure = (bool)fi_m_bEnable.GetValue(tbh);

#if COM3D2
            TBodySkin tbs = maid.body0.goSlot[slotNo];
            DynamicBone db = maid.body0.goSlot[slotNo].obj.GetComponent<DynamicBone>();
            DynamicSkirtBone dsb = maid.body0.goSlot[slotNo].obj.GetComponent<DynamicSkirtBone>();
            BoneHair3 bh3 = tbs.bonehair3;
            yure =
                yure ||
                (db && db.enabled) ||
                (bh3 != null && fi_m_SkirtBone.GetValue(bh3) != null);
#endif

            return yure;
        }

        // 揺れ状態セット
        public static void SetYureState(Maid maid, int slotNo, bool state) {
            if (!maid) return;
            if (slotNo == -2 || slotNo == -1) return;
            if (maid.body0.goSlot[slotNo].obj == null) return;
            TBoneHair_ tbh = maid.body0.goSlot[slotNo].bonehair;
            fi_m_bEnable.SetValue(tbh, state);

#if COM3D2
            TBodySkin tbs = maid.body0.goSlot[slotNo];
            DynamicBone db = maid.body0.goSlot[slotNo].obj.GetComponent<DynamicBone>();
            DynamicSkirtBone dsb = maid.body0.goSlot[slotNo].obj.GetComponent<DynamicSkirtBone>();
            BoneHair3 bh3 = tbs.bonehair3;

            if (db) {
                Dictionary<Transform, Vector3> posDic = new Dictionary<Transform, Vector3>();
                Dictionary<Transform, Quaternion> rotDic = new Dictionary<Transform, Quaternion>();
                Dictionary<Transform, Vector3> sclDic = new Dictionary<Transform, Vector3>();
                foreach (DynamicBone.Particle p in db.m_Particles) {
                    posDic[p.m_Transform] = p.m_Transform.localPosition;
                    rotDic[p.m_Transform] = p.m_Transform.localRotation;
                    sclDic[p.m_Transform] = p.m_Transform.localScale;
                }
                db.enabled = state;
                foreach (DynamicBone.Particle p in db.m_Particles) {
                    p.m_Transform.localPosition = posDic[p.m_Transform];
                    p.m_Transform.localRotation = rotDic[p.m_Transform];
                    p.m_Transform.localScale = sclDic[p.m_Transform];
                }
            }
            if (dsb) {
                dsb.enabled = state;
                if (state) {
                    fi_m_SkirtBone.SetValue(bh3, dsb);
                } else {
                    fi_m_SkirtBone.SetValue(bh3, null);
                }
            }
#endif
        }
    }
