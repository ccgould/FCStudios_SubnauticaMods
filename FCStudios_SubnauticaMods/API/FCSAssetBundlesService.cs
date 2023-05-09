using FCS_AlterraHub.Core.Services;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;


namespace FCS_AlterraHub.API;

public interface IFcAssetBundlesInternalService
{
    Dictionary<string, AssetBundle> loadedAssetBundles { get; }
    Dictionary<string, Texture2D> loadedImages { get; }
    Dictionary<string, GameObject> loadedPrefabs { get; }
}

public interface IFcAssetBundlesPublicService
{
    /// <summary>
    /// The name of the AlterraHub global bundle
    /// </summary>
    string GlobalBundleName { get; }

    /// <summary>
    /// Creates a sprite from an image inside the preloade bundles.
    /// </summary>
    /// <param name="iconName">Name of image</param>
    /// <param name="modPackName">Name of mod pack</param>
    /// <returns>Sprite image.</returns>
    Sprite GetIconByName(string iconName, string modPackName = PluginInfo.PLUGIN_NAME);


    /// <summary>
    /// Creates a sprite from an image inside the preloade bundles.
    /// </summary>
    /// <param name="iconName">Name of image</param>
    /// <param name="modPackName">Name of mod pack</param>
    /// <returns>Sprite image.</returns>
    Sprite GetIconByNameFromFile(string iconName, string bundleName);


    /// <summary>
    /// Gets an <see cref="AssetBundle"/> with the provided name/>
    /// </summary>
    /// <param name="bundleName">Name of bundle.</param>
    /// <returns><see cref="AssetBundle"/></returns>
    AssetBundle GetAssetBundleByName(string bundleName);

    /// <summary>
    /// Gets an <see cref="AssetBundle"/> with the provided name and directory/>
    /// </summary>
    /// <param name="bundleName">Name of bundle.</param>
    /// <param name="executingFolder">Location of t ebundle on the system.</param>
    /// <returns><see cref="AssetBundle"/></returns>
    AssetBundle GetAssetBundleByName(string bundleName, string executingFolder);

    /// <summary>
    /// Looks thru the bundle for the prefab and returns it.
    /// </summary>
    /// <param name="prefabName">Name of the prefab to look for</param>
    /// <param name="bundle">The bundle to search in </param>
    /// <param name="applyShaders">Apply the shaders for the FCS materials</param>
    /// <returns></returns>
    GameObject GetPrefabByName(string prefabName, string bundle,string modPath, bool applyShaders = true);
    GameObject GetLocalPrefab(string prefabName, bool applyShaders = false);

    /// <summary>
    /// Finds a <see cref="Texture2D"/> in the supplied bundle.
    /// </summary>
    /// <param name="imageName">Name of the inmage to find</param>
    /// <param name="bundleName">NAme of the bundle to look in.</param>
    /// <returns></returns>
    Texture2D GetTextureByName(string imageName, string bundleName);
}


public class FCSAssetBundlesService : IFcAssetBundlesPublicService, IFcAssetBundlesInternalService
{

    private static readonly FCSAssetBundlesService singleton = new();

    public static IFcAssetBundlesPublicService PublicAPI { get; } = singleton;

    public static IFcAssetBundlesPublicService InternalAPI { get; } = singleton;

    public string GlobalBundleName => Plugin.ModSettings.AssetBundleName;

    public Dictionary<string, AssetBundle> loadedAssetBundles { get; } =  new();


    public Dictionary<string, Texture2D> loadedImages { get; } = new();

    public Dictionary<string, GameObject> loadedPrefabs { get; } = new();

    private FCSAssetBundlesService()
    {
    }

    public GameObject GetPrefabByName(string prefabName, string bundleName,string modPath, bool applyShaders = true)
    {

        QuickLogger.Debug($"PrefabName: {prefabName} | BundleName: {bundleName} | ModPath: {modPath} | Apply Shaders: {applyShaders}");

        if (ModPrefabService.IsPrefabLoaded(prefabName))
        {
            return ModPrefabService.GetPrefab(prefabName);
        }

        var bundle = GetAssetBundleByName(bundleName, modPath);

        if (bundle == null) return null;
        
        ModPrefabService.LoadAsset(prefabName, bundle, out var go, applyShaders);

        if(go is null)
        {
            QuickLogger.Error<FCSAssetBundlesService>($"[GetPrefabByName] Failed to find Prefab {prefabName}");
        }

        return go;
    }

    public AssetBundle GetAssetBundleByName(string bundleName)
    {
        if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
        {
            return preLoadedBundle;
        }

        var onDemandBundle = AssetBundleHelper.Asset(bundleName);

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

        var onDemandBundle = AssetBundleHelper.Asset(executingFolder, bundleName);

        if (onDemandBundle != null)
        {
            loadedAssetBundles.Add(bundleName, onDemandBundle);
            return onDemandBundle;
        }
        return null;
    }

    public Sprite GetIconByName(string iconName,string modPackName = PluginInfo.PLUGIN_NAME)
    {
        return ModPrefabService.GetIconByName(iconName, modPackName);
    }

    public GameObject GetLocalPrefab(string prefabName, bool applyShaders = false)
    {
        return GetPrefabByName(prefabName, GlobalBundleName,FileSystemHelper.ModDirLocation,applyShaders);
    }

    public Sprite GetIconByNameFromFile(string iconName, string bundleName)
    {
        throw new System.NotImplementedException();
    }

    public Texture2D GetTextureByName(string imageName, string bundleName)
    {
        if (loadedImages.ContainsKey(imageName)) return loadedImages[imageName];

        AssetBundle bundle = null;

        QuickLogger.Debug($"Image {imageName} not already loaded. Trying to locate in bundle {bundleName}");

        if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
        {
            bundle = preLoadedBundle;
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
}
