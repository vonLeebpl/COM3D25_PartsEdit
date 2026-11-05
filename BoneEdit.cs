using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class BoneEdit : MonoBehaviour{
        Dictionary<Transform, Transform> fromToBoneDic = null;
        Dictionary<Transform, Transform> toFromBoneDic = null;
        Transform rootTrs = null;
        List<Transform> copyBoneList = null;
        BoneRendererAssist rootBra = null;
        List<BoneRendererAssist> braList = null;

        Transform boneRendererRoot;

        bool visible = true;
        bool selectable = true;

        GameObject targetObj = null;
        Transform targetBone = null;
        BoneDisplay boneDisplay = BoneDisplay.None;
        bool yureEnable = false;
        bool bEdit = false;
        BoneGizmoRenderer.COORDINATE coordinateType = BoneGizmoRenderer.COORDINATE.Local;
        GizmoType editType = GizmoType.None;

        BoneGizmoRenderer bgr = null;

        List<string> exclusiveBoneList = new List<string>() {
            "Foretwist_L",
            "Foretwist_R",
            "Foretwist1_L",
            "Foretwist1_R",
            "Hip_L",
            "Hip_L_nub",
            "Hip_R",
            "Hip_R_nub",
            "Kata_L",
            "Kata_L_nub",
            "Kata_R",
            "Kata_R_nub",
            "momoniku_L",
            "momoniku_R",
            "momotwist_L",
            "momotwist_R",
            "momotwist2_L",
            "momotwist2_R",
            "Mune_L",
            "Mune_L_sub",
            "Mune_R",
            "Mune_R_sub",
            "Uppertwist_L",
            "Uppertwist_R",
            "Uppertwist1_L",
            "Uppertwist1_R"
        };

        Dictionary<string, string> parentChildDic = new Dictionary<string, string> {
            {"Bip01 Pelvis","Bip01"},
            {"Bip01 L Thigh","Bip01 Pelvis"},
            {"Bip01 L Calf","Bip01 L Thigh"},
            {"Bip01 L Foot","Bip01 L Calf"},
            {"Bip01 L Toe0","Bip01 L Foot"},
            {"Bip01 L Toe01","Bip01 L Toe0"},
            {"Bip01 L Toe0Nub","Bip01 L Toe01"},
            {"Bip01 L Toe1","Bip01 L Foot"},
            {"Bip01 L Toe11","Bip01 L Toe1"},
            {"Bip01 L Toe1Nub","Bip01 L Toe11"},
            {"Bip01 L Toe2","Bip01 L Foot"},
            {"Bip01 L Toe21","Bip01 L Toe2"},
            {"Bip01 L Toe2Nub","Bip01 L Toe21"},
            {"Bip01 R Thigh","Bip01 Pelvis"},
            {"Bip01 R Calf","Bip01 R Thigh"},
            {"Bip01 R Foot","Bip01 R Calf"},
            {"Bip01 R Toe0","Bip01 R Foot"},
            {"Bip01 R Toe01","Bip01 R Toe0"},
            {"Bip01 R Toe0Nub","Bip01 R Toe01"},
            {"Bip01 R Toe1","Bip01 R Foot"},
            {"Bip01 R Toe11","Bip01 R Toe1"},
            {"Bip01 R Toe1Nub","Bip01 R Toe11"},
            {"Bip01 R Toe2","Bip01 R Foot"},
            {"Bip01 R Toe21","Bip01 R Toe2"},
            {"Bip01 R Toe2Nub","Bip01 R Toe21"},
            {"Bip01 Spine","Bip01"},
            {"Bip01 Spine0a","Bip01 Spine"},
            {"Bip01 Spine1","Bip01 Spine0a"},
            {"Bip01 Spine1a","Bip01 Spine1"},
            {"Bip01 L Clavicle","Bip01 Spine1a"},
            {"Bip01 L UpperArm","Bip01 L Clavicle"},
            {"Bip01 L Forearm","Bip01 L UpperArm"},
            {"Bip01 L Hand","Bip01 L Forearm"},
            {"Bip01 L Finger0","Bip01 L Hand"},
            {"Bip01 L Finger01","Bip01 L Finger0"},
            {"Bip01 L Finger02","Bip01 L Finger01"},
            {"Bip01 L Finger0Nub","Bip01 L Finger02"},
            {"Bip01 L Finger1","Bip01 L Hand"},
            {"Bip01 L Finger11","Bip01 L Finger1"},
            {"Bip01 L Finger12","Bip01 L Finger11"},
            {"Bip01 L Finger1Nub","Bip01 L Finger12"},
            {"Bip01 L Finger2","Bip01 L Hand"},
            {"Bip01 L Finger21","Bip01 L Finger2"},
            {"Bip01 L Finger22","Bip01 L Finger21"},
            {"Bip01 L Finger2Nub","Bip01 L Finger22"},
            {"Bip01 L Finger3","Bip01 L Hand"},
            {"Bip01 L Finger31","Bip01 L Finger3"},
            {"Bip01 L Finger32","Bip01 L Finger31"},
            {"Bip01 L Finger3Nub","Bip01 L Finger32"},
            {"Bip01 L Finger4","Bip01 L Hand"},
            {"Bip01 L Finger41","Bip01 L Finger4"},
            {"Bip01 L Finger42","Bip01 L Finger41"},
            {"Bip01 L Finger4Nub","Bip01 L Finger42"},
            {"Bip01 Neck","Bip01 Spine1a"},
            {"Bip01 Head","Bip01 Neck"},
            {"Bip01 HeadNub","Bip01 Head"},
            {"Bip01 R Clavicle","Bip01 Spine1a"},
            {"Bip01 R UpperArm","Bip01 R Clavicle"},
            {"Bip01 R Forearm","Bip01 R UpperArm"},
            {"Bip01 R Hand","Bip01 R Forearm"},
            {"Bip01 R Finger0","Bip01 R Hand"},
            {"Bip01 R Finger01","Bip01 R Finger0"},
            {"Bip01 R Finger02","Bip01 R Finger01"},
            {"Bip01 R Finger0Nub","Bip01 R Finger02"},
            {"Bip01 R Finger1","Bip01 R Hand"},
            {"Bip01 R Finger11","Bip01 R Finger1"},
            {"Bip01 R Finger12","Bip01 R Finger11"},
            {"Bip01 R Finger1Nub","Bip01 R Finger12"},
            {"Bip01 R Finger2","Bip01 R Hand"},
            {"Bip01 R Finger21","Bip01 R Finger2"},
            {"Bip01 R Finger22","Bip01 R Finger21"},
            {"Bip01 R Finger2Nub","Bip01 R Finger22"},
            {"Bip01 R Finger3","Bip01 R Hand"},
            {"Bip01 R Finger31","Bip01 R Finger3"},
            {"Bip01 R Finger32","Bip01 R Finger31"},
            {"Bip01 R Finger3Nub","Bip01 R Finger32"},
            {"Bip01 R Finger4","Bip01 R Hand"},
            {"Bip01 R Finger41","Bip01 R Finger4"},
            {"Bip01 R Finger42","Bip01 R Finger41"},
            {"Bip01 R Finger4Nub","Bip01 R Finger42" }
        };

        Dictionary<string, string> firstChildDic = new Dictionary<string, string> {
            {"Bip01 Spine0a","Bip01 Spine"},
            {"Bip01 Spine1","Bip01 Spine0a"},
            {"Bip01 Spine1a","Bip01 Spine1"},
            {"Bip01 Neck","Bip01 Spine1a"},
            {"Bip01 Head","Bip01 Neck"},
            {"Bip01 L UpperArm","Bip01 L Clavicle"},
            {"Bip01 L Forearm","Bip01 L UpperArm"},
            {"Bip01 L Hand","Bip01 L Forearm"},
            {"Bip01 L Finger4","Bip01 L Hand"},
            {"Bip01 R UpperArm","Bip01 R Clavicle"},
            {"Bip01 R Forearm","Bip01 R UpperArm"},
            {"Bip01 R Hand","Bip01 R Forearm"},
            {"Bip01 R Finger4","Bip01 R Hand"},
            {"Bip01 L Calf","Bip01 L Thigh"},
            {"Bip01 L Foot","Bip01 L Calf"},
            {"Bip01 R Calf","Bip01 R Thigh"},
            {"Bip01 R Foot","Bip01 R Calf"}
        };

        void Start() {
            boneRendererRoot = new GameObject().transform;
            boneRendererRoot.parent = transform;
            boneRendererRoot.name = "BoneRendererRoot";

            InitGizmo();

            Setting.normalBoneColor.mDelegate += OnChangeNormalBoneColor;
            Setting.selectBoneColor.mDelegate += OnChangeSelectBoneColor;
            Setting.bodyBoneDisplay.mDelegate += OnChangeBodyBoneDisplay;
        }

        void Update() {
            CommonDataChangeCheck();

            if (braList == null) return;

            foreach(BoneRendererAssist bra in braList) {
                bra.BRAUpdate();
            }

            if (Setting.boneSelectKey != KeyCode.None && Input.GetKey(Setting.boneSelectKey)) {
                bgr.Visible = false;

                if (selectable && Input.GetMouseButtonDown(0)) {
                    // Unity 2022 / COM3D2.5: raycast w cienkie collidery nie trafial, wiec wybieramy
                    // najblizszy segment kosci w przestrzeni ekranu.
                    Transform picked = PickBoneByScreen();
                    Log.Info("bone pick -> " + (picked ? toFromBoneDic[picked].name : "none"));
                    if (picked) BoneClick(picked);
                }
            }else {
                bgr.Visible = bEdit;
            }

            bgr.smallMoveKey = Setting.gizmoSmallKey;
            bgr.bigMoveKey = Setting.gizmoBigKey; 
        }

        const float pickRadiusPx = 20f;
        const float proxyPickBiasPx = 8f;

        // Zwraca kopie kosci (klucz toFromBoneDic) najblizsza kursorowi, w promieniu pickRadiusPx.
        Transform PickBoneByScreen() {
            Camera cam = Camera.main;
            if (!cam) {
                foreach (Camera c in Camera.allCameras) {
                    if (!cam || c.depth > cam.depth) cam = c;
                }
            }
            if (!cam) return null;

            Vector2 mouse = Input.mousePosition;
            float best = float.MaxValue;
            Transform bestTrs = null;
            foreach (BoneRendererAssist bra in braList) {
                if (!bra.Selectable) continue;
                Vector3 a = cam.WorldToScreenPoint(bra.transform.position);
                Vector3 b = cam.WorldToScreenPoint(bra.GetBoneTailPos());
                if (a.z < 0f && b.z < 0f) continue;
                float d = DistanceToSegment(mouse, a, b);
                if (d > pickRadiusPx) continue;

                // Kosci-zastepniki butow leza dokladnie w tym samym miejscu co prawdziwy Foot/Toe,
                // wiec przy remisie maja pierwszenstwo (inaczej zawsze wygrywa oryginal).
                float score = d;
                Transform real;
                if (toFromBoneDic.TryGetValue(bra.transform, out real) && real && real.name.EndsWith(ShoeProxy.Suffix)) {
                    score -= proxyPickBiasPx;
                }
                if (score < best) {
                    best = score;
                    bestTrs = bra.transform;
                }
            }
            return bestTrs;
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b) {
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 0.0001f) return (p - a).magnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
            return (p - (a + ab * t)).magnitude;
        }

        void CopyBoneConstruction(Transform root, Transform[] fromBones) {
            copyBoneList = new List<Transform>();
            fromToBoneDic = new Dictionary<Transform, Transform>();
            toFromBoneDic = new Dictionary<Transform, Transform>();
            braList = new List<BoneRendererAssist>();

            SetRoot(root);
            foreach (Transform fromBone in fromBones) {
                GetOrSetCopyBone(root, fromBone);
            }
        }

        void SetRoot(Transform root) {
            rootTrs = new GameObject().transform;
            rootTrs.name = root.name;
            copyBoneList.Add(rootTrs);
            fromToBoneDic.Add(root, rootTrs);
            toFromBoneDic.Add(rootTrs, root);
            rootTrs.parent = boneRendererRoot;

            CopyTransform ct = rootTrs.gameObject.AddComponent<CopyTransform>();
            ct.bScl = false;
            ct.SetTarget(toFromBoneDic[rootTrs]);

            BoneRendererAssist bra = rootTrs.gameObject.AddComponent<BoneRendererAssist>();
            bra.SetSelectable(selectable);
            bra.SetVisible(boneDisplay == BoneDisplay.Visible);
            bra.SetColor(Setting.normalBoneColor.GetValue());
            braList.Add(bra);

        }

        Transform GetOrSetCopyBone(Transform root, Transform fromBone) {
            if (fromToBoneDic.ContainsKey(fromBone)) {
                return fromToBoneDic[fromBone];
            }
            if (fromBone.parent == null) {
                return null;
            }

            Transform toParent = null;
            Transform toBone = null;
            BoneRendererAssist braParent = null;

            if (fromToBoneDic.ContainsKey(fromBone.parent)) {
                toParent = fromToBoneDic[fromBone.parent];
                braParent = toParent.GetComponent<BoneRendererAssist>();

                // DistortCorrect対策
                if (parentChildDic.ContainsKey(fromBone.name)) {
                    string parentName = parentChildDic[fromBone.name];
                    Transform fromParent = CMT.SearchObjName(root.transform, parentName, false);
                    if (fromParent) {
                        braParent = GetOrSetCopyBone(root, fromParent).GetComponent<BoneRendererAssist>();
                    }
                }
            } else {
                toParent = GetOrSetCopyBone(root, fromBone.parent);
                if (toParent == null) return null;
                braParent = toParent.GetComponent<BoneRendererAssist>();
            }
            toBone = new GameObject().transform;
            toBone.name = fromBone.name;
            copyBoneList.Add(toBone);
            fromToBoneDic.Add(fromBone, toBone);
            toFromBoneDic.Add(toBone, fromBone);
            toBone.parent = boneRendererRoot;

            CopyTransform ct = toBone.gameObject.AddComponent<CopyTransform>();
            ct.bScl = false;
            ct.SetTarget(toFromBoneDic[toBone]);

            BoneRendererAssist bra = toBone.gameObject.AddComponent<BoneRendererAssist>();
            bra.SetSelectable(selectable);
            if (braParent) {
                bra.SetParent(braParent);
                if (firstChildDic.ContainsKey(toBone.name) && firstChildDic[toBone.name] == braParent.transform.name) {
                    braParent.SetFirstChild(bra);
                } else {
                    braParent.SetChild(bra);
                }
            }

            if (boneDisplay== BoneDisplay.Visible) {
                bra.SetVisible(JudgeVisibleBone(bra));
            } else {
                bra.SetVisible(false);
            }
            if (fromBone.name.EndsWith(ShoeProxy.Suffix)) bra.fixedLength = ShoeProxy.DisplayLength(fromBone.name);
            bra.SetColor(Setting.normalBoneColor.GetValue());
            braList.Add(bra);

            return toBone;
        }

        bool JudgeVisibleBone(BoneRendererAssist bra) {
            if (bra.transform.name.EndsWith("_nub")) return false;
            // Gdy wlaczone sa kosci-zastepniki butow, pokazujemy tylko je (reszta szkieletu slotu to szum).
            if (ShoeProxy.IsActive(targetObj)) return bra.transform.name.EndsWith(ShoeProxy.Suffix);
            if (Setting.targetSelectMode == 0 && exclusiveBoneList.Contains(bra.transform.name)) return false;
            if (Setting.targetSelectMode == 0 && Setting.bodyBoneDisplay.GetValue() == BodyBoneDisplay.Invisible && BoneUtil.IsBodyBone(bra.transform.name)) return false;
            if (bra.transform.name.EndsWith("_SCL_") && bra.parent.transform.name == bra.transform.name.Substring(0, bra.transform.name.Length - 5)) return false;
            return true;
        }

        void Clear() {
            fromToBoneDic = null;
            toFromBoneDic = null;
            copyBoneList = null;
            braList = null;

            foreach(Transform child in boneRendererRoot) {
                Destroy(child.gameObject);
            }

            CommonUIData.bone = null;
            bgr.Visible = false;
        }

        void BoneClick(Transform bone) {
            CommonUIData.bone = toFromBoneDic[bone];
        }

        void InitGizmo() {
            bgr = BoneGizmoRenderer.AddGizmo(transform, "CommonLamia");
            bgr.Visible = false;
            bgr.offsetScale = 0.5f;
            //bgr.eAxis = true;
            //bgr.eRotate = false;
            //bgr.eScal = true;
            bgr.eDragUndo = true;
            //bgr.SetCoordinate(ExGizmoRenderer.COORDINATE.World);
            bgr.dragEndDelegate += GizmoDragEnd;
        }

        void CommonDataChangeCheck() {
            TargetObjectChangeCheck();
            TargetBoneChangeCheck();
            BoneDisplayChange();
            GizmoEditChangeCheck();
            CoordinateTypeChangeCheck();
            GizmoTypeChangeCheck();
        }

        internal static bool requestRebuild = false;

        void TargetObjectChangeCheck() {
            if (requestRebuild) {
                requestRebuild = false;
                targetObj = null;
            }

            if(Setting.targetSelectMode == 0) {
                if(CommonUIData.maid == null || CommonUIData.slotNo == (int)EXSlot.None) {
                    CommonUIData.SetObject(null);
                }else if(CommonUIData.slotNo == (int)EXSlot.Base){
                    CommonUIData.SetObject(CommonUIData.maid.body0.m_Bones);
                }else {
                    CommonUIData.SetObject(CommonUIData.maid.body0.goSlot[CommonUIData.slotNo].obj);
                }
            }

            if (targetObj == CommonUIData.obj && !(targetObj == null && braList != null)) return;

            targetObj = CommonUIData.obj;
            //CheckYureState();
            Clear();

            if (!targetObj) return;

            Transform[] bones;
            if (Setting.targetSelectMode == 0 && CommonUIData.slotNo == -1) {
                bones = CommonUIData.maid.body0.goSlot[0].obj.GetComponentInChildren<SkinnedMeshRenderer>().bones
                    .Where(bone => bone != null)
                    .Select(bone => CMT.SearchObjName(CommonUIData.obj.transform, bone.name, true))
                    .Where(bone => bone != null)
                    .ToArray();
            } else {
                SkinnedMeshRenderer smr = targetObj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr == null) return;

                bones = smr.bones.Where(bone => bone != null).ToArray();
            }
            CopyBoneConstruction(targetObj.transform, bones);
            // Korzen (budowany w SetRoot) nie przechodzi przez JudgeVisibleBone - przy wlaczonych kosciach-zastepnikach
            // chowamy go (gra i tak nadpisuje jego pozycje co klatke), zeby nie dalo sie go zaznaczyc.
            if (ShoeProxy.IsActive(targetObj) && braList != null && braList.Count > 0) braList[0].SetVisible(false);

            //SetComponent();

            //foreach (BoneRendererAssist bra in braList) {
            //    bra.AutoSetUp();
            //}
        }

        void TargetBoneChangeCheck() {
            if (targetBone == CommonUIData.bone) return;

            targetBone = CommonUIData.bone;

            if (targetBone) {
                bgr.SetTarget(targetBone);
                bgr.Visible = bEdit;

                foreach (BoneRendererAssist bra in braList) {
                    Transform real = toFromBoneDic[bra.transform];
                    if (real == targetBone) bra.SetColor(Setting.selectBoneColor.GetValue());
                    else if (IKManager.InAnyChain(real)) bra.SetColor(Setting.ikChainColor.GetValue());
                    else bra.SetColor(Setting.normalBoneColor.GetValue());
                }
            } else {
                bgr.SetTarget(null);
                bgr.Visible = false;
            }
        }

        //FieldInfo fi_BoneHair_Enable = Helper.GetFieldInfo(typeof(TBoneHair_), "m_bEnable");
        //void YureEnableChangeCheck() {
        //    if (yureEnable == CommonUIData.yureEnable) return;

        //    yureEnable = CommonUIData.yureEnable;

        //    Helper.SetInstanceField(typeof(TBoneHair_), CommonUIData.tbs.bonehair, "m_bEnable", yureEnable);
        //}

        void GizmoEditChangeCheck() {
            if (bEdit == (Setting.gizmoType != GizmoType.None)) return;

            bEdit = Setting.gizmoType != GizmoType.None;

            bgr.Visible = bEdit;
        }

        void CoordinateTypeChangeCheck() {
            if (coordinateType == Setting.coordinateType) return;

            coordinateType = Setting.coordinateType;

            bgr.coordinate = coordinateType;
        }

        void GizmoTypeChangeCheck() {
            if (editType == Setting.gizmoType) return;

            editType = Setting.gizmoType;
            bgr.eAxis = false;
            bgr.eRotate = false;
            bgr.eScal = false;
            switch (editType) {
                case GizmoType.Position:
                    bgr.eAxis = true;
                    break;
                case GizmoType.Rotation:
                    bgr.eRotate = true;
                    break;
                case GizmoType.Scale:
                    bgr.eScal = true;
                    break;
                default:
                    break;
            }
        }

        //void CheckYureState() {
        //    if (CommonUIData.tbs == null || CommonUIData.tbs.bonehair == null) {
        //        CommonUIData.yureExist = false;
        //        CommonUIData.yureEnable = false;
        //        yureEnable = false;
        //        return;
        //    }

        //    CommonUIData.yureExist = true;
        //    yureEnable = (bool)Helper.GetInstanceField(typeof(TBoneHair_), CommonUIData.tbs.bonehair, "m_bEnable");
        //    CommonUIData.yureEnable = yureEnable;
        //}

        void BoneDisplayChange() {
            if (boneDisplay == Setting.boneDisplay) return;

            boneDisplay = Setting.boneDisplay;

            if (braList == null) return;
            switch (boneDisplay) {
                case BoneDisplay.None:
                    SetVisible(false);
                    SetSelectable(false);
                    break;
                case BoneDisplay.Visible:
                    SetVisible(true);
                    SetSelectable(true);
                    break;
                case BoneDisplay.Choisable:
                    SetVisible(true);
                    SetSelectable(true);
                    break;
                default:
                    break;
            }
        }

        void SetSelectable(bool fSelectable) {
            selectable = fSelectable;
            foreach(BoneRendererAssist bra in braList) {
                bra.SetSelectable(selectable);
            }
        }

        void SetVisible(bool fVisible) {
            foreach (BoneRendererAssist bra in braList) {
                if (fVisible) {
                    bra.SetVisible(JudgeVisibleBone(bra));
                }else {
                    bra.SetVisible(false);
                }
            }
        }

        void GizmoDragEnd() {
            int gizmoType = bgr.GetSelectedType();
            if (gizmoType <= 0 || gizmoType > 21) return;

            BackUpBoneData boneData;
            switch (Setting.targetSelectMode) {
                case 0:
                    boneData = BackUpData.GetOrAddMaidBoneData(CommonUIData.maid, CommonUIData.slotNo, CommonUIData.obj, CommonUIData.bone);
                    break;
                case 1:
                case 2:
                    boneData = BackUpData.GetOrAddBoneData(CommonUIData.obj, CommonUIData.bone);
                    break;
                default:
                    return;
            }

            if (gizmoType <= 9) {
                // 位置変更
                if (!boneData.changedPos) {
                    boneData.position = bgr.GetBackUpLocalPosition();
                    boneData.changedPos = true;
                }
            }else if(gizmoType > 9 && gizmoType <= 12) {
                // 回転変更
                if (!boneData.changedRot) {
                    boneData.rotation = bgr.GetBackUpLocalRotation();
                    boneData.changedRot = true;
                }
            } else if(gizmoType > 12) {
                // 拡縮変更
                if (!boneData.changedScl) {
                    boneData.scale = bgr.GetBackUpLocalScale();
                    boneData.changedScl = true;
                }
            }           
        }

        void OnChangeNormalBoneColor(Color color) {
            if (braList == null) return;

            foreach(BoneRendererAssist bra in braList) {
                if(toFromBoneDic[bra.transform] != CommonUIData.bone) {
                    bra.SetColor(color);
                }
            }
        }

        void OnChangeSelectBoneColor(Color color) {
            if (braList == null) return;

            foreach (BoneRendererAssist bra in braList) {
                if (toFromBoneDic[bra.transform] == CommonUIData.bone) {
                    bra.SetColor(color);
                }
            }
        }

        void OnChangeBodyBoneDisplay(BodyBoneDisplay bodyBoneDisplay) {
            if (braList == null) return;

            foreach (BoneRendererAssist bra in braList) {
                bra.SetVisible(JudgeVisibleBone(bra));
            }
        }
    }
}
