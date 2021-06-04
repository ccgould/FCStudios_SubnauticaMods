using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using JetBrains.Annotations;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.API
{
    public interface IFcAssetBundlesService
    {
        AssetBundle GetAssetBundleByName(string bundleName);
        Sprite GetIconByName(string iconName);
        AssetBundle GetAssetBundleByName(string bundleName,string executingFolder);
        string GlobalBundleName { get;}
        Atlas.Sprite GetEncyclopediaSprite(string imageName, string globalBundle);
        GameObject GetPrefabByName(string item, string bundle);
    }

    public class FCSAssetBundlesService : IFcAssetBundlesService
    {

        public static IFcAssetBundlesService PublicAPI { get; } = new FCSAssetBundlesService();

        private static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<string, Sprite> loadedIcons = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Atlas.Sprite> loadedImages = new Dictionary<string, Atlas.Sprite>();
        public string GlobalBundleName => Mod.AssetBundleName;
        public Atlas.Sprite GetEncyclopediaSprite(string imageName, string bundleName)
        {
            AssetBundle bundle = null;

            if (loadedImages.ContainsKey(imageName)) return loadedImages[imageName];

            if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
            {
                bundle =  preLoadedBundle;
            }

            if (bundle == null) return SpriteManager.defaultSprite;

            var imageTexture2D = bundle.LoadAsset<Texture2D>(imageName);
            if (imageTexture2D == null)
            {
                QuickLogger.DebugError($"Failed to find sprite {imageName}");
                return SpriteManager.defaultSprite;
            }

            loadedImages.Add(imageName, ImageUtils.LoadSpriteFromTexture(imageTexture2D));
            return loadedImages[imageName];
        }

        public GameObject GetPrefabByName(string item, string bundleName)
        {
            var bundle = GetAssetBundleByName(bundleName);
            if (bundle == null) return null;
            Buildables.AlterraHub.LoadAsset(item,bundle,out var go);
            return go;
        }

        private FCSAssetBundlesService()
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
            
            Texture2D texture = ImageUtils.LoadTextureFromFile(Path.Combine(Mod.GetAssetPath(), $"{iconName}.png"));

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
