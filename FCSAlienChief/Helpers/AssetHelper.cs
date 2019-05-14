using System;
using UnityEngine;

namespace FCSAlienChief.Data
{
    /// <summary>
    /// A  helper class that deals with the AssetBudle
    /// </summary>
    public static class AssetHelper
    {
        /// <summary>
        /// The AssetBundle for the mod
        /// </summary>
        public static AssetBundle Asset = AssetBundle.LoadFromFile($"{Environment.CurrentDirectory}/QMods/FCSAlienChief/fcsalienchief-mod");
    }
}
