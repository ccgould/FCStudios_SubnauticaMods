using FCSTechFabricator.Models;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace FCSTechFabricator
{
    public class Configuration
    {
        public static string IntraBaseTeleporterClassID => "AEIntraBaseTeleporter";
        public static string MiniFountainFilterClassID=> "MiniFountainFilter";
        public static string ExStorageClassID=> "ExStorageDepot";
        public static string SeaBreezeClassID=> "ARSSeaBreezeFCS32";
        public static string AIMarineMonitorClassID=> "AIMarineMonitor";
        public static string AIJetStreamT242ClassID => "AIJetStreamT242";
        public static string PowerStorageKitClassID => "FCSPowerStorage";
        public static string DeepDrillerClassID => "FCS_DeepDriller";
        public static string SeaCookerClassID => "SeaCooker";
        public static string MiniMedBayClassID => "AMMiniMedBay";
        public static string PowerCellSocketClassID => "AIPowerCellSocket";
        [JsonProperty] internal List<ConfigItem> Blueprints { get; set; } = new List<ConfigItem>();


        public List<IngredientItem> GetData(string classId)
        {
            foreach (ConfigItem configItem in Blueprints)
            {
                if (configItem.ItemName.Equals(classId))
                {
                    return configItem.IngredientItem;
                }
            }

            return null;
        }
    }

    internal class ConfigItem
    {
        [JsonProperty] internal string ItemName { get; set; }
        [JsonProperty] internal List<IngredientItem> IngredientItem { get; set; } = new List<IngredientItem>();
    }
}
