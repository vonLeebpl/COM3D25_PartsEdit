using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class MaidSelectUI {
        GUIContent[] maidNameList = null;
        ComboBoxLO combo = null;

        List<Maid> activeMaidList = null;
        MaidObserver maidObserver = new MaidObserver();

        Maid selectedMaid = null;

        public void Draw() {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Text.SelectMaid);
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                {
                    if (combo == null) {
                        ResetMaidList();
                    } else if (maidObserver.CheckActiveChange()) {
                        ResetMaidList();
                    }
                    int selectNum = combo.ShowScroll(GUILayout.ExpandWidth(false));
                    if (selectNum == 0) {
                        selectedMaid = null;
                        CommonUIData.maid = null;
                    } else if (selectNum > 0) {
                        selectedMaid = activeMaidList[selectNum - 1];
                        CommonUIData.maid = selectedMaid;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        public void DrawCombo() {
            GUILayout.BeginVertical();
            {
                if (combo == null) {
                    ResetMaidList();
                } else if (maidObserver.CheckActiveChange()) {
                    ResetMaidList();
                }
                int selectNum = combo.ShowScroll(GUILayout.ExpandWidth(false));
                if (selectNum == 0) {
                    selectedMaid = null;
                    CommonUIData.SetMaid(null);
                } else if (selectNum > 0) {
                    selectedMaid = activeMaidList[selectNum - 1];
                    CommonUIData.SetMaid(selectedMaid);
                }
            }
            GUILayout.EndVertical();
        }

        void ResetMaidList() {
            maidObserver.UpdateMaidActiveState();
            activeMaidList = maidObserver.GetActiveMaidList();
            string selectMaidName;

            // Selektor utworzony po wyborze pokojowki na innym ekranie (np. okno Library) startuje bez wyboru,
            // a ShowScroll zwraca biezacy indeks - bez tej synchronizacji kasowalby wspolny wybor (CommonUIData.maid).
            if (!selectedMaid && CommonUIData.maid && activeMaidList.Contains(CommonUIData.maid)) selectedMaid = CommonUIData.maid;


            if (selectedMaid && activeMaidList.Contains(selectedMaid)){
                selectMaidName = GetMaidFullName(selectedMaid);
            } else {
                selectedMaid = null;
                selectMaidName = Text.NoneSelected;
            }

            maidNameList = new GUIContent[activeMaidList.Count + 1];
            maidNameList[0] = new GUIContent(Tr.T(Text.NoneSelected));
            for (int i = 0; i < activeMaidList.Count; i++) {
                maidNameList[i + 1] = new GUIContent(GetMaidFullName(activeMaidList[i]));
            }
            combo = new ComboBoxLO(new GUIContent(selectMaidName), maidNameList, UIParams.Instance.bStyle, UIParams.Instance.winStyle, UIParams.Instance.listStyle, false);
            if (selectedMaid) combo.SelectedItemIndex = activeMaidList.IndexOf(selectedMaid) + 1;
        }

        string GetMaidFullName(Maid maid) {
#if COM3D2
            return maid.status.lastName + " " + maid.status.firstName;
#else
            return maid.Param.status.last_name + " " + maid.Param.status.first_name;
#endif
        }
    }
}
