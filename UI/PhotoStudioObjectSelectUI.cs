using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CM3D2.PartsEdit.Plugin {
    class PhotoStudioObjectSelectUI {
        GUIContent[] objectNameList = null;
        List<GameObject> objectList = null;
        ComboBoxLO combo = null;
        GameObject selectedObject = null;
        HashSet<GameObject> objectHash = null;
        Type refBgObject = null;
        Type refGenericType = null;

        public PhotoStudioObjectSelectUI() {
            Assembly asm = Assembly.GetAssembly(typeof(CreateBGObjectSubWindow));
            refBgObject = asm.GetType("CreateBGObjectSubWindow+BgObject");
            refGenericType = typeof(List<>).MakeGenericType(refBgObject);
        }

        public void DrawCombo() {
            if (objectNameList == null || CheckObjectChange()) ResetObjectList();
            GUILayout.BeginVertical();
            int num = combo.ShowScroll(GUILayout.ExpandWidth(false));
            if (num == 0) selectedObject = null;
            else if (num > 0) selectedObject = objectList[num - 1];
            CommonUIData.obj = selectedObject;
            GUILayout.EndVertical();
        }

        bool CheckObjectChange() {
            if (objectHash == null) return true;
            List<GameObject> temp = getObjectList();
            if (temp.Count != objectHash.Count) return true;
            foreach (GameObject obj in temp) if (!objectHash.Contains(obj)) return true;
            return false;
        }

        void ResetObjectList() {
            objectList = getObjectList();
            CreateBGObjectSubWindow window = GameObject.FindObjectsOfType<CreateBGObjectSubWindow>().FirstOrDefault();
            object createObjectList = null;
            if (window != null && objectList.Count > 1) {
                createObjectList = typeof(CreateBGObjectSubWindow).GetField("create_obj_list_", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(window);
            }

            objectNameList = new GUIContent[objectList.Count + 1];
            objectNameList[0] = new GUIContent(Tr.T(Text.NoneSelected));
            objectHash = new HashSet<GameObject>();
            for (int i = 0; i < objectList.Count; i++) {
                string name = objectList[i].name;
                if (i == 0 && IsBackground(objectList[i])) name = Text.Background;
                else if (createObjectList != null) name = StudioName(createObjectList, name);
                objectNameList[i + 1] = new GUIContent(name);
                objectHash.Add(objectList[i]);
            }

            int num = -1;
            if (selectedObject) {
                num = objectList.IndexOf(selectedObject);
                if (num == -1) selectedObject = null;
            }
            if (selectedObject != CommonUIData.obj) {
                CommonUIData.obj = selectedObject;
                CommonUIData.bone = null;
            }
            combo = new ComboBoxLO(objectNameList[num + 1], objectNameList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
        }

        string StudioName(object createObjectList, string name) {
            int count = (int)refGenericType.GetProperty("Count").GetValue(createObjectList, null);
            for (int x = 0; x < count; x++) {
                object item = refGenericType.GetProperty("Item").GetValue(createObjectList, new object[] { x });
                string createTime = refBgObject.GetField("create_time").GetValue(item).ToString();
                if (createTime != name) continue;
                PhotoBGObjectData data = refBgObject.GetField("data").GetValue(item) as PhotoBGObjectData;
                if (data != null && !string.IsNullOrEmpty(data.name)) return data.name;
            }
            return name;
        }

        bool IsBackground(GameObject obj) {
            return GameMain.Instance.BgMgr != null && obj == GameMain.Instance.BgMgr.current_bg_object;
        }

        List<GameObject> getObjectList() {
            List<GameObject> list = new List<GameObject>();
            Transform photoPrefab = GameMain.Instance.BgMgr?.bg_parent_object?.transform.Find("PhotoPrefab");
            if (photoPrefab != null) {
                for (int i = 0; i < photoPrefab.childCount; i++) list.Add(photoPrefab.GetChild(i).gameObject);
            }
            if (GameMain.Instance.BgMgr != null && GameMain.Instance.BgMgr.current_bg_object != null) {
                list.Insert(0, GameMain.Instance.BgMgr.current_bg_object);
            }
            AddRootObjects(list);
            return list;
        }

        // Studio parents props under PhotoPrefab. MPS keeps its own prop list.
        void AddRootObjects(List<GameObject> list) {
            HashSet<GameObject> have = new HashSet<GameObject>(list);
            foreach (GameObject prop in GetMpsProps()) {
                if (!prop || !prop.activeInHierarchy || have.Contains(prop)) continue;
                list.Add(prop);
                have.Add(prop);
            }
        }

        // MeidoPhotoStudio exposes Plugin.Api.Prop, an enumerable of PropController with a GameObject.
        IEnumerable<GameObject> GetMpsProps() {
            List<GameObject> found = new List<GameObject>();
            try {
                Type pluginType = null;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
                    pluginType = asm.GetType("MeidoPhotoStudio.Plugin.Plugin");
                    if (pluginType != null) break;
                }
                if (pluginType == null) return found;
                object api = pluginType.GetProperty("Api", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                if (api == null) return found;
                object propApi = api.GetType().GetProperty("Prop").GetValue(api, null);
                if (propApi == null) return found;
                object props = propApi.GetType().GetProperty("Props").GetValue(propApi, null);
                IEnumerable enumerable = props as IEnumerable;
                if (enumerable == null) return found;
                foreach (object controller in enumerable) {
                    if (controller == null || IsMenuProp(controller)) continue;
                    GameObject go = controller.GetType().GetProperty("GameObject").GetValue(controller, null) as GameObject;
                    if (go) found.Add(go);
                }
            } catch (Exception e) {
                Debug.Log("PartsEdit MPS prop scan failed: " + e.Message);
            }
            return found;
        }

        // .menu summons are MenuFilePropModel. Keep photo, desk, and other MPS props.
        bool IsMenuProp(object controller) {
            object model = controller.GetType().GetProperty("PropModel").GetValue(controller, null);
            if (model == null) return false;
            string typeName = model.GetType().Name;
            return typeName == "MenuFilePropModel" || typeName.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
