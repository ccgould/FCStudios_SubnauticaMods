﻿using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.Models.Structs;
using FCSCommon.Utilities;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract;

public abstract class FCSBuildableModBase : ModBase, IModBase
{
    protected FCSModItemSettings _settings;
    private string _modName;
    protected readonly string _classID;
    protected readonly string _friendlyName;
    private readonly string _prefabName;
    private readonly string AssetsFolder;

    protected FCSBuildableModBase(string modName, string prefabName,string modDir, string classId, string friendlyName) : base(friendlyName)
    {
        _modName = modName;
        _classID = classId;
        _friendlyName = friendlyName;
        _prefabName = prefabName;
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();
        Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName, ModRegistrationService.GetModPackData(modName)?.GetBundleName(), modDir) ?? FCSAssetBundlesService.InternalAPI.GetLocalPrefab("DummyObject");
        QuickLogger.Debug($"FCSBuildable Prefab: {Prefab?.name}");
    }

    public virtual void PatchSMLHelper() 
    {
        _settings = FCSModsAPI.InternalAPI.GetModSettings(_modName, _classID);
        PrefabInfo = PrefabInfo.WithTechType(_classID, FriendlyName, _settings.Description)
            .WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{_classID}.png")))
            .WithFileName(_prefabName);
        Register();
    }

    public void Register()
    {
        OnStartRegister?.Invoke(); 

        var prefab = new CustomPrefab(PrefabInfo);

        // Set our prefab to a clone of the Seamoth electrical defense module
        prefab.SetGameObject(SetGameObjectAsync);

        // Make the Vehicle upgrade console a requirement for our item's blueprint
        ScanningGadget scanning = prefab.SetUnlock(TechType.None);

        // Add our item to the Vehicle upgrades category
        scanning.WithPdaGroupCategory(_settings.TechGroup, _settings.TechCategory);

        prefab.SetRecipe(GetRecipe());

        // register the coal to the game
        prefab.Register();

        OnFinishRegister?.Invoke();
    }


    

    public IEnumerator SetGameObjectAsync(IOut<GameObject> gameObject)
    {
        QuickLogger.Debug($"SetGameObjectAsync Prefab: {Prefab?.name}");
        var prefab = GameObject.Instantiate(Prefab);

        //========== Allows the building animation and material colors ==========//
        //GameObjectHelpers.SetDefaultSkyApplier(prefab);
        //========== Allows the building animation and material colors ==========// 


        var constructable = prefab.GetComponent<Constructable>();
        var pid = prefab.GetComponent<PrefabIdentifier>();

        QuickLogger.Info($"==================== PID {pid.Id} Name: {prefab.name} ===================================");

        QuickLogger.Info($"In Base: {prefab.GetComponentInParent<SubRoot>() is null}");

        QuickLogger.Info($"Constructed: {constructable._constructed}");
        QuickLogger.Info($"IsNew: {constructable.isNew}");
        QuickLogger.Info($"IsInside: {constructable.isInside}");
        QuickLogger.Info($"Constructed Amount: {constructable.constructedAmount}");

        QuickLogger.Info($"==================== PID {pid.Id} ===================================");


        var lw = prefab.GetComponent<LargeWorldEntity>();
        if(lw is not null)
            lw.cellLevel = _settings.CellLevel;
        
        if (_settings.HasGlass)
        {
            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Plugin.ModSettings.ModPackID);
        }

        yield return ModifyPrefab(prefab);

        //MaterialUtils.ApplySNShaders(prefab);

        gameObject.Set(prefab);
        yield break;
    }

    /// <summary>
    /// Changes to the prefab can be applied here.
    /// </summary>
    /// <param name="prefab"></param>>
    /// <returns></returns>
    protected abstract IEnumerator ModifyPrefab(GameObject prefab);
}
