using System;
using UnityEngine;

namespace FCSTerminal.Helpers
{
    /// <summary>
    /// A  helper class that deals with the AssetBudle
    /// </summary>
    public static class AssetHelper
    {
        /// <summary>
        /// The AssetBundle for the modd
        /// </summary>
        //public static AssetBundle Asset = AssetBundle.LoadFromFile($"{Environment.CurrentDirectory}/QMods/FCSTerminal/decorationassets.assets");
        public static AssetBundle Asset = AssetBundle.LoadFromFile($"{Environment.CurrentDirectory}/QMods/FCSTerminal/fcs_terminal-mod");
    }
}
