using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace CM3D2.PartsEdit.Plugin {
    class MultipleMaidObjectSelectUI {
        GUIContent[] objectNameList = null;
        List<GameObject> objectList = null;
        ComboBoxLO combo = null;

        GameObject selectedObject = null;

        HashSet<GameObject> objectHash = null;

        public void Draw() {
            if (objectNameList == null) {
                ResetObjectList();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Text.RefreshObjectList, UIParams.Instance.bStyle)) {
                    ResetObjectList();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.SelectObject);
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                {
                    //selectSlotId = combo.ShowScroll(GUILayout.ExpandWidth(true));
                    int num = combo.ShowScroll(GUILayout.ExpandWidth(false));
                    if (num == 0) {
                        selectedObject = null;
                    } else if (num > 0) {
                        selectedObject = objectList[num - 1];
                    }
                    CommonUIData.obj = selectedObject;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        public void DrawListReloadButton() {
            if (objectNameList == null) {
                ResetObjectList();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Text.RefreshObjectList, UIParams.Instance.bStyle)) {
                    ResetObjectList();
                }
            }
            GUILayout.EndHorizontal();
        }

        public void DrawCombo() {
            if (CheckObjectChange()) {
                ResetObjectList();
            }
            GUILayout.BeginVertical();
            {
                //selectSlotId = combo.ShowScroll(GUILayout.ExpandWidth(true));
                int num = combo.ShowScroll(GUILayout.ExpandWidth(false));
                if (num == 0) {
                    selectedObject = null;
                } else if (num > 0) {
                    selectedObject = objectList[num - 1];
                }
                CommonUIData.obj = selectedObject;
            }
            GUILayout.EndVertical();
        }

        bool CheckObjectChange() {
            if (objectHash == null) return true;
            List<GameObject> tempObjectList =
                SceneManager.GetActiveScene().GetRootGameObjects()
                .Where(go => (go.activeSelf && go.name.EndsWith(".menu") && go.GetComponent<Animation>()))
                .Select(smr => smr.gameObject)
                .ToList();

            if (tempObjectList.Count != objectHash.Count) return true;
            foreach(GameObject obj in tempObjectList) {
                if (!objectHash.Contains(obj)) return true;
            }
            return false;
        }

        void ResetObjectList() {
            objectList =
                SceneManager.GetActiveScene().GetRootGameObjects()
                .Where(go => (go.activeSelf && go.name.EndsWith(".menu") && go.GetComponent<Animation>()))
                .Select(smr => smr.gameObject)
                .ToList();

            objectNameList = new GUIContent[objectList.Count + 1];
            objectNameList[0] = new GUIContent(Tr.T(Text.NoneSelected));
            objectHash = new HashSet<GameObject>();
            for (int i = 0; i < objectList.Count; i++) {
                objectNameList[i + 1] = new GUIContent(objectList[i].name);
                objectHash.Add(objectList[i]);
            }

            int num = -1;
            if (selectedObject) {
                num = objectList.IndexOf(selectedObject);
                if (num == -1) {
                    selectedObject = null;
                }
            }
            if(selectedObject != CommonUIData.obj) {
                CommonUIData.obj = selectedObject;
                CommonUIData.bone = null;
            }
            
            combo = new ComboBoxLO(objectNameList[num + 1], objectNameList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
        }
    }
}

