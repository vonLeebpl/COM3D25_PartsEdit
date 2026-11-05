using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class SkinnedMeshObjectSelectUI {
        GUIContent[] objectNameList = null;
        List<GameObject> objectList = null;
        ComboBoxLO combo = null;

        GameObject selectedObject = null;

        public void Draw() {
            if(objectNameList == null) {
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
                    if (CommonUIData.obj != selectedObject) {
                        CommonUIData.obj = selectedObject;
                        CommonUIData.bone = null;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        void ResetObjectList() {
            objectList =
                GameObject.FindObjectsOfType<SkinnedMeshRenderer>()
                .Where(smr => smr.gameObject.GetComponentInParent<Maid>() == null)
                .Select(smr => smr.gameObject)
                .ToList();

            objectNameList = new GUIContent[objectList.Count + 1];
            objectNameList[0] = new GUIContent(Tr.T(Text.NoneSelected));
            for (int i = 0; i < objectList.Count; i++) {
                objectNameList[i + 1] = new GUIContent(objectList[i].name);
            }

            int num = -1;
            if (selectedObject) {
                num = objectList.IndexOf(selectedObject);
            }
            combo = new ComboBoxLO(objectNameList[num + 1], objectNameList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
        }
    }
}
