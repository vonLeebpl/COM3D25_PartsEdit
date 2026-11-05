using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal class UIRadioButton : IUIDrawer{
    int size;
    List<string> nameList;
    RadioButton radioButton;

    public UIRadioButton(int num) {
        size = num;
        radioButton = new RadioButton(size, false);
        nameList = new List<string>(size);
        nameList.ForEach(x => x = "");
    }

    public UIRadioButton(List<string> fNameList) {
        size = fNameList.Count;
        radioButton = new RadioButton(size, false);
        nameList = fNameList;
    }

    public void DrawItem() {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < size; i++) {
            GUILayout.BeginHorizontal();
            radioButton.Draw(i);
            GUILayout.Label(nameList[i]);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }
}
