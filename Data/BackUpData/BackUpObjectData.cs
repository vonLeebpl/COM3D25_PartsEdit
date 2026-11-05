using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BackUpObjectData {
        public bool changedYure;
        public bool bYure = true;
        public Dictionary<Transform, BackUpBoneData> boneDataDic = new Dictionary<Transform, BackUpBoneData>();

        public void Restore(Maid maid, int slotNo, GameObject obj) {
            if (changedYure && maid && slotNo >= 0) {
                YureUtil.SetYureState(maid, slotNo, bYure);
                changedYure = false;
            }
            if (!obj) return;
            List<Transform> bones = new List<Transform>(boneDataDic.Keys);
            foreach (Transform bone in bones) {
                if (!bone) continue;
                boneDataDic[bone].Restore(bone);
            }
        }
    }
}
