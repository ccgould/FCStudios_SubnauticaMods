using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.Models.Structs;
using FCSCommon.Utilities;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
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
    protected readonly string AssetsFolder;

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

        //// Make the Vehicle upgrade console a requirement for our item's blueprint
        //ScanningGadget scanning = prefab.SetUnlock(TechType.None);

        // Add our item to the sets category
        prefab.SetPdaGroupCategory(_settings.TechGroup, _settings.TechCategory);

        KnownTechHandler.UnlockOnStart(prefab.Info.TechType);
        
        //scanning.WithPdaGroupCategory(_settings.TechGroup, _settings.TechCategory);

        prefab.SetRecipe(GetRecipe());

        // register the coal to the game
        prefab.Register();

        OnFinishRegister?.Invoke();
    }


    

    public IEnumerator SetGameObjectAsync(IOut<GameObject> gameObject)
    {
        QuickLogger.Debug($"SetGameObjectAsync Prefab: {Prefab?.name}");
       var prefab = GameObject.Instantiate(Prefab);
        
        if (_settings.HasGlass)
        {
            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Plugin.ModSettings.ModPackID);
        }

        yield return ModifyPrefab(prefab);

        prefab.SetActive(false);

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
