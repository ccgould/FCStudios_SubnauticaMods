using System;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_ProductionSolutions.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("harvesterspawn")]
        public static string AddHarvesterItemCommand(int id, string techTypeString, int amount)
        {
            QuickLogger.Debug($"Executing Command Add Harvester", true);

            var unitName = $"{Mod.HydroponicHarvesterModTabID}{id:D3}";
            var hh = GameObject.FindObjectsOfType<HydroponicHarvesterController>();

            var techType = techTypeString.ToTechType();

            if (techType != TechType.None)
            {
                foreach (HydroponicHarvesterController controller in hh)
                {
                    if (controller.UnitID.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                    {
                        QuickLogger.Debug($"Adding Dummy to harvester", true);
                        controller.GrowBedManager.AddDummy(techType, amount);
                        break;
                    }
                }
            }
            else
            {
                QuickLogger.Error($"Invalid TechType: {techTypeString}",true);
            }
            return $"Parameters: {id} {techTypeString} {amount}";
        }

        [ConsoleCommand("clearharvester")]
        public static string ClearHarvesterCommand(int id)
        {
            var hh = GameObject.FindObjectsOfType<HydroponicHarvesterController>();

            foreach (HydroponicHarvesterController controller in hh)
            {
                if (controller.UnitID.Equals($"{Mod.HydroponicHarvesterModTabID}{id:D3}", StringComparison.OrdinalIgnoreCase))
                {
                    controller.GrowBedManager.ClearGrowBed();
                    break;
                }
            }

            return $"Parameters: {id}";
        }


        [ConsoleCommand("harvesterMode")]
        public static string HarvesterModeCommand(int id, string mode)
        {
            var hh = GameObject.FindObjectsOfType<HydroponicHarvesterController>();

            foreach (HydroponicHarvesterController controller in hh)
            {
                QuickLogger.Debug($"Current ID: {controller.UnitID} || Match: HH{id:D3}");
                if (controller.UnitID.Equals($"HH{id:D3}", StringComparison.OrdinalIgnoreCase))
                {

                    if (Enum.TryParse(mode, true, out SpeedModes result))
                    {
                        controller.GrowBedManager.SetSpeedMode(result);
                    }
                    else
                    {
                        QuickLogger.Error($"Invalid harvester speed mode valid are: Min,Low,High,Max,", true);
                    }
                    break;
                }
            }

            return $"Parameters: {mode}";
        }
    }
}
