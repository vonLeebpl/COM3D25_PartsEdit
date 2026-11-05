using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;

using UnityEngine;

internal class PngData {
    byte[] pngByteData;

    public PngData(string name) {
        string full = PluginInfo.NameSpace + ".PngResource." + name;
        Stream pngStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(full);
        if (pngStream == null) pngStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PngResource." + name);
        if (pngStream == null) {
            Debug.LogWarning("PartsEdit: icon resource not found: " + full);
            pngByteData = new byte[0];
            return;
        }
        int length = (int)pngStream.Length;
        pngByteData = new byte[length];
        pngStream.Read(pngByteData, 0, length);
        pngStream.Close();
    }

    public byte[] GetData() {
        return pngByteData;
    }
}
