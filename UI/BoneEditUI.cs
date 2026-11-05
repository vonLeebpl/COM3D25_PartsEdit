using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BoneEditUI {
        public BoneEditUI() {

        }

        public void Draw() {
            GUILayout.Label(Text.BoneEdit);

            UIUtil.BeginIndentArea();
            {
                if (CommonUIData.bone) {
                    if (BackUpData.GetMaidBoneDataExist(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone)) {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(Text.ResetTransform, UIParams.Instance.bStyle)) {
                                ResetTransform();
                            }
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        UIUtil.BeginIndentArea();
                        {
                            BackUpBoneData boneData = BackUpData.GetOrNullMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
                            if (boneData != null) {
                                if (boneData.changedPos) {
                                    GUILayout.BeginHorizontal();
                                    {
                                        if (GUILayout.Button(Text.ResetPosition, UIParams.Instance.bStyle)) {
                                            ResetPosition();
                                        }
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                if (boneData.changedRot) {
                                    GUILayout.BeginHorizontal();
                                    {
                                        if (GUILayout.Button(Text.ResetRotation, UIParams.Instance.bStyle)) {
                                            ResetRotation();
                                        }
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                if (boneData.changedScl) {
                                    GUILayout.BeginHorizontal();
                                    {
                                        if (GUILayout.Button(Text.ResetScale, UIParams.Instance.bStyle)) {
                                            ResetScale();
                                        }
                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                        UIUtil.EndoIndentArea();
                    }
                }

            }
            UIUtil.EndoIndentArea();
        }

        void ResetTransform() {
            Transform bone = CommonUIData.bone;
            BackUpBoneData boneData = BackUpData.GetOrNullMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, bone);
            if (boneData == null) return;

            if (boneData.changedPos) bone.localPosition = boneData.position;
            if (boneData.changedRot) bone.localRotation = boneData.rotation;
            if (boneData.changedScl) bone.localScale = boneData.scale;

            BackUpData.GetOrNullMaidObjectData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj).boneDataDic.Remove(bone);
        }

        void ResetPosition() {
            Transform bone = CommonUIData.bone;
            BackUpBoneData boneData = BackUpData.GetOrNullMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, bone);
            if (boneData == null) return;

            if (boneData.changedPos) bone.localPosition = boneData.position;
            if(boneData.changedRot || boneData.changedScl) {
                // 回転かスケールが変更されているならトランスフォームの変更を残す
                boneData.changedPos = false;
            }else {
                // 回転もスケールも変更されていないならトランスフォームの変更を削除する
                BackUpData.GetOrNullMaidObjectData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj).boneDataDic.Remove(bone);
            }
        }

        void ResetRotation() {
            Transform bone = CommonUIData.bone;
            BackUpBoneData boneData = BackUpData.GetOrNullMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, bone);
            if (boneData == null) return;

            if (boneData.changedRot) bone.localRotation = boneData.rotation;
            if (boneData.changedPos || boneData.changedScl) {
                // 位置かスケールが変更されているならトランスフォームの変更を残す
                boneData.changedRot = false;
            } else {
                // 位置もスケールも変更されていないならトランスフォームの変更を削除する
                BackUpData.GetOrNullMaidObjectData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj).boneDataDic.Remove(bone);
            }
        }

        void ResetScale() {
            Transform bone = CommonUIData.bone;
            BackUpBoneData boneData = BackUpData.GetOrNullMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, bone);
            if (boneData == null) return;

            if (boneData.changedScl) bone.localScale = boneData.scale;
            if (boneData.changedPos || boneData.changedRot) {
                // 位置か回転が変更されているならトランスフォームの変更を残す
                boneData.changedScl = false;
            } else {
                // 位置も回転も変更されていないならトランスフォームの変更を削除する
                BackUpData.GetOrNullMaidObjectData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj).boneDataDic.Remove(bone);
            }
        }
    }
}
