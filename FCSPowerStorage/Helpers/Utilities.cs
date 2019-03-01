using System.IO;

namespace FCSPowerStorage.Helpers
{
    //TODO Change to Path.combine

    internal static class FilesHelper
    {
        /// <summary>
        /// Returns the save directory of the mod
        /// </summary>
        /// <returns></returns>
        public static string GetSaveFolderPath()
        {
            return Path.Combine(Path.Combine($@"{Directory.GetCurrentDirectory()}\SNAppData\SavedGames\", Utils.GetSavegameDir()), "FCSPowerStorage");
        }
    }
}
