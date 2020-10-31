using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.JetStreamT242.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Commands;

namespace FCS_EnergySolutions.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("activateturbine")]
        public static string ActivateTurbineCommand(int unitID)
        {
            var unitName = $"{Mod.JetStreamT242TabID}{unitID:D3}";

            QuickLogger.Debug($"Trying to find device: {unitName} || Count of Devices: {FCSAlterraHubService.PublicAPI.GetRegisteredDevices()?.Count}",true);
            foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
            {
                var compareResult = device.Key.Equals(unitName, StringComparison.OrdinalIgnoreCase);
                QuickLogger.Debug($"Compare Returned: {compareResult}", true);
                if (compareResult)
                {
                    var controller = device.Value.gameObject.GetComponent<JetStreamT242Controller>();
                    controller.ActivateTurbine();
                }
            }
            return $"Parameters: {unitID}";
        }
    }
}
