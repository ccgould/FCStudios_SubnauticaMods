using FCSTechFabricator.Models;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;

namespace FCSTechFabricator
{
    public class Configuration
    {


        #region Mod Class IDS
        public static string QuantumTeleporterClassID => "QuantumTeleporter";
        public static string MiniFountainFilterClassID => "MiniFountainFilter";
        public static string ExStorageClassID => "ExStorageDepot";
        public static string SeaBreezeClassID => "ARSSeaBreezeFCS32";
        public static string AIMarineMonitorClassID => "AIMarineMonitor";
        public static string AIJetStreamT242ClassID => "AIJetStreamT242";
        public static string PowerStorageClassID => "FCSPowerStorage";
        public static string DeepDrillerClassID => "FCS_DeepDriller";
        public static string SeaCookerClassID => "SeaCooker";
        public static string MiniMedBayClassID => "AMMiniMedBay";
        public static string PowerCellSocketClassID => "AIPowerCellSocket";
        public static string ShippingClassID => "FCSAlterraShipping";
        #endregion

        #region Kit Class ID
        public static string QuantumTeleporterKitClassID => "QuantumTeleporterKit_AE";
        public static string PowerCellSocketKitClassID => "PowerCellSocket_AIS";
        public static string AMMiniMedBayKitClassID => "AMMiniMedBayKit_AMS";
        public static string FocusAttachmentKitClassID => "FocusAttachment_DD";
        public static string SolarAttachmentKitClassID => "SolarAttachment_DD";
        public static string BatteryAttachmentKitClassID => "BatteryAttachment_DD";
        public static string SeaBreezeKitClassID => "SeaBreezeKit_SB";
        public static string ExStorageKitClassID => "ExStorageKit_ASTS";
        public static string MiniFountainFilterKitClassID => "MiniFountainFilterKit_MFF";
        public static string MarineMonitorKitClassID => "MarineMonitorKit_MT";
        public static string JetStreamT242KitClassID => "JetStreamT242Kit_MT";
        public static string SeaCookerBuildableKitClassID => "SeaCookerBuildableKit_SC";
        public static string DeepDrillerKitClassID => "DeepDrillerKit_DD";
        public static string PowerStorageKitClassID => "PowerStorageKit_PS";
        public static string ShippingKitID => "ASShippingKit_AS";
        #endregion

        public static string DrillerMK1ModuleClassID => "DrillerMK1_DD";
        public static string DrillerMK2ModuleClassID => "DrillerMK2_DD";
        public static string DrillerMK3ModuleClassID => "DrillerMK3_DD";

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
