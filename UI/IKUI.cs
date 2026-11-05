using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class IKUI {
        Vector2 scroll;
        string pendingTip = "";
        string rename = "";
        int bindIndex = 0;
        ComboBoxLO bindCombo;

        public void Draw() {
            IKManager.BindObject();
            IKManager.RefreshHandles();
            GUILayout.Label(Text.IK);
            GUILayout.BeginHorizontal();
            GUILayout.Label(CommonUIData.obj ? Text.ObjectColon + CommonUIData.obj.name : Text.NoneSelected);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            GUILayout.EndHorizontal();

            DrawObjectFlags();
            DrawNew();
            DrawList();
            DrawActions();
            DrawParentToBone();
        }

        void DrawObjectFlags() {
            IKFile file = IKManager.CurrentFile();
            if (file == null) return;
            file.frozen = GUILayout.Toggle(file.frozen, Text.IKFreeze, UIParams.Instance.tStyle);
            file.ignoreGravity = GUILayout.Toggle(file.ignoreGravity, Text.IKIgnoreGravity, UIParams.Instance.tStyle);
            file.freezeAngle = GUILayout.Toggle(file.freezeAngle, Text.IKFreezeAngle, UIParams.Instance.tStyle);
        }

        void DrawNew() {
            if (!CommonUIData.obj) return;
            GUILayout.Label(Text.IKNew);
            string current = CommonUIData.bone ? CommonUIData.bone.name : Text.NoneSelected;
            if (string.IsNullOrEmpty(pendingTip)) {
                GUILayout.Label(Text.IKPickEnd + " " + current);
                GUI.enabled = CommonUIData.bone;
                if (GUILayout.Button(Text.IKConfirmTip, UIParams.Instance.bStyle)) pendingTip = CommonUIData.bone.name;
                GUI.enabled = true;
                return;
            }
            GUILayout.Label(Text.IKEnd + pendingTip);
            GUILayout.Label(Text.IKPickRoot + " " + current);
            GUI.enabled = CommonUIData.bone && CommonUIData.bone.name != pendingTip;
            if (GUILayout.Button(Text.IKConfirmRoot, UIParams.Instance.bStyle)) {
                Transform tip = IKManager.FindBone(pendingTip);
                if (IKManager.CreateFromSelection(tip, CommonUIData.bone) != null) pendingTip = "";
            }
            GUI.enabled = true;
            if (GUILayout.Button(Text.Cancel, UIParams.Instance.bStyle)) pendingTip = "";
        }

        void DrawList() {
            GUILayout.Label(Text.IKChains);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(140f));
            foreach (IKChain chain in IKManager.chains) {
                GUILayout.BeginHorizontal();
                chain.enabled = GUILayout.Toggle(chain.enabled, "", UIParams.Instance.tStyle, GUILayout.Width(20f));
                if (GUILayout.Button(chain.name + "  " + chain.rootBone + " > " + chain.endBone, IKManager.selected == chain ? UIParams.Instance.listStyle : UIParams.Instance.bStyle)) {
                    IKManager.selected = chain;
                    rename = chain.name;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        void DrawActions() {
            IKChain chain = IKManager.selected;
            GUI.enabled = chain != null;
            GUILayout.BeginHorizontal();
            rename = GUILayout.TextField(rename, UIParams.Instance.textStyle);
            if (GUILayout.Button(Text.IKRename, UIParams.Instance.bStyle) && chain != null) chain.name = rename;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Text.IKLimits, UIParams.Instance.bStyle)) Setting.mode = Mode.IKLimit;
            if (GUILayout.Button(Text.IKDelete, UIParams.Instance.bStyle)) IKManager.Remove(chain);
            GUILayout.EndHorizontal();
            if (chain != null) chain.poleEnabled = GUILayout.Toggle(chain.poleEnabled, Text.IKPole, UIParams.Instance.tStyle);
            DrawLink(chain);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Text.IKSave, UIParams.Instance.bStyle)) IKManager.Save();
            if (GUILayout.Button(Text.IKSolve, UIParams.Instance.bStyle)) IKManager.SolveSelected();
            GUILayout.EndHorizontal();
        }

        void DrawParentToBone() {
            GUILayout.Label(Text.IKParentTip);
            string bone = CommonUIData.bone ? CommonUIData.bone.name : Text.NoneSelected;
            GUILayout.Label(Text.Bone + bone);
            List<IKChain> all = new List<IKChain>(IKManager.AllChains());
            if (all.Count == 0) return;
            GUIContent[] names = new GUIContent[all.Count];
            for (int i = 0; i < all.Count; i++) names[i] = new GUIContent(all[i].name);
            if (bindCombo == null || bindCombo.ItemCount != names.Length) {
                bindCombo = new ComboBoxLO(names[0], names, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
            }
            bindIndex = bindCombo.ShowScroll(GUILayout.ExpandHeight(false));
            bindIndex = Mathf.Clamp(bindIndex, 0, all.Count - 1);
            GUI.enabled = CommonUIData.bone;
            if (GUILayout.Button(Text.IKBind, UIParams.Instance.bStyle)) IKManager.BindTipToBone(all[bindIndex], CommonUIData.bone);
            GUI.enabled = !string.IsNullOrEmpty(all[bindIndex].bindBone);
            if (GUILayout.Button(Text.IKUnbind, UIParams.Instance.bStyle)) IKManager.Unbind(all[bindIndex]);
            GUI.enabled = true;
            if (!string.IsNullOrEmpty(all[bindIndex].bindBone))
                GUILayout.Label(all[bindIndex].bindObject + " / " + all[bindIndex].bindBone);
        }

        void DrawLink(IKChain chain) {
            if (chain == null) return;
            GUILayout.Label(Text.IKLink);
            if (GUILayout.Button(Text.NoneWord, UIParams.Instance.bStyle)) chain.linkFromId = "";
            foreach (IKChain other in IKManager.chains) {
                if (other == chain) continue;
                string mark = chain.linkFromId == other.id ? "* " : "";
                if (GUILayout.Button(mark + other.name, UIParams.Instance.bStyle)) chain.linkFromId = other.id;
            }
        }
    }

    class IKGravityUI {
        public void Draw() {
            GUILayout.Label(Text.IKGravity);
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            Setting.gravityOn = GUILayout.Toggle(Setting.gravityOn, Text.IKGravity, UIParams.Instance.tStyle);
            Setting.gravityStrength = Float(Text.IKGravityStrength, Setting.gravityStrength, 0f, 10f);
            Setting.gravityX = Float("X", Setting.gravityX, -1f, 1f);
            Setting.gravityY = Float("Y", Setting.gravityY, -1f, 1f);
            Setting.gravityZ = Float("Z", Setting.gravityZ, -1f, 1f);
            if (GUILayout.Button(Text.IKGravityVector + " ↓", UIParams.Instance.bStyle)) {
                Setting.gravityX = 0f; Setting.gravityY = -1f; Setting.gravityZ = 0f;
            }
            Setting.gravityFloorOn = GUILayout.Toggle(Setting.gravityFloorOn, Text.IKFloor, UIParams.Instance.tStyle);
            if (Setting.gravityFloorOn) Setting.gravityFloor = Float(Text.IKFloor, Setting.gravityFloor, -2f, 2f);
        }
        float Float(string label, float value, float min, float max) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(50f));
            string text = GUILayout.TextField(value.ToString("0.00"), GUILayout.Width(50f));
            float parsed;
            if (float.TryParse(text, out parsed)) value = parsed;
            value = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.EndHorizontal();
            return value;
        }
    }

    class IKLimitUI {
        Vector2 scroll;
        public void Draw() {
            IKChain chain = IKManager.selected;
            GUILayout.Label(Text.IKLimits);
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.IK;
            if (chain == null) { GUILayout.Label(Text.NoneSelected); return; }
            GUILayout.Label(chain.name);
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < chain.bones.Count; i++) DrawBone(chain, i);
            GUILayout.EndScrollView();
        }
        void DrawBone(IKChain chain, int i) {
            IKLimit limit = chain.limits[i];
            GUILayout.BeginHorizontal();
            limit.enabled = GUILayout.Toggle(limit.enabled, chain.bones[i], UIParams.Instance.tStyle);
            if (GUILayout.Button(Text.IKMass, UIParams.Instance.bStyle, GUILayout.Width(70f))) IKManager.MassSet(chain, i);
            GUILayout.EndHorizontal();
            if (!limit.enabled) return;
            limit.minX = Float(Text.IKMin + " X", limit.minX); limit.maxX = Float(Text.IKMax + " X", limit.maxX);
            limit.minY = Float(Text.IKMin + " Y", limit.minY); limit.maxY = Float(Text.IKMax + " Y", limit.maxY);
            limit.minZ = Float(Text.IKMin + " Z", limit.minZ); limit.maxZ = Float(Text.IKMax + " Z", limit.maxZ);
        }
        float Float(string label, float value) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(70f));
            string text = GUILayout.TextField(value.ToString("0.0"), GUILayout.Width(60f));
            float parsed;
            if (float.TryParse(text, out parsed)) value = parsed;
            value = GUILayout.HorizontalSlider(value, -180f, 180f);
            GUILayout.EndHorizontal();
            return value;
        }
    }
}
