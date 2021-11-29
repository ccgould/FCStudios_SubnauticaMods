using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Interface;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Mono;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Spawnables;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCS_HomeSolutions.Mods.SeaBreeze.Mono;
using FCS_HomeSolutions.Spawnables;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_HomeSolutions.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("clearseabreeze")]
        public static string ClearSeabreezeCommand(int unitID)
        {
            var unitName = $"{SeaBreezeBuildable.SeaBreezeTabID}{unitID:D3}";

            QuickLogger.Debug($"Trying to find device: {unitName} || Count of Devices: {FCSAlterraHubService.PublicAPI.GetRegisteredDevices()?.Count}",true);
            foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
            {
                var compareResult = device.Key.Equals(unitName, StringComparison.OrdinalIgnoreCase);
                QuickLogger.Debug($"Compare Returned: {compareResult}", true);
                if (compareResult)
                {
                    var controller = device.Value.gameObject.GetComponent<SeaBreezeController>();
                    controller.ClearSeaBreeze();
                }
            }
            return $"Parameters: {unitID}";
        }

        [ConsoleCommand("setallglobal")]
        public static string SetAllGlobal(bool setGlobal)
        {
            foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(QuantumTeleporterBuildable.QuantumTeleporterTabID))
            {
                var controller = device.Value.gameObject.GetComponent<QuantumTeleporterController>();
                controller.IsGlobal = setGlobal;
            }
            return $"Parameters: {setGlobal}";
        }

        [ConsoleCommand("teleport")]
        public static string OnTeleportCommand(string id)
        {

            if (!string.IsNullOrWhiteSpace(id))
            {
                var device = FCSAlterraHubService.PublicAPI.FindDevice(id);

                if (device.Value != null)
                {
                    var destinationTeleporter = device.Value.gameObject.GetComponent<IQuantumTeleporter>();

                    if (!destinationTeleporter.IsOperational)
                    {
                        return "This teleporter is not Operational.";
                    }

                    if (device.Value.Manager.IsSame(Player.main.GetCurrentSub()))
                    {
                        return $"Cannot teleport to the same base.";
                    }

                    var powerBankTechType = "QuantumPowerBank".ToTechType();

                    if (PlayerInteractionHelper.HasItem(powerBankTechType))
                    {
                        var powerBanks = PlayerInteractionHelper.GetItemsOnPlayer(powerBankTechType);

                        foreach (InventoryItem bank in powerBanks)
                        {
                            //Get power bank controller
                            var bankController = bank.item.gameObject.GetComponent<QuantumPowerBankController>();

                            //Check if the power bank has enough power
                            if (!bankController.PowerManager.HasEnoughPower(Player.main.IsPiloting() ? QTTeleportTypes.Vehicle : QTTeleportTypes.Global)) continue;

                            //Check if a valid destination
                            if (!QuantumPowerBankSpawnable.ValidateDestination(destinationTeleporter, out string result)) return result;

                            TeleportManager.TeleportPlayer(bankController, destinationTeleporter, Player.main.IsPiloting() ? QTTeleportTypes.Vehicle : QTTeleportTypes.Global);

                            return "Teleport SuccessFull";
                        }

                        return "Power bank doesn't have enough power for teleporting";
                    }

                    return "Requires a Quantum Power Bank on person";
                }

                return $"Failed to find teleporter with the ID of : {id}";
            }
            
            return $"Parameters: {id}";
        }

        [ConsoleCommand("rechargepowerbanks")]
        public static string OnReChargeAllTeleportsCommand()
        {
            var powerBankTechType = "QuantumPowerBank".ToTechType();

            var powerBanks = PlayerInteractionHelper.GetItemsOnPlayer(powerBankTechType);

            if (powerBanks == null || !powerBanks.Any()) return "No power banks on player";

            foreach (InventoryItem bank in powerBanks)
            {
                var controller = bank.item.GetComponent<QuantumPowerBankController>();
                controller?.PowerManager?.FullReCharge();
            }

            return "Operation Successful";
        }
        
        [ConsoleCommand("saveprefabids")]
        public static string SavePrefabIDS()
        {
            var fp = @"f:\Subnautica_Prefab_IDS_Saves.json";
            // serialize JSON to a string and then write string to a file
            File.WriteAllText(fp, JsonConvert.SerializeObject(Mod.PrefabClassIDS));
            return $"Prefab IDS Saved to {fp}";
        }

        [ConsoleCommand("showBuilderPaths")]
        public static string ShowBuilderPaths(bool value)
        {
            if (!Player.main.IsPiloting()) return "Command Failed";
            //Get the vehicle
            var vehicle = Player.main.GetVehicle();

            if (vehicle == null) return "Command Failed";

            if (value)
            {
                var builderPoints = vehicle.gameObject.FindChild("build_bot_paths");

                //Check If dots exists
                if (vehicle.gameObject.FindChild("FCSBuilderPaths") == null)
                {
                    var fcsBuilderBeams = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fcsBuilderBeams.name = "FCSBuilderPaths";
                    fcsBuilderBeams.transform.SetParent(vehicle.gameObject.transform, true);

                    fcsBuilderBeams.transform.localScale = Vector3.one;

                    foreach (Transform path in builderPoints.transform)
                    {
                        foreach (Transform point in path.transform)
                        {
                            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.name = "FCSTempPathsCube";
                            cube.transform.SetParent(fcsBuilderBeams.transform, false);
                            cube.transform.localScale = new Vector3(.5f, .5f, .5f);
                        }
                    }
                }
            }
            else
            {
                var fcsBuilderBeams = vehicle.gameObject.FindChild("FCSBuilderPaths");
                if (fcsBuilderBeams != null)
                {
                    GameObject.Destroy(fcsBuilderBeams);
                }
            }
            return $"Parameters: {value}";
        }
        
        [ConsoleCommand("showBuilderBeamPoints")]
        public static string ShowBuilderBeamPoints(bool value)
        {
            if (!Player.main.IsPiloting()) return "Command Failed";
            //Get the vehicle
            var vehicle = Player.main.GetVehicle();

            if (vehicle == null) return "Command Failed";

            if (value)
            {
                var builderPoints = vehicle.gameObject.FindChild("buildbotbeampoints");

                //Check If dots exists
                if (vehicle.gameObject.FindChild("FCSBuilderBeams") == null)
                {
                    var fcsBuilderBeams = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fcsBuilderBeams.name = "FCSBuilderBeams";
                    fcsBuilderBeams.transform.SetParent(vehicle.gameObject.transform,true);
                    
                    fcsBuilderBeams.transform.localScale = Vector3.one;

                    foreach (Transform point in builderPoints.transform)
                    {
                        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.name = "FCSTempBeamCube";
                        cube.transform.parent = fcsBuilderBeams.transform;
                        cube.transform.position = point.position;
                        cube.transform.localScale = new Vector3(.5f, .5f, .5f);
                    }
                }
            }
            else
            {
               var fcsBuilderBeams = vehicle.gameObject.FindChild("FCSBuilderBeams");
               if (fcsBuilderBeams != null)
               {
                   GameObject.Destroy(fcsBuilderBeams);
               }
            }
            return $"Parameters: {value}";
        }

    }
}
