using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal static class MaidObserver {
    // SomeDelegate という名前のデリゲート型を定義
    public delegate void MaidAddDelegate(Maid maid);
    public static event MaidAddDelegate maidAddDelegate = delegate (Maid madi) { };
    public delegate void MaidDelDelegate(Maid maid);
    public static event MaidDelDelegate maidDelDelegate = delegate (Maid madi) { };
    public delegate void ActiveMaidChangeDelegate();
    public static event ActiveMaidChangeDelegate activeMaidChangeDelegate = delegate () { };



}
