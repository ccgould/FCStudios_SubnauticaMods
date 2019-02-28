using System.IO;

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
