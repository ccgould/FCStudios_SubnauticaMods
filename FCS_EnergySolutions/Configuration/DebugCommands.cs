using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.JetStreamT242.Mono;
using SMLHelper.V2.Commands;

namespace FCS_EnergySolutions.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("activateturbine")]
        public static string ActivateTurbineCommand(int unitID)
        {
            var unitName = $"{Mod.JetStreamT242TabID}{unitID:D3}";
            foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
            {
                if (device.Key.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                {
                    var controller = device.Value.gameObject.GetComponent<JetStreamT242Controller>();
                    controller.ActivateTurbine();
                }
            }
            return $"Parameters: {unitID}";
        }
    }
}
