using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    public class ObjectData {
        public string version = "0.1";
        public string slotName = "";
        public bool bMaidParts = false;
        public TransformData rootData = null;
        public bool bYure = true;
        public List<TransformData> transformDataList = new List<TransformData>();

        //public ObjectData() {

        //}

        //public ObjectData(GameObject obj) {

        //}

        //public ObjectData(Maid maid, int SlotNo) {

        //}

        public class TransformData {
            //public string path;
            public string name;
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
        }
    }
}
