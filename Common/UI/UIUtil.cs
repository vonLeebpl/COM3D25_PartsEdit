using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal static class UIUtil {
    static int indentWidth = 5;

    public static void BeginIndentArea() {
        BeginIndentArea(indentWidth);
    }

    public static void BeginIndentArea(int width) {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(width));
        GUILayout.BeginVertical();
    }

    public static void EndoIndentArea() {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}
