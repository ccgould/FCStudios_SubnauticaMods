using System.Collections.Generic;

namespace FCS_AlterraHub.Mono
{
    public class BaseSaveData
    {
        public string Version { get; set; } = "1.0"; 
        public string InstanceID { get; set; }
        public string BaseName { get; set; }
        public bool AllowDocking { get; set; }
        public bool HasBreakerTripped { get; set; }
        public List<TechType> BlackList { get; set; }
        public List<BaseTransferOperation> BaseOperations { get; set; }
    }
}