using System;
using System.IO;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.AutoCrafter.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.Mono;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono;
using FCS_ProductionSolutions.Mods.MatterAnalyzer.Mono;
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
                        controller.GrowBedManager.AddSample(techType, amount);
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

                    if (Enum.TryParse(mode, true, out HarvesterSpeedModes result))
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


        [ConsoleCommand("cleardrill")]
        public static string HarvesterModeCommand(int id)
        {
            var dd = GameObject.FindObjectsOfType<FCSDeepDrillerController>();

            foreach (FCSDeepDrillerController controller in dd)
            {
                QuickLogger.Debug($"Current ID: {controller.UnitID} || Match: DD{id:D3}");
                if (controller.UnitID.Equals($"DD{id:D3}", StringComparison.OrdinalIgnoreCase))
                {
                    controller.EmptyDrill();
                    break;
                }
            }

            return $"Parameters: {id}";
        }

        [ConsoleCommand("UnlockDNA")]
        public static string UnlockDNA(string techTypeAsString)
        {
            if (UWE.Utils.TryParseEnum(techTypeAsString, out TechType techType))
            {
                if ((MatterAnalyzerStorage.ValidSeeds.Contains(techType) ||
                     Mod.IsNonePlantableAllowedList.Contains(techType)) &&
                    !Mod.IsHydroponicKnownTech(techType, out var omit))
                {
                    if (Mod.CreateNewDNASampleData(techType, out var data))
                    {
                        Mod.AddHydroponicKnownTech(data);
                        return $"{Language.main.Get(techType)} was unlocked.";
                    }
                }

                return $"{Language.main.Get(techType)} is not scannable by the Matter Analyzer.";
            }
            return $"Could not parse {techTypeAsString} as TechType";
        }


        [ConsoleCommand("LinkCrafter")]
        public static string LinkCrafterCommand(int parentCrafter, int childCrafter)
        {
            QuickLogger.Debug($"Executing Command Add Harvester", true);

            var unitName1 = $"{AutoCrafterPatch.AutoCrafterTabID}{parentCrafter:D3}";
            var unitName2 = $"{AutoCrafterPatch.AutoCrafterTabID}{childCrafter:D3}";

            var crafters = GameObject.FindObjectsOfType<AutoCrafterController>();
            
            foreach (AutoCrafterController controller in crafters)
            {
                if (controller.UnitID.Equals(unitName1, StringComparison.OrdinalIgnoreCase))
                {
                    controller.AddLinkedDevice(crafters.FirstOrDefault(x=>x.UnitID == unitName2));
                    break;
                }
            }
            return $"Parameters: {parentCrafter} {childCrafter}";
        }

        [ConsoleCommand("UnLinkCrafter")]
        public static string UnLinkCrafterCommand(int parentCrafter, int childCrafter)
        {
            QuickLogger.Debug($"Executing Command Add Harvester", true);

            var unitName1 = $"{AutoCrafterPatch.AutoCrafterTabID}{parentCrafter:D3}";
            var unitName2 = $"{AutoCrafterPatch.AutoCrafterTabID}{childCrafter:D3}";

            var crafters = GameObject.FindObjectsOfType<AutoCrafterController>();

            foreach (AutoCrafterController controller in crafters)
            {
                if (controller.UnitID.Equals(unitName1, StringComparison.OrdinalIgnoreCase))
                {
                    controller.RemoveLinkedDevice(crafters.FirstOrDefault(x => x.UnitID == unitName2));
                    break;
                }
            }
            return $"Parameters: {parentCrafter} {childCrafter}";
        }

        [ConsoleCommand("CrafterSet1")]
        public static void CrafterSet1Command()
        {
            PlayerInteractionHelper.GivePlayerItem(TechType.Copper,2);
            PlayerInteractionHelper.GivePlayerItem(TechType.Silver,2);
            PlayerInteractionHelper.GivePlayerItem(TechType.JeweledDiskPiece,2);
            PlayerInteractionHelper.GivePlayerItem(TechType.Gold,3);
        }

        //[ConsoleCommand("testMe")]
        //public static void TestMeCommand()
        //{
        //    QuickLogger.Debug($"{PDAEncyclopedia.entries.Count}");
        //    var sb = new StringBuilder();
        //    foreach (var entry in PDAEncyclopedia.entries)
        //    {
        //        PDAEncyclopedia.GetEntryData(entry.Key, out var entryData);
        //        sb.Append("// --------------------- //");
        //        sb.Append(Environment.NewLine);
        //        sb.Append($"Entry:{ entry.Key}");
        //        sb.Append(Environment.NewLine);
        //        sb.Append($"Path:{ entryData.path}");
        //        sb.Append(Environment.NewLine);
        //        sb.Append($"Key:{ entryData.key}");
        //        sb.Append(Environment.NewLine);
        //        sb.Append($"Time Capsule:{ entryData.timeCapsule}");
        //        sb.Append(Environment.NewLine);
        //        sb.Append($"Unlocked:{ entryData.unlocked}");
        //        sb.Append(Environment.NewLine);
        //        sb.Append($"Nodes:{ entryData.nodes.Length}");
        //        sb.Append(Environment.NewLine);
        //        foreach (string dataNode in entryData.nodes)
        //        {
        //            sb.Append($"Node:{dataNode}");
        //            sb.Append(Environment.NewLine);
        //        }
        //        sb.Append("// --------------------- //");
        //        sb.Append(Environment.NewLine);
        //    }

        //    using (StreamWriter file = new StreamWriter(@"F:\Game Development\EncyclopediaData.txt"))
        //    {
        //        file.WriteLine(sb.ToString());
        //    }
        //}
    }
}
