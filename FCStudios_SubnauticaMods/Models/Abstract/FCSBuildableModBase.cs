using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.Models.Structs;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Utility;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_AlterraHub.Models.Abstract;

public abstract class FCSBuildableModBase : ModBase, IModBase
{
    protected FCSModItemSettings _settings;
    private string _modName;
    private  readonly string _classID;
    private readonly string AssetsFolder;


    protected FCSBuildableModBase(string modName, string prefabName,string modDir, string classId, string friendlyName) : base(friendlyName)
    {
        _modName = modName;
        _classID = classId;
        AssetsFolder = ModRegistrationService.GetModPackData(modName)?.GetAssetPath();
        Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(prefabName, ModRegistrationService.GetModPackData(modName)?.GetBundleName(),modDir);
    }

    public virtual void PatchSMLHelper() 
    {
        _settings = FCSModsAPI.InternalAPI.GetModSettings(_modName, _classID);
        PrefabInfo = PrefabInfo.WithTechType(_classID, FriendlyName, _settings.Description);
        PrefabInfo.WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{_classID}.png")));
        Register();
    }

    public void Register()
    {
        var prefab = new CustomPrefab(PrefabInfo);

        // Set our prefab to a clone of the Seamoth electrical defense module
        prefab.SetGameObject(GetGameObjectAsync);

        // Make the Vehicle upgrade console a requirement for our item's blueprint
        ScanningGadget scanning = prefab.SetUnlock(TechType.None);

        // Add our item to the Vehicle upgrades category
        scanning.WithPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);

        prefab.SetRecipe(GetRecipe());

        // register the coal to the game
        prefab.Register();

        OnFinishRegistering();
    }

    public virtual void OnFinishRegistering()
    {

    }

    public IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        var prefab = GameObject.Instantiate(Prefab);
                    
        GameObjectHelpers.AddConstructableBounds(prefab, _settings.ConstructableSize, _settings.ConstructableCenter);

        var model = prefab.FindChild("model");

        //========== Allows the building animation and material colors ==========//
        GameObjectHelpers.SetDefaultSkyApplier(prefab);
        //========== Allows the building animation and material colors ==========// 

        var lw = prefab.AddComponent<LargeWorldEntity>();
        lw.cellLevel = _settings.CellLevel;

        // Add constructible
        var constructable = prefab.AddComponent<Constructable>();

        constructable.allowedOutside = _settings.AllowedOutside;
        constructable.allowedInBase = _settings.AllowedInBase;
        constructable.allowedOnGround = _settings.AllowedOnGround;
        constructable.allowedOnWall = _settings.AllowedOnWall;
        constructable.rotationEnabled = _settings.RotationEnabled;
        constructable.allowedOnCeiling = _settings.AllowedOnCeiling;
        constructable.allowedInSub = _settings.AllowedInSub;
        constructable.allowedOnConstructables = _settings.AllowedOnConstructables;
        constructable.model = model;
        constructable.techType = PrefabInfo.TechType;

        PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
        prefabID.ClassId = _classID;

        prefab.AddComponent<TechTag>().type = PrefabInfo.TechType;

        var hover = prefab.AddComponent<HoverInteraction>();
        hover.TechType = PrefabInfo.TechType;
        
        if (_settings.HasGlass)
        {
            //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
            MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Plugin.ModSettings.ModPackID);
        }

        Prefab = prefab;

        yield return ModifyPrefab(prefab);

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
