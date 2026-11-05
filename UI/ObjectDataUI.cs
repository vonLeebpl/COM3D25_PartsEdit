using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    class ObjectDataUI {
        ObjectData objectData = null;

        class TransformData {
            bool targetExist = false;
            bool dataExist = false;
            bool enable = false;

            List<TransformData> children;
        }
    }
}
