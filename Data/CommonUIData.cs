using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    static class CommonUIData {
        // メイド選択モード
        public static Maid maid = null;
        public static int slotNo = (int)EXSlot.None;
        //public static TBodySkin tbs = null;

        // メイド選択モード￥複数メイドモード共通
        public static GameObject obj = null;
        public static Transform bone = null;

        public static bool yureExist = false;
        public static bool yureEnable = false;

        public static void SetMaid(Maid fMaid) {
            if(maid != fMaid || fMaid == null) {
                maid = fMaid;
                SetSlot((int)EXSlot.None);
            }
        }

        public static void SetSlot(int fSlotNo) {
            if(slotNo != fSlotNo || fSlotNo == (int)EXSlot.None) {
                slotNo = fSlotNo;
                switch (slotNo) {
                    case (int)EXSlot.None:
                        SetObject(null);
                        break;
                    case (int)EXSlot.Base:
                        SetObject(maid.body0.m_Bones.gameObject);
                        break;
                    default:
                        SetObject(maid.body0.goSlot[slotNo].obj);
                        break;
                }
            }
        }

        public static void SetObject(GameObject fObj) {
            if (obj != fObj || fObj == null) {
                obj = fObj;
                SetBone(null);
            }
        }

        public static void SetBone(Transform fBone) {
            if (bone != fBone) {
                bone = fBone;
            }
        }
    }

    public enum EXSlot {
        None = -2,
        Base = -1
    }
}
