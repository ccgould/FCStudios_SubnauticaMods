using System;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_ProductionSolutions.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("addharvesteritem")]
        public static string AddHarvesterItemCommand(string unitNameAndItem, int amount, bool myBool = false)
        {
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug($"Executing Command Add Harvester", true);
            var param = unitNameAndItem.Split('_');
            QuickLogger.Debug($"Parameter Count: {param.Length}", true);

            if (param.Length == 2)
            {
                var unitName = param[0].Trim();
                QuickLogger.Debug($"Unit Name: {unitName}",true);
                var techType = param[1].Trim().ToTechType();
                QuickLogger.Debug($"TechType: {techType.AsString()}", true);

                var hh = GameObject.FindObjectsOfType<HydroponicHarvesterController>();

                foreach (HydroponicHarvesterController controller in hh)
                {
                    if (controller.UnitID.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                    {
                        QuickLogger.Debug($"Adding Dummy to harvester",true);
                        controller.GrowBedManager.AddDummy(techType,amount);
                        break;
                    }
                }
            }
            
            return $"Parameters: {unitNameAndItem} {amount}";
        }

        [ConsoleCommand("clearharvester")]
        public static string ClearHarvesterCommand(string unitName, int amount, bool myBool = false)
        {
            var hh = GameObject.FindObjectsOfType<HydroponicHarvesterController>();

            foreach (HydroponicHarvesterController controller in hh)
            {
                if (controller.UnitID.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                {
                    controller.GrowBedManager.ClearGrowBed();
                    break;
                }
            }

            return $"Parameters: {unitName}";
        }
    }
}
