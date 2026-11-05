using BepInEx.Logging;

// Logowanie wtyczki. Pod BepInEx wiadomosci ida do zrodla "COM3D25_PartsEdit" (tag w LogOutput.log),
// a przy starcie przez UnityInjector (bez BepInEx) wracamy do Unity Debug.
internal static class Log {
    // ustawiane w PartsEditBepInPlugin.Awake
    internal static ManualLogSource Source;

    const string Prefix = "COM3D25_PartsEdit: ";

    public static void Info(string message) {
        if (Source != null) Source.LogInfo(message);
        else UnityEngine.Debug.Log(Prefix + message);
    }

    public static void Warn(string message) {
        if (Source != null) Source.LogWarning(message);
        else UnityEngine.Debug.LogWarning(Prefix + message);
    }

    public static void Error(string message) {
        if (Source != null) Source.LogError(message);
        else UnityEngine.Debug.LogError(Prefix + message);
    }
}
