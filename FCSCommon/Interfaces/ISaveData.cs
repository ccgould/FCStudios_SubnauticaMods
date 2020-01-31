using System.Collections.Generic;

namespace FCSCommon.Interfaces
{
    public interface ISaveData
    {
        List<ISaveDataEntry> Entries { get; set; }
    }
}
