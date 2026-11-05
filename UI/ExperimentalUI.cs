using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class ExperimentalUI {
        Vector2 scroll;
        Dictionary<string, float> overrides = new Dictionary<string, float>();
        string lastObject = "";

        public void Draw() {
            GUILayout.Label(Text.Experimental, UIParams.Instance.lStyle);
            Setting.experimental = GUILayout.Toggle(Setting.experimental, Text.Experimental, UIParams.Instance.tStyle);
            if (!Setting.experimental) {
                if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
                return;
            }
            GUILayout.Label(Text.Physics);
            if (CommonUIData.obj) {
                bool on = IKManager.PhysicsEnabled(CommonUIData.obj.name);
                bool next = GUILayout.Toggle(on, CommonUIData.obj.name, UIParams.Instance.tStyle);
                if (next != on) IKManager.SetPhysics(CommonUIData.obj.name, next);
                if (on) {
                    float radius = IKManager.RadiusFor(CommonUIData.obj.name);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Text.PhysicsRadius);
                    string r = GUILayout.TextField(radius.ToString("0.00"), GUILayout.Width(50f));
                    float parsed;
                    if (float.TryParse(r, out parsed)) IKManager.SetRadius(CommonUIData.obj.name, Mathf.Max(0.001f, parsed));
                    IKManager.SetRadius(CommonUIData.obj.name, GUILayout.HorizontalSlider(radius, 0.01f, 0.3f));
                    GUILayout.EndHorizontal();
                }
            }
            DrawBones();
            if (GUILayout.Button(Text.Back, UIParams.Instance.bStyle)) Setting.mode = Mode.Edit;
            if (GUILayout.Button(Text.Reset, UIParams.Instance.bStyle) && CommonUIData.obj) ResetBones(CommonUIData.obj.name);
        }

        void DrawBones() {
            if (!CommonUIData.obj) return;
            if (lastObject != CommonUIData.obj.name) {
                lastObject = CommonUIData.obj.name;
                scroll = Vector2.zero;
            }
            GUILayout.Label(Text.Bone + CommonUIData.obj.name);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(160f));
            Transform[] bones = CommonUIData.obj.GetComponentsInChildren<Transform>(true);
            int shown = 0;
            foreach (Transform t in bones) {
                if (shown++ > 60) break;
                string key = CommonUIData.obj.name + "/" + t.name;
                float shownValue;
                bool custom = overrides.TryGetValue(key, out shownValue);
                float baseRadius = IKManager.RadiusFor(CommonUIData.obj.name);
                if (!custom) shownValue = baseRadius;
                Color prev = GUI.color;
                if (!custom) GUI.color = Color.gray;
                GUILayout.BeginHorizontal();
                GUILayout.Label(t.name, GUILayout.Width(120f));
                string text = GUILayout.TextField(shownValue.ToString("0.00"), GUILayout.Width(40f));
                float v;
                if (float.TryParse(text, out v)) {
                    v = Mathf.Max(0.001f, v);
                    if (Mathf.Abs(v - IKManager.RadiusFor(CommonUIData.obj.name)) < 0.0001f) overrides.Remove(key);
                    else overrides[key] = v;
                }
                GUILayout.EndHorizontal();
                GUI.color = prev;
            }
            GUILayout.EndScrollView();
        }

        void ResetBones(string objectName) {
            string prefix = objectName + "/";
            List<string> drop = new List<string>();
            foreach (string key in overrides.Keys) if (key.StartsWith(prefix)) drop.Add(key);
            foreach (string key in drop) overrides.Remove(key);
        }

        public float RadiusFor(string objectName, string bone) {
            float v;
            return overrides.TryGetValue(objectName + "/" + bone, out v) ? v : IKManager.RadiusFor(objectName);
        }
    }
}
