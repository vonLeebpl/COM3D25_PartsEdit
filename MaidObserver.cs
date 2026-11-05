using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

class MaidObserver {
    HashSet<Maid> maidHash = null;

    public bool CheckActiveChange() {
        if (maidHash == null) return true;
        List<Maid> activeMaidList = GetActiveMaidList();
        if (maidHash.Count != activeMaidList.Count) {
            return true;
        }
        foreach (Maid maid in activeMaidList) {
            if (!maidHash.Contains(maid)) {
                return true;
            }
        }
        return false;
    }

    public void UpdateMaidActiveState() {
        maidHash = new HashSet<Maid>(GetActiveMaidList());
    }

    public List<Maid> GetActiveMaidList() {
        List<Maid> activeMaidList = new List<Maid>();
        for(int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++) {
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(i);
            if(maid==null || !maid.gameObject.activeInHierarchy) {
                continue;
            }
            activeMaidList.Add(maid);
        }
        List<Maid> stockList = GameMain.Instance.CharacterMgr.GetStockMaidList();
        activeMaidList.AddRange(stockList.Where(m => (m && m.gameObject.activeInHierarchy)));
        return activeMaidList.Distinct().ToList();
    }
}
