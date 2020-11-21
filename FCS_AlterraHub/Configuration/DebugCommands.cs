using FCS_AlterraHub.Mono.AlterraHub;
using FCS_AlterraHub.Systems;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_AlterraHub.Configuration
{
    internal class DebugCommands
    {
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
    }
}