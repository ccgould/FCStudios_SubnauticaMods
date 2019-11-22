using System;
using System.Collections.Generic;
using FCSCommon.Objects;

namespace AE.IntraBaseTeleporter.Configuration
{
    internal class SaveDataEntry
    {
        public string Id { get; set; }
        public ColorVec4 BodyColor { get; set; }
        public string UnitName { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        public List<SaveDataEntry> Entries { get; set; } = new List<SaveDataEntry>();
    }
}
