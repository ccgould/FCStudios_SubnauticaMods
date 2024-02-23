using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Models.Enumerators;
using System;
using System.Collections.Generic;

namespace FCS_AlterraHub.Models.Mono;
public partial class HabitatManager
{
    public List<string> GetConnectedDevices(object patchedTechType)
    {
        throw new NotImplementedException();
    }

    internal struct DeviceWarning
    {
        public DeviceWarning(string devicId, string warningID, string description, WarningType warningType, FaultType faultType)
        {
            DevicId = devicId;
            WarningID = warningID;
            Description = description;
            WarningType = warningType;
            FaultType = faultType;
        }

        public string DevicId { get; set; }
        public string WarningID { get; set; }
        public string Description { get; set; }
        public WarningType WarningType { get; set; }
        public FaultType FaultType { get; set; }
    }
}
