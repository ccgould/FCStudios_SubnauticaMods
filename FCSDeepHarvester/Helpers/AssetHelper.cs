using System;
using System.IO;
using UnityEngine;

namespace FCSDeepHarvester.Helpers
{
    /// <summary>
    /// A  helper class that deals with the AssetBudle
    /// </summary>
    public static class AssetHelper
    {
        /// <summary>
        /// The AssetBundle for the mod
        /// </summary>
        //public static AssetBundle Asset = AssetBundle.LoadFromFile($"{Environment.CurrentDirectory}/QMods/FCSTerminal/decorationassets.assets");
        public static AssetBundle Asset = AssetBundle.LoadFromFile(Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), Path.Combine("FCSDeepHarvester", "fcsdeepharvesterbundle")));
    }
}
