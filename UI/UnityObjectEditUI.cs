using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class UnityObjectEditUI {
        PhotoStudioObjectSelectUI objectSelectUI = new PhotoStudioObjectSelectUI();

        public void Draw() {
            DrawObject();
        }

        void DrawObject() {
            GUILayout.BeginHorizontal();
            string objectName = Text.NoneSelected;
            if (CommonUIData.obj) objectName = CommonUIData.obj.name;
            GUILayout.Label(Text.ObjectColon + " " + objectName, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            objectSelectUI.DrawCombo();
            if (!CommonUIData.obj || !BackUpData.GetObjectDataExist(CommonUIData.obj)) GUI.enabled = false;
            if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                BackUpData.RestoreObject(CommonUIData.obj);
                BackUpData.Refresh();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            UIUtil.BeginIndentArea();
            DrawImportExport();
            UIUtil.EndoIndentArea();
            UIUtil.BeginIndentArea();
            DrawBone();
            UIUtil.EndoIndentArea();
        }

        void DrawImportExport() {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Text.Preset, UIParams.Instance.lStyle);
            if (!CommonUIData.obj) GUI.enabled = false;
            if (GUILayout.Button("Import", UIParams.Instance.bStyle)) Setting.mode = Mode.Import;
            if (GUILayout.Button("Export", UIParams.Instance.bStyle)) Setting.mode = Mode.Export;
            GUI.enabled = CommonUIData.obj;
            if (GUILayout.Button(Text.IK, UIParams.Instance.bStyle)) Setting.mode = Mode.IK;
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        void DrawBone() {
            GUILayout.BeginHorizontal();
            string boneName = Text.NoneSelected;
            if (CommonUIData.bone) boneName = CommonUIData.bone.name;
            GUILayout.Label(Text.Bone + " " + boneName, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            if (!CommonUIData.bone || !BackUpData.GetBoneDataExist(CommonUIData.obj, CommonUIData.bone)) GUI.enabled = false;
            if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                BackUpData.RestoreBone(CommonUIData.obj, CommonUIData.bone);
                BackUpData.Refresh();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            UIUtil.BeginIndentArea();
            DrawTransform();
            UIUtil.EndoIndentArea();
        }

        void DrawTransform() {
            BackUpBoneData boneData = CommonUIData.bone ? BackUpData.GetOrNullBoneData(CommonUIData.obj, CommonUIData.bone) : null;
            DrawAxis(Text.Position, boneData == null || !boneData.changedPos, delegate {
                BackUpData.RestorePosition(CommonUIData.obj, CommonUIData.bone);
            });
            DrawAxis(Text.Rotation, boneData == null || !boneData.changedRot, delegate {
                BackUpData.RestoreRotation(CommonUIData.obj, CommonUIData.bone);
            });
            DrawAxis(Text.Scale, boneData == null || !boneData.changedScl, delegate {
                BackUpData.RestoreScale(CommonUIData.obj, CommonUIData.bone);
            });
        }

        void DrawAxis(string label, bool disable, System.Action restore) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, UIParams.Instance.lStyle);
            GUILayout.FlexibleSpace();
            if (disable) GUI.enabled = false;
            if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                restore();
                BackUpData.Refresh();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}
