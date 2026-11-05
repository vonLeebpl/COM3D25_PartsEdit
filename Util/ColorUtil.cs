using System;
using System.Globalization;
using UnityEngine;

internal static class ColorUtil {
    public static Color GetColorFromName(string name) {
        return ParseColor(name);
    }

    public static Color ParseColor(string value) {
        Color parsed;
        if (TryParseHex(value, out parsed)) return parsed;
        switch ((value ?? "").ToLower()) {
            case "white": return Color.white;
            case "black": return Color.black;
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "yellow": return Color.yellow;
            case "gray": return Color.gray;
            case "cyan": return Color.cyan;
            default: return FromHex("b80000");
        }
    }

    public static string GetNameFromColor(Color color) {
        return ToHex(color);
    }

    public static string ToHex(Color color) {
        return string.Format("#{0:X2}{1:X2}{2:X2}", Mathf.RoundToInt(color.r * 255f), Mathf.RoundToInt(color.g * 255f), Mathf.RoundToInt(color.b * 255f));
    }

    public static Color FromHex(string hex) {
        Color color;
        return TryParseHex(hex, out color) ? color : new Color(0.722f, 0f, 0f);
    }

    public static bool TryParseHex(string value, out Color color) {
        color = Color.gray;
        if (string.IsNullOrEmpty(value)) return false;
        string hex = value.Trim();
        if (hex.StartsWith("#")) hex = hex.Substring(1);
        if (hex.Length != 6) return false;
        int r, g, b;
        if (!int.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)) return false;
        if (!int.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)) return false;
        if (!int.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b)) return false;
        color = new Color(r / 255f, g / 255f, b / 255f, 1f);
        return true;
    }
}
