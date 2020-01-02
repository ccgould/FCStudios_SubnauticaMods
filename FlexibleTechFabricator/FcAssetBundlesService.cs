namespace FlexibleTechFabricator
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using UnityEngine;

    public interface IFcAssetBundlesService
    {
        AssetBundle GetAssetBundleByName(string bundleName);
    }

    public class FcAssetBundlesService : IFcAssetBundlesService
    {
        public static IFcAssetBundlesService PublicAPI { get; } = new FcAssetBundlesService();

        private static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

        private FcAssetBundlesService()
        { }

        public AssetBundle GetAssetBundleByName(string bundleName)
        {
            if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
            {
                return preLoadedBundle;
            }

            var onDemandBundle = AssetBundle.LoadFromFile(Path.Combine(ExecutingFolder, bundleName));

            if (onDemandBundle != null)
            {
                loadedAssetBundles.Add(bundleName, onDemandBundle);
                return onDemandBundle;
            }

            return null;
        }
    }
}
