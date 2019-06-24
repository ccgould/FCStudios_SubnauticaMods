using FCSCommon.Objects;
using FCSCommon.Utilities.Enums;
using FCSPowerStorage.Models;
using System;
using System.Collections.Generic;

namespace FCSPowerStorage.Configuration
{
    [Serializable]
    internal class SaveData
    {
        public PowerToggleStates ChargeMode { get; set; }
        public ColorVec4 BodyColor { get; set; }
        public List<PowercellModel> Batteries { get; set; }
        public int ToggleMode { get; set; }
        public FCSPowerStates PowerState { get; set; }
    }
}
