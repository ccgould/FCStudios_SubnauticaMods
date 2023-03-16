using FCSCommon.Utilities;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FCSCommon.Helpers
{
    /// <summary>
    /// A  helper class that deals with the AssetBundle
    /// </summary>
    public static class AssetBundleHelper
    {
        public static string ModDirLocation => FileSystemHelper.ModDirLocation;
       
        /// <summary>
        /// The AssetBundle for the mod
        /// </summary>
        [Obsolete("Use Asset(string modBundle) instead to prevent hardcoded paths")]
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

            var path = Path.Combine(modDirName, Path.Combine(modDirName, Path.Combine("Assets", modBundleName)));

            QuickLogger.Info($"Attempting to load bundle {path}");

            return AssetBundle.LoadFromFile(path);
        }

        public static AssetBundle Asset(string modBundleName)
        {
            QuickLogger.Info($"Attempting to load {modBundleName} from file.");

            if (string.IsNullOrWhiteSpace(ModDirLocation) && string.IsNullOrWhiteSpace(modBundleName))
            {
                throw new ArgumentException($"Both {nameof(ModDirLocation)} and {nameof(modBundleName)} are empty");
            }

            if (string.IsNullOrWhiteSpace(ModDirLocation) || string.IsNullOrWhiteSpace(modBundleName))
            {
                var result = string.IsNullOrWhiteSpace(ModDirLocation) ? nameof(ModDirLocation) : nameof(modBundleName);
                throw new ArgumentException($"{result} is empty");
            }

            var output = AssetBundle.LoadFromFile(Path.Combine(ModDirLocation, "Assets", modBundleName));

            if(output == null)
            {
                QuickLogger.Error("Failed to load Bundle from file");
                return null;
            }

            QuickLogger.Info($"Loaded {modBundleName} from file.");
            return output;
        }

        internal static T LoadAsset<T>(AssetBundle bundle, string textureName) where T : UnityEngine.Object
        {
            return bundle.LoadAsset<T>(textureName);
        }
    }
}
