using System.Collections.Generic;

namespace FCSCommon.Interfaces
{
    internal interface ISaveData
    {
        List<ISaveDataEntry> Entries { get; set; }
    }
}
