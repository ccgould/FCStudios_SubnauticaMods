using System.Collections.Generic;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Systems
{
    internal class MessageBoxHandler
    {
        public static MessageBoxHandler main = new MessageBoxHandler();

        internal void Show(string message)
        {
            QuickLogger.Debug($"Temp Message: [{message}]",true);
        }
    }
}
