using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal class TransformUtil {
    public static string GetRelativePath(Transform root, Transform target) {
        string path = "";
        if (root == target) return path;

        Transform temp = target;
        while(root != temp) {
            if(temp == null) {
                return null;
            }

            if(path != "") {
                path = temp.name + "/" + path;
            }else {
                path = temp.name;
            }

            temp = temp.parent;
        }

        return path;
    }

    public static string GetAbsolutePath(Transform target) {
        string path = "";

        Transform temp = target;
        while (temp != null) {
            if (path != "") {
                path = temp.name + "/" + path;
            } else {
                path = temp.name;
            }

            temp = temp.parent;
        }

        return path;
    }

}
