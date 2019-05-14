namespace FCS_Alterra_Refrigeration_Solutions.Configuration
{
    public static class Information
    {
        public static string LANGUAGEDIRECTORY;

        /// <summary>
        /// The mod name "ClassID" of the mod
        /// </summary>
        public static string ModName => "FCSARSolutions";

        /// <summary>
        /// The definition of the mod
        /// </summary>
        public static string ModDescription => "Alterra Refrigeration Solutions brings you the Sea Breeze FCS32. Refrigerate all your spoilables foods and keep them fresh!";

        /// <summary>
        /// The assets folder
        /// </summary>
        public static string ASSETSFOLDER { get; set; }

        /// <summary>
        /// The friendly name of the prefab
        /// </summary>
        public static string ModFriendly => "Alterra Refrigeration Solutions";

        /// <summary>
        /// The name of the asset bundle file
        /// </summary>
        public static string ModBundleName => "fcsarsolutionsbundle";
    }
}
