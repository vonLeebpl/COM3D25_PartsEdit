using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BackUpBoneData {
        public bool changedPos;
        public bool changedRot;
        public bool changedScl;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;

        public void Restore(Transform bone) {
            if (!bone) return;
            if (changedPos) bone.localPosition = position;
            if (changedRot) bone.localRotation = rotation;
            if (changedScl) bone.localScale = scale;
            changedPos = false;
            changedRot = false;
            changedScl = false;
        }

        public bool HasChange() {
            return changedPos || changedRot || changedScl;
        }
    }
}
