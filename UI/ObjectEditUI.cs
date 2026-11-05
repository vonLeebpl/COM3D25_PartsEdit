using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class ObjectEditUI {
        MaidObjectUI maidObjectUI = new MaidObjectUI();
        SkinnedMeshObjectEditUI smoEditUI = new SkinnedMeshObjectEditUI();
        UnityObjectEditUI unityObjectEditUI = new UnityObjectEditUI();

        public void Draw() {
            GUILayout.Label(Text.Edit, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            switch (Setting.targetSelectMode) {
                case 0:
                    maidObjectUI.Draw();
                    break;
                case 1:
                    smoEditUI.Draw();
                    break;
                case 2:
                    unityObjectEditUI.Draw();
                    break;
            }
            UIUtil.EndoIndentArea();
            GUILayout.Space(6);
            GUILayout.Label(Text.Experimental, UIParams.Instance.lStyle);
            UIUtil.BeginIndentArea();
            Setting.experimental = GUILayout.Toggle(Setting.experimental, Text.Experimental, UIParams.Instance.tStyle);
            if (Setting.experimental) {
                if (GUILayout.Button(Text.Physics, UIParams.Instance.bStyle)) Setting.mode = Mode.Experimental;
                if (GUILayout.Button(Text.IKGravity, UIParams.Instance.bStyle)) Setting.mode = Mode.IKGravity;
            }
            UIUtil.EndoIndentArea();
        }
    }
}
