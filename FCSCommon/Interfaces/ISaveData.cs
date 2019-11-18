using System;
using System.Collections.Generic;
using System.Text;

namespace FCSCommon.Interfaces
{
    public interface ISaveData
    {
        List<ISaveDataEntry> Entries { get; set; }
    }
}
