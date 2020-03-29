using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator
{
    public interface IFcAssetBundlesService
    {
        AssetBundle GetAssetBundleByName(string bundleName);
        Sprite GetIconByName(string iconName);
        AssetBundle GetAssetBundleByName(string bundleName,string executingFolder);
        string GlobalBundleName { get;}
    }

    public class FcAssetBundlesService : IFcAssetBundlesService
    {

        public static IFcAssetBundlesService PublicAPI { get; } = new FcAssetBundlesService();

        private static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<string, Sprite> loadedIcons = new Dictionary<string, Sprite>();

        public string GlobalBundleName => Mod.AssetBundleName;

        private FcAssetBundlesService()
        {
        }

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

        public Sprite GetIconByName(string iconName)
        {
            if (loadedIcons.TryGetValue(iconName, out Sprite preLoadedBundle))
            {
                return preLoadedBundle;
            }

            
            Texture2D texture = ImageUtils.LoadTextureFromFile((Path.Combine(Mod.GetAssetPath(), $"{iconName}.png")));

            if (texture != null)
            {
                var icon = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                loadedIcons.Add(iconName, icon);
                return icon;
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
