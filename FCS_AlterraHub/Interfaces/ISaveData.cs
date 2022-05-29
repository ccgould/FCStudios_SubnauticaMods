using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;

namespace FCSCommon.Interfaces
{
    internal interface ISaveData
    {
        List<ISaveDataEntry> Entries { get; set; }
    }
}
