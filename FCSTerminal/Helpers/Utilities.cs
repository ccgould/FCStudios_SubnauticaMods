using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTerminal.Logging;

namespace FCSTerminal.Helpers
{
    internal static class FilesHelper
    {
        public static string GetSaveFolderPath()
        {
            return Path.Combine(Path.Combine($@"{Directory.GetCurrentDirectory()}\SNAppData\SavedGames\", Utils.GetSavegameDir()), "FCSTerminal");
            
        }
    }
}
