using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BoneGizmoRenderer : ExGizmoRenderer{
        Transform targetShaftTrs = null;
        protected override Transform posTargetTrs {
            get {
                if (targetShaftTrs) {
                    return targetShaftTrs;
                }
                return targetTrs;
            }
        }
        protected override Transform rotTargetTrs {
            get {
                if (targetShaftTrs) {
                    return targetShaftTrs;
                }
                return targetTrs;
            }
        }
        protected override Transform sclTargetTrs { get { return targetTrs; } }

        public override void SetTarget(Transform t) {
            targetTrs = t;

            if (t && t.name.EndsWith("_SCL_") && t.parent.name == t.name.Substring(0, t.name.Length - 5)) {
                targetShaftTrs = t.parent;
            }else {
                targetShaftTrs = null;
            }
        }

        static public new BoneGizmoRenderer AddGizmo(Transform parent_tr, string gizmo_name) {
            GameObject go = new GameObject();
            _gameObjects_.Add(go);
            go.transform.SetParent(parent_tr, false);
            go.name = gizmo_name;

            BoneGizmoRenderer mg = go.AddComponent<BoneGizmoRenderer>();
            mg.name = gizmo_name + "_GR";
            return mg;
        }

    }
}
