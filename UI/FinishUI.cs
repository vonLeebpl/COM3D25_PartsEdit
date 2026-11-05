using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class FinishUI {
        UIWindow window = null;

        public FinishUI(UIWindow fWindow) {
            window = fWindow;
        }

        public void Draw() {
            GUILayout.BeginHorizontal();
            {
                if (BackUpData.maidDataDic.Count == 0 && BackUpData.objectDataDic.Count == 0) {
                    GUI.enabled = false;
                }
                    if (GUILayout.Button(Text.Reset, UIParams.Instance.bStyle)) {
                    BackUpData.Restore();
                }
                GUI.enabled = true;
            
                GUILayout.FlexibleSpace();

                if(GUILayout.Button(Text.Close, UIParams.Instance.bStyle)) {
                    CommonUIData.maid = null;
                    CommonUIData.slotNo = -2;
                    CommonUIData.obj = null;
                    CommonUIData.bone = null;
                    window.SetVisible(false);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
