using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class TargetSelectModeUI {
        GUIContent[] targetSelectModeContents = new GUIContent[] {
            new GUIContent(Tr.T(Text.MaidParts)),
            new GUIContent(Tr.T(Text.MPS)),
            new GUIContent(Tr.T(Text.UnityObject)),
        };

        public void Draw() {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Text.ObjectSelectMode, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            Setting.targetSelectMode = GUILayout.Toolbar(Setting.targetSelectMode, targetSelectModeContents, UIParams.Instance.tStyle);
            GUILayout.EndHorizontal();
        }
    }
}
