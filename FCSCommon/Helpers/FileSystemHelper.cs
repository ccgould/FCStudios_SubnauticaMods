using FCS_AlterraHub.Models.Structs;
using FCSCommon.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace FCSCommon.Helpers
{
    internal static class FileSystemHelper
    {
        public static string ModDirLocation { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        public static string ModAssetDirLocation { get; } = Path.Combine(ModDirLocation,"Assets");

        public static string ModSettings { get; } = Path.Combine(ModAssetDirLocation,"ModSettings.bin");

        internal static Dictionary<string, FCSModItemSettings> DeseriaizeSettings(string path)
        {
            Dictionary<string, FCSModItemSettings> obj = null;

            var modSettings = Path.Combine(path,"Assets", "ModSettings.bin");

            if (File.Exists(modSettings))
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(modSettings))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    obj = (Dictionary<string, FCSModItemSettings>)serializer.Deserialize(file, typeof(Dictionary<string, FCSModItemSettings>));
                }
            }
            else
            {
                QuickLogger.Error("Mod Settings were not found!");
            }

            return obj;
        }
    }
}
