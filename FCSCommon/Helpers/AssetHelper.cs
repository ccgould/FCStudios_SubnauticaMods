using System;
using System.IO;
using UnityEngine;

namespace FCSCommon.Helpers
{
    /// <summary>
    /// A  helper class that deals with the AssetBudle
    /// </summary>
    public static class AssetHelper
    {
        /// <summary>
        /// The AssetBundle for the mod
        /// </summary>
        public static AssetBundle Asset(string modName, string modBundleName)
        {
            if (modName.Equals(string.Empty) && modBundleName.Equals(string.Empty))
            {
                throw new ArgumentException($"Both {nameof(modName)} and {nameof(modBundleName)} are empty");
            }

            if (modName.Equals(string.Empty) || modBundleName.Equals(string.Empty))
            {
                var result = modName.Equals(string.Empty) ? nameof(modName) : nameof(modBundleName);
                throw new ArgumentException($"{result} is empty");
            }

            return AssetBundle.LoadFromFile(Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), Path.Combine(modName, Path.Combine("Assets", modBundleName))));
        }

        public static string GetModDirectory(string modName)
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), modName);
        }

        public static string GetAssetFolder(string modName)
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), Path.Combine(modName, "Assets"));
        }

        public static string GetConfigFolder(string modName)
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), Path.Combine(modName, "Configuration"));
        }

        public static AssetBundle Asset(string bundleLocation)
        {
            if (string.IsNullOrEmpty(bundleLocation))
            {
                throw new ArgumentException("No bundle location was provided");
            }

            var myLoadedAssetBundle = AssetBundle.LoadFromFile(bundleLocation);

            if (myLoadedAssetBundle == null)
            {
                throw new ArgumentException("Bundle Returned Null");
            }

            return myLoadedAssetBundle;
        }
    }
}
