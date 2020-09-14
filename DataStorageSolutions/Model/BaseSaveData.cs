using System.Collections.Generic;
using DataStorageSolutions.Configuration;

namespace DataStorageSolutions.Model
{
    internal class BaseSaveData
    {
        public string InstanceID { get; set; }
        public string BaseName { get; set; }
        public bool AllowDocking { get; set; }
        public bool HasBreakerTripped { get; set; }
        public IEnumerable<OperationSaveData> OperationSaveData { get; set; }
        public IEnumerable<OperationSaveData> AutoCraftData { get; set; }
    }
}
