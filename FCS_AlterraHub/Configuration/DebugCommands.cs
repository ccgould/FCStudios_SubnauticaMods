using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.AlterraHub;
using FCS_AlterraHub.Systems;
using FCSCommon.Extensions;
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

        [ConsoleCommand("helpMenu")]
        public static void HelpMenuCommand()
        {
            var hubController = GameObject.FindObjectOfType<AlterraHubController>();
            
            if(hubController == null) return;
            if (hubController.HUD.IsOpen)
            {
                hubController.HUD.Close();
            }
            else
            {
                hubController.HUD.Open();
            }
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
            BaseManager.GlobalNotifyByID(Mod.AlterraHubTabID, "ActivateGoal");
            BaseManager.ActivateGoalTechType = techType.ToTechType();
            return $"Parameters: {techType}";
        }

        [ConsoleCommand("CompleteMission")]
        public static void CompleteMission()
        {
            QPatch.MissionManagerGM.CompleteCurrentMission();
        }

    }
}