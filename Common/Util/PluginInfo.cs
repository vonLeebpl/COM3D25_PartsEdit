using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityInjector;

internal static class PluginInfo {
    static string name;
    public static string Name { get { return name; } }
    static string version;
    public static string Version { get { return version; } }
    static string nameSpace;
    public static string NameSpace { get { return nameSpace; } }

    public static void SetInfo(string name, string version, string nameSpace) {
        PluginInfo.name = name;
        PluginInfo.version = version;
        PluginInfo.nameSpace = nameSpace;
    }
}
