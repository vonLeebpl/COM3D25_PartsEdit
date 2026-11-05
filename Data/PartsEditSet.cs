using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CM3D2.PartsEdit.Plugin {
    // Bundle of existing single-slot ObjectData xml files.
    // Member files stay in Config/PartsEdit so the old Import list still sees them.
    [XmlRoot("PartsEditSet")]
    public class PartsEditSet {
        public string version = "1";
        public string name = "";
        public List<SetEntry> entries = new List<SetEntry>();
    }

    public class SetEntry {
        public string slotName = "";
        public string fileName = "";
        public string rootName = "";
    }
}
