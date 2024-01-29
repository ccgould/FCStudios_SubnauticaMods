using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FCS_AlterraHub.Core.Services;

public static class ModPrefabService
{
    private static Dictionary<string, Material> _v2Materials = new();
    public static Dictionary<string, Sprite> loadedIcons { get; } = new();

    private static bool _v2MaterialsLoaded;

    /// <summary>
    /// Material for fcsBaseColor_Ex
    /// </summary>
    public const string BasePrimaryCol = "fcsBaseColor_Ex";
    /// <summary>
    /// Material for fcsMainLights
    /// </summary>
    public const string BaseSecondaryCol = "fcsMainLights";

    /// <summary>
    /// Material for fcsFloor01_Ex
    /// </summary>
    public const string Floor01 = "fcsFloor01_Ex";
    public const string Floor01Detail = "fcs01_Floor1_D";
    public const string Floor01Normal = "fcs01_Floor01_N";
    public const string Floor01Emission = "fcs01_Floor01_E";


    /// <summary>
    /// Material for fcsFloor02_Ex
    /// </summary>
    public const string Floor02 = "fcsFloor02_Ex";
    public const string Floor02Detail = "fcs01_Floor2_D";
    public const string Floor02Normal = "fcs01_Floor02_N";
    public const string Floor02Spec = "fcs01_Floor02_S";
    public const string Floor02Emission = "fcs01_Floor02_E";


    public const string TBaseDetail = "fcsCore_Tex_Trim_Base";
    public const string TBaseNormal = "fcsCore_Tex_Trim_N";
    public const string TBaseSpec = "fcsCore_Tex_Trim_S";
    public const string TBaseEmission = "fcsCore_Tex_Trim_LM";
    public const string TBaseID = "fcsCore_Tex_Trim_colormask";


    private static Dictionary<string, GameObject> loadedPrefabs = new();

    internal static GameObject GetPrefab(string prefabName)
    {
        if (loadedPrefabs.ContainsKey(prefabName))
        {
            return loadedPrefabs[prefabName];
        }

        QuickLogger.Error($"Failed to get Prefab: {prefabName}");
        return null;
    }

    internal static bool IsPrefabLoaded(string prefabName)
    {
        return loadedPrefabs.ContainsKey(prefabName);
    }

    internal static bool LoadAsset(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
    {
        //We have found the asset bundle and now we are going to continue by looking for the model.
        GameObject prefab = AssetBundleHelper.LoadAsset<GameObject>(assetBundle, prefabName);

        //If the prefab isn't null lets add the shader to the materials
        if (prefab != null)
        {
            if (applyShaders)
            {
                //Lets apply the material shader
                MaterialUtils.ApplySNShaders(prefab, 4, 1, 1,new NoMaterial());

                //ReplaceShadersV2(prefab);
            }

            go = prefab;
            loadedPrefabs.Add(prefabName, go);
            QuickLogger.Debug($"{prefabName} Prefab Found!");
            return true;
        }

        QuickLogger.Error($"{prefabName} Prefab Not Found!");

        go = null;
        return false;
    }

    public static void LoadV2Materials()
    {
        //QuickLogger.Info($"[LoadV2Materials] processing");

        if (_v2MaterialsLoaded) return;

        if(MaterialUtils.IsReady)
        {
            QuickLogger.Debug("Shader Is Ready");
        }
        else
        {
            QuickLogger.Error("Shader Is NOT Ready");
        }

        var shader = Nautilus.Utility.MaterialUtils.Shaders.MarmosetUBER; //MaterialHelpers._Renderer.materials[0].shader;

        var globalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName);

        if (globalBundle is null)
        {
            QuickLogger.Error("Global Bundle is null");
            return;
        }

        #region BasePrimaryCol
        Material basePrimaryCol = AssetBundleHelper.LoadAsset<Material>(globalBundle, BasePrimaryCol);
        QuickLogger.Info("madeit");
        basePrimaryCol.shader = shader;
        QuickLogger.Info("madeit2");

        MaterialHelpers.CreateV2ColorMaskShader(basePrimaryCol, TBaseID, Color.white, Color.white, Color.white, globalBundle);
        MaterialHelpers.CreateV2NormalMaterial(basePrimaryCol, TBaseNormal, globalBundle, shader);
        MaterialHelpers.CreateV2Specular(basePrimaryCol, TBaseSpec, 1, 3, globalBundle, shader);
        MaterialHelpers.CreateV2EmissionMaterial(basePrimaryCol, TBaseEmission, globalBundle, shader, Color.white);
        _v2Materials.Add(BasePrimaryCol, basePrimaryCol);
        #endregion

        #region BaseSecondaryCol
        Material baseSecondaryCol = AssetBundleHelper.LoadAsset<Material>(globalBundle, BaseSecondaryCol);
        baseSecondaryCol.shader = shader;
        MaterialHelpers.CreateV2ColorMaskShader(baseSecondaryCol, TBaseID, Color.white, Color.white, Color.white, globalBundle);
        MaterialHelpers.CreateV2NormalMaterial(baseSecondaryCol, TBaseNormal, globalBundle, shader);
        MaterialHelpers.CreateV2Specular(baseSecondaryCol, TBaseSpec, 1, 3, globalBundle, shader);
        MaterialHelpers.CreateV2EmissionMaterial(baseSecondaryCol, TBaseEmission, globalBundle, shader, Color.white);
        _v2Materials.Add(BaseSecondaryCol, baseSecondaryCol);
        #endregion

        #region Floor01
        Material floor01 = AssetBundleHelper.LoadAsset<Material>(globalBundle, Floor01);
        floor01.shader = shader;
        MaterialHelpers.CreateV2NormalMaterial(floor01, Floor01Normal, globalBundle, shader);
        MaterialHelpers.CreateV2EmissionMaterial(floor01, Floor01Emission, globalBundle, shader, Color.white);
        _v2Materials.Add(Floor01, floor01);
        #endregion

        _v2MaterialsLoaded = true;
    }

    public static void ReplaceShadersV2(GameObject prefab)
    {
        ReplaceShadersV2(prefab, BasePrimaryCol);
        ReplaceShadersV2(prefab, BaseSecondaryCol);
        ReplaceShadersV2(prefab, Floor01);
    }

    private static void ReplaceShadersV2(GameObject prefab, string materialName)
    {

        if (prefab is null)
        {
            QuickLogger.Error("Prefab was null when trying to replace shaders V2");
            return;
        }

        LoadV2Materials();

        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

        //QuickLogger.Debug($"Replacing Shaders on Object: {prefab.gameObject.name} | Render Count: {renderers.Length}");

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null) continue;
            //QuickLogger.Debug($"Processing: {renderer.name} Materials Count: {renderer.materials.Length}");

            for (var index = 0; index < renderer.materials.Length; index++)
            {
                Material material = renderer.materials[index];

                ///QuickLogger.Debug($"Trying to Replacing Material: {material.name} {material.shader.name}");

                if (material.name.RemoveInstance().Equals(materialName, StringComparison.OrdinalIgnoreCase))
                {
                    //QuickLogger.Debug($"Replacing Material: {material.name} {material.shader.name}");

                    renderer.materials[index] = _v2Materials[materialName];

                    //QuickLogger.Debug($"Done Replacing Material: {renderer.materials[index].name} {renderer.materials[index].shader.name} {_v2Materials[materialName].shader.name}");
                }
            }
        }
    }

#if SUBNAUTICA

    public static Sprite GetIconByName(string iconName, string modPackName)
    {
        if (loadedIcons.TryGetValue(iconName, out Sprite preLoadedBundle))
        {
            return preLoadedBundle;
        }
        var bundleName = ModRegistrationService.GetModPackData(modPackName).GetBundleName();
        var g = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(bundleName);
        var result =  g.LoadAsset<Sprite>(iconName);

        return result;
    }

    //public static Atlas.Sprite GetIconByName(string iconName, string modPackName)
    //{
    //    if (loadedIcons.TryGetValue(iconName, out Sprite loadedSprite))
    //    {
    //        return new Atlas.Sprite(loadedSprite);
    //    }

    //    var spritePrefab = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(iconName);
    //    if (spritePrefab != null)
    //    {
    //        var spriteg = spritePrefab.LoadAsset<Sprite>(iconName);
    //        loadedIcons.Add(iconName, spriteg);
    //        return new Atlas.Sprite(spriteg);
    //    }

    //    return null;
    //}
#else
    public static Sprite GetIconByName(string iconName, string modPackName)
    {
        if (loadedIcons.TryGetValue(iconName, out Sprite preLoadedBundle))
        {
            return preLoadedBundle;
        }

        var g = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(iconName);
        return g.LoadAsset<Sprite>(iconName);
    }
#endif

#if SUBNAUTICA
    public static Atlas.Sprite GetIconByNameFromFile(string iconName, string modPackName)
    {
        if (loadedIcons.TryGetValue(iconName, out Sprite preLoadedBundle))
        {
            return new Atlas.Sprite(preLoadedBundle);
        }
        return ImageUtils.LoadSpriteFromFile(Path.Combine(ModRegistrationService.GetModPackData(modPackName).GetAssetPath(), $"{iconName}.png"));
    }

#else
    public static Sprite GetIconByNameFromFile(string iconName, string modPackName)
    {

        if (loadedIcons.TryGetValue(iconName, out Sprite preLoadedBundle))
        {
            return preLoadedBundle;
        }

        return ImageUtils.LoadSpriteFromFile(Path.Combine(ModRegistrationService.GetModPackData(modPackName).GetAssetPath(), $"{iconName}.png"));
    }
#endif
}

internal class NoMaterial : MaterialModifier
{
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {

    }

    public override bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        var particle = renderer.gameObject.GetComponent<ParticleSystem>();

        return particle is not null;
    }
}