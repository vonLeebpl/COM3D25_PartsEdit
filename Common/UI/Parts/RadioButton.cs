using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal class RadioButton {
    List<bool> radioList = new List<bool>();
    bool allOffAble;

    // allOff:全てoffを許容するか
    public RadioButton(int num, bool allOff) {
        allOffAble = allOff;
        for (int i = 0; i < num; i++) {
            radioList.Add(false);
        }
        if (!allOffAble) {
            radioList[0] = true;
        }
    }

    public RadioButton(int num, int onNum, bool allOff) {
        allOffAble = allOff;
        for (int i = 0; i < num; i++) {
            radioList.Add(false);
        }
        Set(onNum, true);
    }

    public void Draw(int index) {
        bool result = GUILayout.Toggle(radioList[index], "", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
        Set(index, result);
    }

    //public void DrawHorizontal() {
    //    GUILayout.BeginHorizontal();
    //    for(int i = 0; i < radioList.Count; i++) {
    //        Draw(i);
    //    }
    //    GUILayout.EndHorizontal();
    //}

    //public void DrawVertical() {
    //    for (int i = 0; i < radioList.Count; i++) {
    //        Draw(i);
    //    }
    //}

    public void Set(int index, bool flg) {
        if (flg == true && radioList[index] == false) {
            // trueになった場合、他をすべてfalseにする
            for (int i = 0; i < radioList.Count; i++) {
                if (i == index) {
                    radioList[i] = true;
                } else {
                    radioList[i] = false;
                }
            }
        } else if (flg == false && radioList[index] == true) {
            // 全てOffを許容する場合のみOnからOffに変更する
            if (allOffAble) {
                radioList[index] = false;
            }
        }
    }

    public void SetAllOff() {
        if (allOffAble) {
            for (int i = 0; i < radioList.Count; i++) {
                radioList[i] = false;
            }
        }
    }

    // Onのインデックスを返す。全てOffの場合は-1
    public int GetIndex() {
        return radioList.IndexOf(true);
    }

    public bool GetBool(int index) {
        return radioList[index];
    }
}
