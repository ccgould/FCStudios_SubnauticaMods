using FCS_AlterraHub.Systems;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Commands;
using UnityEngine;

namespace FCS_AlterraHub.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("givecredit")]
        public static string GiveCreditCommand(string userName, float amount, bool myBool = false)
        {
            if (CardSystem.main.IsAccountNameValid(userName))
            {
                CardSystem.main.AddFinances(amount);
            }
            else
            {
                QuickLogger.Info($"No account found with the username: {userName}",true);
            }

            return $"Parameters: {userName} {amount}";
        }
    }
}
