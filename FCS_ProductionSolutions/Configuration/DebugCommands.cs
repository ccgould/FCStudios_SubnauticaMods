using System;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Extensions;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_ProductionSolutions.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("addharvesteritem")]
        public static string AddHarvesterItemCommand(string unitNameAndItem, int amount, bool myBool = false)
        {
            var param = unitNameAndItem.Split('_');
            if (param.Length == 2)
            {
                var unitName = param[0].Trim();
                var techType = param[1].Trim().ToTechType();

                var hh = GameObject.FindObjectsOfType<HydroponicHarvesterController>();

                foreach (HydroponicHarvesterController controller in hh)
                {
                    if (controller.UnitID.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                    {
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
