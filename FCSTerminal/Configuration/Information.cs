using System;
using System.IO;

namespace FCSTerminal.Configuration
{
    public static class Information
    {
        /// <summary>
        /// THis is the ClassID of the Server Rack
        /// </summary>
        public static string ModRackName => "FCSTerminalServerRack";

        /// <summary>
        /// THis is the description of the server rack
        /// </summary>
        public static string ModRackDescription { get; } =
            "This server rack stores items within hard drives for use with the FCS Terminal System";

        /// <summary>
        /// The is the friendly name for the server rack
        /// </summary>
        public static string ModRackFriendly { get; set; } = "FCS Terminal Server Rack";

        /// <summary>
        /// This is the asset folder location of the mod
        /// </summary>
        public static string ASSETSFOLDER { get; set; }

        public static string QMODFOLDER { get; } = Path.Combine(Environment.CurrentDirectory, "QMods");

        /// <summary>
        /// This is the text on hover for the server rack
        /// </summary>
        public static string ServerRackHandText { get; set; } = "Click to open server rack";

        /// <summary>
        /// Name of the mod
        /// </summary>
        public static string ModName { get; } = "FCSTerminal";

        /// <summary>
        /// Description for the server prefab
        /// </summary>
        public static string ModServerDescription { get; } = "A server for the FCS Terminal's Server Rack";

        /// <summary>
        /// The friendly  name for the server prefab
        /// </summary>
        public static string ModServerFriendly { get; } = "FCS Server";

        /// <summary>
        /// The Name of the server prefab
        /// </summary>
        public static string ModServerName { get; } = "FCSServer";
    }
}
