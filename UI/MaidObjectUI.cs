using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class MaidObjectUI {
        MaidSelectUI maidSelectUI = new MaidSelectUI();
        MaidSlotSelectUI slotSelectUI = new MaidSlotSelectUI();

        public void Draw() {
            DrawMaid();
        }

        void DrawMaid() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.MaidColon/* + maidName*/, UIParams.Instance.lStyle);

                GUILayout.FlexibleSpace();

                maidSelectUI.DrawCombo();

                if (!CommonUIData.maid || !BackUpData.GetMaidDataExist(CommonUIData.maid)) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                    BackUpData.RestoreMaid(CommonUIData.maid);
                    BackUpData.Refresh();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            UIUtil.BeginIndentArea();
            {
                DrawSlot();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawSlot() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.SlotColon /*+ slotName*/, UIParams.Instance.lStyle);

                GUILayout.FlexibleSpace();
                slotSelectUI.DrawCombo();
                if (CommonUIData.slotNo == (int)EXSlot.None || !BackUpData.GetMaidSlotDataExist(CommonUIData.maid, CommonUIData.slotNo)) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                    BackUpData.RestoreSlot(CommonUIData.maid, CommonUIData.slotNo);
                    BackUpData.Refresh();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            UIUtil.BeginIndentArea();
            {
                DrawObject();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawObject() {
            DrawImportExport();
            DrawYure();
            DrawShoeBones();
            DrawBone();
        }

        void DrawShoeBones() {
            // Po wlaczeniu kosci w siatce to *_ShoeAdj, wiec CanApply jest false - wiersz musi zostac, zeby mozna go wylaczyc.
            if (!ShoeProxy.IsActive(CommonUIData.obj) && !ShoeProxy.CanApply(CommonUIData.obj)) return;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.ShoeBones, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                bool active = ShoeProxy.IsActive(CommonUIData.obj);
                bool next = GUILayout.Toggle(active, "", UIParams.Instance.tStyle);
                if (next != active) {
                    if (next) {
                        List<Transform> restored = new List<Transform>();
                        ShoeProxy.Enable(CommonUIData.obj, restored, true);
                        RegisterRestoredBackups(restored);
                    } else {
                        ShoeProxy.Disable(CommonUIData.obj);
                    }
                    BoneEdit.requestRebuild = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        // Przywrocone korekty maja miec kopie zapasowa (przyciski R / Revert), a ich "oryginalem" jest poza identycznosciowa nowego zastepnika.
        void RegisterRestoredBackups(List<Transform> restored) {
            foreach (Transform t in restored) {
                BackUpBoneData data = BackUpData.GetOrAddMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, t);
                if (!data.changedPos && t.localPosition != Vector3.zero) { data.position = Vector3.zero; data.changedPos = true; }
                if (!data.changedRot && t.localRotation != Quaternion.identity) { data.rotation = Quaternion.identity; data.changedRot = true; }
                if (!data.changedScl && t.localScale != Vector3.one) { data.scale = Vector3.one; data.changedScl = true; }
            }
            BackUpData.Refresh();
        }

        void DrawImportExport() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.Preset, UIParams.Instance.lStyle);
                // Import / Export / Set zastapilo okno Library (stare xml sa zaimportowane do bazy)
                if (GUILayout.Button(Text.Library, UIParams.Instance.bStyle)) {
                    Setting.mode = Mode.Library;
                }
                GUI.enabled = CommonUIData.maid;
                if (GUILayout.Button(Text.Copy, UIParams.Instance.bStyle)) {
                    Setting.mode = Mode.Copy;
                }
                GUI.enabled = CommonUIData.obj;
                if (GUILayout.Button(Text.IK, UIParams.Instance.bStyle)) {
                    Setting.mode = Mode.IK;
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }

        void DrawYure() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.YureBone, UIParams.Instance.lStyle);
                GUILayout.FlexibleSpace();
                if (!YureUtil.GetYureAble(CommonUIData.maid, CommonUIData.slotNo)) {
                    GUI.enabled = false;
                    GUILayout.Toggle(false, "", UIParams.Instance.tStyle);
                    GUI.enabled = true;
                } else {
                    bool bYure = YureUtil.GetYureState(CommonUIData.maid, CommonUIData.slotNo);
                    bool tempYure = GUILayout.Toggle(bYure, "", UIParams.Instance.tStyle);
                    if (tempYure != bYure) {
                        YureUtil.SetYureState(CommonUIData.maid, CommonUIData.slotNo, tempYure);

                        BackUpObjectData objectData = BackUpData.GetOrAddMaidObjectData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj);
                        if (objectData.changedYure && tempYure == objectData.bYure) {
                            // ボーンデータがなければオブジェクトデータ削除
                            if (objectData.boneDataDic.Count == 0) {
                                BackUpSlotData slotData = BackUpData.GetOrNullMaidSlotData(CommonUIData.maid, CommonUIData.slotNo);
                                slotData.objectDataDic.Remove(CommonUIData.obj);
                            } else {
                                objectData.changedYure = false;
                            }
                            BackUpData.Refresh();
                        } else {
                            objectData.changedYure = true;
                            objectData.bYure = bYure;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawBone() {
            GUILayout.BeginHorizontal();
            {
                string boneName = Text.NoneSelected;
                if (CommonUIData.bone) {
                    boneName = CommonUIData.bone.name;
                }
                GUILayout.Label(Text.Bone + boneName, UIParams.Instance.lStyle);

                GUILayout.FlexibleSpace();

                if (!CommonUIData.bone || !BackUpData.GetMaidBoneDataExist(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone)) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                    BackUpData.RestoreBone(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
                    BackUpData.Refresh();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            UIUtil.BeginIndentArea();
            {
                DrawTransform();
            }
            UIUtil.EndoIndentArea();
        }

        void DrawTransform() {
            BackUpBoneData boneData = null;
            if (CommonUIData.bone) {
                boneData = BackUpData.GetOrNullMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.Position, UIParams.Instance.lStyle);

                GUILayout.FlexibleSpace();

                if (boneData == null || !boneData.changedPos) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                    BackUpData.RestorePosition(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
                    BackUpData.Refresh();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.Rotation, UIParams.Instance.lStyle);

                GUILayout.FlexibleSpace();

                if (boneData == null || !boneData.changedRot) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                    BackUpData.RestoreRotation(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
                    BackUpData.Refresh();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.Scale, UIParams.Instance.lStyle);

                GUILayout.FlexibleSpace();

                if (boneData == null || !boneData.changedScl) {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("R", UIParams.Instance.bStyle)) {
                    BackUpData.RestoreScale(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
                    BackUpData.Refresh();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }
    }
}
