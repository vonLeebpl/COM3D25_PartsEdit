using System.Collections.Generic;

namespace CM3D2.PartsEdit.Plugin {
    class BackUpMaidData {
        public Dictionary<int, BackUpSlotData> slotDataDic = new Dictionary<int, BackUpSlotData>();

        public void Restore(Maid maid) {
            foreach (KeyValuePair<int, BackUpSlotData> pair in slotDataDic) {
                pair.Value.Restore(maid, pair.Key);
            }
        }
    }
}
