using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BackUpSlotData {
        public Dictionary<GameObject, BackUpObjectData> objectDataDic = new Dictionary<GameObject, BackUpObjectData>();

        public void Restore(Maid maid, int slotNo) {
            List<GameObject> objects = new List<GameObject>(objectDataDic.Keys);
            foreach (GameObject obj in objects) {
                if (!obj) continue;
                objectDataDic[obj].Restore(maid, slotNo, obj);
            }
        }
    }
}
