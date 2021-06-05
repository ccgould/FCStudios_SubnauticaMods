using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_AlterraHub.Configuration
{
    internal class DebugCommands
    {
        private static TeleportScreenFXController _teleportEffects;

        [ConsoleCommand("givecredit")]
        public static string GiveCreditCommand(double amount)
        {
            CardSystem.main.AddFinances((decimal) amount);
            return $"Parameters: {amount}";
        }
        
        [ConsoleCommand("TeleportEffects")]
        public static string TeleportPlayerCommand(bool activate)
        {
            if (_teleportEffects == null)
            {
                _teleportEffects = Player.main.camRoot.mainCam.GetComponentInChildren<TeleportScreenFXController>();
            }
            if (activate)
            {
                _teleportEffects.StartTeleport();
            }
            else
            {
                _teleportEffects.StopTeleport();
            }
            return $"Parameters: {activate}";
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
            OreConsumer.OreProcessingTime = OreConsumer.OreProcessingTime >= 90f ? 1f : 90f;
            QuickLogger.Message($"Changed Processing speed",true);
        }

        [ConsoleCommand("UnlockFCSPDA")]
        public static void UnlockFCSPDA()
        {
            Mod.GamePlaySettings.IsPDAUnlocked = true;
            FCSPDAController.ForceOpen();
            QuickLogger.Message($"FCS PDA Unlocked", true);
        }

        [ConsoleCommand("CreateDummyAccount")]
        public static string CreateDummyAccount(int amount = 0)
        {
            if(!CardSystem.main.HasBeenRegistered())
            {
                CardSystem.main.CreateUserAccount("Ryley Robinson", "RyleyRobinson4546B", "planet4546B", "4546", amount);
                return $"Created account RyleyRobinson4546B with amount {amount} balance.";
            }
            QuickLogger.Message($"Account already exist {CardSystem.main.GetUserName()}", true);
            return $"Parameters: {nameof(amount)} Amount to put in account";
        }

        [ConsoleCommand("WarpStation")]
        public static void WarpStation()
        {
            Player.main.SetPosition(new Vector3(-809.10f, -241.30f, -387.70f));
        }
    }
}