using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Registration;
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
        Texture2D GetEncyclopediaTexture2D(string imageName, string globalBundle = "");
        GameObject GetPrefabByName(string item, string bundle, bool applyShaders = true);
        string GetBundleByModID(string modID);
    }

    public class FCSAssetBundlesService : IFcAssetBundlesService
    {

        public static IFcAssetBundlesService PublicAPI { get; } = new FCSAssetBundlesService();

        private static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly Dictionary<string, AssetBundle> loadedAssetBundles = new();
        private static readonly Dictionary<string, Sprite> loadedIcons = new();
        private static readonly Dictionary<string, Texture2D> loadedImages = new();
        private static readonly Dictionary<string, GameObject> loadedPrefabs = new();
        public string GlobalBundleName => Mod.AssetBundleName;
        public Texture2D GetEncyclopediaTexture2D(string imageName, string bundleName = "")
        {
            QuickLogger.Debug($"Trying to find {imageName} in bundle {bundleName}");
            AssetBundle bundle = null;

            if (string.IsNullOrWhiteSpace(imageName)) return null;

            if (string.IsNullOrWhiteSpace(bundleName))
            {
                bundleName = GlobalBundleName;
            }

            if (loadedImages.ContainsKey(imageName)) return loadedImages[imageName];

            QuickLogger.Debug($"Image {imageName} not already loaded. Trying to locate in bundle {bundleName}");

            if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
            {
                bundle =  preLoadedBundle;
            }

            if (bundle == null)
            {
                QuickLogger.Debug("Bundle returned null. Getting Image failed");
                return null;
            }

            var image = bundle.LoadAsset<Texture2D>(imageName);
            if (image == null)
            {
                QuickLogger.DebugError($"Failed to find image {imageName} in bundle {bundleName}");
                return null;
            }

            loadedImages.Add(imageName, image);
            return loadedImages[imageName];
        }

        public GameObject GetPrefabByName(string item, string bundleName, bool applyShaders = true)
        {
            if (loadedPrefabs.ContainsKey(item))
            {
                return loadedPrefabs[item];
            }

            var bundle = GetAssetBundleByName(bundleName);
            if (bundle == null) return null;
            AlterraHub.LoadAsset(item, bundle, out var go);

            if (applyShaders)
            {
                AlterraHub.ReplaceShadersV2(go);
            }

            loadedPrefabs.Add(item,go);
            return go;
        }

        public string GetBundleByModID(string modID)
        {
            var modPackData = FCSAlterraHubService.InternalAPI.GetRegisteredModData(modID);
            return modPackData != null ? modPackData.ModBundleName : string.Empty;
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
