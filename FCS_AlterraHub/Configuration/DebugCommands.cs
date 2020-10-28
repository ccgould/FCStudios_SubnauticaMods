using FCS_AlterraHub.Systems;
using SMLHelper.V2.Commands;

namespace FCS_AlterraHub.Configuration
{
    internal class DebugCommands
    {
        [ConsoleCommand("givecredit")]
        public static string GiveCreditCommand(float amount)
        {
            CardSystem.main.AddFinances(amount);

            return $"Parameters: {amount}";
        }
    }
}
