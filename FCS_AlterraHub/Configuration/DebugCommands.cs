using System;
using System.IO;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubPod.Mono;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.OreConsumer.Buildable;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using RadicalLibrary;
using SMLHelper.V2.Commands;
using UWE;

namespace FCS_AlterraHub.Configuration
{
    internal class DebugCommands
    {
        private static TeleportScreenFXController _teleportEffects;

        [ConsoleCommand("givecredit")]
        public static string GiveCreditCommand(string amount)
        {
            if (decimal.TryParse(amount, out var result))
            {
                if (CardSystem.main.HasBeenRegistered())
                {
                    CardSystem.main.AddFinances(result);
                }
                else
                {
                    return "No account found. Please create an account to use this command";
                }
            }
            return $"Parameters: {amount}";
        }
        
        [ConsoleCommand("OrePrices")]
        public static void OrePricesCommand()
        {
            for (int i = 0; i < StoreInventorySystem.OrePrices.Count; i++)
            {
                var tech = StoreInventorySystem.OrePrices.ElementAt(i).Key;
                QuickLogger.ModMessage($"{Language.main.Get(tech)} : {StoreInventorySystem.GetOrePrice(tech)}");
            }
        }

        [ConsoleCommand("ResetAccount")]
        public static void ResetAccountCommand()
        {
            CardSystem.main.ResetAccount();
        }

        [ConsoleCommand("UnlockFCSItem")]
        public static string UnlockFCSItem(string techType)
        {
            BaseManager.ActivateGoalTechType = techType.ToTechType();
            return $"Parameters: {techType}";
        }

        [ConsoleCommand("FastOreProcessing")]
        public static void FastOreProcessing()
        {
            OreConsumerPatcher.OreProcessingTime = OreConsumerPatcher.OreProcessingTime >= 90f ? 1f : 90f;
            QuickLogger.Message($"Changed Processing speed",true);
        }

        [ConsoleCommand("UnlockFCSPDA")]
        public static void UnlockFCSPDA(bool value)
        {
            if (value)
            {
                FCSPDAController.ForceOpen();
                QuickLogger.Message($"FCS PDA Unlocked", true);
            }
            else
            {
                FCSPDAController.ForceClose();
                QuickLogger.Message($"FCS PDA locked", true);
            }
        }
        
        [ConsoleCommand("CreateDummyAccount")]
        public static string CreateDummyAccount(string amount = "0")
        {
            if(!CardSystem.main.HasBeenRegistered())
            {
                if (decimal.TryParse(amount, out var result))
                {
                    CardSystem.main.CreateUserAccount("Ryley Robinson", "RyleyRobinson4546B", "planet4546B", "4546", result);
                    return $"Created account RyleyRobinson4546B with amount {amount} balance.";
                }
            }
            QuickLogger.Message($"Account already exist {CardSystem.main.GetUserName()}", true);
            return $"Parameters: {nameof(amount)} Amount to put in account";
        }

        [ConsoleCommand("WarpStation")]
        public static string WarpStation()
        {
            if (AlterraHubPodController.main != null)
            {
                AlterraHubPodController.main.OnConsoleCommand_warp();
                return "Warped player to station";
            }

            return "Warped failed";
        }

        [ConsoleCommand("FcsQuickStart")]
        public static void FcsQuickStart()
        {
            if(!PlayerInteractionHelper.HasItem(TechType.Scanner))
                PlayerInteractionHelper.GivePlayerItem(TechType.Scanner);

            if (!PlayerInteractionHelper.HasItem(TechType.HighCapacityTank))
                PlayerInteractionHelper.GivePlayerItem(TechType.HighCapacityTank);

            if (!PlayerInteractionHelper.HasItem(TechType.Flashlight))
                PlayerInteractionHelper.GivePlayerItem(TechType.Flashlight);

            if (!PlayerInteractionHelper.HasItem(TechType.Rebreather))
                PlayerInteractionHelper.GivePlayerItem(TechType.Rebreather);

            if (!PlayerInteractionHelper.HasItem(TechType.Fins))
                PlayerInteractionHelper.GivePlayerItem(TechType.Fins);

            if (!PlayerInteractionHelper.HasItem(TechType.Welder))
                PlayerInteractionHelper.GivePlayerItem(TechType.Welder);
        }

        [ConsoleCommand("SpawnUWEPrefab")]
        public static string SpawnUWEPrefab(int prefabIndex)
        {
            if (Enum.IsDefined(typeof(UWEPrefabID), prefabIndex))
            {
                CoroutineHost.StartCoroutine(SpawnHelper.SpawnUWEPrefab((UWEPrefabID)prefabIndex,Player.main, Player.main.transform));
            }
            else
            {
                return $"Prefab Index {prefabIndex} doesn't exist";
            }

            return $"Spawned: {(UWEPrefabID) prefabIndex} at {Player.main.transform.position}";
        }

        [ConsoleCommand("ResetTransportDrones")]
        public static string ResetTransportDrones()
        {
            DroneDeliveryService.Main.ResetDrones();

            return $"Drones reset!";
        }        
        
        [ConsoleCommand("Evening")]
        public static string OnEveningCommand()
        {
            DayNightCycle.main.SetDayNightTime(0.86f);

            return $"Time Set to Evening";
        }

        [ConsoleCommand("PrintBaseLog")]
        public static string OnPrintBaseLogCommand()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var curBase in FCSAlterraHubService.PublicAPI.GetRegisteredBases())
            {
                builder.Append($"Base Name {curBase.BaseFriendlyID} | Base ID {curBase.BaseID} | Is Visible: {curBase.IsVisible}");
            }
            
            FileManager.CreateFile(Path.Combine(Mod.GetModDirectory(), "BaseLog.log"), builder.ToString(),true);

            return $"Time Set to Evening";
        }
    }
}