using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCSTechFabricator.Configuration;
using UnityEngine;

namespace FCSTechFabricator
{
    public interface IFcAssetBundlesService
    {
        AssetBundle GetAssetBundleByName(string bundleName);
        AssetBundle GetAssetBundleByName(string bundleName,string executingFolder);
        string GlobalBundleName { get;}
    }

    public class FcAssetBundlesService : IFcAssetBundlesService
    {
        public static IFcAssetBundlesService PublicAPI { get; } = new FcAssetBundlesService();

        private static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();
        
        public string GlobalBundleName => Mod.AssetBundleName;
        
        private FcAssetBundlesService()
        { }

        public AssetBundle GetAssetBundleByName(string bundleName)
        {
            if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
            {
                return preLoadedBundle;
            }

            var onDemandBundle = AssetBundle.LoadFromFile(Path.Combine(ExecutingFolder,"Assets", bundleName));

            if (onDemandBundle != null)
            {
                loadedAssetBundles.Add(bundleName, onDemandBundle);
                return onDemandBundle;
            }

            return null;
        }

        public AssetBundle GetAssetBundleByName(string bundleName, string executingFolder)
        {
            if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
            {
                return preLoadedBundle;
            }

            var onDemandBundle = AssetBundle.LoadFromFile(Path.Combine(executingFolder, "Assets", bundleName));

            if (onDemandBundle != null)
            {
                loadedAssetBundles.Add(bundleName, onDemandBundle);
                return onDemandBundle;
            }

            return null;
        }
    }
}
