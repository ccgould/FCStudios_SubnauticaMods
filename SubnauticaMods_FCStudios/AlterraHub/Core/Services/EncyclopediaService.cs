using FCSCommon.Utilities;
using static AssetBundleManager;
using UnityEngine;
using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using System.Collections.Generic;

namespace FCS_AlterraHub.Core.Services
{
    /// <summary>
    /// This class has all the methods and properties for maintaining the PDA Encyclopedia
    /// </summary>
    internal static class EncyclopediaService
    {
        internal static List<Dictionary<string, List<EncyclopediaEntryData>>> EncyclopediaEntries { get; set; }
        //public Texture2D GetEncyclopediaTexture2D(string imageName, string bundleName = "")
        //{
        //    QuickLogger.Debug($"Trying to find {imageName} in bundle {bundleName}");
        //    AssetBundle bundle = null;

        //    if (string.IsNullOrWhiteSpace(imageName)) return null;

        //    if (string.IsNullOrWhiteSpace(bundleName))
        //    {
        //        bundleName =  FCSAssetBundlesService.PublicAPI.GlobalBundleName;
        //    }

        //    FCSAssetBundlesService.InternalAPI.LoadedImages
        //    if (loadedImages.ContainsKey(imageName)) return loadedImages[imageName];

        //    QuickLogger.Debug($"Image {imageName} not already loaded. Trying to locate in bundle {bundleName}");

        //    if (loadedAssetBundles.TryGetValue(bundleName, out AssetBundle preLoadedBundle))
        //    {
        //        bundle = preLoadedBundle;
        //    }

        //    if (bundle == null)
        //    {
        //        QuickLogger.Debug("Bundle returned null. Getting Image failed");
        //        return null;
        //    }

        //    var image = bundle.LoadAsset<Texture2D>(imageName);
        //    if (image == null)
        //    {
        //        QuickLogger.DebugError($"Failed to find image {imageName} in bundle {bundleName}");
        //        return null;
        //    }

        //    loadedImages.Add(imageName, image);
        //    return loadedImages[imageName];
        //}
    }
}
