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
        public static AssetBundle Asset(string modDirName, string modBundleName)
        {
            if (modDirName.Equals(string.Empty) && modBundleName.Equals(string.Empty))
            {
                throw new ArgumentException($"Both {nameof(modDirName)} and {nameof(modBundleName)} are empty");
            }

            if (modDirName.Equals(string.Empty) || modBundleName.Equals(string.Empty))
            {
                var result = modDirName.Equals(string.Empty) ? nameof(modDirName) : nameof(modBundleName);
                throw new ArgumentException($"{result} is empty");
            }

            return AssetBundle.LoadFromFile(Path.Combine(Path.Combine(Environment.CurrentDirectory, "QMods"), Path.Combine(modDirName, Path.Combine("Assets", modBundleName))));
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
