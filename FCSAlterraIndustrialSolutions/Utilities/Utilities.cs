using FCSAlterraIndustrialSolutions.Configuration;
using System.IO;

namespace FCSAlterraIndustrialSolutions.Utilities
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

        public static string GenerateSaveFileString(string id)
        {
           return Path.Combine(Information.GetSaveFileDirectory(), $"{Information.ModName}_" + id + ".json");
        }
    }
}
