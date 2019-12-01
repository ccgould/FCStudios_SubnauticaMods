using FCSCommon.Objects;
using FCSPowerStorage.Models;
using System;
using System.Collections.Generic;
using FCSCommon.Enums;

namespace FCSPowerStorage.Configuration
{
    [Serializable]
    internal class SaveData
    {
        public PowerToggleStates ChargeMode { get; set; }
        public ColorVec4 BodyColor { get; set; }
        public List<PowercellModel> Batteries { get; set; }
        public bool ToggleMode { get; set; }
        public FCSPowerStates PowerState { get; set; }
        public bool AutoActivate { get; set; } = false;

        /// <summary>
        /// Enables/Disables the ability for the
        /// </summary>
        public bool BaseDrainProtection { get; set; } = false;

        /// <summary>
        /// Minimum amount of power required for charging
        /// </summary>
        public int BaseDrainProtectionGoal { get; set; } = 10;

        /// <summary>
        /// Amount to activate the power storage in case of low power
        /// </summary>
        public int AutoActivateAt { get; set; } = 10;
    }
}
