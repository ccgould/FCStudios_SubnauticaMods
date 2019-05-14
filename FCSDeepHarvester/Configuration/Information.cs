using System;
using System.IO;

namespace FCSDeepHarvester.Configuration
{
    public static class Information
    {
        /// <summary>
        /// THis is the ClassID of the Server Rack
        /// </summary>
        public static string ModRackName => "FCSDeepHarvester";

        /// <summary>
        /// THis is the description of the prefab
        /// </summary>
        public static string ModDescription { get; } =
            "Stores and grows plants and collects droppings";

        /// <summary>
        /// The is the friendly name for the prefab
        /// </summary>
        public static string ModFriendly { get; set; } = "FCS Deep Harvester";

        /// <summary>
        /// This is the asset folder location of the mod
        /// </summary>
        public static string ASSETSFOLDER { get; set; }

        /// <summary>
        /// The location of the QMods directory
        /// </summary>
        public static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");

        /// <summary>
        /// This is the text on hover 
        /// </summary>
        public static string DeepStorageHandHover { get; set; } = "Click to open Deep Harvester's Inventory";

        /// <summary>
        /// Name of the mod
        /// </summary>
        public static string ModName { get; } = "FCSDeepHarvester";
    }
}
