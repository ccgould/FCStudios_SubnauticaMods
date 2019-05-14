using FCSTechWorkBench.Configuration;
using System.IO;

namespace FCSTechWorkBench.Utilities
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
            return Path.Combine(Path.Combine($@"{Directory.GetCurrentDirectory()}\SNAppData\SavedGames\", Utils.GetSavegameDir()), Information.ModName);
        }
    }
}
