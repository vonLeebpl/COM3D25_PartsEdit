using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class SettingUI {
        GUIContent[] boneDsiplayContents = new GUIContent[] {
            new GUIContent(Tr.T(Text.Hidden)),
            new GUIContent(Tr.T(Text.Visible))
        };

        GUIContent[] coordinateTypeContents = new GUIContent[] {
            new GUIContent("Local"),
            new GUIContent("Global"),
            new GUIContent("View")
        };

        GUIContent[] gizmoTypeContents = new GUIContent[] {
            new GUIContent(Tr.T(Text.None)),
            new GUIContent(Tr.T(Text.Position)),
            new GUIContent(Tr.T(Text.Rotation)),
            new GUIContent(Tr.T(Text.IK))
        };

        GUIContent[] gizmoTypeLocalContents = new GUIContent[] {
            new GUIContent(Tr.T(Text.None)),
            new GUIContent(Tr.T(Text.Position)),
            new GUIContent(Tr.T(Text.Rotation)),
            new GUIContent(Tr.T(Text.Scale)),
            new GUIContent(Tr.T(Text.IK))
        };

        public void Draw() {
            GUILayout.Label(Text.Settings, UIParams.Instance.lStyle);

            UIUtil.BeginIndentArea();
            {
                DrawBoneDisplay();
                //DrawGizmoEdit();
                DrawCoordinateType();
                DrawGizmoType();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawBoneDisplay() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.BoneDisplay, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                Setting.boneDisplay = (BoneDisplay)GUILayout.Toolbar((int)Setting.boneDisplay, boneDsiplayContents, UIParams.Instance.tbStyle);
                if (GUILayout.Button(Text.Details)) {
                    Setting.mode = Mode.BoneDisplaySetting;
                }
            }
            GUILayout.EndHorizontal();
        }

        //void DrawGizmoEdit() {
        //    GUILayout.BeginHorizontal();
        //    {
        //        GUILayout.Label(Text.GizmoDisplay, UIParams.Instance.lStyle);
        //        GUILayout.FlexibleSpace();
        //        Setting.gizmoEdit = GUILayout.Toggle(Setting.gizmoEdit, "", UIParams.Instance.tStyle);
        //    }
        //    GUILayout.EndHorizontal();
        //}

        void DrawCoordinateType() {
            GUILayout.BeginHorizontal();
            {
                if (Setting.gizmoType == GizmoType.Scale) {
                    GUI.enabled = false;
                }
                GUILayout.Label(Text.CoordinateType, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                Setting.coordinateType = (BoneGizmoRenderer.COORDINATE) GUILayout.Toolbar((int)Setting.coordinateType, coordinateTypeContents, UIParams.Instance.tbStyle);

                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }

        void DrawGizmoType() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.GizmoType, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                if (Setting.coordinateType == BoneGizmoRenderer.COORDINATE.Local) {
                    Setting.gizmoType = (GizmoType)GUILayout.Toolbar((int)Setting.gizmoType, gizmoTypeLocalContents, UIParams.Instance.tbStyle);
                } else {
                    Setting.gizmoType = (GizmoType)GUILayout.Toolbar((int)Setting.gizmoType, gizmoTypeContents, UIParams.Instance.tbStyle);
                }
                if (GUILayout.Button(Text.Details)) {
                    Setting.mode = Mode.GizmoSetting;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
